using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilingPredType
{
    [Serializable]
    public class PossibleTranslation
    {
        //public string Translation;
        public int TranslationHash;

        public float Score;
    }
}
