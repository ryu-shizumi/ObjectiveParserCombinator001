using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    /// <summary>
    /// コンストラクタに比較関数を指定できる SortedDictionary 派生クラス
    /// </summary>
    /// <typeparam name="TKey">ソートのキーとなる型</typeparam>
    /// <typeparam name="TValue">格納する値の型</typeparam>
    public class ModernSortedDictionary<TKey, TValue> : SortedDictionary<TKey, TValue> where TKey : notnull
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="func">ソートの為の比較関数</param>
        public ModernSortedDictionary(Func<TKey, TKey, int> func)
            : base(func.ToComparer()) { }
    }

    public static class ComparerEx
    {
        public static Comparer<T> ToComparer<T>(this Func<T, T, int> comparer) where T : notnull
        {
            return new TComp<T>(comparer);
        }

        /// <summary>
        /// SortedDictionary に与える、比較用クラス
        /// </summary>
        private class TComp<T> : Comparer<T>
        {
            public TComp(Func<T, T, int> comparer)
            {
                Comparer = comparer;
            }

            public Func<T, T, int> Comparer { get; private set; }

            public override int Compare(T? x, T? y)
            {
                if(x == null && y == null) return 0;
                return Comparer(x, y);
            }
        }
    }
}
