using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilingPredType
{
    [Serializable]
    public class DictItem
    {
        //public string SrcSequence;
        public int SrcHash;

        public List<PossibleTranslation> PossibleTranslations;
    }
}
