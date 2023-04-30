using Parspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parspell
{
    /// <summary>
    /// 否定マッチャー
    /// </summary>
    public class NotMatcher : Matcher
    {
        public Matcher Inner { get; private set; }

        public NotMatcher(Matcher inner, string name = "")
        {
            Inner = inner;
            Name = name;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 内包要素が非マッチを返した時に同じ長さのマッチを返す
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

            if (innerResult.IsSuccess == false)
            {
                result = new Match(this, innerResult, Name);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            result = new FailMatch(this, innerResult);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public NotMatcher this[string name]
        {
            get { return new NotMatcher(Inner, name); }
        }
    }
}
