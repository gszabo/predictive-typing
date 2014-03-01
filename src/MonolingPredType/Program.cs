using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolingPredType
{
    class Program
    {
        private const string tempPath = @"h:\temp\predtype\";

        static void Main(string[] args)
        {
            //Task tDe, tFr, tEs;

            //tDe = new Task(() => processMonolingCorpus("de", "Europarl.de.train", "Europarl.de.eval"));
            //tFr = new Task(() => processMonolingCorpus("fr", "Europarl.fr.train", "Europarl.fr.eval"));
            //tEs = new Task(() => processMonolingCorpus("es", "Europarl.es.train", "Europarl.es.eval"));

            //tDe.Start();
            //tFr.Start();
            //tEs.Start();
            
            //Task.WaitAll(tDe, tFr, tEs);

            processMonolingCorpus("en", "de-en", "Europarl.de-en.en.train", "Europarl.de-en.en.eval");
            //processMonolingCorpus("fr", "Europarl.fr.train", "Europarl.fr.eval");
            //processMonolingCorpus("es", "Europarl.es.train", "Europarl.es.eval");

            //dumpDictionaryXml(@"h:\temp\predtype\fr\11-db.dat", @"h:\temp\predtype\fr\11-db.xml");

            Console.WriteLine("End of program.");

            Console.ReadLine();
        }

        static void processMonolingCorpus(string trg, string folder, string trainFilename, string evalFilename)
        {
            string workFolder = Path.Combine(tempPath, folder);

            var measurer = new Measurer(workFolder, trainFilename, evalFilename);
            Measurement[] measurements = measurer.DoMeasure();
            
            measurer = null;
            GC.Collect();

            string resultPath = Path.Combine(workFolder, "result-lc-wbt-" + trg + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv");

            using (var wrt = new StreamWriter(resultPath, false, Encoding.UTF8))
            {
                wrt.WriteLine("MinThreshold;TrainTime;EvalTime;EvalSentenceCnt;AvgSentenceLength;AvgKeyStrokeSave;AvgCoverage;DbSizeInKb");

                foreach (var m in measurements)
                {
                    wrt.WriteLine(
                        "{0};{1};{2};{3};{4};{5};{6};{7}",
                        m.TrainParams.MinThreshold,
                        m.TrainingTime.ToString("mm\\:ss"),
                        m.EvaluationTime.ToString("mm\\:ss"),
                        m.EvalResult.EvalSentenceCount,
                        m.EvalResult.AvgSentenceLength,
                        m.EvalResult.AvgKeyStrokeSave,
                        m.EvalResult.AvgCoverage,
                        m.DbSizeInKb);
                }
            }
        }

        static void dumpDictionaryXml(string dictPath, string targetPath)
        {
            var engine = Engine.Load(dictPath);

            engine.SaveXml(targetPath);
        }
    }
}
