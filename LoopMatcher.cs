using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    /// ループマッチャー
    /// </summary>
    public abstract class LoopMatcher_Base : Matcher
    {
        public Matcher Inner { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }

        public abstract Func<int, int, int> SortOrderFunc { get; protected set; }

        public LoopMatcher_Base(Matcher inner, int min, int max)
        {
            Inner = inner;
            Min = min;
            Max = max;
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

            return new FailMatch(this, tokenIndex);
            //return Inners.Match(tokenList, tokenIndex);
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
            LoopMatchResults results;

            // マッチリスト（キャッシュ）にある時はそれを返す
            if (_matchLoopList.ContainsKey(tokenIndex, this))
            {
                results = _matchLoopList[tokenIndex, this];

                foreach (Match match in results)
                {
                    yield return match;
                }
                yield break;
            }
            // マッチリストが無ければ作る
            else
            {
                results = new LoopMatchResults(SortOrderFunc);
                _matchLoopList[tokenIndex, this] = results;
            }

            bool isMatchReturned = false;

            Stack<IEnumerator<Match>> stack = new Stack<IEnumerator<Match>>();

            // 最低ゼロ回の時は、長さゼロでマッチを作る
            if (Min == 0)
            {
                var wrap = new Match(this, tokenIndex, tokenIndex);
                // 返信リストに追加する
                results.Add(wrap);
                isMatchReturned = true;
            }

            //int currentIndex = tokenIndex;
            Stack<int> currentIndex = new Stack<int>();
            currentIndex.Push(tokenIndex);

            List<Match> inners = new List<Match>();

            //if (UniqID == 38)
            //{
            //    var temp = "";
            //}

            var enumerator = Inner.EnumMatch(tokenList, currentIndex.Peek()).GetEnumerator();
            stack.Push(enumerator);

            // MoveNext()は水平方向に走査を進める
            // 失敗が返信されたり品切れになったりして水平に進めない時は１層分（１回分）戻る


            while (stack.Count > 0)
            {
                var peek = stack.Peek();
                bool moveNext = peek.MoveNext();
                var currentMatch = peek.Current;
                // bool isSuccess = currentMatch.IsSuccess;

                if ((currentMatch != null) && (currentMatch.TextLength == 0))
                {
                    var generator = currentMatch.Generator;
                    var matchID = currentMatch.Generator.UniqID;
                    var temp = "";
                }

                // 成功マッチが帰ってきた時（水平方向に進めた時）
                if ((moveNext) &&
                    (currentMatch != null) && currentMatch.IsSuccess)
                {
                    // ラッパーマッチに突っ込む内部マッチを追加する
                    inners.Add(stack.Peek().Current);

                    // 最低回数未満の時は回数を追加するだけ
                    if (stack.Count < Min)
                    {
                        // 次にトークンリストとマッチングするインデックスを追加する
                        currentIndex.Push(stack.Peek().Current.TokenEndIndex);
                        // 次のマッチ列挙子を取得する
                        enumerator = Inner.EnumMatch(tokenList, currentIndex.Peek()).GetEnumerator();
                        // マッチ列挙子をスタックに押し込む
                        stack.Push(enumerator);
                    }
                    // 最小回数～最大回数の手前までは回数追加も返信もする
                    else if (stack.Count < Max)
                    {
                        // 次にトークンリストとマッチングするインデックスを追加する
                        currentIndex.Push(stack.Peek().Current.TokenEndIndex);
                        // 次のマッチ列挙子を取得する
                        enumerator = Inner.EnumMatch(tokenList, currentIndex.Peek()).GetEnumerator();
                        // マッチ列挙子をスタックに押し込む
                        stack.Push(enumerator);

                        // ラッパーマッチを作る
                        var wrap = new WrapMatch(this, inners);
                        // 返信リストに追加する
                        results.Add(wrap);
                        // マッチ返信を真にする
                        isMatchReturned = true;
                    }
                    // 最大回数では返信だけをする
                    else if (stack.Count == Max)
                    {
                        // ラッパーマッチを作る
                        var wrap = new WrapMatch(this, inners);
                        // 返信リストに追加する
                        results.Add(wrap);
                        // マッチ返信を真にする
                        isMatchReturned = true;

                        // 内部マッチの末尾を削る
                        inners.RemoveTail();
                    }
                }
                // 水平に進めない時は後退する
                else
                {
                    if (moveNext)
                    {
                        // 次にトークンリストとマッチングするインデックスから、現在マッチ長さを戻す
                        currentIndex.Pop();
                    }
                    // 今のマッチ列挙子は使い物にならないので、スタックから抜く
                    stack.Pop();
                }
            }

            // 一つも返せてない時は
            if (isMatchReturned == false)
            {
                var fail = new FailMatch(this, tokenIndex);
                // 失敗マッチを生成して返信リストに追加する
                results.Add(fail);

                // 失敗マッチを返信する
                yield return fail;
            }
            else
            {
                foreach (var result in results)
                {
                    yield return result;
                }
            }
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            string count = "";
            if((Min == 0) && (Max == int.MaxValue))
            {
                count = "*";
            }
            else if ((Min == 1) && (Max == int.MaxValue))
            {
                count = "+";
            }
            else if ((Min == 0) && (Max == 1))
            {
                count = "?";
            }
            else if(Min == Max)
            {
                count = "{" + Min.ToString() + "}";
            }
            else
            {
                count = "{" + Min.ToString() + ","+ Max.ToString()+ "}";
            }

            Debug.WriteLine($"({Inner}){count}");
        }
    }

    /// <summary>
    /// 最短一致マッチャー
    /// </summary>
    public class ShortMatcher : LoopMatcher_Base
    {
        public ShortMatcher(Matcher inner, int min, int max)
            : base(inner, min, max) { }

        private ShortMatcher(Matcher inner, int min, int max, string name)
            : base(inner, min, max) { Name = name; }

        public override Func<int, int, int> SortOrderFunc
        { get; protected set; } = LoopMatchResults.SmallToLarge;

        public static ShortMatcher operator *(ShortMatcher a, int count)
        {
            return new ShortMatcher(a, a.Min * count, a.Max * count);
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public ShortMatcher this[string Name]
        {
            get
            {
                return new ShortMatcher(Inner, Min, Max, Name);
            }
        }
    }

    /// <summary>
    /// 最長一致マッチャー
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class LongMatcher : LoopMatcher_Base
    {
        public LongMatcher(Matcher inner, int min, int max)
            : base(inner, min, max) { }

        private LongMatcher(Matcher inner, int min, int max, string name)
            : base(inner, min, max) { Name = name; }

        public override Func<int, int, int> SortOrderFunc
        { get; protected set; } = LoopMatchResults.LargeToSmall;

        public static LongMatcher operator *(LongMatcher a, int count)
        {
            return new LongMatcher(a, a.Min * count, a.Max * count);
        }

        /// <summary>
        /// このマッチャーに名前を設定したインスタンスを取得する
        /// </summary>
        /// <param name="Name">名前</param>
        /// <returns>このマッチャーに名前を設定したインスタンス</returns>
        public LongMatcher this[string Name]
        {
            get
            {
                return new LongMatcher(Inner, Min, Max, Name);
            }
        }
    }
}
