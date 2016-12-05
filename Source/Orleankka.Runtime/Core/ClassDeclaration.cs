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
    class ClassDeclaration
    {
        public static IEnumerable<ActorType> Generate(IEnumerable<Assembly> assemblies, IEnumerable<ActorConfiguration> configs)
        {
            var declarations = configs.Select(x => new ClassDeclaration(x)).ToArray();

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

        static string Generate(IEnumerable<Assembly> assemblies, IEnumerable<ClassDeclaration> declarations)
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

        readonly string clazz;
        readonly IList<string> namespaces;
        readonly ActorConfiguration config;

        ClassDeclaration(ActorConfiguration config)
        {
            this.config = config;

            var path = config.Name.Split(separator, StringSplitOptions.RemoveEmptyEntries);
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

            if (IsReentrant)
                src.AppendLine("[Reentrant]");

            if (IsPartiallyReentrant)
                src.AppendLine("[MayInterleave(\"MayInterleave\")]");

            src.AppendLine($"public class {clazz} : global::Orleankka.Core.ActorEndpoint<I{clazz}>, I{clazz} {{");
            src.AppendLine($"public static bool MayInterleave(InvokeMethodRequest req) => type.MayInterleave(req);");
            src.AppendLine("}");
        }

        bool IsReentrant => config.Reentrant;
        bool IsPartiallyReentrant => config.InterleavePredicate != null;

        ActorType From(Assembly asm)
        {
            var grain = asm.GetType(FullPath($"{clazz}"));
            return new ActorType(config.Name, config.KeepAliveTimeout, config.Sticky, config.InterleavePredicate, grain, config.Type, config.Invoker);
        }

        string FullPath(string name) => string.Join(".", new List<string>(namespaces) { name });

        void GenerateAttributes(StringBuilder src)
        {
            if (config.Worker)
            {
                src.AppendLine("[StatelessWorker]");
                return;
            }

            src.AppendLine($"[{GetActorPlacement()}]");
        }

        string GetActorPlacement()
        {
            switch (config.Placement)
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