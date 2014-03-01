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

namespace MonolingPredType
{
    class Engine
    {
        private PredDict dictionary;

        private Engine(PredDict dictionary)
        {
            this.dictionary = dictionary;
        }

        public void Save(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, dictionary);
            }
        }

        public void SaveXml(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(PredDict));
                formatter.Serialize(fs, dictionary);
            }
        }

        public static Engine Load(string path)
        {
            PredDict dict;

            using (FileStream fs = File.OpenRead(path))
            {
                IFormatter formatter = new BinaryFormatter();
                dict = (PredDict) formatter.Deserialize(fs);
            }

            return new Engine(dict);
        }

        // kétszintű "prefix fa", ahol az első két karakter alapján bontódnak szét a stringek
        private Dictionary<char, Dictionary<char, List<DictItem>>> lookupTree = null; 

        private void createLookupTree()
        {
            lookupTree = new Dictionary<char, Dictionary<char, List<DictItem>>>();

            foreach (DictItem dictItem in dictionary.Items)
            {
                char c1 = dictItem.Str[0], c2;
                if (dictItem.Str.Length >= 2)
                {
                    c2 = dictItem.Str[1];
                }
                else
                {
                    c2 = '#'; // ezt úgysem használjuk valódi szóban
                }

                Dictionary<char, List<DictItem>> level2;
                if (!lookupTree.TryGetValue(c1, out level2))
                {
                    level2 = new Dictionary<char, List<DictItem>>();
                    lookupTree.Add(c1, level2);
                }

                List<DictItem> list;
                if (!level2.TryGetValue(c2, out list))
                {
                    list = new List<DictItem>();
                    level2.Add(c2, list);
                }

                list.Add(dictItem);
            }

            foreach (var level2 in lookupTree.Values)
            {
                foreach (var list in level2.Values)
                {
                    list.Sort((item, item2) => item2.Occurrence.CompareTo(item.Occurrence));
                }
            }
        }

        private List<DictItem> getListFromLookupTree(string prefix)
        {
            // feltesszük hogy prefix legalább 1 hosszú
            char c1 = prefix[0], c2;
            Dictionary<char, List<DictItem>> level2;

            if (prefix.Length == 1)
            {
                if (!lookupTree.TryGetValue(c1, out level2))
                {
                    return null;
                }

                return level2.Values.Cast<IEnumerable<DictItem>>().Aggregate((items1, items2) => items1.Union(items2)).ToList();
            }
            
            c2 = prefix[1];
            
            if (!lookupTree.TryGetValue(c1, out level2))
            {
                return null;
            }

            List<DictItem> list;
            if (!level2.TryGetValue(c2, out list))
            {
                return null;
            }

            return list;
        }

        //public string[] Complete(string prefix, int numCandidateWords, int numCandidateSequences)
        public string[] Complete(string prefix, int listLength)
        {
            //string[] result;

            //if (lookupCache.TryGetValue(prefix, out result))
            //    return result;
            
            //const int numCandidateWords = 5;
            //const int numCandidateSequences = 5;

            //List<string> candidateWords = dictionary.Words.Where(item => item.Str.StartsWith(prefix))
            //                                              .OrderByDescending(item => item.Occurrence)
            //                                              .Select(item => item.Str)
            //                                              .Take(numCandidateWords)
            //                                              .ToList();

            //List<string> candidateSequences = dictionary.Sequences.Where(item => item.Str.StartsWith(prefix))
            //                                                      .OrderByDescending(item => item.Occurrence)
            //                                                      .Select(item => item.Str)
            //                                                      .Take(numCandidateSequences)
            //                                                      .ToList();

            //result = candidateWords.Union(candidateSequences).ToArray();

            //lookupCache.Add(prefix, result);

            if (lookupTree == null)
                createLookupTree();

            var list = getListFromLookupTree(prefix);
            if (list == null)
                return new string[0];

            var result = list.Where(item => item.Str.StartsWith(prefix))
                             .OrderByDescending(item => item.Occurrence)
                             .Select(item => item.Str)
                             .Take(listLength)
                             .ToArray();

            return result;
        }

        public bool IsWordInDict(string word)
        {
            if (lookupTree == null)
                createLookupTree();

            var list = getListFromLookupTree(word);
            if (list == null)
                return false;
            
            return list.Any(w => w.Str == word);
        }

        public class Trainer
        {
            public class TrainParams
            {
                public float MinThreshold;
            }


            private readonly string trainPath;
            private readonly TrainParams parameters;

            // private ulong wordCount, sentenceCount;
            private ulong sentenceCount;

            //private Dictionary<int, string> hashResolver;

            //private Dictionary<string, ulong> wordCounters, seqCounters; 
            private Dictionary<string, ulong> counters;
        
            public Trainer(string trainPath, TrainParams parameters)
            {
                this.trainPath = trainPath;
                this.parameters = parameters;
            }

            public Engine Train()
            {
                init();

                using (var rdr = new StreamReader(trainPath, Encoding.UTF8))
                {
                    string line;
                    while ((line = rdr.ReadLine()) != null)
                    {
                        sentenceCount++;

                        processLine(line);
                    }
                }

                filter();

                return createEngine();
            }

            private void init()
            {
                //wordCount = sentenceCount = 0;
                //wordCounters = new Dictionary<string, ulong>();
                //seqCounters = new Dictionary<string, ulong>();
                //hashResolver = new Dictionary<int, string>();
                sentenceCount = 0;
                counters = new Dictionary<string, ulong>();
            }

            private void processLine(string line)
            {
                //// tokenizált a szöveg, elég a space-ek mentén darabolni
                //string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                //for (int i = 0; i < words.Length; i++)
                //{
                //    if (isBoundaryWord(words[i]))
                //        continue;

                //    //wordCount++;

                //    // szavak gyakoriságát gyűjteni
                //    string word = words[i];
                //    increaseOccurence(word, counters);

                //    // bigram és trigram gyakoriságot gyűjteni, a bennük levő egyik szó sem lehet határoló
                //    // bigram
                //    if (i >= 1 && !isBoundaryWord(words[i-1]))
                //    {
                //        string bigram = string.Format("{0} {1}", words[i - 1], words[i]);
                //        increaseOccurence(bigram, counters);
                //    }
                //    // trigram
                //    if (i >= 2 && !isBoundaryWord(words[i-1]) && !isBoundaryWord(words[i-2]))
                //    {
                //        string trigram = string.Format("{0} {1} {2}", words[i - 2], words[i - 1], words[i]);
                //        increaseOccurence(trigram, counters);
                //    }
                //}
                Sequence[] sequences = line.CollectSequences(true);

                foreach (Sequence sequence in sequences)
                {
                    increaseOccurence(sequence.Text, counters, 1UL);
                }

            }

            private bool isBoundaryWord(string word)
            {
                char[] sequenceBorder = { '\'', '"', '.', ',', '(', ')', '!', '?', ';', '%' };

                return (word.Length == 1 && word.IndexOfAny(sequenceBorder) >= 0);
            }

            private void filter()
            {
                //filterWords();

                //filterSequences();

                // most nincs megkülönböztetés szó és bi-trigram között, egyformán szűrünk
                ulong minOccur = (ulong)(sentenceCount * parameters.MinThreshold);

                var dropOutKeys = counters.Where(pair => pair.Value < minOccur).Select(pair => pair.Key).ToList();

                foreach (string dropOutKey in dropOutKeys)
                {
                    counters.Remove(dropOutKey);
                }

                dropOutKeys = null;
            }

            //private void filterWords()
            //{
            //    //const float rareWordFreq = 0.15f;

            //    ulong wordThreshold = (ulong)(wordCount * (1.0f - parameters.RareWordFrequency));

            //    List<KeyValuePair<string, ulong>> wordOccurrences = wordCounters.ToList();
            //    wordOccurrences.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            //    ulong wordSum = 0;
            //    int i;

            //    for (i = 0; i < wordOccurrences.Count && wordSum <= wordThreshold; i++)
            //    {
            //        wordSum += wordOccurrences[i].Value;
            //    }

            //    if (i < wordOccurrences.Count)
            //        wordOccurrences.RemoveRange(i, wordOccurrences.Count - i);

            //    wordCounters = wordOccurrences.ToDictionary(pair => pair.Key, pair => pair.Value);
            //    wordOccurrences = null;
            //}

            //private void filterSequences()
            //{
            //    //const float seqPenetrationRatio = 0.01f;

            //    ulong minOccur = (ulong) (sentenceCount * parameters.SequenceFrequencyThreshold);

            //    var dropOutKeys = seqCounters.Where(pair => pair.Value < minOccur).Select(pair => pair.Key).ToList();

            //    foreach (string dropOutKey in dropOutKeys)
            //    {
            //        seqCounters.Remove(dropOutKey);
            //    }

            //    dropOutKeys = null;
            //}

            private void increaseOccurence(string str, Dictionary<string, ulong> counter)
            {
                //string s;
                //int hash = str.GetHashCode();
                
                //if (hashResolver.TryGetValue(hash, out s))
                //{
                //    if (s != str)
                //        throw new Exception("Hash ütközés: (" + s + "; " + str + ")");

                //    counter[hash]++;
                //}
                //else
                //{
                //    hashResolver.Add(hash, str);
                //    counter[hash] = 1;
                //}

                if (counter.ContainsKey(str))
                    counter[str]++;
                else
                    counter[str] = 1;
            }

            private void increaseOccurence(string str, Dictionary<string, ulong> counter, ulong step)
            {
                if (counter.ContainsKey(str))
                    counter[str] += step;
                else
                    counter[str] = step;
            }

            private Engine createEngine()
            {
                PredDict dict = new PredDict();

                //dict.WordCount = wordCount;
                //dict.SentenceCount = sentenceCount;

                //dict.Words = wordCounters.Select(pair =>
                //                                 new DictItem()
                //                                     {
                //                                         Occurrence = pair.Value,
                //                                         Str = pair.Key
                //                                     }).ToList();

                //dict.Sequences = seqCounters.Select(pair =>
                //                                 new DictItem()
                //                                 {
                //                                     Occurrence = pair.Value,
                //                                     Str = pair.Key
                //                                 }).ToList();

                //wordCounters = seqCounters =  null;

                dict.ItemCount = (ulong)counters.Count;
                dict.Items = counters.Select(pair =>
                                                 new DictItem()
                                                 {
                                                     Occurrence = pair.Key.Count(char.IsWhiteSpace) > 0 ? pair.Value * (ulong)pair.Key.Length : pair.Value,
                                                     Str = pair.Key
                                                 }).ToList();

                return new Engine(dict);
            }
        }
    }
}
