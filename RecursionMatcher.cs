using Parspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Parspell
{
    /// <summary>
    /// 再帰マッチャー
    /// </summary>
    public class RecursionMatcher : Matcher
    {
        public Matcher? Inner
        {
            get
            {
                return _inneWraper.Inner;
            }
            set
            {
                if(value.UniqID == 110)
                {
                    var temp = "";
                }

                _inneWraper.Inner = value;
            }
        }

        private InnerWraper _inneWraper = new InnerWraper();

        public class InnerWraper
        {
            public Matcher? Inner { get; set; } = null;
        }

        public RecursionMatcher() { }

        private RecursionMatcher(InnerWraper innerWraper, string name)
        {
            _inneWraper = innerWraper;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            if (UniqID == 66)
            {
                var temp = "";
            }



            // 中身が無い時は例外を吐く
            if (Inner == null) { throw new NullReferenceException(); }

            //// マッチリストにある時はそれを返す
            //if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            // マッチリストにあるか判定する
            if (_matchList.ContainsKey(tokenIndex, this))
            {
                // 走査中に再帰してしまった時
                if (_matchList[tokenIndex, this] is SearchingMatch)
                {
                    var recursionMatch = new InfiniteRecursionMatch(this, tokenIndex);
                    // 走査中マッチを無限再帰マッチで上書きする
                    _matchList[tokenIndex, this] = recursionMatch;

                    // 無限再帰マッチを返す
                    return recursionMatch;
                }

                // リストのマッチを返す
                return _matchList[tokenIndex, this];
            }

            // Innerをマッチングさせる前に走査中マッチを設定しておく
            _matchList[tokenIndex, this] = new SearchingMatch(this, tokenIndex);

            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;

            var innerResult = Inner.Match(tokenList, tokenIndex);
            if (innerResult.IsSuccess)
            {
                result = new WrapMatch(this, innerResult);
                _matchList[tokenIndex, this] = result;
                return result;
            }

            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            result = new FailMatch(this, tokenIndex);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine(nest + Name + " (" + ClassName + ")");
            if (matchers.Contains(this)) { return; }
            matchers.Add(this);

            if (Inner != null)
            {
                Inner.DebugOut(matchers, nest + "  ");
            }
        }
        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public new RecursionMatcher this[string name]
        {
            get
            {
                return new RecursionMatcher(_inneWraper, name);
            }
        }


    }
}
