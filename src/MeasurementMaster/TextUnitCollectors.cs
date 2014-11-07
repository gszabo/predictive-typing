using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredType.Utils;

namespace MeasurementMaster
{
    static class TextUnitCollectors
    {
        private static readonly Dictionary<string, Func<string, Sequence[]>> delegates;

        static TextUnitCollectors()
        {
            delegates = new Dictionary<string, Func<string, Sequence[]>>();

            delegates["1g"] = delegates["w"] = (string line) => line.CollectWords();
            delegates["2g"] = delegates["b"] = (string line) => line.CollectNGrams(2);
            delegates["3g"] = delegates["t"] = (string line) => line.CollectNGrams(3);
            delegates["4g"] = (string line) => line.CollectNGrams(4);
            delegates["5g"] = (string line) => line.CollectNGrams(5);
            delegates["6g"] = (string line) => line.CollectNGrams(6);
            delegates["7g"] = (string line) => line.CollectNGrams(7);
            delegates["3was"] = (string line) => line.CollectAllSubsetsOfN(3);
        }

        public static Func<string, Sequence[]> GetTextUnitCollector(string tuType)
        {
            return delegates[tuType.ToLowerInvariant()];
        } 
    }
}
