using System;
using System.Collections.Generic;

namespace BannerLord.Common.Extensions
{
    public static class ComparerExtensions
    {
        public static IComparer<T> Create<T>(params Func<T, IComparable>[] keyFunctions)
        {
            IComparer<T> comparer = new FunctionComparer<T>(keyFunctions[0]);
            if (keyFunctions.Length == 1)
            {
                return comparer;
            }

            for (var i = 1; i < keyFunctions.Length; i++)
            {
                comparer = comparer.ThenComparing(keyFunctions[i]);
            }

            return comparer;
        }

        public static IComparer<T> ThenComparing<T>(this IComparer<T> comparator, Func<T, IComparable> thenComparing)
        {
            return new ChainedComparer<T>(comparator, new FunctionComparer<T>(thenComparing));
        }

        private sealed class ChainedComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _comp1;
            private readonly IComparer<T> _comp2;

            internal ChainedComparer(IComparer<T> comp1, IComparer<T> comp2)
            {
                this._comp1 = comp1;
                this._comp2 = comp2;
            }
            public int Compare(T x, T y)
            {
                var compare = this._comp1.Compare(x, y);
                return compare == 0 ? this._comp2.Compare(x, y) : compare;
            }
        }

        private sealed class FunctionComparer<T> : IComparer<T>
        {
            private readonly Func<T, IComparable> _func;

            internal FunctionComparer(Func<T, IComparable> func)
            {
                this._func = func;
            }
            public int Compare(T x, T y)
            {
                return this._func(x).CompareTo(this._func(y));
            }
        }
    }
}