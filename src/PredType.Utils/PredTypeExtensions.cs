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

        // n szóig bezárolag (vagyis n-nél kevesebb szóból állókat is kigyűjti)
        public static Sequence[] CollectNGrams(this string line, int n)
        {
            var result = new Dictionary<string, Sequence>();

            // először a biztos kifejezéshatárok mentén darabolok ( | karakter)
            string[] innerStrings = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string innerString in innerStrings)
            {
                string[] words = innerString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries); // note: regular expression could be better...

                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];

                    if (!result.ContainsKey(word))
                        result.Add(word, new Sequence(word, 1));

                    var gramBuilder = new StringBuilder(word);
                    for (int j = i-1; j >= 0 && j >= (i-n+1); j--)
                    {
                        gramBuilder.Insert(0, words[j] + " ");
                        string gram = gramBuilder.ToString();
                        if (!result.ContainsKey(gram))
                        {
                            result.Add(gram, new Sequence(gram, i-j+1));
                        }
                    }
                }
            }

            return result.Values.ToArray();
        }

        // n-es csúszóablakból az összes részhalmazt képzi
        public static Sequence[] CollectAllSubsetsOfN(this string line, int n)
        {
            var result = new Dictionary<string, Sequence>();

            // először a biztos kifejezéshatárok mentén darabolok ( | karakter)
            string[] innerStrings = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string innerString in innerStrings)
            {
                string[] words = innerString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries); // note: regular expression could be better...

                var wordWindow = new List<string>(n);

                for (int i = 0; i < words.Length; i++)
                {
                    wordWindow.Clear();

                    int upperBound = i;
                    int lowerBound = Math.Max(0, i - n + 1);

                    for (int j = lowerBound; j <= upperBound; j++)
                    {
                        wordWindow.Add(words[j]);
                    }

                    // képezni a részhalmazokat és berakni a resultba
                    var subSetList = new List<string>();
                    for (uint bitVector = 1; bitVector < (1u << wordWindow.Count); bitVector++)
                    {
                        subSetList.Clear();
                        
                        for (int j = 0; j < wordWindow.Count; j++)
                        {
                            if ( (bitVector & (1u << j)) != 0 )
                            {
                                subSetList.Add(wordWindow[j]);
                            }
                        }
                        
                        string subSetString = string.Join(" ", subSetList);
                        if (!result.ContainsKey(subSetString))
                        {
                            result.Add(subSetString, new Sequence(subSetString, -1));
                        }
                    }
                }
            }

            return result.Values.ToArray();
        }

        public static Sequence[] CollectPairsAndWords(this string line)
        {
            var result = new Dictionary<string, Sequence>();

            // először a biztos kifejezéshatárok mentén darabolok ( | karakter)
            string[] innerStrings = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string innerString in innerStrings)
            {
                string[] words = innerString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                // collect words
                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];

                    if (!result.ContainsKey(word))
                        result.Add(word, new Sequence(word, 1));
                }

                // collect pairs
                for (int i = 0; i < words.Length - 1; i++)
                {
                    for (int j = i + 1; j < words.Length; j++)
                    {
                        string pair = string.Format("{0} {1}", words[i], words[j]);

                        if (!result.ContainsKey(pair))
                            result.Add(pair, new Sequence(pair, 2));
                    }                    
                }
            }

            return result.Values.ToArray();
        }
    }
}
