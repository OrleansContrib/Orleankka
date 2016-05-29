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
        public static IEnumerable<ActorType> Generate(Assembly[] assemblies)
        {
            var outdir = AppDomain.CurrentDomain.BaseDirectory;
            var binary = Path.Combine(outdir, "Orleankka.Auto.dll");

            var declarations = assemblies.SelectMany(Scan).ToArray();
            var source = Generate(declarations);

            if (AppDomain.CurrentDomain.ShouldGenerateCode())
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(source);
                var references = AppDomain.CurrentDomain.GetAssemblies()
                    .Concat(assemblies)
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
            }

            var assemblyName = AssemblyName.GetAssemblyName(binary);
            var assembly = Assembly.Load(assemblyName);

            return declarations.Select(x => x.Bind(assembly));
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

        static IEnumerable<ActorDeclaration> Scan(Assembly assembly) => assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Actor).IsAssignableFrom(type))
            .Select(actor => new {actor, code = ActorTypeCode.Of(actor)})
            .Select(x => new ActorDeclaration(x.code, x.actor));

        static readonly string[] separator = {".", "+"};

        readonly string code;
        readonly Type actor;
        readonly string clazz;
        readonly IList<string> namespaces;

        ActorDeclaration(string code, Type actor)
        {
            this.code = code;
            this.actor = actor;

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
            var isActor = actor.GetCustomAttribute<ActorAttribute>() != null;
            var isWorker = actor.GetCustomAttribute<WorkerAttribute>() != null;

            if (isActor && isWorker)
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {actor}");

            src.AppendLine(isWorker
                            ? "[StatelessWorker]"
                            : $"[{GetActorPlacement()}]");

            src.AppendLine($"public class {clazz} : global::Orleankka.Core.ActorEndpoint, I{clazz} {{");
            src.AppendLine($"public {clazz}() : base(\"{code}\") {{}}");
        }

        string GetActorPlacement()
        {
            var attribute = actor.GetCustomAttribute<ActorAttribute>()
                            ?? new ActorAttribute();

            switch (attribute.Placement)
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

        ActorType Bind(Assembly asm)
        {
            var fullName = string.Join(".", new List<string>(namespaces) { $"I{clazz}" });
            var @interface = asm.GetType(fullName);
            return ActorType.From(code, @interface, actor);
        }
    }

    internal static class ActorDeclarationAppDomainExtensions
    {
        public static bool ShouldGenerateCode(this AppDomain domain) =>
            domain.GetData("SuppressCodeGeneration") == null;

        public static void SuppressCodeGeneration(this AppDomain domain) => 
            domain.SetData("SuppressCodeGeneration", true);
    }
}