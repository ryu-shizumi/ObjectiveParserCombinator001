using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC
{
    
    public class TokenList : List<Token>
    {
        public static TokenList Instance;


        public TokenList(string text)
            :base()
        {
            Token.SetText(text);
            LineText.SetText(text);

            // ネストを保持するスタック
            Stack<int> nests = new Stack<int>();

            nests.Push(0);

            // 行単位で取り出す
            foreach (LineText line in ToLines(text))
            {
                // 空行の時
                if(line.IsEmpty)
                {
                    // 何もしない
                }
                else
                {
                    // ネストがあればトークン化して追加する
                    if (line.Indent > 0)
                    {
                        var spaces = new TokenSpaces(line.Start, line.Indent);
                        Add(spaces);
                    }

                    // 同じ深さの時
                    if (nests.Peek() == line.Indent)
                    {
                        // 何もしない
                    }

                    // より深くなった時
                    else if (nests.Peek() < line.Indent)
                    {
                        // インデントトークンを差し込む
                        Add(new TokenSpecialIndent(line.BodyStart));

                        // 新しいネストレベルをスタックに追加する
                        nests.Push(line.Indent);
                    }

                    // 浅くなった時
                    else if (line.Indent < nests.Peek())
                    {
                        // ネストレベルが合うまで何回でもデデント発生
                        while (true)
                        {

                            if (line.Indent == nests.Peek())
                            {
                                break;
                            }
                            else if (line.Indent < nests.Peek())
                            {
                                // デデントトークンを差し込む
                                Add(new TokenSpecialDedent(line.BodyStart));

                                // ネストレベルを１段階戻す
                                nests.Pop();

                                
                            }
                            // デデントされ過ぎた時
                            else if (nests.Peek() < line.Indent)
                            {
                                // デデントエラーが発生したという事であるが、
                                // どう対処すべきかワカランので暫定的にこう処理しておく。

                                // インデントトークンを差し込む
                                Add(new TokenSpecialIndent(line.BodyStart));

                                // 新しいネストレベルをスタックに追加する
                                nests.Push(line.Indent);
                            }
                        }
                    }

                    // 本体部の長さがゼロではない時
                    if (line.BodyLength > 0)
                    {
                        // 行文字列本体部をトークン化する
                        int i = line.BodyStart;
                        while (i < line.End)
                        {
                            var token = TokenChar.Create(text, i);
                            Add(token);
                            i += token.TextLength;
                        }
                    }
                }

                // 改行をトークン化する
                if (line.NewlineLength != 0)
                {
                    var newLine = new TokenNewline(line.End, line.NewlineLength);
                    Add(newLine);
                }

            }

            // 文字列終端トークンを差し込む
            Add(new TokenSpecialTextEnd(text.Length));

        }

        /// <summary>
        /// 文字列から行文字列を列挙する
        /// </summary>
        /// <param name="text">文字列</param>
        /// <returns></returns>
        private static IEnumerable<LineText> ToLines(string text)
        {
            int lineStart = 0;
            int lfPos;
            while (true)
            {
                // LFを探す
                lfPos = text.IndexOf("\n", lineStart);
                if (lfPos < 0) break;

                // １文字前が Cr の時
                if ((1 <= lfPos) && (text[lfPos - 1] == '\r'))
                {
                    yield return new LineText(text, lineStart, lfPos - 1, 2);
                }

                // 前に Cr が無く Lf だけの時
                else
                {
                    yield return new LineText(text, lineStart, lfPos - 1, 1);
                }
                lineStart = lfPos + 1;
            }

            yield return new LineText(text, lineStart, text.Length, 0);
        }

        
    }

    

 


    public class LineText
    {
        public static string Text { get; private set; } = "";
        public static void SetText(string text)
        {
            Text = text;
        }
        public int Start { get; private set; }
        public int End { get; private set; }

        public int NewlineLength { get; private set; }
        public int Indent { get; private set; }
        public bool IsEmpty { get; private set; }

        public int BodyStart { get; private set; }
        public int BodyLength
        {
            get { return End - BodyStart; }
        }
        public LineText(string text, int start, int end, int newlineLength)
        {
            Start = start;
            End = end;
            NewlineLength = newlineLength;
            Indent = CountIndent(text);
            IsEmpty = (Indent == -1);
            BodyStart = start + Indent;
        }
        public string GetText()
        {
            return Text.Substring(Start, End - Start);
        }

        public string GetBodyText()
        {
            if (IsEmpty) return "";

            return Text.Substring(BodyStart, End - BodyStart);
        }

        private int CountIndent(string text)
        {
            int nest = 0;
            for (int i = Start; i < End; i++)
            {
                // 空白に分類できる文字の時
                switch (text[i])
                {
                    case ' ':
                    case '\t':
                    case '\f':
                        nest++;
                        continue;
                }

                return nest;
            }

            // 空行はインデントと見做さない
            return -1;
        }

        public override string ToString()
        {
            return GetText();
        }
    }
}
