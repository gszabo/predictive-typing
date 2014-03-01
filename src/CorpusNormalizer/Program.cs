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
            string inputPath = null, trainPath = null, evalPath = null;
            long trainCount = 0, evalCount = 0;

            foreach (string s in args)
            {
                if (s.StartsWith("-") && s.IndexOf('=') >= 0)
                {
                    string[] parts = s.Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries);
                    string key = parts[0].Trim('-');
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "traincount":
                            trainCount = long.Parse(value);
                            break;
                        case "evalcount":
                            evalCount = long.Parse(value);
                            break;
                        case "inputfile":
                            inputPath = value;
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

            Console.WriteLine("Starting normalizing {0} for {1} training and {2} evaluation lines.\nExisting files will be overwritten.", inputPath, trainCount, evalCount);

            using (var rdr = new StreamReader(inputPath, Encoding.UTF8))
            {
                createFile(trainPath, rdr, trainCount);
                Console.WriteLine("{0} training file created.", trainPath);
                
                createFile(evalPath, rdr, evalCount);
                Console.WriteLine("{0} evaluation file created.", evalPath);
            }

            //Console.ReadLine();
        }

        static void createFile(string outFilePath, StreamReader rdr, long lineCount)
        {
            using (var wrt = new StreamWriter(outFilePath, false, Encoding.UTF8))
            {
                long writtenCount = 0;

                while (writtenCount < lineCount)
                {
                    string lineToWrite = normalizeLine(rdr.ReadLine());
                    
                    // need to keep the empty lines too for the bilingual case
                    wrt.WriteLine(lineToWrite);
                    writtenCount++;
                                        
                }
            }
        }

        static string normalizeLine(string line)
        {
            var sb = new StringBuilder();

            // mondatvégi pont és egyebek leszedése
            line = line.Trim('.', '(', ')', '{', '}', '[', ']', '<', '>');
            
            var lineFormatter = new StringBuilder(line, 3*line.Length);

            // replace problematic special characters
            lineFormatter.Replace("ß", "ss").Replace("œ", "oe").Replace("æ", "ae").Replace("\u00AD", "");

            // árva 's-k eltávolítása
            lineFormatter.Replace("' s ", " ");

            // leszedjuk a kellemetlen karaktereket a szavakról
            char[] uncomfyChars = new char[] {'&', '@', '#', ';', ':', '?', '!', '"', ',', '{', '}', '[', ']', '(', ')', '<', '>'};
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
    }
}
