using System;

namespace BannerLord.Common.Xml
{
    public struct ModuleVersion: IEquatable<ModuleVersion>, IComparable<ModuleVersion>
    {
        private static readonly ModuleVersionTypeComparer TypeComparer;
        private const int DefaultChangeSet = 226961;
        
        static ModuleVersion()
        {
            TypeComparer = new ModuleVersionTypeComparer();
        }
        
        public ModuleVersionType Type { get; }
        public int Major { get; }
        public int Minor { get; }
        public int Revision { get; }
        public int ChangeSet { get; }

        public string AsString
        {
            get
            {
                var changeSet = (this.ChangeSet == DefaultChangeSet) ? string.Empty : $".{this.ChangeSet}";
                return $"{GetPrefix(this.Type)}{this.Major}.{this.Minor}.{this.Revision}{changeSet}";
            }
        }
        
        public static readonly ModuleVersion Empty = new ModuleVersion(ModuleVersionType.Invalid, -1, -1, -1, -1);
        
        public ModuleVersion(ModuleVersionType type, int major, int minor, int revision, int changeSet)
        {
            this.Type = type;
            this.Major = major;
            this.Minor = minor;
            this.Revision = revision;
            this.ChangeSet = changeSet;
        }

        public static ModuleVersion FromString(string input)
        {
            var array = input.Split(new char[]
            {
                '.'
            });
            if (array.Length != 3 && array.Length != 4)
            {
                throw new Exception("Wrong version as string");
            }
            var applicationVersionType = ModuleVersion.ApplicationVersionTypeFromString(array[0][0].ToString());
            var value = array[0].Substring(1);
            var value2 = array[1];
            var value3 = array[2];
            var major = Convert.ToInt32(value);
            var minor = Convert.ToInt32(value2);
            var revision = Convert.ToInt32(value3);
            var changeSet = (array.Length > 3) ? Convert.ToInt32(array[3]) : DefaultChangeSet;
            return new ModuleVersion(applicationVersionType, major, minor, revision, changeSet);
        }

        private static ModuleVersionType ApplicationVersionTypeFromString(string applicationVersionTypeAsString)
        {
            return applicationVersionTypeAsString switch
            {
                "a" => ModuleVersionType.Alpha,
                "b" => ModuleVersionType.Beta,
                "e" => ModuleVersionType.EarlyAccess,
                "v" => ModuleVersionType.Release,
                "d" => ModuleVersionType.Development,
                _ => ModuleVersionType.Invalid
            };
        }

        public static bool operator ==(ModuleVersion a, ModuleVersion b)
        {
            return TypeComparer.Equals(a.Type, b.Type) && a.Major == b.Major && a.Minor == b.Minor && a.Revision == b.Revision &&
                   a.ChangeSet == b.ChangeSet;
        }

        public static bool operator !=(ModuleVersion a, ModuleVersion b)
        {
            return !TypeComparer.Equals(a.Type, b.Type) || a.Major != b.Major || a.Minor != b.Minor || a.Revision != b.Revision ||
                   a.ChangeSet != b.ChangeSet;
        }

        public bool Equals(ModuleVersion other)
        {
            return this == other;
        }

        public int CompareTo(ModuleVersion other)
        {
            var res = TypeComparer.Compare(this.Type, other.Type);
            if (res != 0) return res;
            res = this.Major.CompareTo(other.Major);
            if (res != 0) return res;
            res = this.Minor.CompareTo(other.Minor);
            if (res != 0) return res;
            res = this.Revision.CompareTo(other.Revision);
            if (res != 0) return res;
            res = this.ChangeSet.CompareTo(other.ChangeSet);
            return res;
        }
        
        public override bool Equals(object obj)
        {
            return obj is ModuleVersion other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) this.Type;
                hashCode = (hashCode * 397) ^ this.Major;
                hashCode = (hashCode * 397) ^ this.Minor;
                hashCode = (hashCode * 397) ^ this.Revision;
                hashCode = (hashCode * 397) ^ this.ChangeSet;
                return hashCode;
            }
        }
        
        public static bool operator <(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) >= 0;
        }
        
        private static string GetPrefix(ModuleVersionType applicationVersionType)
        {
            return applicationVersionType switch
            {
                ModuleVersionType.Alpha => "a",
                ModuleVersionType.Beta => "b",
                ModuleVersionType.EarlyAccess => "e",
                ModuleVersionType.Release => "v",
                ModuleVersionType.Development => "d",
                _ => "i"
            };
        }
    }
}