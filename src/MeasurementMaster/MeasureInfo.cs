using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeasurementMaster
{
    class MeasureInfo
    {
        public string Description { get; set; }

        public string TrainPath { get; set; }
        
        public string TrainOutputPath { get; set; }

        public string EvalPath { get; set; }
        
        public string EvalOutputPath { get; set; }
        
        public string Type { get; set; }

        public float[] MinThresholds { get; set; }

        public float[] ScoreThresholds { get; set; }

        public string[] TextUnits { get; set; }

        public string[] ScoreCalculators { get; set; }

        public string[] CalculatorParams { get; set; }

        public string[] Metrics { get; set; }

        public string DictPattern { get; set; }

        public string LogPath { get; set; }
    }
}
