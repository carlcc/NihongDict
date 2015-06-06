using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NihongDict.util
{
    class DictReader
    {
        private Stream dictStream;

        public DictReader(Stream dctStrm)
        {
            this.dictStream = dctStrm;
        }

        public string getContent(int offset, int length)
        {
            byte[] bytes = new byte[24576];
            int count;
            dictStream.Seek(offset, SeekOrigin.Begin);
            count = dictStream.Read(bytes, 0, length);
            return Encoding.UTF8.GetString(bytes, 0, count);
        }
    }
}
