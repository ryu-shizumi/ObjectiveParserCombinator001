using OPC;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using static OPC.IgnoreBlank;
using static OPC.IgnoreBlank.IgnoreStateFlag;

// See https://aka.ms/new-console-template for more information

string text = 
    "012345\r\n"+
    "6789\r\n"+
    "   \r\n" +
    "   01\r\n" +
    "   23\r\n" +
    "   45\r\n" +
    "  67\r\n"+
    "  89\r\n";


var test = "abcd";
var a = 'a'._();
var ab = "ab"._();
var alphabet = 'A'.To('Z') | 'a'.To('z');
var alphabets = alphabet.Above1.Atom;
var number = '0'.To('9')._();
var integer = number.Above1.Atom["Integer"];

var identifier = ( alphabet | '_') + (alphabet | number | '_');

var exp = new RecursionMatcher()["Exp"];

// エスケープされた１文字
var escapedChar = '\\'._() + ('\r'._() | '\n').Not;
// ダブルクォーテーション
var doubleQuote = '"'._();
// ダブルクォーテーション文字列
var doubleQuotestring = doubleQuote + (escapedChar | (doubleQuote | '\r'._() | '\n').Not).Above0.Atom + doubleQuote;
// シングルクォーテーション
var singleQuote = '\''._();
// シングルクォーテーション文字列
var singleQuotestring = singleQuote + (escapedChar | (singleQuote | '\r'._() | '\n').Not).Above0.Atom + singleQuote;

// 文字列リテラル
var stringliteral = doubleQuotestring | singleQuotestring;

IgnoreState = IgnoreSpaceNewLine;

var parenExp = ('(' + exp + ')')["ParenExp"];

var literal = integer | stringliteral;

var primeOperand = parenExp | literal | identifier;

var MulDivExp = new OperationMatcher(primeOperand, '*'._() | '/' | '%')["MulDiv"];
var AddSubExp = new OperationMatcher(MulDivExp, '+'._() | '-')["AddSub"];
var ShiftExp = new OperationMatcher(AddSubExp, ">>"._() | "<<")["Shift"];

IgnoreState = None;

exp.Inner = ShiftExp | AddSubExp | MulDivExp | parenExp | literal;
//exp.Inner = MulDivExp | parenExp | literal;


//Test("11*22", exp);
//Test("11  *22", exp);
//Test("11*  22", exp);
//Test("11  *  22", exp);

Test("\"1+2*3>>4+5*6>>7\">>(8*9)", exp);

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

