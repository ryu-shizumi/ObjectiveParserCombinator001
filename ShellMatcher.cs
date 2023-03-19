using OPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OPC
{
    /// <summary>
    /// 再帰マッチャー
    /// </summary>
    public class RecursionMatcher : Matcher
    {
        public Matcher? Inner { get; set; } = null;

        public RecursionMatcher() { }

        private RecursionMatcher(Matcher inner, string name)
        {
            Inner = inner;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex, string nest)
        {
            if (Inner == null) { throw new NullReferenceException(); }

            // if (DebugName != "") { Debug.WriteLine(nest + DebugName + "[" + tokenIndex.ToString() + "]"); }

            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            //// マッチリストにあるか判定する
            //if (_matchList.ContainsKey(tokenIndex, this))
            //{
            //    // 走査中に再帰してしまった時
            //    if (_matchList[tokenIndex, this] is SearchingMatch)
            //    {
            //        // if (DebugName != "") { Debug.WriteLine(nest + "Fail " + DebugName); }
            //        //// 走査中マッチを再帰失敗マッチで上書きする
            //        //_matchList[tokenIndex, this] = new FailMatch(this, tokenIndex);

            //        // 走査中マッチを削除する
            //        _matchList.RemoveKey(tokenIndex, this);
            //        // 失敗マッチを返す
            //        return new FailMatch(this, tokenIndex);
            //    }

            //    // リストのマッチを返す
            //    return _matchList[tokenIndex, this];
            //}

            //// Innerをマッチングさせる前に走査中マッチを設定しておく
            //_matchList[tokenIndex, this] = new SearchingMatch(this, tokenIndex);

            Match result;

            var innerResult = Inner.Match(tokenList, tokenIndex, nest + "  ");
            if (innerResult.IsSuccess)
            {
                result = new WrapMatch(this, innerResult);
                _matchList[tokenIndex, this] = result;
                return result;
            }

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
        public RecursionMatcher this[string Name]
        {
            get
            {
                return new RecursionMatcher(Inner, Name);
            }
        }


    }
}
