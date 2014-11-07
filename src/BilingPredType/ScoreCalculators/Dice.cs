using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace BilingPredType.ScoreCalculators
{
    /// <summary>
    /// Calculates the Dice coefficient.
    /// </summary>
    class Dice : IScoreCalculator
    {
        public float CalculateScore(Sequence src, Sequence trg, int nSrc, int nTrg, int nTogether, int nSentences)
        {
            // values of a contingency table
            //int n11 = nTogether, n10 = nSrc - nTogether, n01 = nTrg - nTogether;

            return 2.0f*nTogether/(nSrc + nTrg);
        }
    }
}
