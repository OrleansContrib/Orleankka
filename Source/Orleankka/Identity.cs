using System;
using System.Linq;

using Orleans;

namespace Orleankka
{
    static class Identity
    {
        public static string Of(IActor grain)
        {
            string id;
            grain.GetPrimaryKey(out id);
            return id;
        }
    }
}
