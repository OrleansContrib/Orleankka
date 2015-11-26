#if !EXCLUDE_CODEGEN
#pragma warning disable 162
#pragma warning disable 219
#pragma warning disable 414
#pragma warning disable 649
#pragma warning disable 693
#pragma warning disable 1591
#pragma warning disable 1998
[assembly: global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0")]
[assembly: global::Orleans.CodeGeneration.OrleansCodeGenerationTargetAttribute("Example.EventSourcing.Persistence.Streamstone, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
namespace Example
{
    using global::Orleans.Async;
    using global::Orleans;

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.CreateInventoryItem)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_CreateInventoryItemSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.CreateInventoryItem).@GetField("Name", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.CreateInventoryItem input = ((global::Example.CreateInventoryItem)original);
            global::Example.CreateInventoryItem result = (global::Example.CreateInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.CreateInventoryItem));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.CreateInventoryItem input = (global::Example.CreateInventoryItem)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.String));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.CreateInventoryItem result = (global::Example.CreateInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.CreateInventoryItem));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field0.@SetValue(result, (global::System.String)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.String), stream));
            return (global::Example.CreateInventoryItem)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.CreateInventoryItem), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_CreateInventoryItemSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.CheckInInventoryItem)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_CheckInInventoryItemSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.CheckInInventoryItem).@GetField("Quantity", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.CheckInInventoryItem input = ((global::Example.CheckInInventoryItem)original);
            global::Example.CheckInInventoryItem result = (global::Example.CheckInInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.CheckInInventoryItem));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.CheckInInventoryItem input = (global::Example.CheckInInventoryItem)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.Int32));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.CheckInInventoryItem result = (global::Example.CheckInInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.CheckInInventoryItem));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field0.@SetValue(result, (global::System.Int32)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.Int32), stream));
            return (global::Example.CheckInInventoryItem)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.CheckInInventoryItem), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_CheckInInventoryItemSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.CheckOutInventoryItem)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_CheckOutInventoryItemSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.CheckOutInventoryItem).@GetField("Quantity", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.CheckOutInventoryItem input = ((global::Example.CheckOutInventoryItem)original);
            global::Example.CheckOutInventoryItem result = (global::Example.CheckOutInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.CheckOutInventoryItem));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.CheckOutInventoryItem input = (global::Example.CheckOutInventoryItem)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.Int32));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.CheckOutInventoryItem result = (global::Example.CheckOutInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.CheckOutInventoryItem));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field0.@SetValue(result, (global::System.Int32)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.Int32), stream));
            return (global::Example.CheckOutInventoryItem)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.CheckOutInventoryItem), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_CheckOutInventoryItemSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.RenameInventoryItem)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_RenameInventoryItemSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.RenameInventoryItem).@GetField("NewName", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.RenameInventoryItem input = ((global::Example.RenameInventoryItem)original);
            global::Example.RenameInventoryItem result = (global::Example.RenameInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.RenameInventoryItem));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.RenameInventoryItem input = (global::Example.RenameInventoryItem)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.String));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.RenameInventoryItem result = (global::Example.RenameInventoryItem)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.RenameInventoryItem));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field0.@SetValue(result, (global::System.String)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.String), stream));
            return (global::Example.RenameInventoryItem)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.RenameInventoryItem), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_RenameInventoryItemSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.DeactivateInventoryItem)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_DeactivateInventoryItemSerializer
    {
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.DeactivateInventoryItem input = ((global::Example.DeactivateInventoryItem)original);
            global::Example.DeactivateInventoryItem result = new global::Example.DeactivateInventoryItem();
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.DeactivateInventoryItem input = (global::Example.DeactivateInventoryItem)untypedInput;
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.DeactivateInventoryItem result = new global::Example.DeactivateInventoryItem();
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            return (global::Example.DeactivateInventoryItem)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.DeactivateInventoryItem), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_DeactivateInventoryItemSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.GetInventoryItemDetails)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_GetInventoryItemDetailsSerializer
    {
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.GetInventoryItemDetails input = ((global::Example.GetInventoryItemDetails)original);
            global::Example.GetInventoryItemDetails result = new global::Example.GetInventoryItemDetails();
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.GetInventoryItemDetails input = (global::Example.GetInventoryItemDetails)untypedInput;
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.GetInventoryItemDetails result = new global::Example.GetInventoryItemDetails();
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            return (global::Example.GetInventoryItemDetails)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.GetInventoryItemDetails), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_GetInventoryItemDetailsSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.InventoryItemDetails)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_InventoryItemDetailsSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field2 = typeof (global::Example.InventoryItemDetails).@GetField("Active", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.InventoryItemDetails).@GetField("Name", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        private static readonly global::System.Reflection.FieldInfo field1 = typeof (global::Example.InventoryItemDetails).@GetField("Total", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.InventoryItemDetails input = ((global::Example.InventoryItemDetails)original);
            global::Example.InventoryItemDetails result = (global::Example.InventoryItemDetails)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemDetails));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field2.@SetValue(result, field2.@GetValue(input));
            field0.@SetValue(result, field0.@GetValue(input));
            field1.@SetValue(result, field1.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.InventoryItemDetails input = (global::Example.InventoryItemDetails)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field2.@GetValue(input), stream, typeof (global::System.Boolean));
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.String));
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field1.@GetValue(input), stream, typeof (global::System.Int32));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.InventoryItemDetails result = (global::Example.InventoryItemDetails)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemDetails));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field2.@SetValue(result, (global::System.Boolean)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.Boolean), stream));
            field0.@SetValue(result, (global::System.String)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.String), stream));
            field1.@SetValue(result, (global::System.Int32)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.Int32), stream));
            return (global::Example.InventoryItemDetails)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.InventoryItemDetails), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_InventoryItemDetailsSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.InventoryItemCreated)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_InventoryItemCreatedSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.InventoryItemCreated).@GetField("Name", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.InventoryItemCreated input = ((global::Example.InventoryItemCreated)original);
            global::Example.InventoryItemCreated result = (global::Example.InventoryItemCreated)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemCreated));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.InventoryItemCreated input = (global::Example.InventoryItemCreated)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.String));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.InventoryItemCreated result = (global::Example.InventoryItemCreated)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemCreated));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field0.@SetValue(result, (global::System.String)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.String), stream));
            return (global::Example.InventoryItemCreated)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.InventoryItemCreated), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_InventoryItemCreatedSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.InventoryItemCheckedIn)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_InventoryItemCheckedInSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.InventoryItemCheckedIn).@GetField("Quantity", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.InventoryItemCheckedIn input = ((global::Example.InventoryItemCheckedIn)original);
            global::Example.InventoryItemCheckedIn result = (global::Example.InventoryItemCheckedIn)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemCheckedIn));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.InventoryItemCheckedIn input = (global::Example.InventoryItemCheckedIn)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.Int32));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.InventoryItemCheckedIn result = (global::Example.InventoryItemCheckedIn)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemCheckedIn));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field0.@SetValue(result, (global::System.Int32)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.Int32), stream));
            return (global::Example.InventoryItemCheckedIn)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.InventoryItemCheckedIn), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_InventoryItemCheckedInSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.InventoryItemCheckedOut)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_InventoryItemCheckedOutSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.InventoryItemCheckedOut).@GetField("Quantity", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.InventoryItemCheckedOut input = ((global::Example.InventoryItemCheckedOut)original);
            global::Example.InventoryItemCheckedOut result = (global::Example.InventoryItemCheckedOut)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemCheckedOut));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.InventoryItemCheckedOut input = (global::Example.InventoryItemCheckedOut)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.Int32));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.InventoryItemCheckedOut result = (global::Example.InventoryItemCheckedOut)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemCheckedOut));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field0.@SetValue(result, (global::System.Int32)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.Int32), stream));
            return (global::Example.InventoryItemCheckedOut)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.InventoryItemCheckedOut), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_InventoryItemCheckedOutSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.InventoryItemRenamed)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_InventoryItemRenamedSerializer
    {
        private static readonly global::System.Reflection.FieldInfo field1 = typeof (global::Example.InventoryItemRenamed).@GetField("NewName", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        private static readonly global::System.Reflection.FieldInfo field0 = typeof (global::Example.InventoryItemRenamed).@GetField("OldName", (System.@Reflection.@BindingFlags.@Public | System.@Reflection.@BindingFlags.@NonPublic | System.@Reflection.@BindingFlags.@Instance));
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.InventoryItemRenamed input = ((global::Example.InventoryItemRenamed)original);
            global::Example.InventoryItemRenamed result = (global::Example.InventoryItemRenamed)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemRenamed));
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            field1.@SetValue(result, field1.@GetValue(input));
            field0.@SetValue(result, field0.@GetValue(input));
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.InventoryItemRenamed input = (global::Example.InventoryItemRenamed)untypedInput;
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field1.@GetValue(input), stream, typeof (global::System.String));
            global::Orleans.Serialization.SerializationManager.@SerializeInner(field0.@GetValue(input), stream, typeof (global::System.String));
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.InventoryItemRenamed result = (global::Example.InventoryItemRenamed)global::System.Runtime.Serialization.FormatterServices.@GetUninitializedObject(typeof (global::Example.InventoryItemRenamed));
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            field1.@SetValue(result, (global::System.String)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.String), stream));
            field0.@SetValue(result, (global::System.String)global::Orleans.Serialization.SerializationManager.@DeserializeInner(typeof (global::System.String), stream));
            return (global::Example.InventoryItemRenamed)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.InventoryItemRenamed), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_InventoryItemRenamedSerializer()
        {
            Register();
        }
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Orleans-CodeGenerator", "1.0.10.0"), global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute, global::Orleans.CodeGeneration.SerializerAttribute(typeof (global::Example.InventoryItemDeactivated)), global::Orleans.CodeGeneration.RegisterSerializerAttribute]
    internal class OrleansCodeGenExample_InventoryItemDeactivatedSerializer
    {
        [global::Orleans.CodeGeneration.CopierMethodAttribute]
        public static global::System.Object DeepCopier(global::System.Object original)
        {
            global::Example.InventoryItemDeactivated input = ((global::Example.InventoryItemDeactivated)original);
            global::Example.InventoryItemDeactivated result = new global::Example.InventoryItemDeactivated();
            global::Orleans.@Serialization.@SerializationContext.@Current.@RecordObject(original, result);
            return result;
        }

        [global::Orleans.CodeGeneration.SerializerMethodAttribute]
        public static void Serializer(global::System.Object untypedInput, global::Orleans.Serialization.BinaryTokenStreamWriter stream, global::System.Type expected)
        {
            global::Example.InventoryItemDeactivated input = (global::Example.InventoryItemDeactivated)untypedInput;
        }

        [global::Orleans.CodeGeneration.DeserializerMethodAttribute]
        public static global::System.Object Deserializer(global::System.Type expected, global::Orleans.Serialization.BinaryTokenStreamReader stream)
        {
            global::Example.InventoryItemDeactivated result = new global::Example.InventoryItemDeactivated();
            global::Orleans.@Serialization.@DeserializationContext.@Current.@RecordObject(result);
            return (global::Example.InventoryItemDeactivated)result;
        }

        public static void Register()
        {
            global::Orleans.Serialization.SerializationManager.@Register(typeof (global::Example.InventoryItemDeactivated), DeepCopier, Serializer, Deserializer);
        }

        static OrleansCodeGenExample_InventoryItemDeactivatedSerializer()
        {
            Register();
        }
    }
}
#pragma warning restore 162
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 649
#pragma warning restore 693
#pragma warning restore 1591
#pragma warning restore 1998
#endif
