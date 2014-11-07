using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementMaster
{
    static class PlaceholderUtils
    {
        private const string dateFormatString = "yyyy-MM-dd-HH-mm-ss";

        public static string ReplacePlaceholders(this string str, string textUnitType, DateTime now, string scoreCalc = "", float minThreshold = 0, float minScore = 0)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            return str.Replace("{now}", now.ToString(dateFormatString))
                      .Replace("{text_unit}", textUnitType)
                      .Replace("{score}", scoreCalc)
                      .Replace("{min_threshold}", minThreshold.ToString(CultureInfo.InvariantCulture))
                      .Replace("{score_threshold}", minScore.ToString(CultureInfo.InvariantCulture));
        }
    }
}
