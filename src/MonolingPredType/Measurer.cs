using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace MonolingPredType
{
    public class Measurer
    {
        //private readonly string workFolder;

        //private readonly Logger logger;

        //private readonly string trainPath, evalPath;

        private readonly MeasureParams measureParams;

        public Measurer(MeasureParams measureParams)
        {
            this.measureParams = measureParams;
        }

        //public Measurer(string workFolder, string trainFilename, string evalFileName)
        //{
        //    this.workFolder = workFolder;

        //    trainPath = Path.Combine(workFolder, trainFilename);
        //    evalPath = Path.Combine(workFolder, evalFileName);

        //    logger = Logger.Create(workFolder);
        //}

        public void DoMeasure()
        {
            Logger logger = measureParams.Log;
            string trainPath = measureParams.TrainPath;

            List<Measurement> measurements = new List<Measurement>();

            logger.Log("Starting collecting training from: " + trainPath);

            Stopwatch sw = new Stopwatch();

            sw.Start();
            CollectionResult cr = TextUnitCollector.Collect(trainPath, measureParams.TextUnitFunc, logger);
            sw.Stop();

            logger.Log("Collection finished.");

            TimeSpan collectionTime = sw.Elapsed;
            
            //float seqThreshold = 0.005f;
            //float rareWordFreqBase = 0.15f;

            var evalPaths = new List<string>();

            //float[] thresholds = { 0.15f, 0.1f, 5e-2f, 4e-2f, 3e-2f, 2e-2f, 1e-2f, 5e-3f, 1e-3f, 5e-4f, 4e-4f, 3e-4f, 2e-4f, 1e-4f, 5e-5f, 4e-5f, 3e-5f, 2e-5f, 1.5e-5f, 1e-5f, 0.0f };
            float[] thresholds = measureParams.MinThresholds;

            for (int i = 0; i < thresholds.Length; i++)
            {
                float rareWordFreq = thresholds[i];
                
                try
                {
                    sw.Reset();
                    logger.Log("Starting filtering for threshold " + rareWordFreq);
                    sw.Start();
                    Engine e = Engine.Trainer.Filter(cr, rareWordFreq);
                    sw.Stop();
                    logger.Log("Filtering finished");
                    //swTrain.Stop();

                    TimeSpan filterTime = sw.Elapsed;

                    string dbFilePath = measureParams.DictFileSavePattern.Replace("{min_threshold}", rareWordFreq.ToString(CultureInfo.InvariantCulture));

                    e.Save(dbFilePath);
                    logger.Log("Dictionary saved at " + dbFilePath);
                    //e.SaveXml(Path.ChangeExtension(dbFilePath, "xml"));

                    evalPaths.Add(dbFilePath);
                    writeTrainResult(measureParams.TrainResultPath, rareWordFreq, collectionTime, filterTime, new FileInfo(dbFilePath).Length / 1024, e.GetEntryCount(), Path.GetFileName(dbFilePath));

                    GC.Collect();
                }
                catch (Exception e)
                {
                    logger.Log(e.ToString());
                }
            }

            string evalPath = measureParams.EvalPath;
            logger.Log("Starting evaluation from: " + evalPath);

            foreach (string evalEngineFile in evalPaths)
            {
                logger.Log("Loading " + evalEngineFile);
                sw.Reset();
                sw.Start();
                Engine e = Engine.Load(evalEngineFile);
                sw.Stop();
                logger.Log("Load finished.");

                TimeSpan loadTime = sw.Elapsed;

                logger.Log("Evaluating");
                sw.Reset();
                sw.Start();
                EvalResult r = new Evaluator(evalPath, e, measureParams.EvalMetrics, logger).Evaluate();
                sw.Stop();
                logger.Log("Evaluation done.");

                TimeSpan evalTime = sw.Elapsed;

                writeEvalResult(measureParams.EvalResultPath, Path.GetFileName(evalEngineFile), loadTime, evalTime, r.AvgCoverage, r.AvgKeyStrokeSave, r.AvgSentenceLength);

                GC.Collect();
            }
            
            logger.Log("Evaluation finished.");

            logger.Log("\n\n");

            logger.Log("Done.");

            //return measurements.ToArray();
        }

        private const string timeSpanFormat = "hh\\:mm\\:ss";

        private void writeTrainResult(string trainResultPath, float minThreshold, TimeSpan collectionTime, TimeSpan filterTime, long sizeInKb, ulong entryCount, string fileName)
        {
            if (!File.Exists(trainResultPath))
            {
                // write CSV header
                File.WriteAllText(trainResultPath, "MinThreshold;CollectionTime;FilterTime;SizeInKb;EntryCount;DictFileName" + Environment.NewLine);
            }

            File.AppendAllText(trainResultPath, string.Format("{0};{1};{2};{3};{4};{5}" + Environment.NewLine, minThreshold, collectionTime.ToString(timeSpanFormat), filterTime.ToString(timeSpanFormat), sizeInKb, entryCount, fileName));
        }

        private void writeEvalResult(string evalResultPath, string fileName, TimeSpan loadTime, TimeSpan evalTime, float avgCoverage, float avgKss, float avgSentLen)
        {
            if (!File.Exists(evalResultPath))
            {
                // write CSV header
                File.WriteAllText(evalResultPath, "DictFileName;LoadTime;EvalTime;AvgCoverage;AvgKeystrokeSave;AverageSentenceLen" + Environment.NewLine);
            }

            File.AppendAllText(evalResultPath, string.Format("{0};{1};{2};{3};{4};{5}" + Environment.NewLine, fileName, loadTime.ToString(timeSpanFormat), evalTime.ToString(timeSpanFormat), avgCoverage, avgKss, avgSentLen));
        }

    }
}
