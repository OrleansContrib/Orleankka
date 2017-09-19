using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Orleankka.Core
{
    class ActorInterfaceDeclaration
    {
        public static IEnumerable<ActorInterface> Generate(IEnumerable<Assembly> assemblies, IEnumerable<ActorInterfaceMapping> mappings)
        {
            var declarations = mappings.Select(m => new ActorInterfaceDeclaration(m)).Select(d => new
            {
                Declaration = d,
                Interface = d.Find()
            })
            .ToArray();

            var existent = declarations.Where(x => x.Interface != null).ToArray();
            var missing = declarations.Where(x => x.Interface == null).ToArray();

            var dir = Path.Combine(Path.GetTempPath(), "Orleankka.Auto.Interfaces");
            Directory.CreateDirectory(dir);

            var binary = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".dll");
            var source = Generate(assemblies, missing.Select(x => x.Declaration));

            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.IsDynamic || x.Location == "" ? null : MetadataReference.CreateFromFile(x.Location))
                .Where(x => x != null)
                .ToArray();

            var compilation = CSharpCompilation.Create("Orleankka.Auto.Interfaces",
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
            AppDomain.CurrentDomain.Load(assemblyName);

            var existentInterfaces = existent.Select(x => x.Interface);
            var generatedInterfaces = missing.Select(x => x.Declaration.Find());

            return existentInterfaces.Concat(generatedInterfaces);
        }

        static string Generate(IEnumerable<Assembly> assemblies, IEnumerable<ActorInterfaceDeclaration> declarations)
        {
            var sb = new StringBuilder(@"
                 using Orleankka;
                 using Orleankka.Core;
                 using Orleans.CodeGeneration;
            ");

            foreach (var assembly in assemblies)
                sb.AppendLine($"[assembly: KnownAssembly(\"{assembly.GetName().Name}\")]");

            foreach (var declaration in declarations)
                sb.AppendLine(declaration.Generate());

            return sb.ToString();
        }

        static readonly string[] separator = {".", "+"};

        readonly string name;
        readonly string fullPath;
        readonly IList<string> namespaces;
        readonly ActorInterfaceMapping mapping;

        ActorInterfaceDeclaration(ActorInterfaceMapping mapping)
        {
            this.mapping = mapping;
            CheckValidIdentifier(mapping.Name);

            var path = mapping.Name.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            name = path.Last();

            namespaces = path.TakeWhile(x => x != name).ToList();
            namespaces.Insert(0, "Fun");

            name = "I" + name;
            fullPath = string.Join(".", new List<string>(namespaces) {name});
        }

        internal static void CheckValidIdentifier(string name)
        {
            var path = name.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (!path.All(SyntaxFacts.IsValidIdentifier))
                throw new ArgumentException($"'{name}' is not valid actor type identifier", nameof(name));
        }

        ActorInterface Find()
        {
            var interfaceAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(x => x.FullName.Contains("Orleankka.Auto.Interfaces"));

            return interfaceAssembly != null
                ? new ActorInterface(mapping, interfaceAssembly.GetType(fullPath))
                : null;
        }

        string Generate()
        {
            var src = new StringBuilder();

            StartNamespace(src);
            GenerateInterface(src);
            EndNamespace(src);

            return src.ToString();
        }

        void StartNamespace(StringBuilder src) =>
            src.AppendLine($"namespace {string.Join(".", namespaces)} {{");

        static void EndNamespace(StringBuilder src) =>
            src.AppendLine("}");

        void GenerateInterface(StringBuilder src) => 
            src.AppendLine($"public interface {name} : global::Orleankka.Core.IActorEndpoint {{}}");
    }
}