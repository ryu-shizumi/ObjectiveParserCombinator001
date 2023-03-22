using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Enumerator;
using OPC_001;

/// <summary>
/// 
/// 入力される文字列をそのまま（Char型のリスト）ではなく、
/// トークンのリストに変換して処理する。
/// 
/// マッチ失敗もマッチ成功もマッチ結果として返し、
/// マッチ結果はキャッシュしておく。
/// 
/// マッチ結果は、
/// 　　生成元マッチャーＩＤ、
/// 　　トークン位置（トークンリスト中のインデックス）、
/// 　　長さの情報（トークン個数）
/// を持つ？
/// 
/// マッチ結果はトークンのＩＤとそれを発生させたマッチャーのＩＤの組み合わせで、
/// ユニークな値として扱える。
/// 
/// マッチャーが返すマッチ結果は
/// 　　失敗を１個だけ返すかも知れない。
/// 　　成功を１個以上の複数個返すかも知れない。
/// 
/// マッチャーは結合演算してもコピーは作らない
/// 
/// 
/// 
/// </summary>

namespace OPC
{
    /// <summary>
    /// マッチャーの基底クラス
    /// </summary>
    public abstract class Matcher
    {
        /// <summary>
        /// デバッグ用のUniqIDを与える為のインスタンス数カウンタ
        /// </summary>
        private static int _count = 0;

        public int UniqID { get; private set; }
        
        protected Matcher()
        {
            UniqID = _count++;
        }

        /// <summary>
        /// 文字列とのマッチングを行う
        /// </summary>
        /// <param name="text">文字列</param>
        /// <returns>成功すれば成功マッチ。失敗すれば失敗マッチ</returns>
        public Match Match(string text)
        {
            _matchList = new MatchList();
            _matchLoopList = new MatchLoopList();

            Text = text;
            TokenList.Instance = new TokenList(text);

            for (int i = 0; i < TokenList.Instance.Count; i++)
            {
                foreach (var match in EnumMatch(TokenList.Instance, i,""))
                {
                    if (match.IsSuccess)
                    {
                        // 優先度の高いマッチを一つだけ返信する
                        return match;
                    }
                }
            }

            return new FailMatch(this, 0);
        }

        /// <summary>
        /// このマッチャーがトークンリストの任意の位置にマッチするならマッチを返す
        /// </summary>
        /// <param name="tokenList"></param>
        /// <param name="tokenIndex"></param>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="tokenIndex">トークンリスト内の任意の位置</param>
        /// <returns>マッチしていれば１個のマッチを返す。非マッチならFailMatchを返す</returns>
        public abstract Match Match(TokenList tokenList, int tokenIndex, string nest);

        /// <summary>
        /// このマッチャーがトークンリストの任意の位置にマッチするならマッチを１個以上返す
        /// </summary>
        /// <param name="tokenList">トークンリスト</param>
        /// <param name="tokenIndex">トークンリスト内の任意の位置</param>
        /// <returns>マッチしていれば１個以上のマッチを返す</returns>
        /// <remarks>回数指定に範囲のあるマッチの場合、何度もマッチを返す可能性がある</remarks>
        public virtual IEnumerable<Match> EnumMatch(TokenList tokenList, int tokenIndex, string nest)
        {
            var match = Match(tokenList, tokenIndex, nest + "  ");
            if(match.IsSuccess)
            {
                yield return match;
            }
        }
        
        /// <summary>
        /// このマッチャーに割り当てられた名前
        /// </summary>
        public string Name { get; protected set; } = "";

        public static MatchList _matchList = new MatchList();
        //public static MatchEnumList _matchEnumList = new MatchEnumList();
        public static MatchLoopList _matchLoopList = new MatchLoopList();

        public static string Text { get; protected set; }
        //public static TokenList TokenList { get; private set; }

        ///// <summary>
        ///// ２つのマッチャーから選択マッチャーを得る
        ///// </summary>
        ///// <param name="matcherA">マッチャー</param>
        ///// <param name="matcherB">マッチャー</param>
        ///// <returns>選択マッチャー</returns>
        //public static AnyMatcher operator |(Matcher matcherA, Matcher matcherB)
        //{
        //    return new AnyMatcher(matcherA, matcherB);
        //}

        

        /// <summary>
        /// 二項演算式のマッチャーを生成する
        /// </summary>
        /// <param name="operand">オペランド</param>
        /// <param name="perators">演算子</param>
        /// <returns></returns>
        public static RecursionMatcher BinaryOperation(Matcher operand, Matcher operators)
        {
            var result = new RecursionMatcher();
            result.Inner = operand + ((operators + operand) * 1.To(int.MaxValue));
            return result;
        }

        /// <summary>
        /// マッチャーと他のマッチャーを結合させてマッチャーを得る
        /// </summary>
        /// <param name="a">マッチャー</param>
        /// <param name="b">全てのマッチャー</param>
        /// <returns>マッチャー</returns>
        public static UnionMatcher operator +(Matcher a, Matcher b)
        {
            if (IgnoreBlank.IsIgnoreBlank())
            {
                return new UnionMatcher(a, BlankMatcher.GetInstance(), b);
            }
            return new UnionMatcher(a, b);
        }

        /// <summary>
        /// マッチャーと他のマッチャーを結合させてマッチャーを得る
        /// </summary>
        /// <param name="a">マッチャー</param>
        /// <param name="b">全てのマッチャー</param>
        /// <returns>マッチャー</returns>
        public static UnionMatcher operator +(char a, Matcher b)
        {
            return a._() + b;
        }
        /// <summary>
        /// マッチャーと他のマッチャーを結合させてマッチャーを得る
        /// </summary>
        /// <param name="a">マッチャー</param>
        /// <param name="b">全てのマッチャー</param>
        /// <returns>マッチャー</returns>
        public static UnionMatcher operator +(CharRange a, Matcher b)
        {
            return a._() + b;
        }
        /// <summary>
        /// マッチャーと他のマッチャーを結合させてマッチャーを得る
        /// </summary>
        /// <param name="a">マッチャー</param>
        /// <param name="b">全てのマッチャー</param>
        /// <returns>マッチャー</returns>
        public static UnionMatcher operator +(string a, Matcher b)
        {
            return a._() + b;
        }

        /// <summary>
        /// マッチャーと他のマッチャーを結合させてマッチャーを得る
        /// </summary>
        /// <param name="a">マッチャー</param>
        /// <param name="b">全てのマッチャー</param>
        /// <returns>マッチャー</returns>
        public static UnionMatcher operator +(Matcher a, char b)
        {
            return a+ b._();
        }
        /// <summary>
        /// マッチャーと他のマッチャーを結合させてマッチャーを得る
        /// </summary>
        /// <param name="a">マッチャー</param>
        /// <param name="b">全てのマッチャー</param>
        /// <returns>マッチャー</returns>
        public static UnionMatcher operator +(Matcher a, CharRange b)
        {
            return a + b._();
        }

        /// <summary>
        /// マッチャーと他のマッチャーを結合させてマッチャーを得る
        /// </summary>
        /// <param name="a">マッチャー</param>
        /// <param name="b">全てのマッチャー</param>
        /// <returns>マッチャー</returns>
        public static UnionMatcher operator +(Matcher a, string b)
        {
            return a + b._();
        }

        /// <summary>
        /// マッチャーに最大回数を設定して最長一致マッチャーを得る
        /// </summary>
        /// <param name="matcher">マッチャー</param>
        /// <param name="count">最大回数</param>
        /// <returns>最長一致マッチャー</returns>
        public static LongMatcher operator *(Matcher matcher, int count)
        {
            return new LongMatcher(matcher, count, count);
        }

        /// <summary>
        /// マッチャーに回数範囲を設定して最長一致マッチャーを得る
        /// </summary>
        /// <param name="matcher">マッチャー</param>
        /// <param name="range">回数範囲</param>
        /// <returns>最長一致マッチャー</returns>
        public static LongMatcher operator *(Matcher matcher, IntRange range)
        {
            return new LongMatcher(matcher, range.Min, range.Max);
        }

        public static AnyMatcher operator |(Matcher a, Matcher b)
        {
            return new AnyMatcher(a, b);
        }

        public string ClassName
        {
            get
            {
                var name = GetType().Name;
                return name.Substring(0, name.Length - 7);
            }
        }

        public void DebugOut()
        {
            HashSet<RecursionMatcher> matchers = new HashSet<RecursionMatcher>();
            DebugOut(matchers, "");
        }

        public abstract void DebugOut(HashSet<RecursionMatcher> matchers, string nest);

        public virtual AtomicMatcher Atom
        { get { return new AtomicMatcher(this); } }
    }


    /// <summary>
    /// 単語と照合するマッチャー
    /// </summary>
    public class WordMatcher : Matcher
    {
        public string Word { get; private set; }

        public WordMatcher(string word)
        {
            Word = word;
        }
        private WordMatcher(string word, string name)
        {
            Word = word;
            Name = name;
        }

        public override Match Match(TokenList tokenList, int tokenIndex, string nest)
        {
            // マッチリストにある時はそれを返す
            if (_matchList.ContainsKey(tokenIndex, this)) { return _matchList[tokenIndex, this]; }

            Match result;
            int innersCount = 0;

            for (int i = 0; i < Word.Length; i++)
            {
                var peekIndex = tokenIndex + i;
                if(peekIndex < tokenList.Count) 
                {
                    var token = tokenList[tokenIndex + i];
                    if ((token is TokenChar cToken) && (cToken.Char == Word[i]))
                    {
                        innersCount++;
                        continue;
                    }
                }

                result = new FailMatch(this, tokenIndex);
                _matchList[tokenIndex, this] = result;
                return result;
            }
            // 全文字の検査でハジかれなかった時は成功を返す
            result = new Match(this, tokenIndex, tokenIndex + Word.Length);
            _matchList[tokenIndex, this] = result;
            return result;
        }

        public override void DebugOut(HashSet<RecursionMatcher> matchers, string nest)
        {
            Debug.WriteLine(nest + "\"" + Word + "\"");
        }

        public static AnyMatcher operator |(char a, WordMatcher b)
        {
            return new AnyMatcher(a._(), b);
        }
        public static AnyMatcher operator |(string a, WordMatcher b)
        {
            return new AnyMatcher(a._(), b);
        }
        public static AnyMatcher operator |(WordMatcher a, char b)
        {
            return a | b._();
        }
        public static AnyMatcher operator |(WordMatcher a, string b)
        {
            return a | b._();
        }

        public WordMatcher this[string Name]
        {
            get
            {
                return new WordMatcher(Word, Name);
            }
        }
    }

    

    




    /// <summary>
    /// 文字型の拡張メソッド郡
    /// </summary>
    public static class MatcherEx
    {
        public static CharRange To(this char min, char max)
        {
            return new CharRange(min, max);
        }

        public static SimpleCharMatcher _(this char c)
        {
            return new SimpleCharMatcher(c);
        }

        public static WordMatcher _(this string word)
        {
            return new WordMatcher(word);
        }

        public static SimpleCharMatcher _(this CharRange charRange)
        {
            return new SimpleCharMatcher(charRange);
        }
    }
}
