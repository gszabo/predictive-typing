using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredType.Utils
{
    public static class LinePairReader
    {
        public static bool ReadLinePair(TextReader rdr, ref string srcSentence, ref string trgSentence, ref string errorMessage, ref bool isEndOfFile)
        {
            string srcLine = rdr.ReadLine();
            string trgLine = rdr.ReadLine();

            if (srcLine == null || trgLine == null)
            {
                isEndOfFile = true;
                return false;
            }

            if (!srcLine.StartsWith("src", StringComparison.InvariantCultureIgnoreCase) ||
                !trgLine.StartsWith("trg", StringComparison.InvariantCultureIgnoreCase))
            {
                errorMessage = "Line not starting with SRC or TRG.";
                return false;
            }

            int srcHashPos = srcLine.IndexOf('#'), trgHashPos = trgLine.IndexOf('#');

            int srcNum, trgNum;

            if (!int.TryParse(srcLine.Substring(3, srcHashPos - 3), out srcNum) ||
                !int.TryParse(trgLine.Substring(3, trgHashPos - 3), out trgNum) || (srcNum != trgNum))
            {
                errorMessage = "Line number not the same on source and target side.";
                return false;
            }


            srcSentence = srcLine.Substring(srcHashPos + 1);
            trgSentence = trgLine.Substring(trgHashPos + 1);
            return true;
        }
    }
}
