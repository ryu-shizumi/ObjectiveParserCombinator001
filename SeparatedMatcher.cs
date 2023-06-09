using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    /// セパレーターを挟んだループの中身。特殊な列挙処理が可能。
    /// </summary>
    internal class SeparatedMatcher : Matcher
    {
        public Matcher Inner { get; private set; }
        public Matcher? Separater { get; private set; }

        public SeparatedMatcher(Matcher inner, Matcher separater, string name = "")
        {
            Inner = inner;
            Separater = separater;
            Name = name;
        }

        /// <summary>
        /// 最善のマッチを返す
        /// </summary>
        /// <param name="tokenList"></param>
        /// <param name="tokenIndex"></param>
        /// <returns>
        /// 
        /// このマッチャーでは他のマッチャーと違い、EnumMatch から最善の１個を返す
        /// 
        /// </returns>
        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            foreach (var match in EnumMatch(tokenList, tokenIndex))
            {
                return match;
            }

            // ホントはこの一文は実行されないが、
            // 無いとコンパイラに怒られるので書いておく
            return new FailMatch(this, tokenIndex);
        }



        /// <summary>
        /// マッチを列挙する
        /// </summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="tokenIndex">マッチング開始インデックス</param>
        /// <returns>１個以上の成功マッチ。または１個の失敗マッチ</returns>
        /// <remarks>
        /// 
        /// マッチ結果は一旦 LoopMatchResults に貯める。
        /// LoopMatchResults は 「tokenIndex と Matcher の組」毎に用意される。
        /// LoopMatchResults に列挙させると、長さ、子要素の返信順、の優先順位で返信する。
        /// 長さは最長優先か最短優先を関数型の SortOrderFunc メンバで指定し、
        /// これが IComparable.CompareTo() として使用される。
        /// 
        /// </remarks>
        public override IEnumerable<Match> EnumMatch(TokenList tokenList, int tokenIndex)
        {
            Match result;
            Match innerResult;
            Match speResult;
            int nextIndex = tokenIndex;

            innerResult = Inner.Match(tokenList, nextIndex);

            yield return innerResult;

            if(Separater == null)
            {
                while (innerResult.IsSuccess)
                {
                    nextIndex += innerResult.TokenCount;
                    innerResult = Inner.Match(tokenList, nextIndex);
                    yield return innerResult;
                }
                yield break;
            }

            while (true)
            {
                nextIndex += innerResult.TokenCount;
                speResult = Separater.Match(tokenList, nextIndex);

                if (speResult.IsSuccess == false)
                {
                    yield return speResult;
                    yield break;
                }

                nextIndex += speResult.TokenCount;
                innerResult = Inner.Match(tokenList, nextIndex);
                if (innerResult.IsSuccess == false)
                {
                    yield return innerResult;
                    yield break;
                }
                Match[] wrapInner = { speResult, innerResult };
                result = new WrapMatch(this, wrapInner);
                yield return result;
            }
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}({Inner})");
        }

        public override string ToString()
        {
            if (Name != "") { return Name; }
            return ($"({Inner},{Separater})");
        }

    }
}