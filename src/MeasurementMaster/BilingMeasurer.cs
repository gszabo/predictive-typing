using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilingPredType;
using PredType.Utils;

namespace MeasurementMaster
{
    static class BilingMeasurer
    {
        private const string dateFormatString = "yyyy-MM-dd-HH-mm-ss";

        public static void CarryOut(MeasureInfo measureInfo, DateTime now)
        {
            foreach (string textUnitType in measureInfo.TextUnits)
            {
                carryOutForTUType(textUnitType, measureInfo, now);
            }
        }

        private static void carryOutForTUType(string textUnitType, MeasureInfo measureInfo, DateTime now)
        {
            Func<string, Sequence[]> textUnitFunc = TextUnitCollectors.GetTextUnitCollector(textUnitType);
            var logger = new Logger(measureInfo.LogPath.ReplacePlaceholders(textUnitType, now));
            var sw = new Stopwatch();

            var batchTrainParams = new BatchTrainParams(measureInfo.TrainPath, measureInfo.MinThresholds,
                                                        measureInfo.ScoreThresholds, textUnitFunc, 
                                                        measureInfo.ScoreCalculators, measureInfo.CalculatorParams);

            var batchTrainer = new BatchTrainer(batchTrainParams);

            IEnumerable<EngineTrainResult> engineInfos = batchTrainer.Train(logger);

            var engineFiles = new List<string>();

            foreach (EngineTrainResult engineTrainResult in engineInfos)
            {
                string savePath = measureInfo.DictPattern.ReplacePlaceholders(textUnitType, now,
                    engineTrainResult.ScoreCalculator, engineTrainResult.MinThreshold, engineTrainResult.MinScore);

                logger.Log("Saving dictionary to " + savePath);

                engineTrainResult.Engine.Save(savePath);

                engineFiles.Add(savePath);
                logger.Log("Dictionary saved.");

                writeTrainResult(measureInfo.TrainOutputPath.ReplacePlaceholders(textUnitType, now),
                                 engineTrainResult.MinThreshold, engineTrainResult.MinScore, engineTrainResult.ScoreCalculator,
                                 Path.GetFileName(savePath), new FileInfo(savePath).Length, engineTrainResult.SourceEntryNum, engineTrainResult.AllEntryNum);
            }

            // evaluation steps
            foreach (string enginePath in engineFiles)
            {
                logger.Log("Starting evaluation for " + enginePath);

                logger.Log("Loading");

                sw.Reset();
                sw.Start();
                Engine engine = Engine.Load(enginePath);
                sw.Stop();

                TimeSpan loadTime = sw.Elapsed;

                logger.Log("Load finished");

                logger.Log("Running evaluation");

                var evaluator = new Evaluator(measureInfo.EvalPath, measureInfo.Metrics, engine);
                sw.Reset();
                sw.Start();
                EvalResult evalResult = evaluator.Evaluate();
                sw.Stop();

                TimeSpan evalTime = sw.Elapsed;

                logger.Log("Evaluation finished");

                writeEvalResult(measureInfo.EvalOutputPath.ReplacePlaceholders(textUnitType, now),
                                Path.GetFileName(enginePath), loadTime, evalTime, evalResult.AvgCoverage, evalResult.AvgKeyStrokeSave, evalResult.AvgSentenceLength);
                logger.Log("Finished evaluation for " + enginePath);
            }
        }

        private const string timeSpanFormat = "hh\\:mm\\:ss";

        private static void writeTrainResult(string path, float minThreshold, float minScore, string scoreName, string fileName, long fileSize, ulong srcEntryCount, ulong allEntryCount)
        {
            if (!File.Exists(path))
            {
                string header = "MinThreshold;MinScore;ScoreName;FileName;FileSize;SrcEntryNum;AllEntryNum" + Environment.NewLine;
                File.WriteAllText(path, header);
            }

            string row = string.Format("{0};{1};{2};{3};{4};{5};{6}" + Environment.NewLine, minThreshold, minScore, scoreName, fileName, fileSize, srcEntryCount, allEntryCount);
            File.AppendAllText(path, row);
        }

        private static void writeEvalResult(string evalResultPath, string fileName, TimeSpan loadTime, TimeSpan evalTime, float avgCoverage, float avgKss, float avgSentLen)
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
