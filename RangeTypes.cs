using OPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    /// <summary>
    /// 整数範囲型
    /// </summary>
    public class IntRange
    {
        public int Min { get; private set; }
        public int Max { get; private set; }

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// int型の拡張メソッド郡
    /// </summary>
    public static class IntExtensions
    {
        public static IntRange To(this int min, int max)
        {
            return new IntRange(min, max);
        }
    }

    /// <summary>
    /// 文字範囲型
    /// </summary>
    public class CharRange
    {
        public char Min { get; private set; }
        public char Max { get; private set; }

        public CharRange(char min, char max)
        {
            Min = min;
            Max = max;
        }

        public bool IsMatch(char c)
        {
            if (c < Min) { return false; }
            if (Max < c) { return false; }
            return true;
        }

        public static LongMatcher operator *(CharRange a, int count)
        {
            return new LongMatcher(new SimpleCharMatcher(a.Min, a.Max), count, count);
        }
        public static LongMatcher operator *(CharRange a, IntRange range)
        {
            return new LongMatcher(new SimpleCharMatcher(a.Min, a.Max), range.Min, range.Max);
        }
    }
}
