using Parspell;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using static Parspell.IgnoreBlank;
using static Parspell.IgnoreBlank.IgnoreStateFlag;

// See https://aka.ms/new-console-template for more information


//var root = new Indent();

//root.FindNest(2);
//root.FindNest(4);
//root.FindNest(2);

//var last = root.LastItem;
//root.FindNest(0);


//root.FindNest(4);
//root.FindNest(8);
//root.FindNest(8);

//root.DebugOut();

//last.Rollback();
//root.DebugOut();

//last.Rollback();
//root.DebugOut();


//return;

PythonPEG.Test();

return;

string text = 
    "012345\r\n"+
    "6789\r\n"+
    "   \r\n" +
    "   01\r\n" +
    "   23\r\n" +
    "   45\r\n" +
    "  67\r\n"+
    "  89\r\n";


var alphabet = 'A'.To('Z') | 'a'.To('z');
var numeric = '0'.To('9')._();
var integer = numeric.Above1.Atom["Integer"];

var alphabets = alphabet.Above1 + alphabet.Lookahead.Not;

Test("987654abcdefg123", alphabets);


// 識別子
var identifier = (( alphabet | '_') + (alphabet | numeric | '_').Above0).Atom["Identifier"];

// 式全般
var exp = new RecursionMatcher()["Exp"];

// エスケープされた１文字
var escapedChar = '\\'._() + ('\r'._() | '\n').Not;
// ダブルクォーテーション
var doubleQuote = '"'._();
// シングルクォーテーション
var singleQuote = '\''._();

// ダブルクォーテーション文字列
var doubleQuoteString = doubleQuote + (escapedChar | (doubleQuote | '\r' | '\n').Not).Above0.Atom + doubleQuote;
// シングルクォーテーション文字列
var singleQuoteString = singleQuote + (escapedChar | (singleQuote | '\r' | '\n').Not).Above0.Atom + singleQuote;

IgnoreState = IgnoreSpaceNewLine;

// フォーマット済み文字列の置換フィールド
// https://docs.python.org/ja/3/reference/lexical_analysis.html#formatted-string-literals
var fExp = ('{' + exp + '}')["FExp"];


IgnoreState = NoIgnore;
// ダブルクォーテーション補完文字列
var doubleQuoteFString = 'f'._() + doubleQuote + (escapedChar | fExp | (doubleQuote | '\r' | '\n').Not).Above0 + doubleQuote;
// シングルクォーテーション補完文字列
var singleQuoteFString = 'f'._() + singleQuote + (escapedChar | fExp | (singleQuote | '\r' | '\n').Not).Above0 + singleQuote;


// 文字列リテラル
var stringliteral = (doubleQuoteString | singleQuoteString | doubleQuoteFString | singleQuoteFString)["StringLiteral"];

IgnoreState = IgnoreSpaceNewLine;

var parenExp = ('(' + exp + ')')["ParenExp"];

// リテラル全般
var literal = integer | stringliteral;

var primeOperand = parenExp | literal | identifier;

var MulDivExp = new OperationMatcher(primeOperand, '*'._() | '/' | '%')["MulDivExp"];
var AddSubExp = new OperationMatcher(MulDivExp, '+'._() | '-')["AddSubExp"];
var ShiftExp = new OperationMatcher(AddSubExp, ">>"._() | "<<")["ShiftExp"];

IgnoreState = NoIgnore;

exp.Inner = ShiftExp | AddSubExp | MulDivExp | parenExp | literal;
//exp.Inner = MulDivExp | parenExp | literal;


//Test("11*22", exp);
//Test("11  *22", exp);
//Test("11*  22", exp);
//Test("11  *  22", exp);

//Test("\"0\"", exp);

Test("f\"{}1+2*3>>4+5*6>>7\">>(_8*9*_)", exp);
Test("f\"{}1+2*3>>4+5*6>>7\" >> ( _8 * 9 * _)", exp);

Test("\"1+2*3>>4+5*6>>7\">>(_8*9*_)", exp);

Test("f\"1+2*3>>{4+5}*6>>7\">>(8*9)", exp);

Test("\"1+2*\\\"3>>4+5*6>>7\">>(8*9)", exp);

Test("\"1+2*\"3>>4+5*6>>7\">>(8*9)", exp);

Test("\"1+2*3\">>4+5*6>>7\">>(8*9)", exp);

Test("\"1+2*'3>>4+5*6>>7\">>(8*9)", exp);

//Test("1+2*3>>4+5*6>>7>>(8*9)", exp);
//Test("1  + 2 *  3>> 4  + 5 * 6 >> 7   >> (  8 * 9  )", exp);
//Test("7   >> (  8 * 9  )", exp);
//Test("6 >> 7   >> (  8 * 9  )", exp);


//Test("6 >> 7   >> (  8  )", exp, 1); // 失敗
//Test("6 >> 7   >>(8)", exp, 3); // 失敗
//Test("6 >> 7   >>8", exp, 4); // 失敗
//Test("6 >> 7>>8", exp, 5); // 失敗
//Test("6 >> 7>> 8", exp, 6); // 失敗

//Test("6>>7>>(8)", exp, 2);
//Test("6>> 7>> 8", exp, 7);
//Test("6>>7>> 8", exp, 8);
//Test("7   >>8", exp, 9);


//Test("6 *7*8", exp, 10); // 失敗


//Test("1+2", exp);


//Test("(3)", exp, 6, 0, 0);


//Test("4*5", exp, 0, 2, 0);
//Test("1*2", exp, 0, 2, 0);

//Test("1*2*3", exp, 2, 0, 0);
//Test("1*2*3*4", exp, 2, 2, 0);
//Test("1*2*3*4*5", exp, 2, 3, 0);

//Test("1+2", exp, 3, 1, 0);
//Test("1+2+3", exp, 3, 2, 0);
//Test("1+2+3+4", exp, 3, 3, 0);
//Test("1+2+3+4+5", exp, 3, 4, 0);

//Test("1+2*3+4/5", exp, 4, 0, 0);


////Test("1+2*3-4+5", AddSubExp, 3, 0, 0);
////Test("1+2*3-4*5", exp, 4, 0, 0);
////Test("1+2*(3-4)*5", exp, 5, 0, 0);
//Test("(3-4)", exp, 6, 0, 0);
//Test("(3-4)+5", exp, 7, 0, 0);
//Test("(4)+5", exp, 7, 2, 0);
//Test("(3-4)*5", exp, 8, 0, 0);
//Test("(4)*5", exp, 8, 2, 0);
//Test("4*5", exp, 9, 0, 0);

//Test("1*2+3>>4*5+6>>7>>(8*9)", exp, 9, 0, 0);

void Test(string text, Matcher matcher, params int[] numbers)
{
    var match = matcher.Match(text);

    var sb = new StringBuilder();
    foreach (var number in numbers)
    {
        if(sb.Length > 0)
        {  sb.Append("-"); }
        sb.Append(number);
    }
    Debug.WriteLine("|-=-=_-=-=|-=-=_-=-=");
    Debug.WriteLine($"Test {sb}");
    match.DebugPrint();
}

