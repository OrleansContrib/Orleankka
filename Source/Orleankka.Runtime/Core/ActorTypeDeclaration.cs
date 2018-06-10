using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Orleans.Concurrency;
using Orleans.Internals;
using Orleans.MultiCluster;
using Orleans.Placement;
using Orleans.Providers;

namespace Orleankka.Core
{
    class ActorTypeDeclaration
    {
        public static IEnumerable<ActorType> Generate(IEnumerable<Assembly> assemblies, IEnumerable<Type> types, string[] conventions)
        {
            var declarations = types
                .Select(x => new ActorTypeDeclaration(x))
                .ToArray();

            var binary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orleankka.Auto.Implementations.dll");
            var generated = Generate(assemblies, declarations);

            var syntaxTree = CSharpSyntaxTree.ParseText(generated.Source);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Concat(generated.References)
                .Concat(declarations.Select(x => x.@interface.GrainAssembly()))
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

            var assembly = Assembly.LoadFrom(binary);
            return declarations.Select(x => x.From(assembly, conventions)).ToArray();
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
                 using Orleans.MultiCluster;
            ");

            foreach (var assembly in assemblies)
                sb.AppendLine($"[assembly: KnownAssembly(\"{assembly.GetName().Name}\")]");

            var results = declarations.Select(x => x.Generate()).ToArray();
            return new GenerateResult(sb, results);
        }

        public static Assembly GeneratedAssembly() => 
            AppDomain.CurrentDomain
                     .GetAssemblies()
                     .Where(x => x.FullName.Contains("Orleankka.Auto.Implementations"))
                     .SingleOrDefault(x => x.ExportedTypes.Any());

        static readonly string[] separator = {".", "+"};

        readonly Type actor;
        readonly ActorInterface @interface;
        readonly string clazz;
        readonly IList<string> namespaces;

        ActorTypeDeclaration(Type actor)
        {
            this.actor = actor;

            var typeName = ActorTypeName.Of(actor);
            @interface = ActorInterface.Of(typeName);

            var path = typeName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
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
            CopyAttributes(src);

            var reentrant = InterleaveAttribute.IsReentrant(actor);
            if (reentrant)
                src.AppendLine($"[global::{typeof(Orleans.Concurrency.ReentrantAttribute).FullName}]");

            var mayInterleave = InterleaveAttribute.MayInterleavePredicate(actor) != null;
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

        ActorType From(Assembly asm, string[] conventions)
        {
            var grain = asm.GetType(FullPath($"{clazz}"));
            return new ActorType(actor, @interface, grain, conventions);
        }

        string FullPath(string name) => string.Join(".", new List<string>(namespaces) { name });

        void CopyAttributes(StringBuilder src)
        {
            var worker = actor.GetCustomAttribute<StatelessWorkerAttribute>();
            var placement = GetCustomAttributesAssignableFrom<PlacementAttribute>(actor);

            if (worker != null)
            {
                src.AppendLine($"[{nameof(StatelessWorkerAttribute)}({worker.MaxLocalWorkers()})]");
                return;
            }

            if (placement.Length > 1)
                throw new InvalidOperationException(
                    $"Only single placement could be configured for an actor: {actor}");

            if (placement.Any())
                src.AppendLine($"[{GetActorPlacement(placement[0])}]");

            var storageProvider = actor.GetCustomAttribute<StorageProviderAttribute>();
            if (storageProvider != null && placement.Any())
                throw new InvalidOperationException(
                    $"Storage provider cannot be configured for {nameof(StorageProviderAttribute).Replace("Attribute", "")} actor: {actor}");

            if (storageProvider != null)
                src.AppendLine($"[{nameof(StorageProviderAttribute)}(ProviderName=\"{storageProvider.ProviderName}\")]");

            var registration = GetCustomAttributesAssignableFrom<RegistrationAttribute>(actor);
            if (registration.Length > 1)
                throw new InvalidOperationException(
                    $"Multiple multi-cluster registrations are specified for actor: {actor}");

            if (registration.Length > 0)
                src.AppendLine($"[{GetMultiClusterRegistration(registration[0])}]");
        }

        static T[] GetCustomAttributesAssignableFrom<T>(MemberInfo member) => 
            member.GetCustomAttributes().Where(x => x is T).Cast<T>().ToArray();

        static string GetMultiClusterRegistration(RegistrationAttribute registration)
        {
            switch (registration)
            {
                case GlobalSingleInstanceAttribute gs: return typeof(GlobalSingleInstanceAttribute).Name;
                case OneInstancePerClusterAttribute one: return typeof(OneInstancePerClusterAttribute).Name;
                default:
                    throw new InvalidOperationException($"Unsupported {nameof(RegistrationAttribute)}: {registration.GetType()}");
            }
        }

        static string GetActorPlacement(PlacementAttribute placement)
        {
            switch (placement)
            {
                case RandomPlacementAttribute rand: return typeof(RandomPlacementAttribute).Name;
                case PreferLocalPlacementAttribute local: return typeof(PreferLocalPlacementAttribute).Name;
                case ActivationCountBasedPlacementAttribute count: return typeof(ActivationCountBasedPlacementAttribute).Name;
                case HashBasedPlacementAttribute hash: return typeof(HashBasedPlacementAttribute).Name;
                default: return GenerateCustomPlacement(placement);
            }
        }

        static string GenerateCustomPlacement(PlacementAttribute placement)
        {
            var properties = placement.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(x => x.CanRead);

            var setters = properties.Select(x => new {Property = x, Setter = GenerateAttributePropertySetter(x, placement)})
                .Where(x => x.Setter != null)
                .Select(x => $"{char.ToLowerInvariant(x.Property.Name[0]) + x.Property.Name.Substring(1)}:{x.Setter}")
                .ToList();

            return "global::" + placement.GetType().FullName + $"({string.Join(",", setters)})";
        }

        static string GenerateAttributePropertySetter(PropertyInfo p, object obj)
        {
            if (p.PropertyType == typeof(Type))
                return $"typeof(global::{p.GetValue(obj)})";

            if (p.PropertyType.IsEnum)
                return $"global::{p.PropertyType.FullName}.{p.GetValue(obj)}";

            if (p.PropertyType == typeof(string))
                return $"\"{p.GetValue(obj)}\"";

            if (p.PropertyType == typeof(bool))
                return $"{p.GetValue(obj).ToString().ToLower()}";

            if (p.PropertyType == typeof(short) || 
                p.PropertyType == typeof(int) || 
                p.PropertyType == typeof(long))
                return $"{p.GetValue(obj)}";

            if (p.PropertyType == typeof(double))
                return $"{p.GetValue(obj)}d";

            if (p.PropertyType == typeof(float))
                return $"{p.GetValue(obj)}f";

            return null;
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