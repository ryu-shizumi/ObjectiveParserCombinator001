using Parspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Parspell_001
{
    //internal abstract class NestCounter
    //{
    //    public static void CountNest(string text)
    //    {
    //        var reg = new Regex("(?:^(\\s*)$)|(?:^(\\s*)(.*)$)", RegexOptions.Multiline);

    //        var matches = reg.Matches(text);

    //        // ネストを保持するスタック
    //        Stack<int> nests = new Stack<int>();
    //        nests.Push(0);

    //        foreach (System.Text.RegularExpressions.Match match in matches)
    //        {
    //            var body = match.Groups[3].Value;

    //            // 本体が無い時は空行として無視する
    //            if (body.Length == 0) { continue; }

    //            // 本体の前のインデントのみ考慮する
    //            var indent = match.Groups[2].Value.Length;



    //            // 同じ深さの時
    //            if (nests.Peek() == indent)
    //            {
    //                // 何もしない
    //            }
    //            // より深くなった時
    //            else if (nests.Peek() < indent)
    //            {
    //                // インデントトークンを差し込む
    //                Add(new TokenSpecialIndent(line.BodyStart));

    //                // 新しいネストレベルをスタックに追加する
    //                nests.Push(indent);
    //            }
    //            // 浅くなった時
    //            else if (line.Indent < nests.Peek())
    //            {
    //                // ネストレベルが合うまで何回でもデデント発生
    //                while (true)
    //                {

    //                    if (line.Indent == nests.Peek())
    //                    {
    //                        break;
    //                    }
    //                    else if (line.Indent < nests.Peek())
    //                    {
    //                        // デデントトークンを差し込む
    //                        Add(new TokenSpecialDedent(line.BodyStart));

    //                        // ネストレベルを１段階戻す
    //                        nests.Pop();


    //                    }
    //                    // デデントされ過ぎた時
    //                    else if (nests.Peek() < line.Indent)
    //                    {
    //                        // デデントエラーが発生したという事であるが、
    //                        // どう対処すべきかワカランので暫定的にこう処理しておく。

    //                        // インデントトークンを差し込む
    //                        Add(new TokenSpecialIndent(line.BodyStart));

    //                        // 新しいネストレベルをスタックに追加する
    //                        nests.Push(line.Indent);
    //                    }
    //                }

    //            }
    //        }

    //        Token.SetText(text);
    //        LineText.SetText(text);



    //        // 行単位で取り出す
    //        foreach (LineText line in ToLines(text))
    //        {
    //            // 空行の時
    //            if (line.IsEmpty)
    //            {
    //                // 何もしない
    //            }
    //            else
    //            {
    //                // ネストがあればトークン化して追加する
    //                if (line.Indent > 0)
    //                {
    //                    var spaces = new TokenSpaces(line.Start, line.Indent);
    //                    Add(spaces);
    //                }

    //                // 同じ深さの時
    //                if (nests.Peek() == line.Indent)
    //                {
    //                    // 何もしない
    //                }

    //                // より深くなった時
    //                else if (nests.Peek() < line.Indent)
    //                {
    //                    // インデントトークンを差し込む
    //                    Add(new TokenSpecialIndent(line.BodyStart));

    //                    // 新しいネストレベルをスタックに追加する
    //                    nests.Push(line.Indent);
    //                }

    //                // 浅くなった時
    //                else if (line.Indent < nests.Peek())
    //                {
    //                    // ネストレベルが合うまで何回でもデデント発生
    //                    while (true)
    //                    {

    //                        if (line.Indent == nests.Peek())
    //                        {
    //                            break;
    //                        }
    //                        else if (line.Indent < nests.Peek())
    //                        {
    //                            // デデントトークンを差し込む
    //                            Add(new TokenSpecialDedent(line.BodyStart));

    //                            // ネストレベルを１段階戻す
    //                            nests.Pop();


    //                        }
    //                        // デデントされ過ぎた時
    //                        else if (nests.Peek() < line.Indent)
    //                        {
    //                            // デデントエラーが発生したという事であるが、
    //                            // どう対処すべきかワカランので暫定的にこう処理しておく。

    //                            // インデントトークンを差し込む
    //                            Add(new TokenSpecialIndent(line.BodyStart));

    //                            // 新しいネストレベルをスタックに追加する
    //                            nests.Push(line.Indent);
    //                        }
    //                    }
    //                }

    //                // 本体部の長さがゼロではない時
    //                if (line.BodyLength > 0)
    //                {
    //                    // 行文字列本体部をトークン化する
    //                    int i = line.BodyStart;
    //                    while (i < line.End)
    //                    {
    //                        var token = TokenChar.Create(text, i);
    //                        Add(token);
    //                        i += token.TextLength;
    //                    }
    //                }
    //            }

    //            // 改行をトークン化する
    //            if (line.NewlineLength != 0)
    //            {
    //                var newLine = new TokenNewline(line.End, line.NewlineLength);
    //                Add(newLine);
    //            }

    //        }

    //        // 文字列終端トークンを差し込む
    //        Add(new TokenSpecialTextEnd(text.Length));
    //    }

    //    /// <summary>
    //    /// 文字列から行文字列を列挙する
    //    /// </summary>
    //    /// <param name="text">文字列</param>
    //    /// <returns></returns>
    //    private static IEnumerable<LineText> ToLines(string text)
    //    {
    //        int lineStart = 0;
    //        int lfPos;
    //        while (true)
    //        {
    //            // LFを探す
    //            lfPos = text.IndexOf("\n", lineStart);
    //            if (lfPos < 0) break;

    //            // １文字前が Cr の時
    //            if ((1 <= lfPos) && (text[lfPos - 1] == '\r'))
    //            {
    //                yield return new LineText(text, lineStart, lfPos - 1, 2);
    //            }

    //            // 前に Cr が無く Lf だけの時
    //            else
    //            {
    //                yield return new LineText(text, lineStart, lfPos - 1, 1);
    //            }
    //            lineStart = lfPos + 1;
    //        }

    //        yield return new LineText(text, lineStart, text.Length, 0);
    //    }
    //}
}
