using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerator
{
    ///// <summary>
    ///// 要素を追加できる列挙可能型
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public class Enumerator<T> : IEnumerable<T>
    //{
    //    private readonly List<Func<IEnumerable<T>>> _list 
    //        = new List<Func<IEnumerable<T>>>();

    //    public Enumerator(T item)
    //    {
    //        Add(item);
    //    }

    //    public Enumerator( IEnumerable< T> items)
    //    {
    //        Add(items);
    //    }

    //    public void Add(T item)
    //    {
    //        IEnumerable<T> f()
    //        {
    //            yield return item;
    //        }

    //        _list.Add(f); 
    //    }
    //    public void Add(IEnumerable<T> items)
    //    {
    //        IEnumerable<T> f()
    //        {
    //            foreach (T item in items)
    //            {
    //                yield return item;
    //            }
    //        }

    //        _list.Add(f);
    //    }

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        foreach(var func in _list)
    //        {
    //            foreach(T item in func())
    //            {
    //                yield return item;
    //            }
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        foreach (var func in _list)
    //        {
    //            foreach (T item in func())
    //            {
    //                yield return item;
    //            }
    //        }
    //    }

    //    public static Enumerator<T> operator +(Enumerator<T> enumerator, T item)
    //    {
    //        var result = new Enumerator<T>(enumerator);
    //        result.Add(item);
    //        return result;
            
    //    }

    //    public List<T> ToList()
    //    {
    //        return new List<T>(this);
    //    }
    //}

    //public static class IEnumerableEx
    //{
    //    /// <summary>
    //    /// 要素を追加できる列挙可能型に変換する
    //    /// </summary>
    //    /// <typeparam name="T">列挙対象の型</typeparam>
    //    /// <param name="items">元の列挙可能型</param>
    //    /// <returns>要素を追加できる列挙可能型</returns>
    //    public static Enumerator<T> _<T>(this IEnumerable<T> items)
    //    {
    //        return new Enumerator<T>(items);
    //    }

    //    /// <summary>
    //    /// 要素を追加できる列挙可能型に変換する
    //    /// </summary>
    //    /// <typeparam name="T">列挙対象の型</typeparam>
    //    /// <param name="item">元の型</param>
    //    /// <returns>要素を追加できる列挙可能型</returns>
    //    public static Enumerator<T> Enum<T>(this T item)
    //    {
    //        return new Enumerator<T>(item);
    //    }
    //}
}
