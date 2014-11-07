using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilingPredType.ScoreCalculators;

namespace BilingPredType
{
    static class ScoreCalculatorFactory
    {
        public static IScoreCalculator GetCalculatorByName(string name, string param)
        {
            switch (name)
            {
                case "dice":
                    return new Dice();
                case "dicelen":
                    return new DiceLen(float.Parse(param));
                default:
                    throw new NotSupportedException("Calculator type not supported: " + name);
            }
        }
    }
}
