using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    public class BlankMatcher : Matcher
    {
        private Matcher _inner;

        internal BlankMatcher(Matcher inner)
        {
            _inner = inner;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            throw new NotImplementedException();
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            List<Match> matchList = new List<Match>();
            int nextIndex = tokenIndex;
            Match result;

            Match match = _inner.Match(tokenList, nextIndex);
            result = new BlankMatch(this, match.TokenBeginIndex, match.TokenBeginIndex + match.TokenCount);
            _matchList[tokenIndex, this] = result;
            return result;
        }
    }
}
