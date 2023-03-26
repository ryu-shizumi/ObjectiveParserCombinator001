using OPC_001;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Parspell.IgnoreBlank;

namespace Parspell
{
    /// <summary>
    /// 二項演算に最適化されたマッチャー
    /// </summary>
    /// <remarks>
    /// 
    /// オペランド (演算子 オペランド){0,int.MaxValue} というルールでマッチングさせる。
    /// 生成されるマッチが無駄にネストが深くならないように簡素化する。
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


            var namedOperators = GetNamedOperator(operators,"Operator");

            switch (IgnoreState)
            {
            case IgnoreStateFlag.IgnoreSpace:
                _right = new UnionMatcher
                     (BlankSpace, namedOperators,
                     BlankSpace, operand) * 0.To(int.MaxValue);
                break;
            case IgnoreStateFlag.IgnoreNewline:
                _right = new UnionMatcher
                     (BlankNewLine, namedOperators,
                     BlankNewLine, operand) * 0.To(int.MaxValue);
                break;
            case IgnoreStateFlag.IgnoreSpaceNewLine:
                _right = new UnionMatcher
                     (BlankSpaceNewline, namedOperators,
                     BlankSpaceNewline, operand) * 0.To(int.MaxValue);
                break;
            default:
                _right = (namedOperators + operand) * 0.To(int.MaxValue);
                break;
            }
        }

        private OperationMatcher(OperationMatcher org, string name)
        {
            _left = org._left;
            _right = org._right;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            Match result;
            var leftResult = _left.Match(tokenList, tokenIndex);

            if (leftResult.IsSuccess == false)
            {
                result = new FailMatch(this, tokenIndex);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            var rightResult = _right.Match(tokenList, tokenIndex + leftResult.TokenCount);

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

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public OperationMatcher this[string name]
        {
            get { return new OperationMatcher(this, name); }
        }

        private Matcher GetNamedOperator(Matcher matcher, string name)
        {
            if (matcher is AnyMatcher any) { return any[name]; }
            if (matcher is AnyCharMatcher ac) { return ac[name]; }
            if (matcher is SimpleCharMatcher sc) { return sc[name]; }
            if (matcher is NotCharMatcher nc) { return nc[name]; }
            if (matcher is ShortMatcher sht) { return sht[name]; }
            if (matcher is LongMatcher l) { return l[name]; }
            if (matcher is AtomicMatcher atom) { return atom[name]; }
            if (matcher is UnionMatcher uni) { return uni[name]; }
            if (matcher is WordMatcher word) { return word[name]; }

            throw new TypeAccessException();
        }

    }
}
