using System;
using System.Linq;

namespace Orleankka
{
    namespace Annotations
    {
        /// <summary>
        /// Indicates that the value of the marked element could be <c>null</c> sometimes,
        /// so the check for <c>null</c> is necessary before its usage
        /// </summary>
        /// <example><code>
        /// [CanBeNull] public object Test() { return null; }
        /// public void UseTest() {
        ///   var p = Test();
        ///   var s = p.ToString(); // Warning: Possible 'System.NullReferenceException'
        /// }
        /// </code></example>
        [AttributeUsage(
          AttributeTargets.Method | AttributeTargets.Parameter |
          AttributeTargets.Property | AttributeTargets.Delegate |
          AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
        public sealed class CanBeNullAttribute : Attribute { }

        /// <summary>
        /// Indicates that the value of the marked element could never be <c>null</c>
        /// </summary>
        /// <example><code>
        /// [NotNull] public object Foo() {
        ///   return null; // Warning: Possible 'null' assignment
        /// }
        /// </code></example>
        [AttributeUsage(
          AttributeTargets.Method | AttributeTargets.Parameter |
          AttributeTargets.Property | AttributeTargets.Delegate |
          AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
        public sealed class NotNullAttribute : Attribute { }

        /// <summary>
        ///   Indicates that the function argument should be string literal and match one  of the parameters of the caller
        ///   function.
        ///   For example, <see cref="ArgumentNullException" /> has such parameter.
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
        sealed class InvokerParameterNameAttribute : Attribute
        {}

        /// <summary>
        ///   Indicates that the marked method is assertion method, i.e. it halts control flow if one of the conditions is
        ///   satisfied.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        sealed class AssertionMethodAttribute : Attribute
        {}

        /// <summary>
        /// Indicates that the marked symbol is used implicitly
        /// (e.g. via reflection, in external library), so this symbol
        /// will not be marked as unused (as well as by other usage inspections)
        /// </summary>
        [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true), MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
        public sealed class UsedImplicitlyAttribute : Attribute
        {
            public UsedImplicitlyAttribute()
                : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) { }

            public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags)
                : this(useKindFlags, ImplicitUseTargetFlags.Default) { }

            public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags)
                : this(ImplicitUseKindFlags.Default, targetFlags) { }

            public UsedImplicitlyAttribute(
              ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
            {
                UseKindFlags = useKindFlags;
                TargetFlags = targetFlags;
            }

            public ImplicitUseKindFlags UseKindFlags { get; private set; }
            public ImplicitUseTargetFlags TargetFlags { get; private set; }
        }

        /// <summary>
        /// Should be used on attributes and causes ReSharper
        /// to not mark symbols marked with such attributes as unused
        /// (as well as by other usage inspections)
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        public sealed class MeansImplicitUseAttribute : Attribute
        {
            public MeansImplicitUseAttribute()
                : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) { }

            public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags)
                : this(useKindFlags, ImplicitUseTargetFlags.Default) { }

            public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags)
                : this(ImplicitUseKindFlags.Default, targetFlags) { }

            public MeansImplicitUseAttribute(
              ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
            {
                UseKindFlags = useKindFlags;
                TargetFlags = targetFlags;
            }

            [UsedImplicitly] public ImplicitUseKindFlags UseKindFlags { get; private set; }
            [UsedImplicitly] public ImplicitUseTargetFlags TargetFlags { get; private set; }
        }

        [Flags]
        public enum ImplicitUseKindFlags
        {
            Default = Access | Assign | InstantiatedWithFixedConstructorSignature,
            /// <summary>Only entity marked with attribute considered used</summary>
            Access = 1,
            /// <summary>Indicates implicit assignment to a member</summary>
            Assign = 2,
            /// <summary>
            /// Indicates implicit instantiation of a type with fixed constructor signature.
            /// That means any unused constructor parameters won't be reported as such.
            /// </summary>
            InstantiatedWithFixedConstructorSignature = 4,
            /// <summary>Indicates implicit instantiation of a type</summary>
            InstantiatedNoFixedConstructorSignature = 8,
        }

        /// <summary>
        /// Specify what is considered used implicitly
        /// when marked with <see cref="MeansImplicitUseAttribute"/>
        /// or <see cref="UsedImplicitlyAttribute"/>
        /// </summary>
        [Flags]
        public enum ImplicitUseTargetFlags
        {
            Default = Itself,
            Itself = 1,
            /// <summary>Members of entity marked with attribute are considered used</summary>
            Members = 2,
            /// <summary>Entity marked with attribute and all its members considered used</summary>
            WithMembers = Itself | Members
        }

        /// <summary>
        /// This attribute is intended to mark publicly available API
        /// which should not be removed and so is treated as used
        /// </summary>
        [MeansImplicitUse]
        public sealed class PublicAPIAttribute : Attribute
        {
            public PublicAPIAttribute() { }
            public PublicAPIAttribute([NotNull] string comment)
            {
                Comment = comment;
            }

            [NotNull]
            public string Comment { get; private set; }
        }

        /// <summary>
        /// Tells code analysis engine if the parameter is completely handled
        /// when the invoked method is on stack. If the parameter is a delegate,
        /// indicates that delegate is executed while the method is executed.
        /// If the parameter is an enumerable, indicates that it is enumerated
        /// while the method is executed
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
        public sealed class InstantHandleAttribute : Attribute { }
    }
}
