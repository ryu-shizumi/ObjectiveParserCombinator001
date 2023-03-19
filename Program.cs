using OPC;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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
var alphabet = 'a'.To('z')._();
var alphabets = alphabet * 1.To(5);


var number = '0'.To('9');
var integer = (number * 1.To(999)).Atom["Integer"];
var literal = integer;

var exp = new RecursionMatcher()["Exp"];
var parenExp = ('(' + exp + ')')["ParenExp"];

var primeOperand = parenExp | literal;

var MulDivExp = new OperationMatcher(primeOperand, '*'._() | '/' | '%')["MulDiv"];
var AddSubExp = new OperationMatcher(MulDivExp, '+'._() | '-')["AddSub"];
var ShiftExp = new OperationMatcher(AddSubExp, ">>"._() | "<<")["Shift"];

exp.Inner = ShiftExp | AddSubExp | MulDivExp | parenExp | literal;

Test("1*2+3>>4*5+6>>7>>(8*9)", exp);

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

