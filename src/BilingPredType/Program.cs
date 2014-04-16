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

            //foreach (Sequence sequence in s.CollectAllSubsetsOfN(4))
            //{
            //    Console.WriteLine(sequence.Text);
            //}

            //doForLangPair("en", "de");

            doForLangPair("en", "fr");

            //doForLangPair("en", "es");

            //doForLangPair(args[0], args[1]);

            //tryDict(@"h:\temp\predtype\en-fr\Europarl.en-fr.en.train", @"h:\temp\predtype\en-fr\Europarl.en-fr.fr.train", @"h:\temp\predtype\en-fr\try-db.dat", @"h:\temp\predtype\en-fr\lookup-output.txt");

            Console.WriteLine("Done.");

            Application.SetSuspendState(PowerState.Suspend, true, true);

            Console.ReadKey();
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
