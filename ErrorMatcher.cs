using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    /// 構文エラーを補足するマッチャー
    /// </summary>
    internal class ErrorMatcher : Matcher
    {
        public Matcher Inner { get; private set; }

        private ErrorMatcher(Matcher inner, string name = "")
        {
            Inner = inner;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;

            var innerResult = Inner.Match(tokenList, tokenIndex);

            if (innerResult.IsSuccess)
            {

                // 構文エラーマッチを作成する
                result = new ErrorMatch(this, innerResult);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            result = innerResult;
            _matchList[tokenIndex, this] = result;
            return result;
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
        public ErrorMatcher this[string Name]
        {
            get
            {
                return new ErrorMatcher(Inner, Name);
            }
        }
    }
}
