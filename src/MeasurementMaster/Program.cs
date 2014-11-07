using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MeasurementMaster
{
    class Program
    {
        static void log(string s)
        {
            Console.WriteLine(DateTime.Now.ToString("hh-mm-ss") + ": " + s);
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: <executable file> <configuration file>");
                return;
            }

            log("Reading measurement configuration from: " + args[0]);

            IEnumerable<MeasureInfo> measurementRequests = readConfigFile(args[0]);
            carryOutMeasurements(measurementRequests);

            log("End of MeasureMaster.");
            Console.ReadLine();
        }

        private static IEnumerable<MeasureInfo> readConfigFile(string configPath)
        {
            var result = new List<MeasureInfo>();

            MeasureInfo current = null;

            using (var rdr = new StreamReader(configPath, Encoding.UTF8))
            {
                string line;

                while ((line = rdr.ReadLine()) != null)
                {
                    line = removeCommentFromLine(line);

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if ("[measurement]".Equals(line, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (current != null)
                        {
                            result.Add(current);
                        }
                        current = new MeasureInfo();
                    }
                    else
                    {
                        if (current == null)
                        {
                            throw new InvalidOperationException("Option without measurement section.");
                        }

                        int equalPos = line.IndexOf('=');
                        // it is assumed that the line contains an = character
                        string key = line.Substring(0, equalPos).Trim().ToLowerInvariant();
                        string value = line.Substring(equalPos + 1).Trim();

                        switch (key)
                        {
                            case "description":
                                current.Description = value;
                                break;
                            case "train":
                                current.TrainPath = value;
                                break;
                            case "eval":
                                current.EvalPath = value;
                                break;
                            case "type":
                                current.Type = value.ToLowerInvariant();
                                break;
                            case "min_threshold":
                                current.MinThresholds = splitAndParseFloat(value, ';');
                                break;
                            case "score_threshold":
                                current.ScoreThresholds = splitAndParseFloat(value, ';');
                                break;
                            case "text_unit":
                                current.TextUnits = splitAndTrim(value.ToLowerInvariant(), ';');
                                break;
                            case "score":
                                current.ScoreCalculators = splitAndTrim(value.ToLowerInvariant(), ';');
                                break;
                            case "scoreparams":
                                current.CalculatorParams = splitAndTrim(value.ToLowerInvariant(), ';');
                                break;
                            case "measure":
                                current.Metrics = splitAndTrim(value.ToLowerInvariant(), ';');
                                break;
                            case "trainoutput":
                                current.TrainOutputPath = value;
                                break;
                            case "evaloutput":
                                current.EvalOutputPath = value;
                                break;
                            case "dict":
                                current.DictPattern = value;
                                break;
                            case "log":
                                current.LogPath = value;
                                break;
                        }
                    }
                }

                // add the last one measurement entry in the file
                if (current != null)
                {
                    result.Add(current);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Removes the comment from the line and trims the result.
        /// </summary>
        private static string removeCommentFromLine(string line)
        {
            const string commentMarker = "#";

            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            int commentPos = line.IndexOf(commentMarker, StringComparison.InvariantCultureIgnoreCase);

            if (commentPos >= 0)
            {
                return line.Substring(0, commentPos).Trim();
            }

            return line.Trim();
        }

        private static void carryOutMeasurements(IEnumerable<MeasureInfo> measurementRequests)
        {
            DateTime now = DateTime.Now;

            foreach (MeasureInfo measureInfo in measurementRequests)
            {
                log("Starting \"" + measureInfo.Description + "\"");

                if (measureInfo.Type == "mono")
                {
                    MonolingMeasurer.CarryOut(measureInfo, now);
                }
                else if (measureInfo.Type == "bi")
                {
                    BilingMeasurer.CarryOut(measureInfo, now);
                }
                
                log("End of \"" + measureInfo.Description + "\"");
                log("");
            }
        }

        private static string[] splitAndTrim(string str, char separator)
        {
            return str.Split(new char[] {separator}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }

        private static float[] splitAndParseFloat(string str, char separator)
        {
            return str.Split(new char[] {separator}, StringSplitOptions.RemoveEmptyEntries).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        }
    }
}
