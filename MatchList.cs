using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parspell
{
    /// <summary>
    /// マッチ結果をキャッシュしておくリスト
    /// </summary>
    public class MatchList
    {
        private Dictionary<int, Dictionary<Matcher, Match>> _dict =
            new Dictionary<int, Dictionary<Matcher, Match>>();

        public bool ContainsKey(int index, Matcher matcher)
        {
            if (_dict.ContainsKey(index) == false) { return false; }
            if (_dict[index].ContainsKey(matcher) == false) { return false; }
            return true;
        }
        public void RemoveKey(int index, Matcher matcher)
        {
            if (_dict.ContainsKey(index) == false) { return; }
            if (_dict[index].ContainsKey(matcher) == false) { return; }
            _dict[index].Remove(matcher);
        }

        public Match this[int index, Matcher matcher]
        {
            get
            {
                return _dict[index][matcher];
            }
            set
            {
                Dictionary<Matcher, Match> innerDict;

                if (_dict.ContainsKey(index))
                {
                    innerDict = _dict[index];
                }
                else
                {
                    innerDict = new Dictionary<Matcher, Match>();
                    _dict.Add(index, innerDict);
                }

                if (innerDict.ContainsKey(matcher))
                {
                    innerDict[matcher] = value;
                }
                else
                {
                    innerDict.Add(matcher, value);
                }
                

            }
        }
    }
    /// <summary>
    /// 選択マッチャーのマッチ結果を、マッチャーとインデックスの組毎にキャッシュしておくリスト
    /// </summary>
    public class MatchEnumList
    {
        private Dictionary<int, Dictionary<Matcher, List<Match>>> _dict =
            new Dictionary<int, Dictionary<Matcher, List<Match>>>();

        public bool ContainsKey(int index, Matcher matcher)
        {
            if (_dict.ContainsKey(index) == false) { return false; }
            if (_dict[index].ContainsKey(matcher) == false) { return false; }
            return true;
        }

        public List<Match> this[int index, Matcher matcher]
        {
            get
            {
                return _dict[index][matcher];
            }
            set
            {
                Dictionary<Matcher, List<Match>> innerDict;

                if (_dict.ContainsKey(index))
                {
                    innerDict = _dict[index];
                }
                else
                {
                    innerDict = new Dictionary<Matcher, List<Match>>();
                    _dict.Add(index, innerDict);
                }

                if (innerDict.ContainsKey(matcher))
                {
                    throw new InvalidOperationException();
                }
                innerDict[matcher] = value;
            }
        }
    }

    /// <summary>
    /// ループマッチャーのマッチ結果を、マッチャーとインデックスの組毎にキャッシュしておくリスト
    /// </summary>
    public class MatchLoopList
    {
        private Dictionary<int, Dictionary<Matcher, LoopMatchResults>> _dict =
            new Dictionary<int, Dictionary<Matcher, LoopMatchResults>>();

        public bool ContainsKey(int index, Matcher matcher)
        {
            if (_dict.ContainsKey(index) == false) { return false; }
            if (_dict[index].ContainsKey(matcher) == false) { return false; }
            return true;
        }

        public LoopMatchResults this[int index, Matcher matcher]
        {
            get
            {
                return _dict[index][matcher];
            }
            set
            {
                Dictionary<Matcher, LoopMatchResults> innerDict;

                if (_dict.ContainsKey(index))
                {
                    innerDict = _dict[index];
                }
                else
                {
                    innerDict = new Dictionary<Matcher, LoopMatchResults>();
                    _dict.Add(index, innerDict);
                }

                if (innerDict.ContainsKey(matcher))
                {
                    throw new InvalidOperationException();
                }
                innerDict[matcher] = value;
            }
        }
    }

    /// <summary>
    /// ループマッチャーの返信を長さ順に保持するリスト
    /// </summary>
    /// <remarks>
    /// ソート関数に基づく長さが優先。長さが同じなら子要素から Add された順番。
    /// </remarks>
    public class LoopMatchResults : IEnumerable<Match>
    {
        private ModernSortedDictionary<int, List<Match>> _dict;

        /// <summary>
        /// 小から大の順に列挙させる為の比較関数
        /// </summary>
        public static readonly Func<int, int, int> SmallToLarge
            = (a, b) => a - b;

        /// <summary>
        /// 大から小の順に列挙させる為の比較関数
        /// </summary>
        public static readonly Func<int, int, int> LargeToSmall
            = (a, b) => b - a;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="compFunc">ソート順を決める比較関数</param>
        public LoopMatchResults(Func<int, int, int> compFunc)
        {
            _dict = new ModernSortedDictionary<int, List<Match>>(compFunc);
        }

        public void Add(Match match)
        {
            int textLength = match.TextLength;
            List<Match> stack;

            if (_dict.ContainsKey(textLength))
            {
                stack = _dict[textLength];
            }
            else
            {
                stack = new List<Match>();
                _dict.Add(textLength, stack);
            }

            stack.Add(match);
        }

        /// <summary>
        /// 長さの順に列挙する。長さが同じなら Add された順番を優先して列挙する。
        /// </summary>
        /// <returns>格納している要素</returns>
        public IEnumerator<Match> GetEnumerator()
        {
            foreach (var pair in _dict)
            {
                foreach (var match in pair.Value)
                {
                    yield return match;
                }
            }
        }

        /// <summary>
        /// 長さの順に列挙する。長さが同じなら Add された順番を優先して列挙する。
        /// </summary>
        /// <returns>格納している要素</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var pair in _dict)
            {
                foreach (var match in pair.Value)
                {
                    yield return match;
                }
            }
        }
    }
}
