using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilingPredType
{
    [Serializable]
    public class BilingPredDict
    {
        public ulong ItemCount;

        //public Dictionary<int, string> SourceHashResolver;
        public Dictionary<int, string> TargetHashResolver;

        public List<DictItem> DictItems;
    }
}
