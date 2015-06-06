using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NihongDict.util
{
    class Dictionary
    {
        private DictReader dr;
        private DictionaryInfo info;
        private KeyValuePair<string, int[]>[] wordMap;

        public delegate void OnWordReferencedEvent(string word);
        private OnWordReferencedEvent _onWordReferenced;
        public OnWordReferencedEvent onWordReferenced
        {
            private get { return this._onWordReferenced; }
            set { this._onWordReferenced = value; }
        }
        
        public Dictionary(string dictPath, string indexPath, string infoPath)
        {
            info = InfoReader.readInfo(new FileStream(infoPath, FileMode.Open, FileAccess.Read));
            dr = new DictReader(new FileStream(dictPath, FileMode.Open, FileAccess.Read));
            wordMap = IndexReader.makeIndex(new FileStream(indexPath, FileMode.Open, FileAccess.Read), info.wordCount);
        }

        public KeyValuePair<string, int[]> this[int index]
        {
            get { return wordMap[index]; }
        }

        public Int32 length
        {
            get { return this.wordMap.Length; }
        }

        private static int stringCompare(string a, string b)
        {
            int i = 0;
            while (i < a.Length && i < b.Length)
            {
                if (a[i] != b[i])
                    return (int)(a[i] - b[i]);
                ++i;
            }
            if (a.Length > b.Length)
                return a[i];
            else if (a.Length < b.Length)
                return -b[i];
            return 0;
        }


        public Int32 getWordIndex(string word)
        {
            int start = 0, end = this.length - 1, mid = 0;
            int tmp;

            // 二分查找单词缩在位置
            // 使用等号，方便后面一个while循环统一操作
            while (start <= end)
            {
                mid = (start + end) / 2;
                tmp = stringCompare(word, wordMap[mid].Key);
                if (tmp == 0)
                {
                    break;
                }
                if (tmp > 0)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }

            if (mid < start)
            {
                // 更准确说，是mid == start-1
                // 说明没有匹配的单词，并且mid所指向的单词的字典序小于输入的单词，第一条应该显示start指向的单词
                return start;
            }
            while (mid > 0 && stringCompare(word, wordMap[mid - 1].Key) == 0)
                mid--;

            return mid;
        }

        public string getContent(int index)
        {
            if (this.onWordReferenced != null)
                this.onWordReferenced(wordMap[index].Key);

            return dr.getContent(wordMap[index].Value[0], wordMap[index].Value[1]);
        }
    }
}
