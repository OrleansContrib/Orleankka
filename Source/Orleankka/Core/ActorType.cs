using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Orleankka.Core
{
    using Utility;
    using Streams;

    class ActorType : IEquatable<ActorType>
    {
        static readonly Dictionary<string, ActorType> codes =
                    new Dictionary<string, ActorType>();

        static readonly Dictionary<Type, ActorType> types =
                    new Dictionary<Type, ActorType>();

        public static void Register(IEnumerable<Assembly> assemblies)
        {
            var actors = assemblies.SelectMany(Scan).ToArray();
            var source = Generate(actors);

            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.IsDynamic ? null : MetadataReference.CreateFromFile(x.Location))
                .Where(x => x != null)
                .ToArray();

            var assemblyName = Path.GetRandomFileName();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] {syntaxTree},
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            Assembly asm;
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                    throw new Exception("Bad code.\n\n" + string.Join("\n", failures));
                }

                ms.Seek(0, SeekOrigin.Begin);
                asm = Assembly.Load(ms.ToArray());
            }

            foreach (var actor in actors)
                Register(actor, asm);
        }

        static string Generate(IEnumerable<ActorType> actors)
        {
            var sb = new StringBuilder(@"
                 using Orleankka;
                 using Orleankka.Core;
                 using Orleankka.Core.Endpoints;
            ");

            foreach (var actor in actors)
            {
                var declaration = new ActorDeclaration(actor.Code);
                sb.AppendLine(declaration.Generate());
            }

            return sb.ToString();
        }

        class ActorDeclaration
        {
            static readonly string[] separator = {".", "+"};

            readonly string clazz;
            readonly IList<string> namespaces;

            public ActorDeclaration(string code)
            {
                var path = code.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                clazz = path.Last();

                namespaces = path.TakeWhile(x => x != clazz).ToList();
                namespaces.Insert(0, "Fun");
            }

            public string Generate()
            {
                var src = new StringBuilder($"namespace {string.Join(".", namespaces)}");
                src.AppendLine("{");
                src.AppendLine($"public interface I{clazz} : global::Orleankka.Core.Endpoints.IActorEndpoint {{}}");
                src.AppendLine($"public class {clazz} : global::Orleankka.Core.ActorEndpoint, I{clazz} {{}}");
                src.AppendLine("}");
                return src.ToString();
            }
        }

        static IEnumerable<ActorType> Scan(Assembly assembly) => assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Actor).IsAssignableFrom(type))
            .Select(actor => new {actor, code = ActorTypeCode.Of(actor)})
            .Select(x => From(x.code, x.actor));

        static void Register(ActorType actor, Assembly asm)
        {
            var registered = codes.Find(actor.Code);
            if (registered != null)
                throw new ArgumentException(
                    $"An actor with {actor.Code} has been already registered");

            actor.Interface.Bind(actor.Code, asm);

            codes.Add(actor.Code, actor);
            types.Add(actor.Interface.Type, actor);

            StreamSubscriptionMatcher.Register(actor);
        }

        public static void Reset()
        {
            codes.Clear();
            types.Clear();

            StreamSubscriptionMatcher.Reset();
        }

        public readonly string Code;
        public readonly ActorInterface Interface;
        public readonly ActorImplementation Implementation;

        ActorType(string code, ActorInterface @interface, ActorImplementation implementation)
        {
            // TODO: Check that code contain valid C# identifier chars only

            Code = code;
            Interface = @interface;
            Implementation = implementation;
        }

        public static ActorType Registered(Type type)
        {
            var result = types.Find(type);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map type '{type}' to the corresponding actor type. " +
                     "Make sure that you've registered the assembly containing this type");

            return result;
        }

        public static ActorType Registered(string code)
        {
            var result = codes.Find(code);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map code '{code}' to the corresponding actor type. " +
                     "Make sure that you've registered the assembly containing this type");

            return result;
        }

        static ActorType From(string code, Type type)
        {
            var @interface = ActorInterface.From(type);

            var implementation = type != null 
                ? ActorImplementation.From(type) 
                : ActorImplementation.Undefined;

            return new ActorType(code, @interface, implementation);
        }

        public bool Equals(ActorType other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(Code, other.Code));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj.GetType() == GetType() && Equals((ActorType) obj));
        }

        public static bool operator ==(ActorType left, ActorType right) => Equals(left, right);
        public static bool operator !=(ActorType left, ActorType right) => !Equals(left, right);

        public override int GetHashCode() => Code.GetHashCode();
        public override string ToString() => Code;
    }

    static class ActorTypeActorSystemExtensions
    {
        internal static ActorRef ActorOf(this IActorSystem system, ActorType type, string id)
        {
            return system.ActorOf(ActorPath.From(type.Code, id));
        }
    }
}