using System.Collections.Generic;

namespace BannerLord.Common.Xml
{
    public class ModuleVersionTypeComparer : IComparer<ModuleVersionType>, IEqualityComparer<ModuleVersionType> {

        // ReSharper disable AssignNullToNotNullAttribute
        public int Compare(ModuleVersionType x, ModuleVersionType y)
            =>
                this.GetHashCode(x) - this.GetHashCode(y);
        // ReSharper restore AssignNullToNotNullAttribute

        public bool Equals(ModuleVersionType x, ModuleVersionType y)
            =>
                this.Compare(x, y) == 0;

        public int GetHashCode(ModuleVersionType obj)
            => (int) obj + 1 % 5;
    }
}