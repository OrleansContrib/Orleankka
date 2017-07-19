using System;

using Orleans.CodeGeneration;

namespace Orleans.Internals
{
    static class GrainUtility
    {
        public static int InterfaceId(this Type grainInterface) => GrainInterfaceUtils.GetGrainInterfaceId(grainInterface);
        public static ushort InterfaceVersion(this Type grainInterface) => GrainInterfaceUtils.GetGrainInterfaceVersion(grainInterface);
    }
}