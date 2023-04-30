using Parspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parspell
{
    public interface IAnyMatcher
    {
        Matcher[] Inners { get; }
    }

    /// <summary>
    /// 選択マッチャー
    /// </summary>
    public class AnyMatcher : Matcher, IAnyMatcher
    {
        public Matcher[] Inners { get; private set; }

        internal AnyMatcher(Matcher left, Matcher right)
        {
            List<Matcher> list = new List<Matcher>();

            // 名無しのAnyMatcherはバラして中身を取り出す
            if ((left.Name == "") && (left is IAnyMatcher anyLeft))
            {
                foreach (var item in anyLeft.Inners)
                {
                    list.Add(item);
                }
            }
            // AnyMatcher以外、または名前持ちのAnyMatcherは単独扱いする
            else
            {
                list.Add(left);
            }

            // 名無しのAnyMatcherはバラして中身を取り出す
            if ((right.Name == "") &&(right is IAnyMatcher anyRight))
            {
                foreach (var item in anyRight.Inners)
                {
                    list.Add(item);
                }
            }
            // AnyMatcher以外、または名前持ちのAnyMatcherは単独扱いする
            else
            {
                list.Add(right);
            }


            Inners = list.ToArray();
        }

        internal AnyMatcher(Matcher[] inners, string name = "")
        {
            Inners = inners;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }
            // インデントのロールバックに備えて現在値を取得しておく
            var lastNest = Nest.Root.LastItem;

            Match result;

            int currentIndex = tokenIndex;
            Match tempResult;

            foreach (var inner in Inners)
            {
                tempResult = inner.Match(tokenList, currentIndex);
                if (tempResult.IsSuccess)
                {
                    // このマッチャーに名前がある時
                    if (Name != "")
                    {
                        // 上がってきたマッチにも名前がある時
                        if(tempResult.Name != "")
                        {
                            // 名前を付けながらラップする
                            result = new WrapMatch(this, tempResult, Name);
                        }
                        // 上がってきたマッチに名前が無い時
                        else
                        {
                            // 上がってきたマッチに名前を付ける
                            result = tempResult[Name];
                        }
                    }
                    else
                    {
                        // 上がってきたマッチをそのまま返信する
                        result = tempResult;
                    }
                    _matchList[tokenIndex, this] = result;
                    return result;
                }
            }
            // マッチ失敗なのでインデントをロールバックする
            lastNest.Rollback();

            result = new FailMatch(this, tokenIndex);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            foreach (var inner in Inners)
            {
                inner.DebugOut(matchers, nest + "  ");
            }
        }
        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public AnyMatcher this[string name]
        {
            get { return new AnyMatcher(Inners, name); }
        }
    }
}
