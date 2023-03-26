using Parspell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_001
{
    /// <summary>
    /// 最小要素として扱うマッチャー
    /// </summary>
    public class AtomicMatcher : Matcher
    {
        public Matcher Inner { get; private set; }

        public AtomicMatcher(Matcher inner)
        {
            Inner = inner;
        }
        public AtomicMatcher(Matcher inner, string name)
        {
            Inner = inner;
            Name = name;
        }
        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            if (Inner == null) { throw new NullReferenceException(); }

            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            Match result;

            var innerResult = Inner.Match(tokenList, tokenIndex);
            if (innerResult.IsSuccess)
            {
                result = new Match(this, innerResult);
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

            if (Inner != null)
            {
                Inner.DebugOut(matchers, nest + "  ");
            }
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public AtomicMatcher this[string name]
        {
            get { return new AtomicMatcher(Inner, name); }
        }
    }
}
