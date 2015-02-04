using System;
using System.Diagnostics;
using System.Linq;

namespace Orleankka
{
    [DebuggerDisplay("{ToString()}")]
    public struct ObserverPath : IEquatable<ObserverPath>
    {
        public static readonly ObserverPath Empty = new ObserverPath();

        readonly string path;

        ObserverPath(string path)
        {
            this.path = path;
        }

        public static ObserverPath Parse(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An observer path cannot be empty or contain whitespace only", "path");

            return Deserialize(path);
        }
        
        public static ObserverPath Deserialize(string path)
        {
            return new ObserverPath(path);
        }

        public string Serialize()
        {
            return path;
        }

        public bool Equals(ObserverPath other)
        {
            return string.Equals(path, other.path);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is ObserverPath && Equals((ObserverPath)obj));
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }

        public static bool operator ==(ObserverPath left, ObserverPath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObserverPath left, ObserverPath right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}
