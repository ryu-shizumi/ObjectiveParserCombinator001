using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    ///  + 演算で空白などを無視する時に使うマッチャーを保持するクラス
    /// </summary>
    public abstract class IgnoreBlank
    {
        /// <summary>
        /// 半角空白かタブ文字の連続の存在を無視できるマッチャー
        /// </summary>
        public static BlankMatcher BlankSpace =
            new BlankMatcher(((' '._() | '\t') * 0.To(int.MaxValue))["Blank"]);

        /// <summary>
        /// Cr か Lf の連続を無視できるマッチャー
        /// </summary>
        public static BlankMatcher BlankNewLine =
            new BlankMatcher((('\r'._() | '\n') * 0.To(int.MaxValue))["Blank"]);

        /// <summary>
        /// 半角空白かタブ文字か Cr か Lf の連続を無視できるマッチャー
        /// </summary>
        public static BlankMatcher BlankSpaceNewline =
            new BlankMatcher(((' '._() | '\t' | ' '._() | '\t') * 0.To(int.MaxValue))["Blank"]);



        public enum IgnoreStateFlag
        {
            /// <summary>
            /// 空白・改行を無視しない
            /// </summary>
            NoIgnore = 0,
            /// <summary>
            /// 空白のみ無視する
            /// </summary>
            IgnoreSpace = 1,
            /// <summary>
            /// 改行のみ無視する
            /// </summary>
            IgnoreNewline = 2,

            /// <summary>
            /// インデント・デデントのみ無視する
            /// </summary>
            IgnoreIndentDedent = 4,
            /// <summary>
            /// 空白・改行共に無視する
            /// </summary>
            IgnoreSpaceNewLine = IgnoreSpace | IgnoreNewline,

            // インデント・デデント・空白・改行の全てを検知する
            //     識別子などの不可分な要素

            //OffsideRule
            // インデント・デデントは検知するが空白・改行は無視できる
            //     Python の普通

            // インデント・デデント・空白・改行は無視できる
            //     C言語など、一般的な言語の普通
        }

        /// <summary>
        /// マッチャーを + 演算子で結合する時に空白・改行を無視するか否かを取得・設定します
        /// </summary>
        public static IgnoreStateFlag IgnoreState { get; set; } = IgnoreStateFlag.NoIgnore;


    }
}
