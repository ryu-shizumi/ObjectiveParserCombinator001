using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    /// トークンの基底クラス
    /// </summary>
    public abstract class Token
    {
        public static string Text { get; private set; } = "";
        public static void SetText(string text)
        {
            Text = text;
        }
        public Token(int charIndex)
        {
            TextIndex = charIndex;
        }

        /// <summary>
        /// 文字列内のインデックス
        /// </summary>
        public int TextIndex { get; private set; }
        /// <summary>
        /// 文字列としての長さ
        /// </summary>
        public abstract int TextLength { get; }

        /// <summary>
        /// インデックスと長さで示される部分文字列を取得する
        /// </summary>
        /// <param name="text">元の文字列</param>
        /// <returns>インデックスと長さで示される部分文字列</returns>
        public string GetValue(string text)
        {
            return text.Substring(TextIndex, TextLength);
        }

        public override string ToString()
        {
            return Text.Substring(TextIndex, TextLength);
        }
    }

    /// <summary>
    /// 文字トークン
    /// </summary>
    public class TokenChar : Token
    {
        internal TokenChar(int charIndex)
            : base(charIndex)
        {
        }

        public override int TextLength { get { return 1; } }

        public char Char
        {
            get { return Text[TextIndex]; }
        }

        public virtual bool IsSpace
        {
            get { return false; }
        }

        public static TokenChar Create(string text, int index)
        {
            // サロゲート文字の判定
            if ((Char.IsSurrogate(text, index)) && (index + 1 < Text.Length))
            {
                return new TokenCharSurrogate(index);
            }

            // 空白に分類できる文字の時
            switch (text[index])
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return new TokenSpace(index);
            }

            return new TokenChar(index);
        }

        public virtual int Code { get { return Text[TextIndex]; } }
    }

    /// <summary>
    /// サロゲート文字トークン
    /// </summary>
    public class TokenCharSurrogate : TokenChar
    {
        public TokenCharSurrogate(int charIndex) : base(charIndex)
        {
        }

        public override int TextLength { get { return 2; } }

        public override int Code
        {
            get { return Char.ConvertToUtf32(Text[TextIndex], Text[TextIndex + 1]); }
        }
    }

    /// <summary>
    /// 空白文字トークン
    /// </summary>
    public class TokenSpace : TokenChar
    {
        public TokenSpace(int charIndex) : base(charIndex)
        {
        }

        public override bool IsSpace { get { return true; } }

        public override string ToString()
        {
            switch(Text[TextIndex])
            {
                case ' ':
                    return " ";
                case '\t':
                    return @"\t";
                case '\f':
                    return @"\f";
            }
            return base.ToString();
        }
    }

    /// <summary>
    /// 空白連続トークン
    /// </summary>
    public class TokenSpaces : Token
    {
        public TokenSpaces(int charIndex, int length)
            : base(charIndex)
        {
            _length = length;
        }

        private int _length;

        public override int TextLength { get { return _length; } }
        public override string ToString()
        {
            var sb = new StringBuilder();

            for(int i = TextIndex; i < TextIndex + TextLength; i++)
            {
                switch (Text[i])
                {
                    case ' ':
                        sb.Append(" ");
                        break;
                    case '\t':
                        sb.Append(@"\t");
                        break;
                    case '\f':
                        sb.Append(@"\f");
                        break;
                    default:
                        sb.Append(Text[i]);
                        break;
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 改行トークン
    /// </summary>
    public class TokenNewline : Token
    {
        public TokenNewline(int charIndex, int length)
            : base(charIndex)
        {
            _length = length;
        }

        private int _length;

        public override int TextLength { get { return _length; } }

        public override string ToString()
        {
            if (TextLength == 1) { return @"\n"; }
            if (TextLength == 2) { return @"\r\n"; }
            return "";
        }
    }

    /// <summary>
    /// 特殊トークンの基底クラス
    /// </summary>
    public class TokenSpecial : Token
    {
        public TokenSpecial(int charIndex)
            : base(charIndex) { }

        public override int TextLength { get { return 0; } }

        public override string ToString()
        {
            
            var typeName = this.GetType().Name;
            
            return typeName.Substring("TokenSpecial".Length);
        }
    }

    /// <summary>
    /// 文字列開始トークン
    /// </summary>
    public class TokenSpecialTextBegin : TokenSpecial
    {
        public TokenSpecialTextBegin(int charIndex)
            : base(charIndex) { }
    }

    /// <summary>
    /// 文字列終了トークン
    /// </summary>
    public class TokenSpecialTextEnd : TokenSpecial
    {
        public TokenSpecialTextEnd(int charIndex)
            : base(charIndex) { }
    }

    /// <summary>
    /// 行開始トークン
    /// </summary>
    public class TokenSpecialLineBegin : TokenSpecial
    {
        public TokenSpecialLineBegin(int charIndex)
            : base(charIndex) { }
    }

    /// <summary>
    /// インデントトークン
    /// </summary>
    public class TokenSpecialIndent : TokenSpecial
    {
        public TokenSpecialIndent(int charIndex)
            : base(charIndex) { }
    }

    /// <summary>
    /// デデントトークン
    /// </summary>
    public class TokenSpecialDedent : TokenSpecial
    {
        public TokenSpecialDedent(int charIndex)
            : base(charIndex) { }
    }
}
