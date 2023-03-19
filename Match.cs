using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Enumerator;
using System.Diagnostics;
using System.Collections;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace OPC
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
        public int TokenIndex { get; private set; }

        /// <summary>
        /// トークン数
        /// </summary>
        public int TokenCount { get; private set; }

        public int TextIndex
        {
            get { return TokenList.Instance[TokenIndex].TextIndex; }
        }

        public int TextLength { get; private set; }

        public string Name { get; protected set; }

        public void SatName(string name)
        {
            Name = name;
        }

        public Match(Matcher generator, int tokenIndex, int tokenCount)
        {
            Generator = generator;
            TokenIndex = tokenIndex;
            TokenCount = tokenCount;
            Name = generator.Name;

            var textLength = 0;

            for (int i = 0; i < tokenCount; i++)
            {
                textLength += TokenList.Instance[TokenIndex + i].TextLength;
            }
            TextLength = textLength;
        }
        public Match(Matcher generator, int tokenIndex, int tokenCount, string name)
        {
            Generator = generator;
            TokenIndex = tokenIndex;
            TokenCount = tokenCount;
            Name = name;

            var textLength = 0;

            for (int i = 0; i < tokenCount; i++)
            {
                textLength += TokenList.Instance[TokenIndex + i].TextLength;
            }
            TextLength = textLength;
        }

        public Match(Matcher generator, Match match)
        {
            Generator = generator;
            TokenIndex = match.TokenIndex;
            TokenCount += match.TokenCount;
            TextLength = match.TextLength;
            Name = generator.Name;
        }

        public Match(Matcher generator, IEnumerable<Match> matches)
        {
            Generator = generator;
            TokenIndex = -1;
            TokenCount = 0;
            foreach (var match in matches)
            {
                // 最初のマッチのトークンインデックスは取得しておく
                if (TokenIndex == -1) { TokenIndex = match.TokenIndex; }
                TokenCount += match.TokenCount;
            }

            var textLength = 0;

            foreach (var match in matches)
            {
                textLength += match.TextLength;
            }
            TextLength = textLength;
            Name = generator.Name;
        }

        public Match(Matcher generator, IEnumerable<Match> matches, string name)
        {
            Generator = generator;
            TokenIndex = -1;
            TokenCount = 0;
            foreach (var match in matches)
            {
                // 最初のマッチのトークンインデックスは取得しておく
                if (TokenIndex == -1) { TokenIndex = match.TokenIndex; }
                TokenCount += match.TokenCount;
            }

            var textLength = 0;

            foreach (var match in matches)
            {
                textLength += match.TextLength;
            }
            TextLength = textLength;
            Name = name;
        }

        public Match(Match org, string name)
            : this(org.Generator, org)
        {
            Name = name;
        }

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

            Debug.WriteLine($"{nest}{Detail} {matchName}{name}");
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
        public FailMatch(Matcher generator, int tokenIndex)
            : base(generator, tokenIndex, 0) { }
        public FailMatch(Matcher generator, int tokenIndex, int tokenCount)
            : base(generator, tokenIndex, tokenCount) { }
        public FailMatch(Matcher generator, int tokenIndex, int tokenCount, string name)
            : base(generator, tokenIndex, tokenCount, name) { }

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
            get { return new FailMatch(Generator,TokenIndex,TokenCount, name); }
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
            : base(generator, tokenIndex, 0) { }

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
}
