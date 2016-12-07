using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Orleans.Placement;

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
            var source = Generate(assemblies, declarations);

            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Select(ToMetadataReference)
                .Where(x => x != null)
                .Concat(ActorInterface.Registered().Select(x => x.GrainAssembly()).Distinct().Select(ToMetadataReference))
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

        static string Generate(IEnumerable<Assembly> assemblies, IEnumerable<ActorTypeDeclaration> declarations)
        {
            var sb = new StringBuilder(@"
                 using Orleankka;
                 using Orleankka.Core;
                 using Orleans.Placement;
                 using Orleans.Concurrency;
                 using Orleans.CodeGeneration;
            ");

            foreach (var assembly in assemblies)
                sb.AppendLine($"[assembly: KnownAssembly(\"{assembly.GetName().Name}\")]");

            foreach (var declaration in declarations)
                sb.AppendLine(declaration.Generate());

            return sb.ToString();
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

        string Generate()
        {
            var src = new StringBuilder();

            StartNamespace(src);
            GenerateImplementation(src);
            EndNamespace(src);

            return src.ToString();
        }

        void StartNamespace(StringBuilder src) =>
            src.AppendLine($"namespace {string.Join(".", namespaces)} {{");

        static void EndNamespace(StringBuilder src) =>
            src.AppendLine("}");

        void GenerateImplementation(StringBuilder src)
        {
            GenerateAttributes(src);

            var reentrant = ReentrantAttribute.IsReentrant(actor);
            if (reentrant)
                src.AppendLine("[Reentrant]");

            var mayInterleave = ReentrantAttribute.MayInterleavePredicate(actor) != null;
            if (mayInterleave)
                src.AppendLine("[MayInterleave(\"MayInterleave\")]");

            src.AppendLine($"public class {clazz} : global::Orleankka.Core.ActorEndpoint<I{clazz}>, I{clazz} {{");
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
    }
}