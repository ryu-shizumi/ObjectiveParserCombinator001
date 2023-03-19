using OPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OPC
{
    /// <summary>
    /// 連結マッチャー
    /// </summary>
    public class UnionMatcher : Matcher
    {
        public Matcher[] Inner { get; private set; }
        public UnionMatcher(Matcher left, Matcher right)
        {
            List<Matcher> list = new List<Matcher>();
            if (left is UnionMatcher unionLeft)
            {
                list.AddRange(unionLeft.Inner);
            }
            else
            {
                list.Add(left);
            }
            if (right is UnionMatcher unionRight)
            {
                list.AddRange(unionRight.Inner);
            }
            else
            {
                list.Add(right);
            }
            Inner = list.ToArray();
        }
        private UnionMatcher(Matcher left, Matcher right, string name)
            : this(left, right)
        {
            Name = name;
        }
        private UnionMatcher(Matcher[] inner, string name)
        {
            Inner = inner;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex, string nest)
        {
            // if (DebugName != "") { Debug.WriteLine(nest + DebugName + "[" + tokenIndex.ToString() + "]"); }

            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            List<Match> matchList = new List<Match>();
            int nextIndex = tokenIndex;
            Match result;

            foreach (Matcher matcher in Inner)
            {
                Match match = matcher.Match(tokenList, nextIndex, nest + "  ");
                if (match.IsSuccess == false)
                {
                    result = new FailMatch(this, tokenIndex);
                    _matchList[tokenIndex, this] = result;
                    return result;
                }
                matchList.Add(match);
                nextIndex += match.TokenCount;
            }

            result = new WrapMatch(this, matchList.ToArray());
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            foreach (Matcher matcher in Inner)
            {
                matcher.DebugOut(matchers, nest + "  ");
            }
        }

        public UnionMatcher this[string Name]
        {
            get
            {
                return new UnionMatcher(Inner, Name);
            }
        }
    }
}
