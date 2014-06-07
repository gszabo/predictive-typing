using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace BilingPredType
{
    partial class Engine
    {
        public class Trainer
        {
            private readonly string srcPath, trgPath;
            private readonly TrainParams trainParams;
            private readonly BatchTrainParams batchTrainParams;

            public Trainer(string srcPath, string trgPath, TrainParams trainParams)
            {
                this.srcPath = srcPath;
                this.trgPath = trgPath;
                this.trainParams = trainParams;
                this.batchTrainParams = null;
            }

            public Trainer(string srcPath, string trgPath, BatchTrainParams batchTrainParams)
            {
                this.srcPath = srcPath;
                this.trgPath = trgPath;
                this.trainParams = null;
                this.batchTrainParams = batchTrainParams;
            }

            #region train variables

            private int lineCnt;

            private Dictionary<Sequence, int> srcCounter, trgCounter;

            //private Dictionary<SequencePair, int> togetherCounter;
            //private Dictionary<int, int> togetherCounter;
            private Dictionary<int, Dictionary<int, int>> togetherCounter; 

            private Dictionary<Sequence, List<PossibleTranslation>> possibleTranslations;

            private Dictionary<int, string> srcHashes, trgHashes;

            //private Dictionary<int, int> srcCounter, trgCounter; 

            #endregion

            public IEnumerable<Engine> BatchTrain()
            {
                foreach (float monolingThreshold in batchTrainParams.MonolingThresholds)
                {
                    Console.WriteLine("{1}: Egynyelvű szűrés, küszöb: {0}", monolingThreshold, DateTime.Now);
                    
                    // az elején párhuzamosan gyűjtjük a két nyelvből a kifejezéseket
                    Task tSrc, tTrg;

                    tSrc = Task.Factory.StartNew(() =>
                    {
                        using (var srcReader = new StreamReader(srcPath, Encoding.UTF8))
                        {
                            lineCnt = 0;
                            srcCounter = new Dictionary<Sequence, int>();

                            string srcLine;

                            while ((srcLine = srcReader.ReadLine()) != null)
                            {
                                lineCnt++;

                                processLineForSequences(srcLine, srcCounter);
                            }
                        }
                    });

                    // sorokat csak a source oldali számol, elvileg egyenlők mindkét fájlban :)
                    tTrg = Task.Factory.StartNew(() =>
                    {
                        using (var trgReader = new StreamReader(trgPath, Encoding.UTF8))
                        {
                            trgCounter = new Dictionary<Sequence, int>();

                            string trgLine;

                            while ((trgLine = trgReader.ReadLine()) != null)
                            {
                                processLineForSequences(trgLine, trgCounter);
                            }
                        }
                    });

                    Task.WaitAll(tSrc, tTrg);

                    tSrc = tTrg = null;

                    GC.Collect();

                    Console.WriteLine("{0}: Gyűjtés kész, ritkák szűrése", DateTime.Now);

                    filterRare(monolingThreshold);

                    GC.Collect();

                    Console.WriteLine("{0}: Együttes előfordulás számolás elkezdése", DateTime.Now);

                    // együttes előfordulások számolása
                    //togetherCounter = new Dictionary<SequencePair, int>();
                    //togetherCounter = new Dictionary<int, int>();
                    togetherCounter = new Dictionary<int, Dictionary<int, int>>();
                    using (var srcReader = new StreamReader(srcPath, Encoding.UTF8))
                    using (var trgReader = new StreamReader(trgPath, Encoding.UTF8))
                    {
                        string srcLine, trgLine;

                        while ((srcLine = srcReader.ReadLine()) != null &&
                               (trgLine = trgReader.ReadLine()) != null)
                        {
                            processLinePairForCorrelation(srcLine, trgLine);
                        }
                    }

                    Console.WriteLine("{0}: Együttes előfordulások megszámolva, szótárak készítése", DateTime.Now);

                    foreach (float minScore in batchTrainParams.MinScores)
                    {
                        Console.WriteLine("{1}: Szótár készítés, minScore: {0}", minScore, DateTime.Now);

                        possibleTranslations = new Dictionary<Sequence, List<PossibleTranslation>>();

                        createDictionary(minScore);

                        var bilingPredDict = new BilingPredDict();
                        bilingPredDict.ItemCount = (ulong)possibleTranslations.Count;
                        bilingPredDict.DictItems = new List<DictItem>();
                        foreach (var possibleTranslation in possibleTranslations)
                        {
                            List<PossibleTranslation> list = possibleTranslation.Value;
                            list.Sort((translation, translation1) =>
                                translation1.Score.CompareTo(translation.Score));

                            bilingPredDict.DictItems.Add(new DictItem()
                            {
                                SrcHash = possibleTranslation.Key.Text.GetHashCode(),
                                PossibleTranslations = list
                            });
                        }
                        //bilingPredDict.SourceHashResolver = srcHashes;
                        bilingPredDict.TargetHashResolver = trgHashes;

                        yield return new Engine(bilingPredDict, false, monolingThreshold, minScore);
                    }                    
                }                
            }

            public Engine Train()
            {
                // az elején párhuzamosan gyűjtjük a két nyelvből a kifejezéseket
                Task tSrc, tTrg;
                
                tSrc = Task.Factory.StartNew(() =>
                    {
                        using (var srcReader = new StreamReader(srcPath, Encoding.UTF8))
                        {
                            lineCnt = 0;
                            srcCounter = new Dictionary<Sequence, int>();

                            string srcLine;

                            while ((srcLine = srcReader.ReadLine()) != null)
                            {
                                lineCnt++;

                                processLineForSequences(srcLine, srcCounter);
                            }
                        }
                    });

                // sorokat csak a source oldali számol, elvileg egyenlők mindkét fájlban :)
                tTrg = Task.Factory.StartNew(() =>
                    {
                        using (var trgReader = new StreamReader(trgPath, Encoding.UTF8))
                        {
                            trgCounter = new Dictionary<Sequence, int>();

                            string trgLine;

                            while ((trgLine = trgReader.ReadLine()) != null)
                            {
                                processLineForSequences(trgLine, trgCounter);
                            }
                        }
                    });

                Task.WaitAll(tSrc, tTrg);
                
                GC.Collect();

                Console.WriteLine("Gyűjtés kész, ritkák szűrése");

                filterRare(trainParams.MonolingThreshold);

                GC.Collect();

                Console.WriteLine("Együttes előfordulás elkezdése");

                // együttes előfordulások számolása
                //togetherCounter = new Dictionary<SequencePair, int>();
                //togetherCounter = new Dictionary<int, int>();
                togetherCounter = new Dictionary<int, Dictionary<int, int>>();
                using (var srcReader = new StreamReader(srcPath, Encoding.UTF8))
                using (var trgReader = new StreamReader(trgPath, Encoding.UTF8))
                {
                    string srcLine, trgLine;

                    while ((srcLine = srcReader.ReadLine()) != null &&
                           (trgLine = trgReader.ReadLine()) != null)
                    {
                        processLinePairForCorrelation(srcLine, trgLine);
                    }
                }

                possibleTranslations = new Dictionary<Sequence, List<PossibleTranslation>>();

                createDictionary(trainParams.MinScore);

                var bilingPredDict = new BilingPredDict();
                bilingPredDict.ItemCount = (ulong)possibleTranslations.Count;
                bilingPredDict.DictItems = new List<DictItem>();
                foreach (var possibleTranslation in possibleTranslations)
                {
                    List<PossibleTranslation> list = possibleTranslation.Value;
                    list.Sort((translation, translation1) => 
                        translation1.Score.CompareTo(translation.Score));

                    bilingPredDict.DictItems.Add(new DictItem()
                        {
                            SrcHash = possibleTranslation.Key.Text.GetHashCode(),
                            PossibleTranslations = list
                        });
                }
                //bilingPredDict.SourceHashResolver = srcHashes;
                bilingPredDict.TargetHashResolver = trgHashes;
                
                return new Engine(bilingPredDict);
            }

            private void processLineForSequences(string line, Dictionary<Sequence, int> counter)
            {
                Sequence[] sequences = line.CollectNGrams(Program.WordNum);
                //Sequence[] sequences = line.CollectWords();
                foreach (Sequence sequence in sequences)
                {
                    if (!counter.ContainsKey(sequence))
                        counter[sequence] = 1;
                    else
                        counter[sequence]++;
                }
            }

            private void processLinePairForCorrelation(string srcLine, string trgLine)
            {
                Sequence[] srcSequences = srcLine.CollectNGrams(Program.WordNum);
                Sequence[] trgSequences = trgLine.CollectNGrams(Program.WordNum);
                
                foreach (Sequence srcSequence in srcSequences)
                {
                    foreach (Sequence trgSequence in trgSequences)
                    {
                        if (srcCounter.ContainsKey(srcSequence) && trgCounter.ContainsKey(trgSequence))
                        {
                            //unchecked
                            //{
                            //    // hash SequencePair helyett memóriatakarékosságból
                            //    int pairHash = (srcSequence.GetHashCode() * 397) ^ (trgSequence.GetHashCode());
                            //    if (togetherCounter.ContainsKey(pairHash))
                            //        togetherCounter[pairHash]++;
                            //    else
                            //        togetherCounter[pairHash] = 1; 
                            //}
                            
                            // SequencePair-es változat
                            //var pair = new SequencePair(srcSequence, trgSequence);
                            //if (togetherCounter.ContainsKey(pair))
                            //    togetherCounter[pair]++;
                            //else
                            //    togetherCounter[pair] = 1;

                            // dupla hashtáblás változat
                            int srcHash = srcSequence.GetHashCode(), trgHash = trgSequence.GetHashCode();
                            Dictionary<int, int> srcList;
                            if (togetherCounter.TryGetValue(srcHash, out srcList))
                            {
                                if (srcList.ContainsKey(trgHash))
                                {
                                    srcList[trgHash]++;
                                }
                                else
                                {
                                    srcList[trgHash] = 1;
                                }
                            }
                            else
                            {
                                srcList = new Dictionary<int, int>();
                                srcList[trgHash] = 1;
                                togetherCounter.Add(srcHash, srcList);
                            }
                        }
                    }
                }
            }

            // ritkák szűrése egy nyelv szintjén
            private void filterRare(float monolingThreshold)
            {
                if (monolingThreshold > 0.0005f)
                {
                    int minOccur = (int)(lineCnt * monolingThreshold);

                    var removeSrc = srcCounter.Where(pair => pair.Value < minOccur).Select(pair => pair.Key).ToList();
                    foreach (Sequence sequence in removeSrc)
                    {
                        srcCounter.Remove(sequence);
                    }
                    removeSrc = null;

                    var removeTrg = trgCounter.Where(pair => pair.Value < minOccur).Select(pair => pair.Key).ToList();
                    foreach (Sequence sequence in removeTrg)
                    {
                        trgCounter.Remove(sequence);
                    }
                    removeTrg = null;
                }
            }


            private void createDictionary(float minScore)
            {
                srcHashes = new Dictionary<int, string>();
                trgHashes = new Dictionary<int, string>();

                // korreláció számítás
                foreach (Sequence srcSequence in srcCounter.Keys)
                {
                    foreach (Sequence trgSequence in trgCounter.Keys)
                    {
                        int n11;

                        //unchecked
                        //{
                        //    int pairHash = (srcSequence.GetHashCode() * 397) ^ (trgSequence.GetHashCode());
                        //    // ha együtt nem szerepelnek, nem is kerülhetnek a szótárba
                        //    if (!togetherCounter.TryGetValue(pairHash, out n11))
                        //        continue; 
                        //}

                        // SequencePair-es változat
                        // var pair = new SequencePair(srcSequence, trgSequence);
                        // ha együtt nem szerepelnek, nem is kerülhetnek a szótárba
                        //if (!togetherCounter.TryGetValue(pair, out n11))
                        //    continue;

                        // dupla hashtáblás változat
                        int srcHash = srcSequence.GetHashCode(), trgHash = trgSequence.GetHashCode();
                        Dictionary<int, int> srcList;
                        if (!togetherCounter.TryGetValue(srcHash, out srcList))
                        {
                            continue;
                        }

                        if (!srcList.TryGetValue(trgHash, out n11))
                        {
                            continue;
                        }

                        int npp = lineCnt, n1p = srcCounter[srcSequence], np1 = trgCounter[trgSequence];

                        var ct = new ContingencyTable();
                        ct.N11 = n11;
                        ct.N12 = n1p - ct.N11;
                        ct.N21 = np1 - ct.N11;
                        ct.N22 = npp - ct.N11 - ct.N12 - ct.N21;

                        float score = calcScore(ct);

                        if (score > minScore)
                        {
                            //srcHashes[srcSequence.Text.GetHashCode()] = srcSequence.Text;
                            trgHashes[trgSequence.Text.GetHashCode()] = trgSequence.Text;

                            if (!possibleTranslations.ContainsKey(srcSequence))
                                possibleTranslations[srcSequence] = new List<PossibleTranslation>();

                            possibleTranslations[srcSequence].Add(new PossibleTranslation()
                            {
                                Score = score,
                                TranslationHash = trgSequence.Text.GetHashCode()
                            });
                        }
                    }
                }
            }

            private struct ContingencyTable
            {
                public int N11, N12, N21, N22;
            }

            // dice comparator
            private float calcScore(ContingencyTable ct)
            {
                int n1p = ct.N11 + ct.N12;
                int np1 = ct.N11 + ct.N21;

                return 2.0f*ct.N11/(np1 + n1p);
            }

            private class SequencePair
            {
                public Sequence SrcSequence { get; private set; }

                public Sequence TrgSequence { get; private set; }

                public SequencePair(Sequence srcSequence, Sequence trgSequence)
                {
                    SrcSequence = srcSequence;
                    TrgSequence = trgSequence;
                    unchecked
                    {
                        hash = ((SrcSequence != null ? SrcSequence.GetHashCode() : 0) * 397) ^ (TrgSequence != null ? TrgSequence.GetHashCode() : 0);
                    }
                    
                }

                private static Dictionary<int, List<SequencePair>> pairs = new Dictionary<int, List<SequencePair>>();

                public static SequencePair GetOrCreate(Sequence src, Sequence trg)
                {
                    unchecked
                    {
                        int hash = ((src != null ? src.GetHashCode() : 0) * 397) ^ (trg != null ? trg.GetHashCode() : 0);
                        List<SequencePair> pairList;
                        SequencePair result;
                        if (pairs.TryGetValue(hash, out pairList))
                        {
                            result = pairList.SingleOrDefault(p => p.SrcSequence.Equals(src) && p.TrgSequence.Equals(trg));
                            if (result == null)
                            {
                                result = new SequencePair(src, trg);
                                pairList.Add(result);
                            }
                        }
                        else
                        {
                            pairList = new List<SequencePair>();
                            result = new SequencePair(src, trg);
                            pairList.Add(result);
                            pairs.Add(hash, pairList);
                        }

                        return result;
                    }
                    
                }

                public bool Equals(SequencePair other)
                {
                    return SrcSequence.Equals(other.SrcSequence) && TrgSequence.Equals(other.TrgSequence);
                }

                public override bool Equals(object obj)
                {
                    //if (ReferenceEquals(null, obj)) return false;
                    //if (ReferenceEquals(this, obj)) return true;
                    //if (obj.GetType() != this.GetType()) return false;
                    return Equals((SequencePair)obj);
                }

                //private bool hashCalculated;
                private readonly int hash;

                public override int GetHashCode()
                {
                    //if (hashCalculated)
                        return hash;

                    //unchecked
                    //{
                    //    hash = ((SrcSequence != null ? SrcSequence.GetHashCode() : 0) * 397) ^ (TrgSequence != null ? TrgSequence.GetHashCode() : 0);
                    //    hashCalculated = true;
                    //    return hash;
                    //}
                }
            }
        }
    }

    class TrainParams
    {
        public float MonolingThreshold;

        public float MinScore;
    }

    class BatchTrainParams // for each monolingh threshold every minscore
    {
        public float[] MonolingThresholds;

        public float[] MinScores;
    }
}
