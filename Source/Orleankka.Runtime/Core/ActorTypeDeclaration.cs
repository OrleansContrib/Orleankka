using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Orleans.Placement;
using Orleans.Providers;

namespace Orleankka.Core
{
    class ActorTypeDeclaration
    {
        public static IEnumerable<ActorType> Generate(Assembly[] assemblies)
        {
            var declarations = assemblies
                .SelectMany(x => x.ActorTypes())
                .Select(x => new ActorTypeDeclaration(x))
                .ToArray();

            var dir = Path.Combine(Path.GetTempPath(), "Orleankka.Auto.Implementations");
            Directory.CreateDirectory(dir);

            var binary = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".dll");
            var generated = Generate(assemblies, declarations);

            var syntaxTree = CSharpSyntaxTree.ParseText(generated.Source);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Concat(generated.References)
                .Concat(ActorInterface.Registered().Select(x => x.GrainAssembly()))
                .Distinct()
                .Select(ToMetadataReference)
                .Where(x => x != null)
                .ToArray();

            var compilation = CSharpCompilation.Create("Orleankka.Auto.Implementations",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var result = compilation.Emit(binary);
            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);
                throw new Exception("Bad type.\n\n" + string.Join("\n", failures));
            }

            var assemblyName = AssemblyName.GetAssemblyName(binary);
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);

            return declarations.Select(x => x.From(assembly));
        }

        static PortableExecutableReference ToMetadataReference(Assembly x) => 
            x.IsDynamic || x.Location == "" ? null : MetadataReference.CreateFromFile(x.Location);

        static GenerateResult Generate(IEnumerable<Assembly> assemblies, IEnumerable<ActorTypeDeclaration> declarations)
        {
            var sb = new StringBuilder(@"
                 using Orleankka;
                 using Orleankka.Core;
                 using Orleans.Placement;
                 using Orleans.Concurrency;
                 using Orleans.CodeGeneration;
                 using Orleans.Providers;
            ");

            foreach (var assembly in assemblies)
                sb.AppendLine($"[assembly: KnownAssembly(\"{assembly.GetName().Name}\")]");

            var results = declarations.Select(x => x.Generate()).ToArray();
            return new GenerateResult(sb, results);
        }

        static readonly string[] separator = {".", "+"};

        readonly Type actor;
        readonly string clazz;
        readonly IList<string> namespaces;

        ActorTypeDeclaration(Type actor)
        {
            this.actor = actor;

            var path = ActorTypeName.Of(actor).Split(separator, StringSplitOptions.RemoveEmptyEntries);
            clazz = path.Last();

            namespaces = path.TakeWhile(x => x != clazz).ToList();
            namespaces.Insert(0, "Fun");
        }

        GenerateResult Generate()
        {
            var src = new StringBuilder();
            var references = new List<Assembly>();

            StartNamespace(src);
            GenerateImplementation(src, references);
            EndNamespace(src);

            return new GenerateResult(src.ToString(), references);
        }

        void StartNamespace(StringBuilder src) =>
            src.AppendLine($"namespace {string.Join(".", namespaces)} {{");

        static void EndNamespace(StringBuilder src) =>
            src.AppendLine("}");

        void GenerateImplementation(StringBuilder src, List<Assembly> references)
        {
            GenerateAttributes(src);

            var reentrant = ReentrantAttribute.IsReentrant(actor);
            if (reentrant)
                src.AppendLine("[Reentrant]");

            var mayInterleave = ReentrantAttribute.MayInterleavePredicate(actor) != null;
            if (mayInterleave)
                src.AppendLine("[MayInterleave(\"MayInterleave\")]");

            string impl = $"Orleankka.Core.ActorEndpoint<I{clazz}>";
            if (IsStateful())
            {
                var stateType = GetStateArgument();
                var stateTypeFullName = stateType.FullName.Replace("+", ".");
                impl = $"Orleankka.Core.StatefulActorEndpoint<I{clazz}, global::{stateTypeFullName}>";
                references.Add(stateType.Assembly);
            }

            src.AppendLine($"public class {clazz} : global::{impl}, I{clazz} {{");
            src.AppendLine($"public static bool MayInterleave(InvokeMethodRequest req) => type.MayInterleave(req);");
            src.AppendLine("}");
        }

        ActorType From(Assembly asm)
        {
            var grain = asm.GetType(FullPath($"{clazz}"));
            return new ActorType(actor, grain);
        }

        string FullPath(string name) => string.Join(".", new List<string>(namespaces) { name });

        void GenerateAttributes(StringBuilder src)
        {
            var worker = actor.GetCustomAttribute<WorkerAttribute>() != null;
            var singleton = actor.GetCustomAttribute<WorkerAttribute>() == null;

            if (singleton && worker)
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {actor}");

            if (worker)
            {
                src.AppendLine("[StatelessWorker]");
                return;
            }

            src.AppendLine($"[{GetActorPlacement()}]");

            var storageProvider = actor.GetCustomAttribute<StorageProviderAttribute>();
            if (storageProvider != null)
                src.AppendLine($"[StorageProvider(ProviderName=\"{storageProvider.ProviderName}\")]");
        }

        string GetActorPlacement()
        {
            var placement = (actor.GetCustomAttribute<ActorAttribute>() ?? new ActorAttribute()).Placement;

            switch (placement)
            {
                case Placement.Random:
                    return typeof(RandomPlacementAttribute).Name;
                case Placement.PreferLocal:
                    return typeof(PreferLocalPlacementAttribute).Name;
                case Placement.DistributeEvenly:
                    return typeof(ActivationCountBasedPlacementAttribute).Name;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        bool IsStateful() => typeof(IStatefulActor).IsAssignableFrom(actor);

        Type GetStateArgument()
        {
            var current = actor;
            while (current.BaseType != null && 
                current.BaseType.GetGenericTypeDefinition() != typeof(StatefulActor<>))
                current = current.BaseType;

            Debug.Assert(current.BaseType != null);
            return current.BaseType.GetGenericArguments()[0];
        }

        class GenerateResult
        {
            public readonly string Source;
            public readonly IEnumerable<Assembly> References;

            public GenerateResult(string source, IEnumerable<Assembly> references)
            {
                Source = source;
                References = references;
            }

            public GenerateResult(StringBuilder sb, GenerateResult[] results)
            {
                Array.ForEach(results, x => sb.AppendLine(x.Source));
                References = results.SelectMany(x => x.References).ToList();
                Source = sb.ToString();
            }
        }
    }
}