using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parspell
{
    public abstract class CharMatcher : Matcher
    {

        public static AnyCharMatcher operator |(CharMatcher a, CharMatcher b)
        {
            return new AnyCharMatcher(a, b);
        }
        public static AnyCharMatcher operator |(CharMatcher a, char b)
        {
            return a | b._();
        }
        public static AnyCharMatcher operator |(CharMatcher a, CharRange b)
        {
            return a | b._();
        }
        public static AnyCharMatcher operator |(char a, CharMatcher b)
        {
            return a._() | b;
        }
        public static AnyCharMatcher operator |(CharRange a, CharMatcher b)
        {
            return a._() | b;
        }
        public abstract string CharRangeString { get; }
    }

    /// <summary>
    /// 文字１個と照合するマッチャー
    /// </summary>
    public class SimpleCharMatcher : CharMatcher
    {
        public CharRange CharRange { get; private set; }


        public SimpleCharMatcher(char c)
        {
            CharRange = new CharRange(c, c);
        }
        public SimpleCharMatcher(char min, char max)
        {
            CharRange = new CharRange(min, max);
        }
        public SimpleCharMatcher(CharRange charRange)
        {
            CharRange = charRange;
        }

        private SimpleCharMatcher(CharRange charRange, string name)
        {
            CharRange = charRange;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;
            var token = tokenList[tokenIndex];

            int tokenCount = 0;

            // 現在位置が文字トークンの時
            if (token is TokenChar cToken)
            {
                tokenCount = 1;
                // 範囲が合致した時
                if (CharRange.IsMatch(cToken.Char))
                {
                    // 正解マッチを作成する
                    result = new Match(this, tokenIndex, tokenIndex + tokenCount);
                    _matchList[tokenIndex, this] = result;
                    return result;
                }
            }

            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            // 失敗マッチを作成する
            result = new FailMatch(this, tokenIndex, tokenIndex + tokenCount);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override string CharRangeString
        {
            get
            {
                if (CharRange.Min == CharRange.Max)
                {
                    return CharRange.Min.Escape();
                }
                else
                {
                    return $"{CharRange.Min.Escape()}-{CharRange.Max.Escape()}";
                }
            }
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}{this}");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (CharRange.Min == CharRange.Max)
            {
                sb.Append('"');
                sb.Append(CharRange.ToString());
                sb.Append('"');
            }
            else
            {
                sb.Append("[");
                sb.Append(CharRange.ToString());
                sb.Append("]");
            }
            return sb.ToString();
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public new SimpleCharMatcher this[string name]
        {
            get
            {
                return new SimpleCharMatcher(CharRange, name);
            }
        }
    }

    /// <summary>
    /// 選択文字マッチャー
    /// </summary>
    public class AnyCharMatcher : CharMatcher
    {
        private CharMatcher[] _inners;

        public AnyCharMatcher(CharMatcher left, CharMatcher right)
        {
            List<CharMatcher> list = new List<CharMatcher>();

            if(left is AnyCharMatcher anyLeft)
            {
                list.AddRange(anyLeft._inners);
            }
            else
            {
                list.Add(left);
            }

            if (right is AnyCharMatcher anyRight)
            {
                list.AddRange(anyRight._inners);
            }
            else
            {
                list.Add(right);
            }

            _inners = list.ToArray();
        }
        private AnyCharMatcher(CharMatcher[] inners, string name)
        {
            _inners = inners;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;

            int currentIndex = tokenIndex;
            Match tempResult;

            foreach (var inner in _inners)
            {
                tempResult = inner.Match(tokenList, currentIndex);
                if (tempResult.IsSuccess)
                {
                    result = tempResult;// new WrapMatch(this, tempResult);
                    if(Name != "")
                    {
                        result = tempResult[Name];
                    }
                    _matchList[tokenIndex, this] = result;
                    return result;
                }
            }

            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            result = new FailMatch(this, tokenIndex, tokenIndex + 1);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override string CharRangeString
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var inner in _inners)
                {
                    sb.Append(inner.CharRangeString);
                }
                return sb.ToString();
            }
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}{this}");

            foreach (var inner in _inners)
            {
                inner.DebugOut(matchers, nest + "  ");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var inner in _inners)
            {
                sb.Append(inner.CharRangeString);
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public new AnyCharMatcher this[string name]
        {
            get
            {
                return new AnyCharMatcher(_inners, name);
            }
        }
    }

    ///// <summary>
    ///// 文字１個の否定と照合するマッチャー
    ///// </summary>
    //public class NotCharMatcher : Matcher
    //{
    //    public CharMatcher Inner { get; private set; }

    //    public NotCharMatcher(CharMatcher inner)
    //    {
    //        Inner = inner;
    //    }
    //    private NotCharMatcher(CharMatcher inner, string name)
    //    {
    //        Inner = inner;
    //        name = name;
    //    }

    //    public override Match Match(TokenList tokenList, int tokenIndex)
    //    {
    //        // マッチリストにある時はそれを返す
    //        if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
    //        // インデントのロールバックに備えて現在値を取得しておく
    //        var lastNest = Nest.Root.LastItem;

    //        Match result;

    //        var innerResult = Inner.Match(tokenList, tokenIndex);

    //        if(innerResult.IsSuccess)
    //        {
    //            // マッチ失敗なのでインデントをロールバックする
    //            lastNest.Rollback();

    //            // 失敗マッチを作成する
    //            result = new FailMatch(this, innerResult.TokenBeginIndex, innerResult.TokenEndIndex);
    //            _matchList[tokenIndex, this] = result;
    //            return result;

    //        }
    //        result = new Match(this, innerResult);
    //        _matchList[tokenIndex, this] = result;
    //        return result;
    //    }

    //    public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    /// <summary>
    //    /// このマッチャーに名前を設定したインスタンスを取得する
    //    /// </summary>
    //    /// <param name="name">名前</param>
    //    /// <returns>このマッチャーに名前を設定したインスタンス</returns>
    //    public NotCharMatcher this[string name]
    //    {
    //        get
    //        {
    //            return new NotCharMatcher(Inner, name);
    //        }
    //    }
    //}
}
