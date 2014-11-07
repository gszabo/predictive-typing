using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonolingPredType;
using PredType.Utils;

namespace MeasurementMaster
{
    static class MonolingMeasurer
    {
        private const string dateFormatString = "yyyy-MM-dd-HH-mm-ss";

        public static void CarryOut(MeasureInfo measureInfo, DateTime now)
        {
            if (measureInfo.Type != "mono")
            {
                throw new ArgumentException("Not monolingual measurement", "measureInfo");
            }

            foreach (string textUnitType in measureInfo.TextUnits)
            {
                var measureParams = new MeasureParams()
                {
                    DictFileSavePattern = measureInfo.DictPattern.ReplacePlaceholders(textUnitType, now),
                    EvalMetrics = measureInfo.Metrics,
                    Log = new Logger(measureInfo.LogPath.ReplacePlaceholders(textUnitType, now)),
                    MinThresholds = measureInfo.MinThresholds,
                    TextUnitFunc = TextUnitCollectors.GetTextUnitCollector(textUnitType),
                    TrainPath = measureInfo.TrainPath,
                    TrainResultPath = measureInfo.TrainOutputPath.ReplacePlaceholders(textUnitType, now),
                    EvalPath = measureInfo.EvalPath,
                    EvalResultPath = measureInfo.EvalOutputPath.ReplacePlaceholders(textUnitType, now)
                };

                var measurer = new MonolingPredType.Measurer(measureParams);

                measurer.DoMeasure();
            }
        }
    }
}
