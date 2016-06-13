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
    class ActorDeclaration
    {
        public static IEnumerable<ActorType> Generate(IEnumerable<ActorConfiguration> configs)
        {
            var declarations = configs.Select(x => new ActorDeclaration(x)).ToArray();

            var dir = Path.Combine(Path.GetTempPath(), "Orleankka.Auto");
            Directory.CreateDirectory(dir);

            var binary = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".dll");
            var source = Generate(declarations);

            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.IsDynamic ? null : MetadataReference.CreateFromFile(x.Location))
                .Where(x => x != null)
                .ToArray();

            var compilation = CSharpCompilation.Create("Orleankka.Auto",
                syntaxTrees: new[] {syntaxTree},
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

        static string Generate(IEnumerable<ActorDeclaration> declarations)
        {
            var sb = new StringBuilder(@"
                 using Orleankka;
                 using Orleankka.Core;
                 using Orleankka.Core.Endpoints;
                 using Orleans.Placement;
            ");

            foreach (var declaration in declarations)
                sb.AppendLine(declaration.Generate());

            return sb.ToString();
        }

        static readonly string[] separator = {".", "+"};

        readonly string code;
        readonly string clazz;
        readonly IList<string> namespaces;
        readonly ActorConfiguration config;

        ActorDeclaration(ActorConfiguration config)
        {
            this.config = config;
            this.code = config.Code;

            var path = code.Split(separator, StringSplitOptions.RemoveEmptyEntries);
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
            src.AppendLine(config.Worker  
                            ? "[StatelessWorker]"
                            : $"[{GetActorPlacement()}]");

            src.AppendLine($"public class {clazz} : global::Orleankka.Core.ActorEndpoint, I{clazz} {{");
            src.AppendLine($"public {clazz}() : base(\"{code}\") {{}}");
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

        ActorType From(Assembly asm)
        {
            var fullName = string.Join(".", new List<string>(namespaces) { $"I{clazz}" });
            var @interface = asm.GetType(fullName);
            return ActorType.From(config, @interface);
        }
    }
}