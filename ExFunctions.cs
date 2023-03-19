using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    public static  class ExFunctions
    {
        /// <summary>
        /// リストの末尾要素を削除する
        /// </summary>
        /// <typeparam name="T">リストが格納する型</typeparam>
        /// <param name="list">リスト</param>
        /// <exception cref="ArgumentNullException">リストがnullの時に例外が発生する</exception>
        /// <exception cref="IndexOutOfRangeException">リストが空の時に例外が発生する</exception>
        public static void RemoveTail<T>(this List<T> list)
        {
            if (list == null) { throw new ArgumentNullException("list"); }
            if (list.Count == 0) { throw new IndexOutOfRangeException(); }

            list.RemoveAt(list.Count - 1);
        }
    }
}
