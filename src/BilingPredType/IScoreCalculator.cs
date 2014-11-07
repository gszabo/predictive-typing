using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace BilingPredType
{
    /// <summary>
    /// Object that calculates the score of a text unit pair.
    /// </summary>
    interface IScoreCalculator
    {
        /// <summary>
        /// Calculates the score for a (source; target) text unit pair.
        /// </summary>
        /// <param name="src">Source language text unit.</param>
        /// <param name="trg">Target language text unit.</param>
        /// <param name="nSrc">Number of source text unit occurence.</param>
        /// <param name="nTrg">Number of target text unit occurrence.</param>
        /// <param name="nTogether">Number of concomitant occurrences.</param>
        /// <param name="nSentences">Number of sentence pairs.</param>
        /// <returns>The score of the pair.</returns>
        float CalculateScore(Sequence src, Sequence trg, int nSrc, int nTrg, int nTogether, int nSentences);
    }
}
