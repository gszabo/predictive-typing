using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorpusNormalizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcPath = null, trgPath = null, trainPath = null, evalPath = null;
            long trainCount = 0, evalCount = 0;

            if (args.Length == 1 && args[0].Equals("-debug", StringComparison.InvariantCultureIgnoreCase))
            {
                debugMode();
                return;
            }

            foreach (string s in args)
            {
                if ((s.StartsWith("-") || s.StartsWith("/")) && s.IndexOf('=') >= 0)
                {
                    string[] parts = s.Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries);
                    string key = parts[0].TrimStart('-', '/').ToLowerInvariant();
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "traincount":
                            trainCount = long.Parse(value);
                            break;
                        case "evalcount":
                            evalCount = long.Parse(value);
                            break;
                        case "sourcefile":
                        case "srcfile":
                            srcPath = value;
                            break;
                        case "targetfile":
                        case "trgfile":
                            trgPath = value;
                            break;
                        case "trainfile":
                            trainPath = value;
                            break;
                        case "evalfile":
                            evalPath = value;
                            break;
                    }
                }
            }

            Console.WriteLine("Starting normalizing ({0}, {1}) for {2} training and {3} evaluation lines.\nExisting files will be overwritten.", Path.GetFileName(srcPath), Path.GetFileName(trgPath), trainCount, evalCount);

            using (var srcReader = new StreamReader(srcPath, Encoding.UTF8))
            using (var trgReader = new StreamReader(trgPath, Encoding.UTF8))
            {
                createFile(trainPath, srcReader, trgReader, trainCount);
                Console.WriteLine("{0} training file created.", trainPath);
                
                createFile(evalPath, srcReader, trgReader, evalCount);
                Console.WriteLine("{0} evaluation file created.", evalPath);
            }

            //Console.ReadLine();
        }

        static void createFile(string outFilePath, TextReader srcReader, TextReader trgReader, long lineCount)
        {
            if (lineCount <= 0)
            {
                throw new ArgumentOutOfRangeException("lineCount", lineCount, "Value should be a positive integer number.");
            }

            // number of digits that the line numbers can be written in
            int numOfDigits = ((int)Math.Floor(Math.Log10(lineCount))) + 1;
            string srcLineFormat = "SRC{0:D" + numOfDigits + "}#{1}";
            string trgLineFormat = "TRG{0:D" + numOfDigits + "}#{1}";

            using (var wrt = new StreamWriter(outFilePath, false, Encoding.UTF8))
            {
                long writtenCount = 0;
                
                while (writtenCount < lineCount)
                {
                    string srcLine = normalizeLine(srcReader.ReadLine());
                    string trgLine = normalizeLine(trgReader.ReadLine());

                    // if any of the lines is empty, skip to the next
                    if (string.IsNullOrWhiteSpace(srcLine) || string.IsNullOrWhiteSpace(trgLine))
                    {
                        continue;
                    }
                    
                    wrt.WriteLine(srcLineFormat, writtenCount + 1, srcLine);
                    wrt.WriteLine(trgLineFormat, writtenCount + 1, trgLine);
                    writtenCount++;
                }
            }
        }

        static string normalizeLine(string line)
        {
            var sb = new StringBuilder();

            char[] uncomfyChars = { '&', '@', '#', ';', ':', '.', '?', '!', '"', ',', '{', '}', '[', ']', '(', ')', '<', '>' };

            // mondatvégi pont és egyebek leszedése
            line = line.Trim(uncomfyChars);

            // convert to lower case
            line = line.ToLowerInvariant();
            
            var lineFormatter = new StringBuilder(line, 3*line.Length);

            // replace problematic special characters
            lineFormatter.Replace("ß", "ss").Replace("œ", "oe").Replace("æ", "ae").Replace("\u00AD", "");

            // árva 's-k eltávolítása
            lineFormatter.Replace("' s ", " ");

            // leszedjuk a kellemetlen karaktereket a szavakról
            foreach (var uncomfyChar in uncomfyChars)
            {
                lineFormatter.Replace(uncomfyChar.ToString(), " " + uncomfyChar.ToString() + " ");
            }

            string[] wordTokens = lineFormatter.ToString().Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            //if (wordTokens.Length == 0)
            //    return "";

            bool strongWS = false;

            foreach (string wordToken in wordTokens)
            {
                if (wordToken.Length > 1)
                {
                    sb.Append(strongWS ? "|" : " ").Append(wordToken);
                    strongWS = false;
                }
                else
                {
                    // wordToken.Length == 1 here
                    char c = wordToken[0];
                    
                    if (char.IsLetter(c))
                    {
                        sb.Append(strongWS ? "|" : " ").Append(wordToken);
                        strongWS = false;                        
                    }
                    else
                    {
                        strongWS = true;
                    }
                }
            }

            // remove first space or |
            if (sb.Length >= 1) 
                sb.Remove(0, 1);

            return sb.ToString();
        }

        private static void debugMode()
        {
            Console.WriteLine("Debug mode. Please enter a line, then the normalized line will be displayed. Exit with the 'exit' line.");
            string line;
            while (!"exit".Equals(line = Console.ReadLine(), StringComparison.InvariantCultureIgnoreCase) && line != null)
            {
                Console.WriteLine(normalizeLine(line));
                Console.WriteLine();
            }
        }
    }
}
