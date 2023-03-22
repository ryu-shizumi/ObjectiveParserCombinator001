using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    /// <summary>
    /// 二項演算に最適化されたマッチャー
    /// </summary>
    /// <remarks>
    /// 
    /// オペランド (演算子 オペランド)* というルールでマッチングさせる。
    /// 
    /// 
    /// </remarks>
    public class OperationMatcher : Matcher
    {
        private Matcher _left;
        private LongMatcher _right;

        // 二項演算式にマッチするマッチャーを生成する
        public OperationMatcher(Matcher operand, Matcher operators)
        {
            _left = operand;


            var namedOperators = GetObjectAtIndex(operators,"Operator");

            if(IgnoreBlank.IsIgnoreBlank())
            {
                _right = new UnionMatcher
                    (BlankMatcher.Instance , namedOperators , 
                    BlankMatcher.Instance , operand) * 0.To(int.MaxValue);
            }
            else
            {
                _right = (namedOperators + operand) * 0.To(int.MaxValue);
            }
            
        }

        private OperationMatcher(OperationMatcher org, string name)
        {
            _left = org._left;
            _right = org._right;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex, string nest)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            Match result;
            var leftResult = _left.Match(tokenList, tokenIndex, nest + "  ");

            if (leftResult.IsSuccess == false)
            {
                result = new FailMatch(this, tokenIndex);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            var rightResult = _right.Match(tokenList, tokenIndex + leftResult.TokenCount, nest + "  ");

            if (rightResult.IsSuccess == false)
            {
                result = new FailMatch(this, tokenIndex);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            // 右側が０回の時（左辺のみが合致した時）
            if (rightResult.TokenCount == 0)
            {
                result = leftResult;
                _matchList[tokenIndex, this] = result;
                return result;
            }

            result = Wrap(leftResult, rightResult);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        private Match Wrap(Match leftResult , Match RightResult)
        {
            Match left = leftResult;
            var inners = ((WrapMatch)RightResult).Inners;

            for (int i = 0; i < inners.Count; i++)
            {
                WrapMatch wrap = (WrapMatch)inners[i];
                Match op = wrap.Inners[0];
                Match right = wrap.Inners[1];

                if(right is BlankMatch)
                {
                    right = wrap.Inners[2];
                }

                left = new OperationMatch(this, left, op, right);
            }

            return left;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            throw new NotImplementedException();
        }

        public OperationMatcher this[string name]
        {
            get { return new OperationMatcher(this, name); }
        }

        public static Matcher GetObjectAtIndex(object obj, string name)
        {
            Type type = obj.GetType();
            PropertyInfo indexer = type.GetProperty("Item", new[] { typeof(string) });
            if (indexer != null)
            {
                return (Matcher)(indexer.GetValue(obj, new object[] { name }));
            }
            else
            {
                throw new ArgumentException("指定されたオブジェクトにインデクサが見つかりませんでした。");
            }
        }
    }
}
