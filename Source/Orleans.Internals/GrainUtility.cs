using System;

using Orleans;
using Orleans.Core;
using Orleans.CodeGeneration;
using Orleans.Runtime;

namespace Orleans.Internals
{
    public static class GrainUtility
    {
        public static IGrainRuntime Runtime(this Grain grain) => grain.Runtime();
        public static int TypeCode(this Type type) => GrainInterfaceUtils.GetGrainClassTypeCode(type);
        public static ushort InterfaceVersion(this Type grainInterface) => GrainInterfaceUtils.GetGrainInterfaceVersion(grainInterface);
    }
}