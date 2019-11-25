using System.IO;

namespace ProcessManager
{
    public static class Extensions
    {
        public static void DeleteFileIfExists(this string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}