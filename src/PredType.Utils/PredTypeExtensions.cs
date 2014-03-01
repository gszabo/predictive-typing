using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredType.Utils
{
    public static class PredTypeExtensions
    {
        public static Sequence[] CollectSequences(this string line, bool toLowerCase = false)
        {
            var result = new Dictionary<string, Sequence>();

            if (toLowerCase)
                line = line.ToLower();

            // először a biztos kifejezéshatárok mentén darabolok ( | karakter)
            string[] innerStrings = line.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);

            foreach (string innerString in innerStrings)
            {
                string[] words = innerString.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries); // note: regular expression could be better...

                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i], bigram = null, trigram = null;

                    if ((i-1) >= 0)
                        bigram = string.Format("{0} {1}", words[i - 1], words[i]);

                    if ((i-2) >= 0)
                        trigram = string.Format("{0} {1} {2}", words[i-2], words[i - 1], words[i]);

                    //var s1 = new Sequence(word, 1);
                    if (!result.ContainsKey(word))
                        result.Add(word, new Sequence(word, 1));

                    if (bigram != null)
                    {
                        //var s2 = new Sequence(bigram, 2);
                        if (!result.ContainsKey(bigram))
                            result.Add(bigram, new Sequence(bigram, 2));
                    }
                    
                    if (trigram != null)
                    {
                        //var s3 = new Sequence(trigram, 3);
                        if (!result.ContainsKey(trigram))
                            result.Add(trigram, new Sequence(trigram, 3));
                    }                    
                }
            }

            return result.Values.ToArray();
        }

        public static Sequence[] CollectWords(this string line, bool toLowerCase = false)
        {
            var result = new Dictionary<string, Sequence>();

            if (toLowerCase)
                line = line.ToLower();

            // először a biztos kifejezéshatárok mentén darabolok ( | karakter)
            string[] innerStrings = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string innerString in innerStrings)
            {
                string[] words = innerString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];

                    if (!result.ContainsKey(word))
                        result.Add(word, new Sequence(word, 1));
                }
            }

            return result.Values.ToArray();
        }
    }
}
