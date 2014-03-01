using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolingPredType
{
    [Serializable]
    public class PredDict
    {
        //public ulong WordCount;

        //public List<DictItem> Words;

        //public ulong SentenceCount;

        //public List<DictItem> Sequences;

        public ulong ItemCount;

        public List<DictItem> Items;
    }

    [Serializable]
    public class DictItem
    {
        public string Str;

        public ulong Occurrence;
    }
}
