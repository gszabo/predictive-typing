using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace BilingPredType
{
    class LookupTree
    {
        private Dictionary<char, Dictionary<char, List<TreeItem>>> root;

        public LookupTree(BilingPredDict dict)
        {
            root = new Dictionary<char, Dictionary<char, List<TreeItem>>>();

            foreach (var dictItem in dict.DictItems)
            {
                putIntoTree(dictItem, dict);
            }

        }

        private void putIntoTree(DictItem item, BilingPredDict dict)
        {
            foreach (PossibleTranslation possibleTranslation in item.PossibleTranslations)
            {
                string trHint = dict.TargetHashResolver[possibleTranslation.TranslationHash];
                List<TreeItem> list = makeList(trHint);
                list.Add(new TreeItem()
                    {
                        Score = possibleTranslation.Score,
                        SrcHash = item.SrcHash,
                        TranslationHint = trHint
                    });
            }
            
        }

        //private void sort()
        //{
            
        //}

        // létrehozza a listát a prefixnek ha kell
        private List<TreeItem> makeList(string translationString)
        {
            char c1 = translationString[0], c2;
            if (translationString.Length >= 2)
            {
                c2 = translationString[1];
            }
            else
            {
                c2 = '#'; // ezt úgysem használjuk valódi szóban
            }

            Dictionary<char, List<TreeItem>> level2;
            if (!root.TryGetValue(c1, out level2))
            {
                level2 = new Dictionary<char, List<TreeItem>>();
                root.Add(c1, level2);
            }

            List<TreeItem> list;
            if (!level2.TryGetValue(c2, out list))
            {
                list = new List<TreeItem>();
                level2.Add(c2, list);
            }

            return list;
        }

        private List<TreeItem> getListForPrefix(string prefix)
        {
            // feltesszük hogy prefix legalább 1 hosszú
            char c1 = prefix[0], c2;
            Dictionary<char, List<TreeItem>> level2;

            if (prefix.Length == 1)
            {
                if (!root.TryGetValue(c1, out level2))
                {
                    return new List<TreeItem>(); ;
                }

                return level2.Values.Cast<IEnumerable<TreeItem>>().Aggregate((items1, items2) => items1.Union(items2)).ToList();
            }

            c2 = prefix[1];

            if (!root.TryGetValue(c1, out level2))
            {
                return new List<TreeItem>();
            }

            List<TreeItem> list;
            if (!level2.TryGetValue(c2, out list))
            {
                return new List<TreeItem>();
            }

            return list;
        }

        public List<LookupHit> Search(int srcHash, string prefix)
        {
            return getListForPrefix(prefix).Where(ti => ti.SrcHash == srcHash).Select(ti => new LookupHit()
                {
                    Score = ti.Score,
                    TranslationHint = ti.TranslationHint
                }).ToList();
        }
    }

    class TreeItem
    {
        public int SrcHash;

        public string TranslationHint;

        public float Score;
    }
}
