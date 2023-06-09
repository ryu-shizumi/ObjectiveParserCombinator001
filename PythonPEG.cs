using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Parspell;
using static Parspell.IgnoreBlank;
using static Parspell.IgnoreBlank.IgnoreStateFlag;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Parspell
{
    public abstract class PythonPEG
    {
        public static string GetFileText(string filename, Encoding encoding)
        {
            string result;
            using (StreamReader sr = new StreamReader(filename, encoding))
            {
                result = sr.ReadToEnd(); 
            }
            return result;
        }

        public static void Test()
        {
            const string FileSpec = "PythonPEG.txt";
            //const string FileSpec = "PythonPEG_Part.txt";
            string fileText;
            //テキストファイルを読み込む
            using (var fs = File.OpenRead(FileSpec))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                fileText = sr.ReadToEnd();    // 全文              
            }

            //string testText = fileText;
            //string testText = "file: [statements] ENDMARKER";
            //string testText = "interactive: statement_newline ";
            //string testText = "file: [statements] ENDMARKER \r\ninteractive: statement_newline \r\neval: expressions NEWLINE* ENDMARKER ";
            string testText = "aaaa: bbbb CCCC \r\nDDDD: EEEE \r\nFFFF: GGGG HHHH* IIII ";

            //testText = "file: [statements] ENDMARKER ";

            // https://docs.python.org/ja/3/library/tokenize.html
            // NEWLINE は改行文字を指す
            // 行末を示すトークンは無い

            // 空白を読み飛ばさない
            IgnoreState = NoIgnore;

            var alphabet = 'A'.To('Z') | 'a'.To('z');
            var numeric = '0'.To('9')._();

            var newLine = "\r\n"._();

            // コメント
            var comment = ('#' + ('\r'._() | '\n').Not.Above0()).Atom["Comment"];
            var comments = (comment + (newLine.Above1().Atom + comment).Above0())["Comments"];

            // 識別子
            var identifier = ((alphabet | '_') + (alphabet | numeric | '_').Above0()).Atom["Identifier"];

            // 文字列リテラル
            var stringliteral = ('\'' + '\''._().Not.Above1().Atom["StringBody"] + '\'')["StringLiteral"];

            // 単独要素として扱える式
            var exp = new RecursionMatcher()["Exp"];

            IgnoreState = IgnoreSpaceNewLine;

            // レコードの先頭部分
            var recoardHead = (identifier + ':')["RecoardHead"];

            // 式に含まれる識別子
            var identifierOnExp = (recoardHead.Lookahead.Not + identifier)["IdentifierOnExp"];

            // (e) 括弧式
            var parenExp =  ('(' + exp + ')')["ParenExp"];

            // [e] 角括弧式 0回か1回か
            var bracketExp = ('[' + exp + ']')["BracketExp"];

            // 最小要素。前置単項演算子を付与できる。後置単項演算子を付与できる
            var primary = identifierOnExp | stringliteral | parenExp | bracketExp;

            // e? 0回か1回か
            var questionExp = primary + '?';

            // &e 肯定先読み
            var lookAhead = ('&' + primary)["LookAhead"];

            // !e 否定先読み
            var lookAheadNot = ('!' + primary)["LookAheadNot"];

            // e* 0回以上
            var above0 = (primary + '*')["Above0"];

            // e+ 1回以上
            var above1 = (primary + '+')["Above1"];

            IgnoreState = IgnoreSpace;
            // s.e+ 区切りを挟んだ繰り返し
            var separated = (primary["Separator"] + '.' + primary["Item"] + '+')["Separated"];

            // 連結のオペランドになれる要素
            var operand = (questionExp | lookAhead | lookAheadNot | above0 | above1 | separated | primary)["Operand"];

            // 空白を読み飛ばさない（区切り文字として必要）
            IgnoreState = NoIgnore;
            // e1 e2 要素の連結
            var joinExp = (operand + (' '._().Above1().Atom + operand).Above0())["JoinExp"];

            // 空白・改行を読み飛ばす
            IgnoreState = IgnoreSpaceNewLine;
            // e1 | e2 要素の選択
            var orExp = (joinExp + ('|' + joinExp).Above0())["OrExp"];

            exp.Inner = orExp;

            //exp.DebugOut();
            Debug.WriteLine("|-=-=_-=-=|-=-=_-=-=");



            IgnoreState = IgnoreSpaceNewLine;
            var recoard = (recoardHead + exp)["Recoard"];
            DebugCount.WriteLine(); 
            var recoards = recoard.Above1()["Recoards"];


            //recoard.DebugOut();
            //Test("b C: d e f g: h", recoard, 9, 3, 0);





            var recoards_or_comments = (recoards | comments);
            
            var PEG = recoards_or_comments.Above1()["PEG"];
            testText = "PythonPEG_Part.txt".GetText_UTF8();

            //Test(testText, PEG, 10);

            //testText = "# PEG grammar for Python\r\n\r\n\r\n\r\n# ========================= START OF THE GRAMMAR =========================\r\n\r\n# General grammatical elements and rules:";
            testText = "#a\r\n#b";

            //Test("#a", comment, 11, 0, 0);
            //Test("#ab", comment, 11, 0, 1);
            //Test("#abc", comment, 11, 0, 2);
            //Test("#abcd", comment, 11, 0, 3);
            //Test("#abcde", comment, 11, 0, 4);

            comments.DebugOut();

            //Test("#a", comments, 11,1);
            //Test("#a\r\n#b", comments, 11,2);
            //Test("#a\r\n#b\r\n", comments, 11, 2, 2);
            //Test("#a\r\n#b  ", comments, 11, 2, 3);
            //Test("#a\r\n#b\r\n#c", comments, 11, 3);

            //Test("a+", exp, 1, 0, 0);
            //Test("a b", exp, 2, 0, 0);
            //Test("a b [C] d* e+ f g: h", exp, 3, 0, 0);

            Test("g: h", recoard, 9, 3, 0, 0);
            Test("g: h", recoards, 9, 3, 0, 1);
            Test("a b: C d e f g: h", recoards, 9, 3, 0);
            //Test("b C d e : f g", identifierOnExp.Above1, 9, 4, 0);

            //Test("#a", comments, 12, 1);
            //Test("#a", PEG, 12, 2);
            //Test("#a", recoards_or_comments, 12, 2,2);
            //Test("#a\r\n#b", PEG, 12, 2);
            //Test("#a\r\n#b\r\n", PEG, 12, 2, 2);
            //Test("#a\r\n#b  ", PEG, 12, 2, 3);
            //Test("#a\r\n#b\r\n#c", PEG, 12, 3);

            //Test("#a\r\n#b\r\n#c\r\nx:y z\r\nL:M N", PEG, 13, 0);

            Test("x : y L : M", recoards, 14, 0);
            Test("x:y L:M", recoards, 14, 0,2);
            Test("x:y\r\nL:M", recoards, 14, 0, 3);
            Test("x : y L : M", PEG, 14, 1);
            Test("x:y L:M", PEG, 14, 2);
            Test("x:y\r\nL:M", PEG, 14, 3);
            Test("x:y z\r\nL:M N", PEG, 14, 4);

            Test("#a\r\nx:y", PEG, 13, 1);
            Test("#a\r\n#b\r\nx:y", PEG, 13, 2);
            Test("#a\r\n#b\r\n#c\r\nx:y z\r\nL:M N", PEG, 13, 3);


        }

        private static void Test(string text, Matcher matcher, params int[] numbers)
        {
            var match = matcher.Match(text);

            var sb = new StringBuilder();
            foreach (var number in numbers)
            {
                if (sb.Length > 0)
                { sb.Append("-"); }
                sb.Append(number);
            }
            Debug.WriteLine("|-=-=_-=-=|-=-=_-=-=");
            Debug.WriteLine($"Test {sb}");
            match.DebugPrint();
        }

        public static void Test_Org()
        {
            const string FileSpec = "PythonPEG.txt";
            //const string FileSpec = "PythonPEG_Part.txt";
            string fileText;
            //テキストファイルを読み込む
            using (var fs = File.OpenRead(FileSpec))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                fileText = sr.ReadToEnd();    // 全文              
            }

            //string testText = fileText;
            //string testText = "file: [statements] ENDMARKER";
            //string testText = "interactive: statement_newline ";
            //string testText = "file: [statements] ENDMARKER \r\ninteractive: statement_newline \r\neval: expressions NEWLINE* ENDMARKER ";
            string testText = "aaaa: [bbbb] CCCC \r\nDDDD: EEEE \r\nFFFF: GGGG HHHH* IIII ";

            //testText = "file: [statements] ENDMARKER ";

            // https://docs.python.org/ja/3/library/tokenize.html
            // NEWLINE は改行文字を指す
            // 行末を示すトークンは無い

            IgnoreState = NoIgnore;

            var alphabet = 'A'.To('Z') | 'a'.To('z');
            var numeric = '0'.To('9')._();

            // コメント
            var comment = ('#' + ('\r'._() | '\n').Not.Above0()).Atom["Comment"];


            // 識別子
            var identifier = ((alphabet | '_') + (alphabet | numeric | '_').Above0()).Atom["Identifier"];

            // 文字列リテラル
            var stringliteral = ('\'' + '\''._().Not.Above1().Atom["StringBody"] + '\'')["StringLiteral"];

            // 単独要素として扱える式
            var exp = new RecursionMatcher()["Exp"];

            // 単独として扱えない複合要素
            //var uniExp = new RecursionMatcher();

            IgnoreState = IgnoreSpace;
            var recoardHead = identifier + ':';
            // e1 e2
            var joinExp = (exp["JoinLeft"] + (recoardHead.Lookahead.Not + exp).Above0()["JoinRight"])["JoinExp"]; //new OperationMatcher(exp, ' '._().Above1)["JoinExp"];
                                                                                                                //var joinExp = (exp["JoinLeft"] + (recoardHead.Lookahead.Not + exp).Above0)["JoinExp"]; //new OperationMatcher(exp, ' '._().Above1)["JoinExp"];
                                                                                                                //var joinExp = (exp +  (recoardHead.Lookahead.Not + exp).Above0["JoinRight"])["JoinExp"]; //new OperationMatcher(exp, ' '._().Above1)["JoinExp"];


            IgnoreState = IgnoreSpaceNewLine;
            // e1 | e2
            var orExp = (joinExp + ('|' + joinExp).Above0())["OrExp"]; //new OperationMatcher(joinExp, "|"._())["OrExp"];

            IgnoreState = IgnoreSpace;
            // [e]  or  e?
            var optionalExp = (('[' + exp + ']') | (exp + '?'))["OptionalExp"];

            // (e)
            var parenExp = ('(' + exp + ')')["ParenExp"];

            // s.e+
            var separatedItems = (exp["Separator"] + '.' + exp["Item"] + '+')["SeparatedItems"];

            // &e
            var lookAhead = ('&' + exp)["LookAhead"];

            // !e
            var lookAheadNot = ('!' + exp)["LookAheadNot"];

            // e*
            var above0 = (exp + '*')["Above0"];

            // e+
            var above1 = (exp + '+')["Above1"];

            //IgnoreState = NoIgnore;
            exp.Inner =
                //joinExp
                orExp
                //| parenExp
                | optionalExp
                //| separated 
                //| lookAhead
                //| lookAheadNot
                //| above0
                //| above1
                //| stringliteral
                | identifier
                ;


            IgnoreState = IgnoreSpaceNewLine;
            var recoard = (recoardHead + exp)["Recoard"];
            var recoards = recoard.Above1();

            IgnoreState = IgnoreNewline;
            var comments = (comment + comment.Above0())["Comments"];

            IgnoreState = IgnoreSpaceNewLine;
            var peg = (comments | recoard).Above1();




            var match = recoards.Match(testText);
            match.DebugPrint();
        }
    }

    public static class TextfileEx
    {
        public static string GetText(this string fileSpec, Encoding encoding)
        {
            string fileText = "";
            //テキストファイルを読み込む
            using (var fs = File.OpenRead(fileSpec))
            using (var sr = new StreamReader(fs, encoding))
            {
                fileText = sr.ReadToEnd();    // 全文              
            }

            return fileText;
        }

        public static string GetText_UTF8(this string fileSpec)
        {
            return fileSpec.GetText(Encoding.UTF8);
        }
    }


}
