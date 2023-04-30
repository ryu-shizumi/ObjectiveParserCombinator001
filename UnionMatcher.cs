using Parspell;
using System;
using System.Collections.Generic;
using System.Linq;
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
                if (inner is UnionMatcher unionLeft)
                {
                    list.AddRange(unionLeft.Inners);
                }
                else
                {
                    list.Add(inner);
                }
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
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            List<Match> matchList = new List<Match>();
            int nextIndex = tokenIndex;
            Match result;

            foreach (Matcher matcher in Inners)
            {
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


            result = new WrapMatch(this, matchList.ToArray());
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            foreach (Matcher matcher in Inners)
            {
                matcher.DebugOut(matchers, nest + "  ");
            }
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public UnionMatcher this[string Name]
        {
            get
            {
                return new UnionMatcher(Name, Inners);
            }
        }
    }
}
