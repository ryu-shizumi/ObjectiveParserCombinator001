using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Enumerator;
using System.Diagnostics;
using System.Collections;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace Parspell
{
    /// <summary>
    /// マッチ結果
    /// </summary>
    public abstract class Match
    {
        /// <summary>
        /// デバッグ用のUniqIDを与える為のインスタンス数カウンタ
        /// </summary>
        private static int _count = 0;

        public string UniqID { get; private set; }

        protected Match()
        {
            UniqID = $"P{_count++}";
        }
        /// <summary>
        /// このマッチを生成したマッチャー
        /// </summary>
        public Matcher Generator { get; private set; }

        /// <summary>
        /// マッチャーに与えたトークン（トークンリスト内のインデックス）
        /// </summary>
        public int TokenBeginIndex { get; private set; }
        public int TokenEndIndex { get; private set; }

        /// <summary>
        /// トークン数
        /// </summary>
        public virtual int TokenCount
        {
            get { return TokenEndIndex - TokenBeginIndex; }
        }

        public virtual int TextIndex
        {
            get
            {
                // 文字位置が最終トークンの終端から始まる時
                if(TokenBeginIndex == TokenList.Instance.Count)
                {
                    return Token.Text.Length;
                }

                Token token = TokenList.Instance[TokenBeginIndex];
                return token.TextIndex; 
            }
        }
        public virtual int TextEndIndex
        {
            get
            {
                // 文字位置が最終トークンの終端から始まる時
                if (TokenBeginIndex == TokenList.Instance.Count)
                {
                    return Token.Text.Length;
                }
                if(TokenEndIndex == 0)
                {
                    return 0;
                }

                Token token = TokenList.Instance[TokenEndIndex-1];
                return token.TextIndex + token.TextLength; 
            }
        }
        public int TextLength
        {
            get
            {
                // 文字位置が最終トークンの終端から始まる時
                if (TokenBeginIndex == TokenList.Instance.Count)
                {
                    return 0;
                }

                if(TokenCount == 0)
                {
                    return 0;
                }

                int totleLength = 0;

                for(int i = TokenBeginIndex; i < TokenEndIndex; i++)
                {
                    totleLength += TokenList.Instance[i].TextLength;
                }

                return totleLength;
            }
        }

        public string Name { get; protected set; }
        public Match(Matcher generator, int tokenIndex, string name = "")
            : this()
        {

            Generator = generator;
            TokenBeginIndex = tokenIndex;
            TokenEndIndex = tokenIndex;
            Name = (name != "") ? name : generator.Name;

            if (TokenCount < 0) { var temp = ""; }
        }

        public Match(Matcher generator, int tokenBeginIndex, int tokenEndIndex, string name = "")
            : this()
        {
            
            Generator = generator;
            TokenBeginIndex = tokenBeginIndex;
            TokenEndIndex = tokenEndIndex;
            Name = (name != "") ? name : generator.Name;

            if (TokenCount < 0) { var temp = ""; }
        }

        public Match(Matcher generator, Match match, string name = "")
            : this()
        {
            Generator = generator;
            TokenBeginIndex = match.TokenBeginIndex;
            TokenEndIndex = match.TokenEndIndex;
            Name = (name != "") ? name : generator.Name;

            if (TokenCount < 0) { var temp = ""; }
        }

        public Match(Matcher generator, IEnumerable<Match> matches, string name = "")
            : this()
        {
            Generator = generator;

            GetTokenRange(matches, out int begin, out int end);
            TokenBeginIndex = begin;
            TokenEndIndex = end;

            Name = (name != "") ? name : generator.Name;

            if (TokenCount < 0) { var temp = ""; }
        }

        private static void GetTokenRange(IEnumerable<Match> matches, out int tokenBeginIndex, out int tokenEndIndex)
        {
            tokenBeginIndex = -1;
            Match? lastMatch = null;
            foreach (var match in matches)
            {
                lastMatch = match;
                // 最初のマッチのトークンインデックスは取得しておく
                if (tokenBeginIndex == -1) { tokenBeginIndex = match.TokenBeginIndex; }
            }
            if (lastMatch != null)
            {
                tokenEndIndex = lastMatch.TokenEndIndex;
            }
            else
            {
                tokenEndIndex = tokenBeginIndex;
            }
        }

        private Match(Match org, string name)
            : this(org.Generator, org, name) { }


        public abstract bool IsSuccess { get;}

        public override string ToString()
        {
            var str = Token.Text.Substring(TextIndex, TextLength);

            str = str.Replace("\r", "\\r");
            str = str.Replace("\n", "\\n");

            return str;
        }

        /// <summary>
        /// このマッチが示す文字列とその範囲
        /// </summary>
        public virtual string Detail
        {
            get { return $"{this} [{TextIndex}-{TextIndex + TextLength}]"; }
        }

        /// <summary>
        /// このマッチを文字列化して出力ウィンドウに出力する
        /// </summary>
        /// <param name="nest"></param>
        public virtual void DebugPrint(string nest = "")
        {
            if (UniqID == "P32")
            {
                var temp = "";
            }

            var typeName = Generator.GetType().Name;
            var matchName = typeName.Substring(0, typeName.Length - "Matcher".Length);

            var name = "";
            if (Name.Length > 0)
            {
                name = $" [{Name}]";
            }

            //Debug.WriteLine($"{nest}{Detail} {matchName}{name}");
            Debug.WriteLine($"{nest}{this} {name} [{TextIndex}-{TextEndIndex}]:{UniqID} {Generator.UniqID}");
        }

        public virtual Match this[string name]
        {get { throw new Exception(); }
        }
    }

    public abstract class NotableMatch : Match
    {
        public abstract Match GetNot(Matcher generator);

        public NotableMatch(Matcher generator, int tokenIndex, string name = "")
            : base(generator, tokenIndex, name) { }
        public NotableMatch(Matcher generator, int tokenIndex, int tokenEndIndex, string name = "")
            : base(generator, tokenIndex, tokenEndIndex, name) { }
        public NotableMatch(Matcher generator, Match match, string name = "")
            : base(generator, match, name) { }

    }

    /// <summary>
    /// 成功マッチ
    /// </summary>
    public class SuccessMatch : NotableMatch
    {
        public SuccessMatch(Matcher generator, int tokenIndex, string name = "")
            : base(generator, tokenIndex, name) { }
        public SuccessMatch(Matcher generator, int tokenIndex, int tokenEndIndex, string name = "")
            : base(generator, tokenIndex, tokenEndIndex, name) { }
        public SuccessMatch(Matcher generator, Match match, string name = "")
            : base(generator, match, name) { }

        public override bool IsSuccess
        { get { return true; } }


        
        public override Match this[string name]
        {
            get { return new SuccessMatch(Generator, TokenBeginIndex, TokenCount, name); }
        }
        public override Match GetNot(Matcher generator)
        {
            return new FailMatch(generator, this);
        }
    }

    /// <summary>
    /// 失敗マッチ
    /// </summary>
    public class FailMatch : NotableMatch
    {
        public FailMatch(Matcher generator, int tokenIndex, string name = "")
            : base(generator, tokenIndex, name) { }
        public FailMatch(Matcher generator, int tokenIndex, int tokenEndIndex, string name = "")
            : base(generator, tokenIndex, tokenEndIndex, name) { }
        public FailMatch(Matcher generator, Match match, string name = "")
            : base(generator, match, name) { }

        public override bool IsSuccess
        { get { return false; } }

        public override void DebugPrint(string nest = "")
        {
            Debug.WriteLine($"{nest}(Fail)");
        }

        public override string ToString()
        {
            return "(Fail)";
        }

        public override Match this[string name]
        {
            get { return new FailMatch(Generator,TokenBeginIndex,TokenCount, name); }
        }
        public override Match GetNot(Matcher generator)
        {
            return new SuccessMatch(generator, this);
        }
    }

    /// <summary>
    /// 範囲外マッチ
    /// </summary>
    public class RangeOutMatch : Match
    {
        public RangeOutMatch(Matcher generator, int tokenIndex, string name = "")
            : base(generator, tokenIndex, name) { }

        public override bool IsSuccess
        { get { return false; } }
    }

    /// <summary>
    /// 再帰マッチャーが無限再帰に陥った事を検知するマッチ
    /// </summary>
    /// <remarks>
    /// 
    /// 再帰マッチャーでキャッシュが無い時に Match() に侵入したら、
    /// キャッシュ領域にこのマッチを設定する。
    /// 
    /// 再帰マッチャーでキャッシュがこのマッチの時は無限再帰に陥っているので、
    /// FailMatchを返して脱出する。
    /// 
    /// キャッシュ領域は、マッチャーと(トークンリスト上の)インデックスの組で分けられており、
    /// キャッシュ領域にこのマッチの存在を検知したら、即ち無限再帰と見なせる。
    /// 
    /// </remarks>
    public class SearchingMatch : Match
    {
        public SearchingMatch(Matcher generator, int tokenIndex, string name = "")
            : base(generator, tokenIndex, name) { }

        public override bool IsSuccess
        { get { return false; } }

        public override void DebugPrint(string nest = "")
        {
            Debug.WriteLine($"{nest}(Searching)");
        }

        public override string ToString()
        {
            return "(Searching)";
        }
    }

    /// <summary>
    /// 無限再帰を強引に脱出した時に生成されるマッチ
    /// </summary>
    public class InfiniteRecursionMatch : Match
    {
        public InfiniteRecursionMatch(Matcher generator, int tokenIndex, string name = "")
            : base(generator, tokenIndex, name) { }

        public override bool IsSuccess
        { get { return false; } }

        public override void DebugPrint(string nest = "")
        {
            Debug.WriteLine($"{nest}(InfiniteRecursion)");
        }

        public override string ToString()
        {
            return "(InfiniteRecursion)";
        }
    }

    /// <summary>
    /// 入れ子マッチ
    /// </summary>
    public class WrapMatch : Match
    {
        /// <summary>
        /// 内部のマッチ
        /// </summary>
        private Match[] _inners;

        public IList<Match> Inners { get { return _inners; } }

        public WrapMatch(Matcher generator, IEnumerable<Match> inners, string name = "")
            : base(generator, inners, name)
        {
            if(inners is Match[] array)
            {
                _inners = new Match[array.Length];
                array.CopyTo(_inners, 0);
            }
            _inners = inners.ToList().ToArray();
        }
        public WrapMatch(Matcher generator, Match inner, string name = "")
            : base(generator, inner, name)
        {
            _inners = new Match[] { inner };
        }

        public override bool IsSuccess
        { get { return true; } }

        public override void DebugPrint(string nest = "")
        {
            base.DebugPrint(nest);
            foreach (Match match in _inners)
            {
                match.DebugPrint(nest + "  ");
            }
        }
        public override Match this[string name]
        {
            get { return new WrapMatch(Generator, _inners, name); }
        }
    }

    /// <summary>
    /// 構文エラーマッチ
    /// </summary>
    public class SyntaxErrorMatch : WrapMatch
    {
        public SyntaxErrorMatch(Matcher generator, Match inner, string name = "")
            : base(generator, inner, name) { }
    }

    /// <summary>
    /// 二項演算マッチ
    /// </summary>
    public class OperationMatch : Match
    {
        public Match Operator { get; private set; }

        public Match Left { get; private set; }
        public Match Right { get; private set; }


        public OperationMatch(Matcher generator, Match left, Match @operator, Match right, string name = "")
            :base(generator,new Match[] {left,@operator,right }, name)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }
        public OperationMatch(OperationMatch org, string name)
            : this(org.Generator, org.Left, org.Operator,org.Right)
        {
            Name = name;
        }
        public override bool IsSuccess
        { get { return true; } }

        public override void DebugPrint(string nest = "")
        {
            base.DebugPrint(nest);

            Left.DebugPrint(nest + "  ");
            Operator.DebugPrint(nest + "  ");
            Right.DebugPrint(nest + "  ");
        }
        public override Match this[string name]
        {
            get { return new OperationMatch(this, name); }
        }
    }

    /// <summary>
    /// 空白マッチ
    /// </summary>
    /// <remarks>
    /// 無視できる空白を抽象構文木から除外し易くする為に、最初から空白マッチとしてヒットさせておく
    /// </remarks>
    public class BlankMatch : Match
    {
        public BlankMatch(Matcher generator, int tokenBeginIndex, int tokenEndIndex, string name = "")
            : base(generator, tokenBeginIndex, tokenEndIndex, name) { }

        public override bool IsSuccess
        { get { return true; } }

    }

}
