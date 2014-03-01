using System;
using System.Collections.Generic;
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

            public Trainer(string srcPath, string trgPath, TrainParams trainParams)
            {
                this.srcPath = srcPath;
                this.trgPath = trgPath;
                this.trainParams = trainParams;
            }

            #region train variables

            private int lineCnt;

            private Dictionary<Sequence, int> srcCounter, trgCounter;

            private Dictionary<SequencePair, int> togetherCounter;

            private Dictionary<Sequence, List<PossibleTranslation>> possibleTranslations;

            private Dictionary<int, string> srcHashes, trgHashes; 

            #endregion

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
                
                //using (var srcReader = new StreamReader(srcPath, Encoding.UTF8))
                //using (var trgReader = new StreamReader(trgPath, Encoding.UTF8))
                //{
                //    lineCnt = 0;
                //    srcCounter = new Dictionary<Sequence, int>();
                //    trgCounter = new Dictionary<Sequence, int>();
                //    togetherCounter = new Dictionary<SequencePair, int>();

                //    string srcLine, trgLine;

                //    while ((srcLine = srcReader.ReadLine()) != null &&
                //           (trgLine = trgReader.ReadLine()) != null)
                //    {
                //        lineCnt++;

                //        processLinePairForSequences(srcLine, trgLine);
                //    }
                //}


                filterRare();

                // együttes előfordulások számolása
                togetherCounter = new Dictionary<SequencePair, int>();
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

                createDictionary();

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
                //Sequence[] sequences = line.CollectSequences();
                Sequence[] sequences = line.CollectWords();
                foreach (Sequence sequence in sequences)
                {
                    if (!counter.ContainsKey(sequence))
                        counter[sequence] = 1;
                    else
                        counter[sequence]++;
                }
            }

            //private void processLinePairForSequences(string srcLine, string trgLine)
            //{
            //    Task tSrc, tTrg;

            //    tSrc = Task.Factory.StartNew(() =>
            //        {
            //            Sequence[] srcSequences = srcLine.CollectSequences();
            //            foreach (Sequence sequence in srcSequences)
            //            {
            //                if (!srcCounter.ContainsKey(sequence))
            //                    srcCounter[sequence] = 1;
            //                else
            //                    srcCounter[sequence]++;
            //            }
            //        });

            //    tTrg = Task.Factory.StartNew(() =>
            //    {
            //        Sequence[] trgSequences = trgLine.CollectSequences();
            //        foreach (Sequence sequence in trgSequences)
            //        {
            //            if (!trgCounter.ContainsKey(sequence))
            //                trgCounter[sequence] = 1;
            //            else
            //                trgCounter[sequence]++;
            //        }
            //    });

            //    Task.WaitAll(tSrc, tTrg);
            //}

            private void processLinePairForCorrelation(string srcLine, string trgLine)
            {
                //Sequence[] srcSequences = srcLine.CollectSequences();
                //Sequence[] trgSequences = trgLine.CollectSequences();
                Sequence[] srcSequences = srcLine.CollectWords();
                Sequence[] trgSequences = trgLine.CollectWords();

                foreach (Sequence srcSequence in srcSequences)
                {
                    foreach (Sequence trgSequence in trgSequences)
                    {
                        if (srcCounter.ContainsKey(srcSequence) && trgCounter.ContainsKey(trgSequence))
                        {
                            SequencePair pair = new SequencePair(srcSequence, trgSequence);
                            if (togetherCounter.ContainsKey(pair))
                                togetherCounter[pair]++;
                            else
                                togetherCounter[pair] = 1;
                        }
                    }
                }
            }

            // ritkák szűrése
            private void filterRare()
            {
                if (trainParams.MonolingThreshold > 0.0005f)
                {
                    int minOccur = (int)(lineCnt * trainParams.MonolingThreshold);

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


            private void createDictionary()
            {
                srcHashes = new Dictionary<int, string>();
                trgHashes = new Dictionary<int, string>();

                // korreláció számítás
                foreach (Sequence srcSequence in srcCounter.Keys)
                {
                    foreach (Sequence trgSequence in trgCounter.Keys)
                    {
                        var pair = new SequencePair(srcSequence, trgSequence);

                        int n11;

                        // ha együtt nem szerepelnek, nem is kerülhetnek a szótárba
                        if (!togetherCounter.TryGetValue(pair, out n11))
                            continue;

                        int npp = lineCnt, n1p = srcCounter[srcSequence], np1 = trgCounter[trgSequence];

                        var ct = new ContingencyTable();
                        ct.N11 = n11;
                        ct.N12 = n1p - ct.N11;
                        ct.N21 = np1 - ct.N11;
                        ct.N22 = npp - ct.N11 - ct.N12 - ct.N21;

                        float score = calcScore(ct);

                        if (score > trainParams.MinScore)
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
                }

                protected bool Equals(SequencePair other)
                {
                    return SrcSequence.Equals(other.SrcSequence) && TrgSequence.Equals(other.TrgSequence);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    //if (obj.GetType() != this.GetType()) return false;
                    return Equals((SequencePair)obj);
                }

                private bool hashCalculated = false;
                private int hash;

                public override int GetHashCode()
                {
                    if (hashCalculated)
                        return hash;

                    unchecked
                    {
                        hash = ((SrcSequence != null ? SrcSequence.GetHashCode() : 0) * 397) ^ (TrgSequence != null ? TrgSequence.GetHashCode() : 0);
                        hashCalculated = true;
                        return hash;
                    }
                }
            }
        }
    }

    class TrainParams
    {
        public float MonolingThreshold;

        public float MinScore;
    }
}
