using System;

using Orleans.CodeGeneration;
using Orleans.Runtime;

namespace Orleans.Internals
{
    public static class GrainUtility
    {
        public static IGrainRuntime Runtime(this Grain grain) => grain.Runtime;
        public static int InterfaceId(this Type grainInterface) => GrainInterfaceUtils.GetGrainInterfaceId(grainInterface);
        public static ushort InterfaceVersion(this Type grainInterface) => GrainInterfaceUtils.GetGrainInterfaceVersion(grainInterface);
    }
}