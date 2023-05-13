using OPC_001;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Matcher _right;

        /// <summary>
        /// 二項演算式にマッチするマッチャーを生成する
        /// </summary>
        /// <param name="operand">オペランド</param>
        /// <param name="operators">演算子</param>
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
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;
            var leftResult = _left.Match(tokenList, tokenIndex);

            if (leftResult.IsSuccess == false)
            {
                // マッチ失敗なのでインデントをロールバックする
                lastNest.Rollback();

                result = new FailMatch(this, tokenIndex);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            var rightResult = _right.Match(tokenList, tokenIndex + leftResult.TokenCount);

            if (rightResult.IsSuccess == false)
            {
                // マッチ失敗なのでインデントをロールバックする
                lastNest.Rollback();

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
            Debug.WriteLine($"{nest}{_left} {_right}");
            _left.DebugOut(matchers, nest + "  ");
            _right.DebugOut(matchers, nest + "  ");
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public new OperationMatcher this[string name]
        {
            get { return new OperationMatcher(this, name); }
        }

        private Matcher GetNamedOperator(Matcher matcher, string name)
        {
            if (matcher is AnyMatcher _any) { return _any[name]; }
            if (matcher is AtomicMatcher _atomic) { return _atomic[name]; }
            if (matcher is SimpleCharMatcher _simplechar) { return _simplechar[name]; }
            if (matcher is AnyCharMatcher _anychar) { return _anychar[name]; }
            if (matcher is ErrorMatcher _error) { return _error[name]; }
            if (matcher is LookaheadMatcher _lookahead) { return _lookahead[name]; }
            if (matcher is ShortMatcher _short) { return _short[name]; }
            if (matcher is LongMatcher _long) { return _long[name]; }
            if (matcher is ZeroLengthMatcher _zerolength) { return _zerolength[name]; }
            if (matcher is WordMatcher _word) { return _word[name]; }
            if (matcher is NotMatcher _not) { return _not[name]; }
            if (matcher is OperationMatcher _operation) { return _operation[name]; }
            if (matcher is RecursionMatcher _recursion) { return _recursion[name]; }
            if (matcher is UnionMatcher _union) { return _union[name]; }


            throw new TypeAccessException();
        }

    }
}
