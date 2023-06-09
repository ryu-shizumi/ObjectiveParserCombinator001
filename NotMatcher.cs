using Parspell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parspell
{
    /// <summary>
    /// 否定可能なマッチャー（の基底クラス）
    /// </summary>
    /// <remarks>
    /// 否定可能なマッチャーとは、マッチする長さが確定しているマッチャー。
    /// 「１文字」
    /// 「区切り文字（長さゼロ）」
    /// 「否定先読み（長さゼロ）」
    /// 「否定後読み（長さゼロ）」
    /// が該当する。
    /// </remarks>
    public abstract class NotableMatcher : Matcher
    {
        public NotableMatcher() { }

        public NotMatcher Not
        {
            get
            {
                return new NotMatcher(this);
            }
        }

    }

    /// <summary>
    /// 否定マッチャー
    /// </summary>
    /// <remarks>
    /// 内包する否定可能なマッチャーを否定するマッチャー
    /// </remarks>
    public class NotMatcher : Matcher
    {
        public NotableMatcher Inner { get; private set; }

        public NotMatcher(NotableMatcher inner, string name = "")
        {
            Inner = inner;
            Name = name;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}{this}");
            Inner.DebugOut(matchers, nest + "  ");
        }

        public override string ToString()
        {
            return $"({Inner}).Not";
        }

        /// <summary>
        /// 内包要素が非マッチを返した時に同じ長さのマッチを返す
        /// </summary>
        /// <param name="tokenList"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            ProcessCount.Add();

            if (ProcessCount.Count == 2)
            {
                var temp = "";
            }

            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;

            int currentIndex = tokenIndex;
            Match innerResult = Inner.Match(tokenList, currentIndex);

            // Notでひっくり返せる時はひっくり返す
            if(innerResult is NotableMatch notable)
            {
                result = notable.GetNot(this);
                _matchList[tokenIndex, this] = result;
            }
            // ひっくり返せないものはそのまま返す。
            else
            {
                result = innerResult;
            }

            // ひっくり返せるか否かに関わらず不成功の時
            if (innerResult.IsSuccess == false)
            {
                // マッチ失敗なのでインデントをロールバックする
                lastNest.Rollback();
            }


            return result;
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public new NotMatcher this[string name]
        {
            get { return new NotMatcher(Inner, name); }
        }
    }
}
