using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    internal abstract class ProcessCount
    {
        public static int Count { get; private set; } = 0;
        public static void Add()
        {
            Count++;
        }
    }
}
