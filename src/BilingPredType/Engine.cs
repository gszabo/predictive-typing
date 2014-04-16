using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PredType.Utils;

namespace BilingPredType
{
    partial class Engine
    {
        private readonly BilingPredDict dict;

        public BilingPredDict Dictionary { get { return dict; } }

        private Dictionary<int, DictItem> lookupDict; 

        private Engine(BilingPredDict dict)
        {
            this.dict = dict;

            createLookupDict();
        }

        private void createLookupDict()
        {
            lookupDict = new Dictionary<int, DictItem>();
            foreach (DictItem dictItem in dict.DictItems)
            {
                lookupDict.Add(dictItem.SrcHash, dictItem);
            }            
        }

        public static Engine Load(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                IFormatter formatter = new BinaryFormatter();
                var dict = (BilingPredDict) formatter.Deserialize(fs);
                return new Engine(dict);
            }
        }

        public void Save(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, dict);
            }
        }

        public void SaveXml(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(BilingPredDict));
                formatter.Serialize(fs, dict);
            }
        }

        //private LookupTree lookupTree = null;

        //public LookupHit[] Lookup(string srcText, string prefix)
        //{
        //    if (lookupTree == null)
        //    {
        //        lookupTree = new LookupTree(dict);
        //    }

        //    Sequence[] sequences = srcText.CollectSequences();

        //    //var result = new Dictionary<string, LookupHit>();

        //    //foreach (Sequence sequence in sequences)
        //    //{
        //    //    DictItem item;
        //    //    if (lookupDict.TryGetValue(sequence.Text.GetHashCode(), out item))
        //    //    {
        //    //        foreach (PossibleTranslation possibleTranslation in item.PossibleTranslations)
        //    //        {
        //    //            string tr = dict.TargetHashResolver[possibleTranslation.TranslationHash];
        //    //            float score = possibleTranslation.Score;

        //    //            LookupHit lh;
        //    //            if (!result.TryGetValue(tr, out lh))
        //    //            {
        //    //                lh = new LookupHit()
        //    //                    {
        //    //                        GeneratingString = sequence.Text,
        //    //                        TranslationHint = tr,
        //    //                        Score = score
        //    //                    };
        //    //                result.Add(tr, lh);
        //    //            }
        //    //            else if (lh.Score < score)
        //    //            {
        //    //                lh.Score = score;
        //    //                lh.GeneratingString = sequence.Text;
        //    //            }
        //    //        }                    
        //    //    }
        //    //}

        //    //return result.Values.ToArray();

        //    var result = new Dictionary<string, LookupHit>();

        //    foreach (Sequence sequence in sequences)
        //    {
        //        foreach (LookupHit lookupHit in lookupTree.Search(sequence.Text.GetHashCode(), prefix))
        //        {
        //            LookupHit lh;
        //            if (!result.TryGetValue(lookupHit.TranslationHint, out lh))
        //            {
        //                result.Add(lookupHit.TranslationHint, lookupHit);
        //            }
        //            else if (lh.Score < lookupHit.Score)
        //            {
        //                lh.Score = lookupHit.Score;
        //                lh.GeneratingString = sequence.Text;
        //            }
        //        }
        //    }

        //    return result.Values.ToArray();
        //}

        public LookupHit[] Lookup(string srcText)
        {
            Sequence[] sequences = srcText.CollectAllSubsetsOfN(4);
            //Sequence[] sequences = srcText.CollectWords();

            var result = new Dictionary<string, LookupHit>();

            foreach (Sequence sequence in sequences)
            {
                DictItem item;
                if (lookupDict.TryGetValue(sequence.Text.GetHashCode(), out item))
                {
                    foreach (PossibleTranslation possibleTranslation in item.PossibleTranslations)
                    {
                        string tr = dict.TargetHashResolver[possibleTranslation.TranslationHash];
                        float score = possibleTranslation.Score;

                        LookupHit lh;
                        if (!result.TryGetValue(tr, out lh))
                        {
                            lh = new LookupHit()
                                {
                                    GeneratingString = sequence.Text,
                                    TranslationHint = tr,
                                    Score = score
                                };
                            result.Add(tr, lh);
                        }
                        else if (lh.Score < score)
                        {
                            lh.Score = score;
                            lh.GeneratingString = sequence.Text;
                        }
                    }
                }
            }

            return result.Values.ToArray();            
        }        
    }
    
    public class LookupHit
    {
        public string GeneratingString;

        public string TranslationHint;

        public float Score;
    }
}
