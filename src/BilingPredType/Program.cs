using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PredType.Utils;

namespace BilingPredType
{
    class Program
    {
        private const string rootFolder = @"h:\temp\predtype";

        public static int WordNum;

        static void Main(string[] args)
        {
            #region első próba

            //Stopwatch sw = new Stopwatch();

            //sw.Start();

            //var engine = new Engine.Trainer(
            //    @"h:\temp\monoling-predtype\en-de\Europarl.en-de.en.train",
            //    @"h:\temp\monoling-predtype\en-de\Europarl.en-de.de.train",
            //    new TrainParams()
            //        {
            //            MinScore = 0.3f,
            //            MonolingThreshold = 0.01f
            //        }).Train();

            //sw.Stop();

            //engine.SaveXml(@"h:\temp\monoling-predtype\en-de\db.xml");
            //engine.Save(@"h:\temp\monoling-predtype\en-de\db.dat");

            //Console.WriteLine("Done training");

            //Engine e = Engine.Load(@"h:\temp\monoling-predtype\en-de\db.dat");

            //Evaluator eval = new Evaluator(
            //    @"h:\temp\monoling-predtype\en-de\Europarl.en-de.en.eval",
            //    @"h:\temp\monoling-predtype\en-de\Europarl.en-de.de.eval",
            //    e);

            //EvalResult result = eval.Evaluate();

            //using (var wrt = new StreamWriter(@"h:\temp\monoling-predtype\en-de\result.txt", false, Encoding.UTF8))
            //{
            //    wrt.WriteLine("Train time: {0}", sw.Elapsed.ToString("mm\\:ss"));
            //    wrt.WriteLine("Eval time: {0}", result.EvalTime.ToString("mm\\:ss"));
            //    wrt.WriteLine("Eval sentence count: {0}", result.SentenceCount);
            //    wrt.WriteLine("Avg sentence length: {0}", result.AvgSentenceLength);
            //    wrt.WriteLine("Avg keystroke save: {0}", result.AvgKeyStrokeSave);
            //    wrt.WriteLine("Avg coverage: {0}", result.AvgCoverage);
            //}

            //Console.WriteLine("Done evaluating");

            //Console.ReadKey();

            #endregion

            //Engine e = Engine.Load(@"h:\temp\monoling-predtype\en-de\db.dat");

            //string srcLine =
            //    "Another key issue|apart from tax law|when it comes to the decision-making ability of each national parliament and government is defence policy";
            //string trgLine =
            //    "Eine weitere Kernfrage für die Souveränität jedes nationalen Parlaments und jeder einzelstaatlichen Regierung ist neben dem Besteuerungsrecht die Verteidigungspolitik";

            //using (var wrt = new StreamWriter(@"h:\temp\proba.txt", false, Encoding.UTF8))
            //{
            //    wrt.WriteLine("Source: {0}", srcLine);
            //    wrt.WriteLine("Target: {0}", trgLine);
            //    wrt.WriteLine("Hits:");
            //    foreach (LookupHit lookupHit in e.Lookup(srcLine).OrderByDescending(hit => hit.Score))
            //    {
            //        wrt.WriteLine("-------");
            //        wrt.WriteLine("Generating: {0}", lookupHit.GeneratingString);
            //        wrt.WriteLine("TranslationHint: {0}", lookupHit.TranslationHint);
            //        wrt.WriteLine("Score: {0}", lookupHit.Score);
            //        wrt.WriteLine("-------\n\n");
            //    }
            //}
            //Console.WriteLine("Done");


            //string s = "aaa bbb ccc ddd eee fff|ggg hhh";

            //foreach (Sequence sequence in s.CollectPairsAndWords())
            //{
            //    Console.WriteLine(sequence.Text);
            //}

            //doForLangPair("en", "de");

            //WordNum = 2;
            //batchDoForLangPair("en", "hu");

            //WordNum = 3;
            //batchDoForLangPair("en", "hu");

            //WordNum = 7;
            //collectStatistics("en", "fr");
            
            //batchDoForLangPair("en", "fr");

            //doForLangPair("en", "es");

            //doForLangPair(args[0], args[1]);

            //tryDict(@"h:\temp\predtype\en-fr\Europarl.en-fr.en.train", @"h:\temp\predtype\en-fr\Europarl.en-fr.fr.train", @"h:\temp\predtype\en-fr\try-db.dat", @"h:\temp\predtype\en-fr\lookup-output.txt");

            //debug();

            Console.WriteLine(DateTime.Now + ": Done.");

            //Application.SetSuspendState(PowerState.Suspend, true, true);
            Console.Beep();

            Console.ReadKey();
        }

        static void collectStatistics(string src, string trg)
        {
            string folder = Path.Combine(rootFolder, src + "-" + trg);

            string trainSrc = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.train", src, trg, src));
            string trainTrg = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.train", src, trg, trg));

            string dateString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            string statSrcPath = Path.Combine(folder, "stat-" + src + "-" + WordNum + "gram-" + dateString + ".csv");
            string statTrgPath = Path.Combine(folder, "stat-" + trg + "-" + WordNum + "gram-" + dateString + ".csv");

            float threshold = 0.001f;

            writeStatistics(statSrcPath, countSeqs(trainSrc, threshold));
            writeStatistics(statTrgPath, countSeqs(trainTrg, threshold));
        }

        static Dictionary<Sequence, int> countSeqs(string inputPath, float rareThreshold)
        {
            var counter = new Dictionary<Sequence, int>();

            int lineCount = 0;

            // count
            using (var srcReader = new StreamReader(inputPath, Encoding.UTF8))
            {
                string srcLine;

                while ((srcLine = srcReader.ReadLine()) != null)
                {
                    lineCount++;

                    Sequence[] sequences = srcLine.CollectNGrams(WordNum);
                    foreach (Sequence s in sequences)
                    {
                        if (counter.ContainsKey(s))
                        {
                            counter[s]++;
                        }
                        else
                        {
                            counter[s] = 1;
                        }
                    }
                }
            }

            // filter rare
            int minOccur = (int) (lineCount*rareThreshold);
            List<Sequence> removeKeys = counter.Where(pair => pair.Value < minOccur).Select(pair => pair.Key).ToList();
            foreach (Sequence key in removeKeys)
            {
                counter.Remove(key);
            }

            return counter;
        } 

        static void writeStatistics(string outPath, Dictionary<Sequence, int> counter)
        {
            var distribution = new Dictionary<int, int>();

            foreach (KeyValuePair<Sequence, int> pair in counter)
            {
                int wordCount = pair.Key.WordCount;
                if (distribution.ContainsKey(wordCount))
                {
                    distribution[wordCount]++;
                }
                else
                {
                    distribution[wordCount] = 1;
                }
            }

            using (var writer = new StreamWriter(outPath, false, Encoding.UTF8))
            {
                writer.WriteLine("Word count;Count");
                foreach (int wordCount in distribution.Keys.OrderBy(key => key))
                {
                    writer.WriteLine("{0};{1}", wordCount, distribution[wordCount]);
                }
            }
        }
        
        static void debug()
        {
            string pairPath = @"h:\temp\predtype\en-fr\debug-pair.db";
            string hashPath = @"h:\temp\predtype\en-fr\debug-hash.db";

            string pairOut = @"h:\temp\predtype\en-fr\out-pair.txt";
            string hashOut = @"h:\temp\predtype\en-fr\out-hash.txt";

            Console.WriteLine(DateTime.Now + ": loading engines");

            Engine hash = Engine.Load(hashPath);
            Engine pair = Engine.Load(pairPath);
            
            Console.WriteLine(DateTime.Now + ": loaded");

            string srcLine = "The public demand more and better information on products and foodstuffs|how and where they are produced|under what conditions and with what ingredients";
            string trgLine = "Le public demande plus d'informations de meilleure qualité sur les produits et les aliments|comment et où ils ont été produits|dans quelles conditions et avec quels ingrédients";

            Console.WriteLine(DateTime.Now + ": looking up hash");

            using (var wrt = new StreamWriter(hashOut, true, Encoding.UTF8))
            {
                wrt.WriteLine("Source: {0}", srcLine);
                wrt.WriteLine("Target: {0}", trgLine);
                LookupHit[] hits = hash.Lookup(srcLine).ToArray();
                float kss = calcKeyStrokeSave(trgLine, hits, wrt);
                wrt.WriteLine("Keystroke saving: {0}", kss);
                wrt.WriteLine("Hits:");
                foreach (LookupHit lookupHit in hits.OrderByDescending(hit => hit.Score))
                {
                    wrt.WriteLine("-------");
                    wrt.WriteLine("Generating: {0}", lookupHit.GeneratingString);
                    wrt.WriteLine("TranslationHint: {0}", lookupHit.TranslationHint);
                    wrt.WriteLine("Score: {0}", lookupHit.Score);
                    wrt.WriteLine("-------\n\n");
                }
                wrt.WriteLine();
                wrt.WriteLine();
            }

            Console.WriteLine(DateTime.Now + ": looking up pair");

            using (var wrt = new StreamWriter(pairOut, true, Encoding.UTF8))
            {
                wrt.WriteLine("Source: {0}", srcLine);
                wrt.WriteLine("Target: {0}", trgLine);
                LookupHit[] hits = pair.Lookup(srcLine).ToArray();
                float kss = calcKeyStrokeSave(trgLine, hits, wrt);
                wrt.WriteLine("Keystroke saving: {0}", kss);
                wrt.WriteLine("Hits:");
                foreach (LookupHit lookupHit in hits.OrderByDescending(hit => hit.Score))
                {
                    wrt.WriteLine("-------");
                    wrt.WriteLine("Generating: {0}", lookupHit.GeneratingString);
                    wrt.WriteLine("TranslationHint: {0}", lookupHit.TranslationHint);
                    wrt.WriteLine("Score: {0}", lookupHit.Score);
                    wrt.WriteLine("-------\n\n");
                }
                wrt.WriteLine();
                wrt.WriteLine();
            }

            Console.WriteLine(DateTime.Now + ": end of debug");
        }

        private static float calcKeyStrokeSave(string trgLine, LookupHit[] hits, StreamWriter writer)
        {
            float savedRatio = 0.0f;
            int savedStroke = 0;
            var lookupCache = new Dictionary<string, string[]>();

            // normalizált a szöveg, nem kell ezeket leszedni
            // leszedem a mondatvégi pontot és WS-t
            //line = line.TrimEnd('.', ' ', '\t');

            // itt a pontot nem kell átugrani, mert lehet pl. "Dr." ami egy token
            //const string ignoreChar = "'\"?!(), \t";
            // kétfajta whitespace-ünk van, a puha és a kemény
            const string ignoreChar = " |";

            // This list represents a mapping for the text of the segment
            // I believe the mapping is necessary because a hit doesn't always cover the whole length of a word
            // Each item represents a character in the string
            // -1: it does not count in mapping (space, dot, etc.); 0: not mapped to a hit; 1: mapped to a hit
            List<int> charMap = new List<int>(trgLine.Length);
            // initialize the mapping
            foreach (var ch in trgLine)
            {
                if (ignoreChar.IndexOf(ch) >= 0)
                {
                    charMap.Add(-1);
                }
                else
                {
                    charMap.Add(0);
                }
            }
            int characterCount = charMap.Count(f => f != -1);

            if (characterCount > 0)
            {
                int pos = 0;
                bool lineEnd = false;

                lookupCache.Clear();

                while (!lineEnd)
                {
                    // megkeres a következő értelmes karaktert
                    while (pos < trgLine.Length && charMap[pos] == -1)
                        pos++;

                    // ha a sor végére értünk, kilép a ciklusból
                    if (pos >= trgLine.Length)
                        break;

                    // a completionba beletartozik a prefix is

                    int prefixLen = 1;
                    string chosenCompletion = null;
                    int chosenCompletionIndex = -1;
                    while (chosenCompletion == null)
                    {
                        // a sor végére értünk
                        if ((pos + prefixLen) >= trgLine.Length)
                        {
                            lineEnd = true;
                            break;
                        }

                        // a prefix már egy teljes szó, ha eddig nem jött rá találat, ezután se fog, hagyjuk, menjünk a következő szóra
                        if (charMap[pos + prefixLen - 1] == -1)
                        {
                            pos = pos + prefixLen - 1;
                            break;
                        }

                        string prefix = trgLine.Substring(pos, prefixLen);

                        string[] completions;
                        if (!lookupCache.TryGetValue(prefix, out completions))
                        {
                            completions = hits.Where(eh => eh.TranslationHint.StartsWith(prefix) &&
                                                             eh.TranslationHint.Length > prefixLen)
                                                .OrderByDescending(eh => eh.Score)
                                                .Take(6)
                                                .Select(eh => eh.TranslationHint)
                                                .ToArray();
                            lookupCache.Add(prefix, completions);
                        }

                        // ha üres completions tömb jön vissza, akkor hosszabb prefixre se jön találat, ugrás a köv szóra
                        if (completions.Length == 0)
                        {
                            while (pos < trgLine.Length && charMap[pos] != -1)
                                pos++;
                            break;
                        }

                        // leghosszabb completion keresése
                        for (int i = 0; i < completions.Length; i++)
                        {
                            // csak azokat szúrjuk be, amelyik hosszabb a prefixnél
                            if (trgLine.Substring(pos).StartsWith(completions[i]))
                            {
                                if (chosenCompletion == null)
                                {
                                    chosenCompletion = completions[i];
                                    chosenCompletionIndex = i;
                                }
                                else if (chosenCompletion.Length < completions[i].Length)
                                {
                                    chosenCompletion = completions[i];
                                    chosenCompletionIndex = i;
                                }
                            }
                        }

                        if (chosenCompletion == null)
                            prefixLen++;
                    }

                    if (chosenCompletion != null)
                    {
                        writer.WriteLine("Chosen completion: {0} (prefixLen: {1}; index: {2})", chosenCompletion, prefixLen, chosenCompletionIndex);
                        try
                        {
                            for (int i = 0; i < chosenCompletion.Length; i++)
                            {
                                if (charMap[pos + i] == 0)
                                    charMap[pos + i] = 1;
                            }
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            //Logger.Create(Path.GetDirectoryName(evalPath)).Log(
                            //    string.Format("------\nTarget string: {0}\nMatch: {1}\n------", line, chosenCompletion));
                            //savedStroke = 0;
                            return 0.0f;
                        }

                        savedStroke += chosenCompletion.Length - prefixLen - chosenCompletionIndex;

                        pos += chosenCompletion.Length;
                        // lehet, hogy a completion egy szónak a közepén fejeződött be, a maradékot átugorjuk
                        while (pos < trgLine.Length && charMap[pos] != -1)
                            pos++;
                    }
                }

                savedRatio = ((float)savedStroke) / charMap.Count(f => f != -1);
            }

            //if (savedRatio > 1)
            //    Debugger.Break();

            return savedRatio;
        }

        static void batchDoForLangPair(string src, string trg)
        {
            //string folder = Path.Combine(rootFolder, src + "-" + trg);

            //string trainSrc = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.train", src, trg, src));
            //string trainTrg = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.train", src, trg, trg));
            //string evalSrc = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.eval", src, trg, src));
            //string evalTrg = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.eval", src, trg, trg));

            //string dateString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            //string trainResultPath = Path.Combine(folder, "trainResult-" + WordNum + "gram-" + src + "-" + trg + "-" + dateString + ".csv");
            //string evalResultPath = Path.Combine(folder, "evalResult-" + WordNum + "gram-" + src + "-" + trg + "-" + dateString + ".csv");

            //float[] thresholds = { 1e-3f };
            //float[] minScores = { 0.3f, 0.25f, 0.2f, 0.15f, 0.1f, 5e-2f, 4e-2f, 3e-2f, 2e-2f, 1.5e-2f, 1e-2f, 9e-3f, 8e-3f, 7e-3f, 6e-3f, 5.5e-3f, 5e-3f };

            //Console.WriteLine("\n\nStarting for lang pair {0}-{1}.", src, trg);

            //// batch train kör
            ////var btp = new BatchTrainParams();
            ////btp.MonolingThresholds = thresholds;
            ////btp.MinScores = minScores;

            //var dbFilePaths = new List<string>(thresholds.Length * minScores.Length);

            //Stopwatch sw = new Stopwatch();

            ////IEnumerable<Engine> engines = new Engine.Trainer(trainSrc, trainTrg, btp).BatchTrain();

            ////IEnumerator<Engine> engineEnumerator = engines.GetEnumerator();
            //int i = 0;
            //sw.Start();

            //while (engineEnumerator.MoveNext())
            //{
            //    Engine e = engineEnumerator.Current;
            //    sw.Stop();

            //    string dbPath = Path.Combine(folder, "db-" + i + ".dat");

            //    ulong srcEntryCount = e.Dictionary.ItemCount;
            //    ulong allEntryCount = (ulong)e.Dictionary.DictItems.SelectMany(di => di.PossibleTranslations).Count();
            //    string fileName = Path.GetFileName(dbPath);

            //    e.Save(dbPath);

            //    long dbSizeInKb = new FileInfo(dbPath).Length / 1024;

            //    dbFilePaths.Add(dbPath);

            //    writeTrainResult(trainResultPath, e.MinThreshold, e.MinScore, sw.Elapsed,
            //                     fileName, dbSizeInKb, srcEntryCount, allEntryCount);

            //    e = null;

            //    GC.Collect();

            //    i++;
            //    sw.Reset();
            //    sw.Start();
            //}

            //sw.Stop();
            //engines = null;
            //engineEnumerator = null;

            //GC.Collect();

            //// evaluation kör
            //foreach (string dbFilePath in dbFilePaths)
            //{
            //    string fileName = Path.GetFileName(dbFilePath);
            //    sw.Reset();
            //    sw.Start();
            //    Engine e = Engine.Load(dbFilePath);
            //    sw.Stop();
            //    Console.WriteLine("\n\n{1}: Starting evaluating for {0}.", fileName, DateTime.Now);

            //    Evaluator eval = new Evaluator(evalSrc, evalTrg, e);
            //    EvalResult evalResult = eval.Evaluate();

            //    writeEvalResult(evalResultPath, fileName, evalResult, sw.Elapsed);

            //    GC.Collect();

            //    Console.WriteLine("{0}: Evaluation finished", DateTime.Now);
            //}

            //Console.WriteLine("Done for lang pair {0}-{1}.", src, trg);
        }

        static void writeTrainResult(string path, 
                                     float minThreshold, float minScore, TimeSpan trainTime, string dbFileName, 
                                     long dbSizeInKb, ulong srcEntryCount, ulong allEntryCount)
        {
            bool writeHeader = !File.Exists(path);

            using (var wrt = new StreamWriter(path, true, Encoding.UTF8))
            {
                if (writeHeader)
                    wrt.WriteLine("MinThreshold;MinScore;TrainTime;DbFileName;DbSizeInKb;DbSrcEntryCount;DbAllEntryCount");

                wrt.WriteLine(
                        "{0};{1};{2};{3};{4};{5};{6}",
                        minThreshold,
                        minScore,
                        trainTime.ToString("mm\\:ss"),
                        dbFileName,
                        dbSizeInKb,
                        srcEntryCount,
                        allEntryCount);
            }
        }

        static void writeEvalResult(string path, string dbFileName, EvalResult r, TimeSpan loadTime)
        {
            bool writeHeader = !File.Exists(path);

            using (var wrt = new StreamWriter(path, true, Encoding.UTF8))
            {
                if (writeHeader)
                    wrt.WriteLine("DbFileName;LoadTime;EvalTime;EvalSentenceCnt;AvgSentenceLength;AvgKeyStrokeSave;AvgCoverage");

                wrt.WriteLine(
                        "{0};{1};{2};{3};{4};{5};{6}",
                        dbFileName,
                        loadTime.ToString("mm\\:ss"),
                        r.EvalTime.ToString("mm\\:ss"),
                        r.SentenceCount,
                        r.AvgSentenceLength,
                        r.AvgKeyStrokeSave,
                        r.AvgCoverage);
            }
        }

        static void doForLangPair(string src, string trg)
        {
            string folder = Path.Combine(rootFolder, src + "-" + trg);

            string trainSrc = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.train", src, trg, src));
            string trainTrg = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.train", src, trg, trg));
            string evalSrc = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.eval", src, trg, src));
            string evalTrg = Path.Combine(folder, string.Format("Europarl.{0}-{1}.{2}.eval", src, trg, trg));

            string dbPath = Path.Combine(folder, "db.dat");

            string resultPath = Path.Combine(folder, "result-allsubset4-" + src + "-" + trg + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv");

            //var measurements = new List<Measurement>();

            float[] thresholds = {1e-3f};
            float[] minScores = { 0.3f, 0.25f, 0.2f, 0.15f, 0.1f, 5e-2f, 4e-2f, 3e-2f, 2e-2f, 1.5e-2f, 1e-2f, 9e-3f, 8e-3f, 7e-3f, 6e-3f, 5.5e-3f, 5e-3f };

            Console.WriteLine("\n\nStarting for lang pair {0}-{1}.", src, trg);

            foreach (float threshold in thresholds)
            {
                foreach (float minScore in minScores)
                {
                    var measurement = new Measurement();
                    measurement.MinScore = minScore;
                    measurement.MinThreshold = threshold;

                    var tp = new TrainParams();
                    tp.MinScore = minScore;
                    tp.MonolingThreshold = threshold;

                    Console.WriteLine("Starting training for ({0}; {1}) .", threshold, minScore);
                    var sw = new Stopwatch();
                    sw.Start();
                    Engine e = new Engine.Trainer(trainSrc, trainTrg, tp).Train();
                    sw.Stop();
                    Console.WriteLine("Training finished.");

                    measurement.TrainTime = sw.Elapsed;
                    measurement.SrcEntryCount = e.Dictionary.ItemCount;
                    measurement.AllEntryCount = (ulong)e.Dictionary.DictItems.SelectMany(di => di.PossibleTranslations).Count();

                    e.Save(dbPath);
                    measurement.DbSizeInKb = new FileInfo(dbPath).Length/1024;

                    GC.Collect();

                    Console.WriteLine("Starting evaluation.");
                    Evaluator eval = new Evaluator(evalSrc, evalTrg, e);
                    measurement.EvalResult = eval.Evaluate();
                    Console.WriteLine("Evaluation finished.");

                    GC.Collect();

                    writeMeasurement(resultPath, measurement);
                    //measurements.Add(measurement);
                }
            }

            //writeResults(folder, src, trg, measurements);

            Console.WriteLine("Done for lang pair {0}-{1}.", src, trg);
        }

        static void writeMeasurement(string path, Measurement m)
        {
            bool writeHeader = !File.Exists(path);

            using (var wrt = new StreamWriter(path, true, Encoding.UTF8))
            {
                if (writeHeader)
                    wrt.WriteLine("MinThreshold;MinScore;TrainTime;EvalTime;EvalSentenceCnt;AvgSentenceLength;AvgKeyStrokeSave;AvgCoverage;DbSizeInKb;DbSrcEntryCount;DbAllEntryCount");

                wrt.WriteLine(
                        "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}",
                        m.MinThreshold,
                        m.MinScore,
                        m.TrainTime.ToString("mm\\:ss"),
                        m.EvalResult.EvalTime.ToString("mm\\:ss"),
                        m.EvalResult.SentenceCount,
                        m.EvalResult.AvgSentenceLength,
                        m.EvalResult.AvgKeyStrokeSave,
                        m.EvalResult.AvgCoverage,
                        m.DbSizeInKb,
                        m.SrcEntryCount,
                        m.AllEntryCount);
            }
        }

        static void writeResults(string folder, string src, string trg, IEnumerable<Measurement> measurements)
        {
            string resultPath = Path.Combine(folder, "result-" + src + "-" + trg + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv");

            using (var wrt = new StreamWriter(resultPath, false, Encoding.UTF8))
            {
                wrt.WriteLine("MinThreshold;MinScore;TrainTime;EvalTime;EvalSentenceCnt;AvgSentenceLength;AvgKeyStrokeSave;AvgCoverage;DbSizeInKb");

                foreach (var m in measurements)
                {
                    wrt.WriteLine(
                        "{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                        m.MinThreshold,
                        m.MinScore,
                        m.TrainTime.ToString("mm\\:ss"),
                        //m.LookupParams.LookupWords,
                        //m.LookupParams.LookupSequences,
                        m.EvalResult.EvalTime.ToString("mm\\:ss"),
                        m.EvalResult.SentenceCount,
                        m.EvalResult.AvgSentenceLength,
                        m.EvalResult.AvgKeyStrokeSave,
                        m.EvalResult.AvgCoverage,
                        m.DbSizeInKb);
                }
            }
        }

        static void tryDict(string trainSrcPath, string trainTrgPath, string savePath, string outputPath)
        {
            var engine = new Engine.Trainer(trainSrcPath, trainTrgPath, new TrainParams() { MonolingThreshold = 1e-2f, MinScore = 0.3f }).Train();

            engine.Save(savePath);

            string[] toLookup = {"I would", "new", "Parliament", "and I", "and"};

            using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                foreach (string s in toLookup)
                {
                    writer.WriteLine("Lookup: {0}", s);
                    foreach (LookupHit hit in engine.Lookup(s))
                    {
                        writer.WriteLine("({0}; {1})", hit.TranslationHint, hit.Score);
                    }
                    writer.WriteLine();
                }
            }            
        }
    }

    class Measurement
    {
        public TimeSpan TrainTime;

        public float MinThreshold;

        public float MinScore;

        public EvalResult EvalResult;

        public long DbSizeInKb;

        public ulong SrcEntryCount;

        public ulong AllEntryCount;
    }
}
