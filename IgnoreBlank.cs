using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    /// このクラスのインスタンスが存在する間は + 演算で空白を無視するようにする
    /// </summary>
    public abstract class IgnoreBlank
    {
        public static BlankMatcher BlankSpace =
            new BlankMatcher(((' '._() | '\t') * 0.To(int.MaxValue))["Blank"]);
        public static BlankMatcher BlankNewLine =
            new BlankMatcher((('\r'._() | '\n') * 0.To(int.MaxValue))["Blank"]);
        public static BlankMatcher BlankSpaceNewline =
            new BlankMatcher(((' '._() | '\t' | ' '._() | '\t') * 0.To(int.MaxValue))["Blank"]);



        public enum IgnoreStateFlag
        {
            /// <summary>
            /// 空白・改行を無視しない
            /// </summary>
            None = 0,
            /// <summary>
            /// 空白のみ無視する
            /// </summary>
            IgnoreSpace = 1,
            /// <summary>
            /// 改行のみ無視する
            /// </summary>
            IgnoreNewline = 2,
            /// <summary>
            /// 空白・改行共に無視する
            /// </summary>
            IgnoreSpaceNewLine = IgnoreSpace | IgnoreNewline,
        }

        /// <summary>
        /// 空白・改行を無視するか否かを取得・設定します
        /// </summary>
        public static IgnoreStateFlag IgnoreState { get; set; } = IgnoreStateFlag.None;


    }
}
