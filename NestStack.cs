using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Parspell
{
    public abstract class Nest
    {
        public static Indent Root = new Indent();

        public int UniqID { get; private set; }

        public Indent? Parent { get; private set; }

        /// <summary>
        /// 字下げされている文字数
        /// </summary>
        public int NestLevel { get; private set; }

        public Nest()
        {
            UniqID = 0;
            Parent = null;
            NestLevel = 0;
        }
        public Nest(Indent parent, int nestLevel)
        {
            var last = parent.LastItem;

            UniqID = last.UniqID +1;
            Parent = parent;
            NestLevel = nestLevel;
        }

        /// <summary>
        /// 自分が最新要素だった時に戻す（自分の後続要素を消去する）
        /// </summary>
        public abstract void Rollback();

        public void DebugOut()
        {
            var nestString = new string(' ', NestLevel);

            if (this is Indent indent)
            {
                Debug.WriteLine($"{nestString}({NestLevel})→[{UniqID}]");
                foreach (var inner in indent.Inners)
                {
                    inner.DebugOut();
                }

                indent.Dedent?.DebugOut();
            }
            else if(this is Dedent)
            {
                Debug.WriteLine($"{nestString}({NestLevel})←[{UniqID}]");
            }

            if(Parent == null)
            {
                Debug.WriteLine("-----------------------------------------------");
            }
        }
    }

    public class Indent : Nest
    {
        public List<Indent> Inners { get; set; } = new List<Indent>();

        public bool IsDedent
        {
            get { return Dedent != null; }
        }

        /// <summary>
        /// デデント
        /// </summary>
        public Dedent? Dedent { get; set; }

        public Indent()
        {
        }

        public Indent(Indent parent, int nest)
            : base(parent, nest)
        {
        }

        /// <summary>
        /// 行頭空白を発見した時の処理
        /// </summary>
        /// <param name="nestLevel"></param>
        public void FindNest(int nestLevel)
        {
            var last = LastItem;

            // 同じネストレベルの時は何もしない
            if(last.NestLevel == nestLevel) { return; }

            if((last.NestLevel == 2) && nestLevel == 4)
            {
                var temp = "";
            }

            // 最終要素がインデントの時
            if (last is Indent indent)
            {
                // インデントを要求された時
                if (last.NestLevel < nestLevel)
                {
                    // 最終要素に、内包要素としてインデントを追加する
                    indent.Inners.Add(new Indent(indent,nestLevel));
                }
                // デデントを要求された時
                else
                {
                    // 最終要素をデデントで閉じる
                    indent.Dedent = new Dedent(indent, nestLevel);
                }

            }
            // 最終要素がデデントの時
            else if( last is Dedent dedent)
            {
                var lastParent = last.Parent.Parent;
                // インデントを要求された時
                if (last.NestLevel < nestLevel)
                {
                    // 親要素に、内包要素としてインデントを追加する
                    lastParent.Inners.Add(new Indent(lastParent, nestLevel));
                }
                // デデントを要求された時
                else
                {
                    // 親要素をデデントで閉じる
                    lastParent.Dedent = new Dedent(lastParent, nestLevel);
                }
            }
        }

        /// <summary>
        /// このインスタンスが属するツリーの最終要素を取得する
        /// </summary>
        public Nest LastItem
        {
            get
            {
                if (Inners.Count == 0) { return this; }
                var lastIndent = Inners[Inners.Count - 1];
                if(lastIndent.Dedent != null)
                {
                    return lastIndent.Dedent;
                }
                return lastIndent.LastItem;
            }
        }

        /// <summary>
        /// デデントの対象となるインデントを取得する
        /// </summary>
        public Indent DedentTarget
        {
            get
            {
                if (Inners.Count == 0) { return this; }
                var lastIndent = Inners[Inners.Count - 1];
                if (lastIndent.Dedent == null)
                {
                    return lastIndent.DedentTarget;
                }
                return this;
            }
        }


        /// <summary>
        /// 自分が最新要素だった時に戻す（自分の後続要素を消去する）
        /// </summary>
        public override void Rollback()
        {
            // 自分と同レベルの後続要素を全て消去する
            Parent?.RemoveInnerFollowing(this);
        }

        /// <summary>
        /// 指定内包要素の後続要素を消去する
        /// </summary>
        public void RemoveInnerFollowing(Indent inner)
        {
            // 指定内包要素の次まで、末尾から順に消していく
            for (int i = Inners.Count - 1; i >= 0; i--)
            {
                if (Inners[i] == inner) { break; }

                Inners[i].Delete();
                Inners.RemoveAt(i);
            }

            Dedent = null;

            // 自分と同レベルの後続要素を全て消去する
            Parent?.RemoveInnerFollowing(this);
        }

        /// <summary>
        /// 内容要素を全て消去する
        /// </summary>
        public void Delete()
        {
            // 内包要素を末尾から順に消していく
            for(int i = Inners.Count - 1; i >= 0; i--)
            {
                Inners[i].Delete();
                Inners.RemoveAt(i);
            }

            // デデントも消去する
            Dedent = null;
        }

        public Indent Root
        {
            get
            {
                if(Parent == null) { return this; }

                Indent? parent = Parent;

                while (parent != null)
                {
                    if (parent.Parent == null)
                    {
                        return parent;
                    }
                    parent = parent.Parent;
                }

                return parent;
            }
        }
    }

    public class Dedent : Nest
    {
        public Dedent(Indent parent ,int nestLevel)
            : base(parent, nestLevel) { }

        /// <summary>
        /// 自分が最新要素だった時に戻す（自分の後続要素を消去する）
        /// </summary>
        public override void Rollback()
        {
            var indent = (Indent)Parent;
            indent?.RemoveInnerFollowing(indent);
        }
    }


    //internal class NestStack 
    //{
    //    private List<Indent> _list = new List<Indent>();

    //    private Stack<Indent> _unpaired = new Stack<Indent>();



    //    public void FindNest(int nest)
    //    {
    //        var currentItem = CurrentItem;
    //        var currentCount = CurrentItem.Count;

    //        if (Current < nest )
    //        {

    //        }

    //        // デデント発生時
    //        else if(nest < Current)
    //        {
    //            int pairIndex;

    //            // 前のアイテムもデデントなら、前のデデントのペアとなるインデントの更に前を取得する
    //            if (currentItem is Dedent Dedent)
    //            {
    //                pairIndex = Dedent.PairIndex - 1;
    //            }
    //            else
    //            {
    //                pairIndex = _list.Count - 1;
    //            }


    //            // var Dedent = new Dedent(nest, )
    //        }
    //    }

    //    private Indent CurrentItem
    //    {
    //        get
    //        {
    //            return _list[_list.Count - 1];
    //        }
    //    }

    //    public int Count
    //    {
    //        get { return _list.Count; }
    //        set
    //        {
    //            var removeCount = _list.Count - value;
    //            _list.RemoveRange(value, removeCount);
    //        }
    //    }

    //    public int Current
    //    {
    //        get
    //        {
    //            var item = _list[_list.Count - 1];
    //            return item.Count;
    //        } 
    //    }

    //    private abstract class Indent
    //    {
    //        public Indent? Prev { get; set; } = null;
    //        public Indent? Next { get; set; } = null;


    //        public int Count;
    //        public Indent(int count)
    //        {
    //            Count = count;
    //        }

    //        public void RemoveNest()
    //        {
    //            if (Next != null)
    //            {
    //                Next.RemoveNest();
    //                Next = null;
    //            }
    //        }
    //    }

    //    private class Indent : Indent
    //    {
    //        public Indent(int count)
    //            :base(count)
    //        { }
    //    }

    //    private class Dedent : Indent
    //    {
    //        public Dedent(int count,int pairIndex)
    //            : base(count)
    //        {
    //            PairIndex = pairIndex;
    //        }

    //        public int PairIndex { get; private set; }
    //    }

    //}
    //internal class NestStackItem
    //{

    //    public int NestLevel { get; private set; }
    //}
}
