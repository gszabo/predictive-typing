using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonolingPredType
{
    class Measurer
    {
        private readonly string workFolder;

        private readonly Logger logger;

        private readonly string trainPath, evalPath;

        public Measurer(string workFolder, string trainFilename, string evalFileName)
        {
            this.workFolder = workFolder;

            trainPath = Path.Combine(workFolder, trainFilename);
            evalPath = Path.Combine(workFolder, evalFileName);

            logger = Logger.Create(workFolder);
        }

        public Measurement[] DoMeasure()
        {
            //createTrainEvalFiles();

            List<Measurement> measurements = new List<Measurement>();

            //float seqThreshold = 0.005f;
            //float rareWordFreqBase = 0.15f;

            float[] thresholds = { 0.15f, 0.1f, 5e-2f, 4e-2f, 3e-2f, 2e-2f, 1e-2f, 5e-3f, 1e-3f, 5e-4f, 4e-4f, 3e-4f, 2e-4f, 1e-4f, 5e-5f, 4e-5f, 3e-5f, 2e-5f, 1.5e-5f, 1e-5f, 0.0f };

            for (int i = 0; i < thresholds.Length; i++)
            {
                float rareWordFreq = thresholds[i];
                //float rareWordFreq = 0.0f;

                try
                {
                    logger.Log("Starting training from: " + trainPath);

                    var tp = new Engine.Trainer.TrainParams()
                        {
                            MinThreshold = rareWordFreq
                        };

                    Stopwatch swTrain = new Stopwatch();
                    swTrain.Start();

                    Engine e = new Engine.Trainer(trainPath, tp).Train();

                    swTrain.Stop();

                    string dbFilePath = Path.Combine(workFolder, i + "-db.dat");

                    e.Save(dbFilePath);
                    //e.SaveXml(Path.ChangeExtension(dbFilePath, "xml"));

                    GC.Collect();

                    logger.Log("Finished training, DB saved");

                    logger.Log("Starting evaluation from: " + evalPath);


                    Stopwatch swEval = new Stopwatch();
                    swEval.Start();

                    EvalResult r = new Evaluator(evalPath, e).Evaluate();

                    swEval.Stop();

                    logger.Log("Evaluation finished.");

                    logger.Log("\n\n");

                    measurements.Add(new Measurement()
                        {
                            EvalResult = r,
                            EvaluationTime = swEval.Elapsed,
                            TrainParams = tp,
                            TrainingTime = swTrain.Elapsed,
                            DbSizeInKb = new FileInfo(dbFilePath).Length / 1024
                        });

                    e = null;
                    GC.Collect();

                }
                catch (Exception e)
                {
                    logger.Log(e.ToString());
                }

                GC.Collect();
            }

            logger.Log("Done.");

            return measurements.ToArray();
        }

    }
}
