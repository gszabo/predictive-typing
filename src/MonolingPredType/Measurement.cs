using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolingPredType
{
    struct Measurement
    {
        public Engine.Trainer.TrainParams TrainParams;

        //public Evaluator.LookupParams LookupParams;

        public EvalResult EvalResult;

        public TimeSpan TrainingTime;
        
        public TimeSpan EvaluationTime;

        public long DbSizeInKb;

        public long EnryNum;
    }
}
