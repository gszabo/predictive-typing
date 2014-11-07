using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace MonolingPredType
{
    public class MeasureParams
    {
        public string TrainPath { get; set; }

        public string TrainResultPath { get; set; }

        public string EvalPath { get; set; }

        public string EvalResultPath { get; set; }

        public string[] EvalMetrics { get; set; }

        public Logger Log { get; set; }

        public float[] MinThresholds { get; set; }

        public Func<string, Sequence[]> TextUnitFunc { get; set; }

        public string DictFileSavePattern { get; set; }
    }
}
