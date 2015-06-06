using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NihongDict.util
{
    class InfoReader
    {
        public static DictionaryInfo readInfo(Stream ifoStrm)
        {
            StreamReader sr = new StreamReader(ifoStrm);
            DictionaryInfo di = new DictionaryInfo();
            di.name = sr.ReadLine();
            di.wordCount = Convert.ToInt32(sr.ReadLine());
            return di;
        }

    }
}
