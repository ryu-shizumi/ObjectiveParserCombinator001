using Parspell;
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
    /// <summary>
    /// 連結マッチャー
    /// </summary>
    public class UnionMatcher : Matcher
    {
        public Matcher[] Inners { get; private set; }

        /// <summary>
        /// ブランクマッチャーの連続が存在するかを判定するデバッグ用メソッド
        /// </summary>
        /// <returns></returns>
        public bool CheckContinuousBlank()
        {
            for (int i = 0; i < Inners.Length - 1; i++)
            {
                if ((Inners[i] is BlankMatcher) && (Inners[i + 1] is BlankMatcher))
                { return true; }
            }
            return false;
        }

        private UnionMatcher(string name, params Matcher[] inners)
        {
            List<Matcher> list = new List<Matcher>();

            foreach (var inner in inners)
            {
                if(inner.Name != "")
                {
                    list.Add(inner);
                    continue;
                }

                if (inner is UnionMatcher unionLeft)
                {
                    list.AddRange(unionLeft.Inners);
                    continue;
                }
                list.Add(inner);
            }

            Inners = list.ToArray();
            Name = name;
        }

        public UnionMatcher(Matcher left, Matcher right, string name = "")
            : this(name, left, right) { }

        public UnionMatcher(Matcher a, Matcher b,Matcher c, string name = "")
            : this(name, a, b, c) { }

        public UnionMatcher(params Matcher[] inners)
            : this("", inners) { }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            if(Name == "OrExp")
            {
                var temp = "";
            }
            if(Name == "JoinExp")
            {
                var temp = "";
            }

            if (UniqID == "G125")
            {
                var temp = "";
            }
            if ((UniqID == "G123") && (tokenIndex == 3))
            {
                var temp = "";
            }
            if ((UniqID == "G114") && (tokenIndex == 3))
            {
                var temp = "";
            }

            // 範囲外の時は範囲外マッチを返す
            if (tokenList.IsRangeOut(tokenIndex)) { return new RangeOutMatch(this, tokenIndex); }

            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            List<Match> matchList = new List<Match>();
            int nextIndex = tokenIndex;
            Match result;

            foreach (Matcher matcher in Inners)
            {
                if ((matcher.UniqID == "G58") && (nextIndex == 3))
                {
                    var temp = "";
                }
                if ((matcher.UniqID == "G123") && (nextIndex == 3))
                {
                    var temp = "";
                }

                Match match = matcher.Match(tokenList, nextIndex);
                if (match.IsSuccess == false)
                {
                    // マッチ失敗なのでインデントをロールバックする
                    lastNest.Rollback();

                    result = new FailMatch(this, tokenIndex);
                    _matchList[tokenIndex, this] = result;
                    return result;
                }
                nextIndex += match.TokenCount;
                if (match is BlankMatch)
                { continue; }
                matchList.Add(match);
            }


            result = new WrapMatch(this, matchList);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}{this}{Name}");
            foreach (Matcher matcher in Inners)
            {
                matcher.DebugOut(matchers, nest + "  ");
            }
        }

        public override string ToString()
        {
            if (Name != "") { return Name; }

            var sb = new StringBuilder();
            foreach (Matcher matcher in Inners)
            {
                if(sb.Length > 0)
                {
                    sb.Append('+');
                }
                sb.Append(matcher.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public new UnionMatcher this[string name]
        {
            get
            {
                return new UnionMatcher(name, Inners);
            }
        }
    }
}
