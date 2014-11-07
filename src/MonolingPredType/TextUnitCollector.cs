using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace MonolingPredType
{
    static class TextUnitCollector
    {
        public static CollectionResult Collect(string filePath, Func<string, Sequence[]> textUnitFunc, Logger logger)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("File does not exists", "filePath");
            }

            if (textUnitFunc == null || logger == null)
            {
                throw new ArgumentNullException();
            }

            ulong sentenceCount = 0;
            var counters = new Dictionary<string, ulong>();

            using (var rdr = new StreamReader(filePath, Encoding.UTF8))
            {
                bool quit = false, endOfFile = false;
                string srcSentence = null, trgSentence = null, errorMessage = null;

                while (!quit)
                {
                    quit = !LinePairReader.ReadLinePair(rdr, ref srcSentence, ref trgSentence, ref errorMessage, ref endOfFile);

                    if (!quit)
                    {
                        sentenceCount++;
                        foreach (Sequence seq in textUnitFunc(trgSentence))
                        {
                            if (!counters.ContainsKey(seq.Text))
                            {
                                counters[seq.Text] = 1;
                            }
                            else
                            {
                                counters[seq.Text]++;
                            }
                        }
                    }
                    else if (!endOfFile)
                    {
                        // error
                        logger.Log("Error in " + filePath + ". " + errorMessage + " Read sentences: " + sentenceCount);
                    }
                }
            }

            return new CollectionResult()
            {
                Counters = counters,
                NumOfSentences = sentenceCount
            };
        }
    }

    class CollectionResult
    {
        public ulong NumOfSentences { get; set; }

        public Dictionary<string, ulong> Counters { get; set; }
    }
}
