using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Enumerator;
using System.Diagnostics;
using System.Collections;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Parspell
{
    /// <summary>
    /// マッチ結果
    /// </summary>
    public class Match
    {
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
            get { return TokenList.Instance[TokenBeginIndex].TextIndex; }
        }
        public virtual int TextEndIndex
        {
            get
            {
                var token = TokenList.Instance[TokenEndIndex - 1];
                return token.TextIndex + token.TextLength; 
            }
        }
        public int TextLength
        {
            get { return TextEndIndex - TextIndex; }
        }

        public string Name { get; protected set; }

        public Match(Matcher generator, int tokenBeginIndex, int tokenEndIndex, string name = "")
        {
            Generator = generator;
            TokenBeginIndex = tokenBeginIndex;
            TokenEndIndex = tokenEndIndex;
            Name = (name != "") ? name : generator.Name;

            if (TokenCount < 0) { var temp = ""; }
        }

        public Match(Matcher generator, Match match, string name = "")
        {
            Generator = generator;
            TokenBeginIndex = match.TokenBeginIndex;
            TokenEndIndex = match.TokenEndIndex;
            Name = (name != "") ? name : generator.Name;

            if (TokenCount < 0) { var temp = ""; }
        }

        public Match(Matcher generator, IEnumerable<Match> matches, string name = "")
        {
            Generator = generator;
            TokenBeginIndex = -1;
            Match? lastMatch = null;
            foreach (var match in matches)
            {
                lastMatch = match;
                // 最初のマッチのトークンインデックスは取得しておく
                if (TokenBeginIndex == -1) { TokenBeginIndex = match.TokenBeginIndex; }
            }
            if(lastMatch != null)
            {
                TokenEndIndex = lastMatch.TokenEndIndex;
            }
            Name = (name != "") ? name : generator.Name;

            if (TokenCount < 0) { var temp = ""; }
        }

        private Match(Match org, string name)
            : this(org.Generator, org, name) { }

        public virtual bool IsSuccess
        { get { return true; } }


        public override string ToString()
        {
            return Token.Text.Substring(TextIndex, TextLength);
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
            var typeName = Generator.GetType().Name;
            var matchName = typeName.Substring(0, typeName.Length - "Matcher".Length);

            var name = "";
            if (Name.Length > 0)
            {
                name = $" [{Name}]";
            }

            //Debug.WriteLine($"{nest}{Detail} {matchName}{name}");
            Debug.WriteLine($"{nest}{this} {name}");
        }

        public virtual Match this[string name]
        {
            get { return new Match(this, name); }
        }
    }

    /// <summary>
    /// 失敗マッチ
    /// </summary>
    public class FailMatch : Match
    {
        public FailMatch(Matcher generator, int tokenBeginIndex)
            : base(generator, tokenBeginIndex, tokenBeginIndex) { }
        public FailMatch(Matcher generator, int tokenBeginIndex, int tokenEndIndex)
            : base(generator, tokenBeginIndex, tokenEndIndex) { }
        public FailMatch(Matcher generator, int tokenIndex, int tokenEndIndex, string name)
            : base(generator, tokenIndex, tokenEndIndex, name) { }
        public FailMatch(Matcher generator, Match match, string name = "")
            : base(generator, match, name) { }

        public override int TextIndex
        { get { return -1; } }

        public override int TextEndIndex
        { get { return -1; } }

        public override int TokenCount
        { get { return 0; } }

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
        public SearchingMatch(Matcher generator, int tokenIndex)
            : base(generator, tokenIndex, tokenIndex) { }

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
        public InfiniteRecursionMatch(Matcher generator, int tokenBeginIndex)
            : base(generator, tokenBeginIndex, tokenBeginIndex) { }

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

        public WrapMatch(Matcher generator, IEnumerable<Match> inners)
            : base(generator, inners)
        {
            _inners = inners.ToList().ToArray();
        }
        public WrapMatch(Matcher generator, Match inner)
            : base(generator, inner)
        {
            _inners = new Match[] { inner };
        }

        public WrapMatch(Matcher generator, params Match[] inners)
            : base(generator, inners)
        {
            _inners = inners;
        }
        public WrapMatch(Matcher generator, Match[] inners, string name)
            : base(generator, inners,name)
        {
            _inners = inners;
        }
        public WrapMatch(Matcher generator, Match inner, string name)
            : base(generator, inner.TokenBeginIndex, inner.TokenEndIndex, name)
        {
            _inners = new Match[] { inner };
        }

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
    public class ErrorMatch : WrapMatch
    {
        public ErrorMatch(Matcher generator, Match inner)
            : base(generator, inner) { }
    }

    /// <summary>
    /// 二項演算マッチ
    /// </summary>
    public class OperationMatch : Match
    {
        public Match Operator { get; private set; }

        public Match Left { get; private set; }
        public Match Right { get; private set; }


        public OperationMatch(Matcher generator, Match left, Match @operator, Match right)
            :base(generator,new Match[] {left,@operator,right })
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
        public BlankMatch(Matcher generator, int tokenBeginIndex, int tokenEndIndex)
            : base(generator, tokenBeginIndex, tokenEndIndex) { }
    }

    /// <summary>
    /// 先読みマッチ
    /// </summary>
    public class LookaheadMatch : Match
    {
        public LookaheadMatch(Matcher generator, int tokenBeginIndex)
            : base(generator, tokenBeginIndex, tokenBeginIndex) { }
        public LookaheadMatch(Matcher generator, int tokenBeginIndex, string name)
            : base(generator, tokenBeginIndex, tokenBeginIndex,name) { }
    }
}
