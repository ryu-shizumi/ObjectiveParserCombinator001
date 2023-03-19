using OPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OPC
{
    public interface IAnyMatcher
    {
        Matcher[] Inners { get; }
    }

    /// <summary>
    /// 選択マッチャー
    /// </summary>
    public class AnyMatcher : Matcher, IAnyMatcher
    {
        public Matcher[] Inners { get; private set; }

        internal AnyMatcher(Matcher left, Matcher right)
        {
            List<Matcher> list = new List<Matcher>();

            if (left is IAnyMatcher anyLeft)
            {
                foreach (var item in anyLeft.Inners)
                {
                    list.Add(item);
                }
            }
            else
            {
                list.Add(left);
            }
            if (right is IAnyMatcher anyRight)
            {
                foreach (var item in anyRight.Inners)
                {
                    list.Add(item);
                }
            }
            else
            {
                list.Add(right);
            }


            Inners = list.ToArray();
        }
        internal AnyMatcher(Matcher[] inners)
        {
            Inners = inners;
        }

        internal AnyMatcher(Matcher[] inners, string name)
        {
            Inners = inners;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex, string nest)
        {
            // if (DebugName != "") { Debug.WriteLine(nest + DebugName + "[" + tokenIndex.ToString() + "]"); }

            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            Match result;

            int currentIndex = tokenIndex;
            Match tempResult;

            foreach (var inner in Inners)
            {
                tempResult = inner.Match(tokenList, currentIndex, nest + "  ");
                if (tempResult.IsSuccess)
                {
                    result = tempResult; //new WrapMatch(this, tempResult);
                    if(Name != "")
                    {
                        result = tempResult[Name];
                    }
                    _matchList[tokenIndex, this] = result;
                    return result;
                }
            }

            result = new FailMatch(this, tokenIndex);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            foreach (var inner in Inners)
            {
                inner.DebugOut(matchers, nest + "  ");
            }
        }
        public AnyMatcher this[string name]
        {
            get { return new AnyMatcher(Inners, name); }
        }
    }
}
