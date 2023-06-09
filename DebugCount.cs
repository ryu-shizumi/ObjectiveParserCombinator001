using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Parspell
{
    internal class DebugCount
    {
        public static int Count { get; private set; } = 0;

        public static void Add() { Count++; }

        public static void WriteLine() { Debug.WriteLine(Count); }
    }
}
