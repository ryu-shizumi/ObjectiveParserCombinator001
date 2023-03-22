using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    /// <summary>
    /// このクラスのインスタンスが存在する間は + 演算で空白を無視するようにする
    /// </summary>
    public class IgnoreBlank : IDisposable
    {

        internal static Stack<IgnoreBlank> Instances = new Stack<IgnoreBlank>();
        internal static bool IsIgnoreBlank()
        {
            return Instances.Count > 0;
        }
        public IgnoreBlank()
        {
            Instances.Push(this);
        }
        public void Dispose()
        {
            Instances.Pop();
        }
    }
}
