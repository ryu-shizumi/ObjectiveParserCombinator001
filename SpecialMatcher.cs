using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    public abstract class SpecialMatcher : NotableMatcher
    {
        internal static bool IsTextBegin(TokenList tokenList, int tokenIndex)
        {
            if (tokenIndex <= 0)
            {
                return true;
            }
            return false;
        }
        internal static bool IsTextEnd(TokenList tokenList, int tokenIndex)
        {
            if (tokenList.Count <= tokenIndex)
            {
                return true;
            }
            if (tokenList[tokenIndex] is TokenChar c)
            {
                return false;
            }

            return true;
        }

        //internal static string BLANKS = "\t \r\n";
        internal static string Spaces = " \t";
    }

    /// <summary>
    /// 文字列開始マッチャー
    /// </summary>
    public class TextBeginMatcher : SpecialMatcher
    {
        public TextBeginMatcher() { }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}{this}");
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            if(IsTextBegin(tokenList, tokenIndex))
            {
                return new SuccessMatch(this, 0);
            }
            return new FailMatch(this, 0);
        }

        public override string ToString()
        {
            return "(TEXTBEGIN)";
        }
    }

    /// <summary>
    /// 文字列終端マッチャー
    /// </summary>
    public class TextEndMatcher : SpecialMatcher
    {
        public TextEndMatcher() { }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}{this}");
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            if (IsTextEnd(tokenList, tokenIndex))
            {
                return new SuccessMatch(this, tokenIndex);
            }
            return new FailMatch(this, tokenIndex);
        }

        public override string ToString()
        {
            return "(TEXTEND)";
        }
    }

    /// <summary>
    /// インデントマッチャー
    /// </summary>
    public class IndentMatcher : SpecialMatcher
    {
        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
           Debug.WriteLine($"{nest}{this}");
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;
            Match result;


            // トークナイズの段階で行頭空白の後に（インデントになる保証は無いが）
            // 「ネストトークン」を差し込んでおく？

            if (tokenList[tokenIndex] is TokenSpecialNest nest)
            {
                // インデントスタックにネストを報告する
                // (インデントになるかは自動で判定される)
                Nest.Root.FindNest(nest.NestCount);

            }


            //do
            //{
            //    if (IsTextBegin(tokenList, tokenIndex)) { break; }
            //    if (IsTextEnd(tokenList, tokenIndex)) { break; }

            //    // 次の文字が空白以外である事を確認する
            //    if (tokenList[tokenIndex] is TokenChar cNext)
            //    {
            //        if(Spaces.Contains(cNext.Char) == true) { break; }
            //    }
            //    else { break; }

            //    // 前の文字が空白である事を確認する
            //    if (tokenList[tokenIndex-1] is TokenChar cPrev)
            //    {
            //        if (Spaces.Contains(cPrev.Char) == false) { break; }
            //    }
            //    else { break; }

            //    int nestCount = 1;

            //    // 前に向かって空白の数を数える
            //    for(int i = tokenIndex -2; i <= 0; i--)
            //    {
            //        if ((tokenList[i] is TokenChar cCurrent)&&
            //            (Spaces.Contains(cCurrent.Char)))
            //        { nestCount++; }
            //        else
            //        { break; }
            //    }

            //    // インデントスタックにネストを報告する
            //    // (インデントになるかは自動で判定される)
            //    Nest.Root.FindNest(nestCount);

            //} while (false);

            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            // 失敗マッチを作成する
            result = new FailMatch(this, tokenIndex);
            _matchList[tokenIndex, this] = result;
            return result;
        }
        public override string ToString()
        {
            return "(INDENT)";
        }
    }
}
