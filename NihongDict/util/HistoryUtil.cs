using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NihongDict.util
{
    class HistoryUtil
    {
        private string path = "dictionarys/history.dat";
        private int capacity;

        private Queue<string> hstr;

        public HistoryUtil(int historySize)
        {
            capacity = historySize;
            hstr = new Queue<string>(historySize);
        }

        public void loadHistory()
        {
            string tmp = null;

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);

            StreamReader hstrStrmRdr = new StreamReader(fs);
            while ((tmp = hstrStrmRdr.ReadLine()) != null)
            {
                hstr.Enqueue(tmp);
            }
            hstrStrmRdr.Close();
            fs.Close();
        }

        public void storeHistory()
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            foreach (string s in hstr)
            {
                sw.WriteLine(s);
            }
            sw.Close();
            fs.Close();
        }

        public void newHistory(string str)
        {
            if (hstr.Count == capacity)
                hstr.Dequeue();
            hstr.Enqueue(str);
        }

        /// <summary>
        /// 队列长度
        /// </summary>
        public int length
        {
            get { return this.hstr.Count; }
        }

        /// <summary>
        /// 返回存储历史的队列的副本
        /// </summary>
        public Queue<string> bufferQueue
        {
            get { return new Queue<string>(this.hstr); }
        }

        public void clear()
        {
            this.hstr.Clear();
        }
    }
}
