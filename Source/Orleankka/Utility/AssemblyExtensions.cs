using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Orleankka.Utility
{
    public static class AssemblyExtensions
    {
        public static TextReader LoadEmbeddedResource(this Assembly assembly, string path)
        {
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    throw new MissingManifestResourceException(
                        string.Format("Unable to find resource with the path {0} in assembly {1}", path, assembly.FullName));

                return new StringReader(new StreamReader(stream).ReadToEnd());
            }
        }

    }
}
