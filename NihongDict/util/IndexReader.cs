using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NihongDict.util
{
    class IndexReader
    {
        private static int getInt32(Stream indexStream)
        {
            return indexStream.ReadByte() << 24 | indexStream.ReadByte() << 16 |
                indexStream.ReadByte() << 8 | indexStream.ReadByte();
        }

        private static string getString(Stream indexStream)
        {
            byte[] buffer = new byte[512];
            int tmp = 0;
            int i = 0;
            while ((tmp = indexStream.ReadByte()) != 0)
            {
                if (tmp == -1)
                    return null;
                buffer[i++] = (byte)tmp;
            }
            return Encoding.UTF8.GetString(buffer, 0, i);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ndxStrm"></param>
        /// <param name="length">列表的长度，必须正确</param>
        /// <returns></returns>
        public static KeyValuePair<string, int[]>[] makeIndex(Stream ndxStrm, int length)
        {
            string thisWord = null;
            int i = 0;
            KeyValuePair<string, int[]>[] dict = new KeyValuePair<string, int[]>[length];

            ndxStrm.Seek(0, SeekOrigin.Begin);
            while (true)
            {
                thisWord = getString(ndxStrm);
                if (thisWord == null)
                    return dict;
                dict[i++] = new KeyValuePair<string, int[]>(
                    thisWord, new int[2] { getInt32(ndxStrm), getInt32(ndxStrm) }
                );
            }
        }
    }
}
