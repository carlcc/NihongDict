using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NihongDict
{
    class DictionaryInfo
    {
        private string _name;
        private Int32 _wordCount;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Int32 wordCount
        {
            get { return _wordCount; }
            set { _wordCount = value; }
        }
    }
}
