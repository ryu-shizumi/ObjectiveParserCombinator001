using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    public class BlankMatcher : Matcher
    {
        public static BlankMatcher Instance { get; private set; } = new BlankMatcher();
        private Matcher _inner;

        private BlankMatcher()
        {
            _inner = ((' '._() | '\t') * 0.To(int.MaxValue))["Blank"];
        }
        public static Matcher GetInstance()
        {
            return Instance;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            throw new NotImplementedException();
        }

        public override Match Match(TokenList tokenList, int tokenIndex, string nest)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            List<Match> matchList = new List<Match>();
            int nextIndex = tokenIndex;
            Match result;

            Match match = _inner.Match(tokenList, nextIndex, nest + "  ");
            result = new BlankMatch(this, match.TokenBeginIndex, match.TokenBeginIndex + match.TokenCount);
            _matchList[tokenIndex, this] = result;
            return result;
        }
    }
}
