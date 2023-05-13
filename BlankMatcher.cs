using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace Parspell
{
    /// <summary>
    /// 空白などを無視する時の専用マッチャー
    /// </summary>
    public class BlankMatcher : Matcher
    {
        private Matcher _inner;

        internal BlankMatcher(Matcher inner)
        {
            _inner = inner;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}Blank");
            _inner.DebugOut(matchers, nest + "  ");
        }

        public override string ToString()
        {
            return _inner.ToString();
        }
        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            List<Match> matchList = new List<Match>();
            int nextIndex = tokenIndex;
            Match result;

            Match match = _inner.Match(tokenList, nextIndex);

            if(match.IsSuccess == false)
            {
                // マッチ失敗なのでインデントをロールバックする
                lastNest.Rollback();
            }

            // 空白マッチを作る
            result = new BlankMatch(this, match.TokenBeginIndex, match.TokenBeginIndex + match.TokenCount);
            _matchList[tokenIndex, this] = result;
            return result;
        }
    }
}
