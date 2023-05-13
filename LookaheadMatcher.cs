using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    /// 先読みマッチャー
    /// </summary>
    public class LookaheadMatcher : Matcher
    {
        public Matcher Inner { get; private set; }

        public LookaheadMatcher(Matcher inner, string name = "")
        {
            Inner = inner;
            Name = name;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine($"{nest}{Inner}.LookAhead");
            Inner.DebugOut(matchers, nest + "  ");

        }

        public override string ToString()
        {
            return $"({Inner}).LookAhead";
        }

        /// <summary>
        /// マッチしていれば長さゼロのマッチオブジェクトを返す
        /// </summary>
        /// <param name="tokenList"></param>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;

            int currentIndex = tokenIndex;
            Match innerResult = Inner.Match(tokenList, currentIndex);

            if (innerResult.IsSuccess)
            {
                result = new LookaheadMatch(this, innerResult.TokenBeginIndex, Name);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            result = new FailMatch(this, tokenIndex);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public new LookaheadMatcher this[string name]
        {
            get { return new LookaheadMatcher(Inner, name); }
        }
    }
}
