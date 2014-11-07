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
    public class BatchTrainParams
    {
        public readonly string TrainPath;

        public readonly float[] MonolingThresholds;

        // for each monolingual threshold every minscore
        public readonly float[] MinScores;

        public readonly Func<string, Sequence[]> TextUnitFunc;

        public readonly string[] ScoreCalculators;

        public readonly string[] CalculatorParams;

        public BatchTrainParams(string trainPath, float[] monolingThresholds, float[] minScores,
            Func<string, Sequence[]> textUnitFunc, string[] scoreCalculators, string[] calculatorParams)
        {
            TrainPath = trainPath;
            MonolingThresholds = monolingThresholds;
            MinScores = minScores;
            TextUnitFunc = textUnitFunc;
            ScoreCalculators = scoreCalculators;
            CalculatorParams = calculatorParams;
        }
    }

    public class EngineTrainResult
    {
        public float MinThreshold { get; set; }

        public float MinScore { get; set; }
        
        public string ScoreCalculator { get; set; }

        //public TimeSpan TrainTime { get; set; }

        public ulong SourceEntryNum { get; set; }

        public ulong AllEntryNum { get; set; }

        public Engine Engine { get; set; }
    }

    public class BatchTrainer
    {
        private readonly BatchTrainParams trainParams;

        public BatchTrainer(BatchTrainParams trainParams)
        {
            this.trainParams = trainParams;
        }

        public IEnumerable<EngineTrainResult> Train(Logger logger)
        {
            int lineCnt;
            var srcCounter = new Dictionary<Sequence, int>();
            var trgCounter = new Dictionary<Sequence, int>();
            var togetherCounter = new Dictionary<int, Dictionary<int, int>>();

            var sw = new Stopwatch();

            foreach (float monolingThreshold in trainParams.MonolingThresholds)
            {
                sw.Start();
                logger.Log("Collecting monolingual text units from: " + trainParams.TrainPath);

                using (var rdr = new StreamReader(trainParams.TrainPath, Encoding.UTF8))
                {
                    lineCnt = 0;

                    string srcSentence = null, trgSentence = null, errorMsg = null;
                    bool endOfFile = false, quit = false;

                    while (!quit)
                    {
                        quit = LinePairReader.ReadLinePair(rdr, ref srcSentence, ref trgSentence, ref errorMsg, ref endOfFile);

                        if (!quit)
                        {
                            lineCnt++;

                            addSequencesToCounter(trainParams.TextUnitFunc(srcSentence), srcCounter);
                            addSequencesToCounter(trainParams.TextUnitFunc(srcSentence), trgCounter);
                        }
                        else if (errorMsg != null)
                        {
                            logger.Log("Error in file.");
                        }
                    }
                }

                logger.Log("Collecting done.");

                logger.Log("Filtering for both languages with threshold: " + monolingThreshold);

                filterCounter(srcCounter, monolingThreshold, lineCnt);
                filterCounter(trgCounter, monolingThreshold, lineCnt);

                logger.Log("Filtering done.");

                logger.Log("Processing file again for concomitant occurences.");

                using (var rdr = new StreamReader(trainParams.TrainPath, Encoding.UTF8))
                {
                    string srcSentence = null, trgSentence = null, errorMsg = null;
                    bool endOfFile = false, quit = false;

                    while (!quit)
                    {
                        quit = LinePairReader.ReadLinePair(rdr, ref srcSentence, ref trgSentence, ref errorMsg, ref endOfFile);

                        if (!quit)
                        {
                            Sequence[] freqSrcSequences = trainParams.TextUnitFunc(srcSentence).Where(srcCounter.ContainsKey).ToArray();
                            Sequence[] freqTrgSequences = trainParams.TextUnitFunc(trgSentence).Where(trgCounter.ContainsKey).ToArray();

                            foreach (Sequence srcSeq in freqSrcSequences)
                            {
                                foreach (Sequence trgSeq in freqTrgSequences)
                                {
                                    addPairToTogetherCounter(srcSeq, trgSeq, togetherCounter);
                                }
                            }
                        }
                        else if (errorMsg != null)
                        {
                            logger.Log("Error in file.");
                        }
                    }
                }

                logger.Log("Second processing done, concomitant occurrences counted.");

                for (int i = 0; i < trainParams.ScoreCalculators.Length; i++)
                {
                    string calcName = trainParams.ScoreCalculators[i];
                    string calcParam = trainParams.CalculatorParams[i];
                    // get calculator object
                    IScoreCalculator calculator = ScoreCalculatorFactory.GetCalculatorByName(calcName, calcParam);

                    logger.Log("Calculating scores with " + calcName);

                    foreach (float minScore in trainParams.MinScores)
                    {
                        logger.Log("Creating dictionary with minScore: " + minScore);

                        var possibleTranslations = new Dictionary<Sequence, List<PossibleTranslation>>();
                        var targetHashResolver = new Dictionary<int, string>();

                        foreach (Sequence srcSeq in srcCounter.Keys)
                        {
                            int srcHash = srcSeq.GetHashCode();
                            foreach (Sequence trgSeq in trgCounter.Keys)
                            {
                                int trgHash = trgSeq.GetHashCode();
                                int nTogether;

                                Dictionary<int, int> srcList;
                                if (!togetherCounter.TryGetValue(srcHash, out srcList))
                                {
                                    continue;
                                }

                                if (!srcList.TryGetValue(trgHash, out nTogether))
                                {
                                    continue;
                                }

                                float score = calculator.CalculateScore(srcSeq, trgSeq, srcCounter[srcSeq], trgCounter[trgSeq], nTogether, lineCnt);

                                if (score > minScore)
                                {
                                    targetHashResolver[trgSeq.Text.GetHashCode()] = trgSeq.Text;

                                    if (!possibleTranslations.ContainsKey(srcSeq))
                                        possibleTranslations[srcSeq] = new List<PossibleTranslation>();

                                    possibleTranslations[srcSeq].Add(new PossibleTranslation()
                                    {
                                        Score = score,
                                        TranslationHash = trgSeq.Text.GetHashCode()
                                    });
                                }
                            }
                        }

                        logger.Log(string.Format("Dictionary created. Monoling threshold: {0}; Min score: {1}; Score calculator: {2}", monolingThreshold, minScore, calcName));

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

                        bilingPredDict.TargetHashResolver = targetHashResolver;

                        var engine = new Engine(bilingPredDict, false, monolingThreshold, minScore);

                        yield return new EngineTrainResult()
                        {
                            Engine = engine,
                            MinThreshold = monolingThreshold,
                            MinScore = minScore,
                            ScoreCalculator = calcName,
                            SourceEntryNum = bilingPredDict.ItemCount,
                            AllEntryNum = (ulong)bilingPredDict.DictItems.SelectMany(di => di.PossibleTranslations).Count()
                        };
                    }

                    logger.Log("Score calculator " + calcName + " finished.");
                }
            }
        }

        private static void addSequencesToCounter(IEnumerable<Sequence> sequences, IDictionary<Sequence, int> counter)
        {
            foreach (Sequence seq in sequences)
            {
                if (counter.ContainsKey(seq))
                {
                    counter[seq]++;
                }
                else
                {
                    counter[seq] = 1;
                }
            }
        }

        private static void filterCounter(IDictionary<Sequence, int> counter, float threshold, int lineCnt)
        {
            if (threshold > 5e-4f)
            {
                int minOccur = (int) (threshold*lineCnt);
                var removeKeys = counter.Where(pair => pair.Value < minOccur).Select(pair => pair.Key).ToList();

                foreach (Sequence seq in removeKeys)
                {
                    counter.Remove(seq);
                }

                removeKeys = null;
            }
        }

        private static void addPairToTogetherCounter(Sequence srcSeq, Sequence trgSeq, IDictionary<int, Dictionary<int, int>> togetherCounter)
        {
            int srcHash = srcSeq.GetHashCode(), trgHash = trgSeq.GetHashCode();

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
