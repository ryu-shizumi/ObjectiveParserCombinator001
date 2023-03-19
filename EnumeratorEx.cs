using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    //public class ItemPicker<T>
    //{
    //    public IEnumerator<T> Sorce { get; private set; }
    //    public ItemPicker(IEnumerable<T> sorce)
    //    {
    //        Sorce = sorce.GetEnumerator();
    //    }
    //    public ItemPicker(IEnumerator<T> sorce)
    //    {
    //        Sorce = sorce;
    //    }

    //    public T Current
    //    {
    //        get { return Sorce.Current; }
    //    }

    //    public bool IsEmpty { get; private set; }
    //    public bool IsLast { get; private set; }

    //    public bool MoveNext()
    //    {
    //        if(IsLast) { return false; }
    //        var result = Sorce.MoveNext();
    //        IsLast = !result;
    //        IsEmpty = !result;
    //        return result;
    //    }
    //}
}
