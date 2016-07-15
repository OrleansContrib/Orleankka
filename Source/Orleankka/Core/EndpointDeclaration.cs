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
    abstract class EndpointDeclaration
    {
        public static IEnumerable<ActorType> Generate(IEnumerable<EndpointConfiguration> configs)
        {
            var declarations = configs.Select(x => x.Declaration()).ToArray();

            var dir = Path.Combine(Path.GetTempPath(), "Orleankka.Auto");
            Directory.CreateDirectory(dir);

            var binary = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".dll");
            var source = Generate(declarations);

            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.IsDynamic || x.Location == "" ? null : MetadataReference.CreateFromFile(x.Location))
                .Where(x => x != null)
                .ToArray();

            var compilation = CSharpCompilation.Create("Orleankka.Auto",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var result = compilation.Emit(binary);
            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);
                throw new Exception("Bad code.\n\n" + string.Join("\n", failures));
            }

            var assemblyName = AssemblyName.GetAssemblyName(binary);
            var assembly = Assembly.Load(assemblyName);

            return declarations.Select(x => x.From(assembly));
        }

        static string Generate(IEnumerable<EndpointDeclaration> declarations)
        {
            var sb = new StringBuilder(@"
                 using Orleankka;
                 using Orleankka.Core;
                 using Orleankka.Core.Endpoints;
                 using Orleans.Placement;
                 using Orleans.Concurrency;
            ");

            foreach (var declaration in declarations)
                sb.AppendLine(declaration.Generate());

            return sb.ToString();
        }

        public static bool IsValidIdentifier(string code)
        {
            var path = code.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return path.All(SyntaxFacts.IsValidIdentifier);
        }

        static readonly string[] separator = {".", "+"};

        readonly string clazz;
        readonly IList<string> namespaces;
        readonly EndpointConfiguration config;

        protected EndpointDeclaration(EndpointConfiguration config)
        {
            this.config = config;

            var path = config.Code.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            clazz = path.Last();

            namespaces = path.TakeWhile(x => x != clazz).ToList();
            namespaces.Insert(0, "Fun");
        }

        string Generate()
        {
            var src = new StringBuilder();

            StartNamespace(src);
            GenerateInterface(src);
            GenerateImplementation(src);
            EndNamespace(src);

            return src.ToString();
        }

        void StartNamespace(StringBuilder src) =>
            src.AppendLine($"namespace {string.Join(".", namespaces)}");

        static void EndNamespace(StringBuilder src) =>
            src.AppendLine("}}");

        void GenerateInterface(StringBuilder src)
        {
            src.AppendLine("{");
            src.AppendLine($"public interface I{clazz} : global::Orleankka.Core.Endpoints.IActorEndpoint {{}}");
        }

        void GenerateImplementation(StringBuilder src)
        {
            GenerateAttributes(src);

            src.AppendLine($"public class {clazz} : global::Orleankka.Core.ActorEndpoint, I{clazz} {{");
            src.AppendLine($"public {clazz}() : base(\"{config.Code}\") {{}}");
        }

        protected abstract void GenerateAttributes(StringBuilder src);

        ActorType From(Assembly asm)
        {
            var fullName = string.Join(".", new List<string>(namespaces) { $"I{clazz}" });
            var @interface = asm.GetType(fullName);
            return Build(@interface);
        }

        protected abstract ActorType Build(Type @interface);
    }

    class ActorDeclaration : EndpointDeclaration
    {
        readonly ActorConfiguration config;

        public ActorDeclaration(ActorConfiguration config)
            : base(config)
        {
            this.config = config;
        }

        protected override ActorType Build(Type @interface) => 
            new ActorType(config.Code, config.KeepAliveTimeout, config.Sticky, config.Reentrancy, @interface, config.Receiver);

        protected override void GenerateAttributes(StringBuilder src) => 
            src.AppendLine($"[{GetActorPlacement()}]");

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

    class WorkerDeclaration : EndpointDeclaration
    {
        readonly WorkerConfiguration config;

        public WorkerDeclaration(WorkerConfiguration config)
            : base(config)
        {
            this.config = config;
        }

        protected override ActorType Build(Type @interface) =>
            new ActorType(config.Code, config.KeepAliveTimeout, config.Sticky, config.Reentrancy, @interface, config.Receiver);
        
        protected override void GenerateAttributes(StringBuilder src) => 
            src.AppendLine("[StatelessWorker]");
    }
}