using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace BilingPredType.ScoreCalculators
{
    class DiceLen : IScoreCalculator
    {
        /// <summary>
        /// Average ratio of the length of text and translation (trg len / src len).
        /// </summary>
        private readonly float lenRatio;

        public DiceLen(float lenRatio)
        {
            this.lenRatio = lenRatio;
        }

        public float CalculateScore(Sequence src, Sequence trg, int nSrc, int nTrg, int nTogether, int nSentences)
        {
            float diceScore = 2.0f*nTogether/(nSrc + nTrg);

            float avgLenTranslation = lenRatio*src.Text.Length;

            return diceScore*factor((trg.Text.Length - avgLenTranslation) / avgLenTranslation);
        }

        private static float factor(float x)
        {
            if (x < -1)
            {
                throw new ArgumentOutOfRangeException("x", "Difference ratio cannot be less than -1.");
            }

            // return value should be around 1.0 when x is around 
            throw new NotImplementedException();
        }
    }
}
