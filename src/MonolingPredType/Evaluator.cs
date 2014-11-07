using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredType.Utils;

namespace MonolingPredType
{
    class Evaluator
    {
        //public class LookupParams
        //{
        //    public int LookupWords;

        //    public int LookupSequences;
        //}

        private readonly string evalPath;

        private readonly Engine engine;

        private readonly string[] evalMetrics;

        private readonly Logger logger;

        //private readonly LookupParams lParams;

        public Evaluator(string evalPath, Engine engine, string[] evalMetrics, Logger logger)
        {
            this.evalPath = evalPath;
            this.engine = engine;
            this.evalMetrics = evalMetrics;
            this.logger = logger;
            //this.lParams = lParams;
        }

        public EvalResult Evaluate()
        {
            ulong lineCnt = 0, sumSentenceLen = 0;
            float sumCoverage = 0.0f, sumSavedKeyStroke = 0.0f;

            bool measureKss = evalMetrics.Contains("kss"), measureCoverage = evalMetrics.Contains("coverage");

            using (var rdr = new StreamReader(evalPath, Encoding.UTF8))
            {
                string srcSentence = null, trgSentence = null, errorMsg = null;
                bool isEndOfFile = false, quit = false;

                while (!quit)
                {
                    quit = !LinePairReader.ReadLinePair(rdr, ref srcSentence, ref trgSentence, ref errorMsg, ref isEndOfFile);

                    if (!quit)
                    {
                        lineCnt++;
                        sumSentenceLen += (ulong)trgSentence.Length;

                        if ((lineCnt % 200) == 0)
                            Console.WriteLine(lineCnt);

                        if (measureCoverage)
                        {
                            sumCoverage += calcCoverageForLine(trgSentence/*, out savedStroke*/);
                        }

                        if (measureKss)
                        {
                            sumSavedKeyStroke += calcKeyStrokeRatio(trgSentence);
                        }
                    }
                    else if (!isEndOfFile)
                    {
                        // error
                        logger.Log("Error in " + evalPath + ". " + errorMsg + " Read sentences: " + lineCnt);
                    }
                }
            }

            return new EvalResult()
                {
                    EvalSentenceCount = lineCnt,
                    AvgCoverage = sumCoverage/lineCnt,
                    AvgKeyStrokeSave = (sumSavedKeyStroke)/lineCnt,
                    AvgSentenceLength = ((float)sumSentenceLen)/lineCnt
                };
        }

        private Dictionary<string, string[]> lookupCache = new Dictionary<string, string[]>(); 

        private Dictionary<string, bool> containCache = new Dictionary<string, bool>(); 

        private float calcCoverageForLine(string line/*, out uint savedStroke*/)
        {
            float covRatio = 0.0f;
            //savedStroke = 0;

            // normalizált a szöveg, nem kell ezeket leszedni
            // leszedem a mondatvégi pontot és WS-t
            //line = line.TrimEnd('.', ' ', '\t');

            // itt a pontot nem kell átugrani, mert lehet pl. "Dr." ami egy token
            //const string ignoreChar = "'\"?!(), \t";
            // kétfajta whitespace-ünk van, a puha és a kemény
            const string ignoreChar = " |";

            // This list represents a mapping for the text of the segment
            // I believe the mapping is necessary because a hit doesn't always cover the whole length of a word
            // Each item represents a character in the string
            // -1: it does not count in mapping (space, dot, etc.); 0: not mapped to a hit; 1: mapped to a hit
            List<int> charMap = new List<int>(line.Length);
            // initialize the mapping
            foreach (var ch in line)
            {
                if (ignoreChar.IndexOf(ch) >= 0)
                {
                    charMap.Add(-1);
                }
                else
                {
                    charMap.Add(0);
                }
            }
            int characterCount = charMap.Count(f => f != -1);

            if (characterCount > 0)
            {
                //int pos = 0;
                //bool lineEnd = false;

                int coveredLen = 0;

                foreach (string word in line.Split(ignoreChar.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    bool isInDict;

                    if (!containCache.TryGetValue(word, out isInDict))
                    {
                        isInDict = engine.IsWordInDict(word);
                        containCache.Add(word, isInDict);
                    }

                    if (isInDict)
                    {
                        coveredLen += word.Length;
                    }
                }

                //while (!lineEnd)
                //{
                //    // megkeres a következő értelmes karaktert
                //    while (pos < line.Length && charMap[pos] == -1)
                //        pos++;

                //    // ha a sor végére értünk, kilép a ciklusból
                //    if (pos >= line.Length)
                //        break;

                //    // a completionba beletartozik a prefix is

                //    int prefixLen = 1;
                //    string chosenCompletion = null;
                //    while (chosenCompletion == null)
                //    {
                //        // a sor végére értünk
                //        if ((pos + prefixLen) >= line.Length)
                //        {
                //            lineEnd = true;
                //            break;
                //        }

                //        // a prefix már egy teljes szó, ha eddig nem jött rá találat, ezután se fog, hagyjuk, menjünk a következő szóra
                //        if (charMap[pos + prefixLen - 1] == -1)
                //        {
                //            pos = pos + prefixLen - 1;
                //            break;
                //        }

                //        string prefix = line.Substring(pos, prefixLen);

                //        // cache lookup, hogy gyorsabbak legyünk
                //        // és itt cache-elünk, nem az engine-ben, hogy más paraméterekkel is lehessen tőle kérdezni
                //        string[] completions;
                //        if (!lookupCache.TryGetValue(prefix, out completions))
                //        {
                //            completions = engine.Complete(prefix, lParams.LookupWords, lParams.LookupSequences);
                //            lookupCache.Add(prefix, completions);
                //        }
                        
                //        // ha üres completions tömb jön vissza, akkor hosszabb prefixre se jön találat, ugrás a köv szóra
                //        if (completions.Length == 0)
                //        {
                //            while (pos < line.Length && charMap[pos] != -1)
                //                pos++;
                //            break;
                //        }

                //        // leghosszabb completion keresése
                //        for (int i = 0; i < completions.Length; i++)
                //        {
                //            if (line.Substring(pos).StartsWith(completions[i]))
                //            {
                //                if (chosenCompletion == null)
                //                    chosenCompletion = completions[i];
                //                else if (chosenCompletion.Length < completions[i].Length)
                //                    chosenCompletion = completions[i];
                //            }
                //        }

                //        if (chosenCompletion == null)
                //            prefixLen++;                        
                //    }

                //    if (chosenCompletion != null)
                //    {
                //        try
                //        {
                //            for (int i = 0; i < chosenCompletion.Length; i++)
                //            {
                //                if (charMap[pos + i] == 0)
                //                    charMap[pos + i] = 1;
                //            }
                //        }
                //        catch (ArgumentOutOfRangeException e)
                //        {
                //            Logger.Create(Path.GetDirectoryName(evalPath)).Log(
                //                string.Format("------\nTarget string: {0}\nMatch: {1}\n------", line, chosenCompletion));
                //            //savedStroke = 0;
                //            return 0.0f;
                //        }

                //        //savedStroke += (uint)(chosenCompletion.Length - prefixLen);

                //        pos += chosenCompletion.Length;
                //        // lehet, hogy a completion egy szónak a közepén fejeződött be, a maradékot átugorjuk
                //        while (pos < line.Length && charMap[pos] != -1)
                //            pos++;
                //    }                    
                //}

                //covRatio = ((float)charMap.Count(f => f == 1)) / charMap.Count(f => f != -1);
                covRatio = ((float)coveredLen) / charMap.Count(f => f != -1);
            }

            return covRatio;
        }

        private float calcKeyStrokeRatio(string line)
        {
            line = line.ToLower();

            float savedRatio = 0.0f;
            int savedStroke = 0;

            // normalizált a szöveg, nem kell ezeket leszedni
            // leszedem a mondatvégi pontot és WS-t
            //line = line.TrimEnd('.', ' ', '\t');

            // itt a pontot nem kell átugrani, mert lehet pl. "Dr." ami egy token
            //const string ignoreChar = "'\"?!(), \t";
            // kétfajta whitespace-ünk van, a puha és a kemény
            const string ignoreChar = " |";

            // This list represents a mapping for the text of the segment
            // I believe the mapping is necessary because a hit doesn't always cover the whole length of a word
            // Each item represents a character in the string
            // -1: it does not count in mapping (space, dot, etc.); 0: not mapped to a hit; 1: mapped to a hit
            List<int> charMap = new List<int>(line.Length);
            // initialize the mapping
            foreach (var ch in line)
            {
                if (ignoreChar.IndexOf(ch) >= 0)
                {
                    charMap.Add(-1);
                }
                else
                {
                    charMap.Add(0);
                }
            }
            int characterCount = charMap.Count(f => f != -1);

            if (characterCount > 0)
            {
                int pos = 0;
                bool lineEnd = false;

                while (!lineEnd)
                {
                    // megkeres a következő értelmes karaktert
                    while (pos < line.Length && charMap[pos] == -1)
                        pos++;

                    // ha a sor végére értünk, kilép a ciklusból
                    if (pos >= line.Length)
                        break;

                    // a completionba beletartozik a prefix is

                    int prefixLen = 1;
                    string chosenCompletion = null;
                    int chosenCompletionIndex = -1;
                    while (chosenCompletion == null)
                    {
                        // a sor végére értünk
                        if ((pos + prefixLen) >= line.Length)
                        {
                            lineEnd = true;
                            break;
                        }

                        // a prefix már egy teljes szó, ha eddig nem jött rá találat, ezután se fog, hagyjuk, menjünk a következő szóra
                        if (charMap[pos + prefixLen - 1] == -1)
                        {
                            pos = pos + prefixLen - 1;
                            break;
                        }

                        string prefix = line.Substring(pos, prefixLen);

                        // cache lookup, hogy gyorsabbak legyünk
                        // és itt cache-elünk, nem az engine-ben, hogy más paraméterekkel is lehessen tőle kérdezni
                        string[] completions;
                        if (!lookupCache.TryGetValue(prefix, out completions))
                        {
                            //completions = engine.Complete(prefix, lParams.LookupWords, lParams.LookupSequences);
                            completions = engine.Complete(prefix, 6);
                            lookupCache.Add(prefix, completions);
                        }

                        // ha üres completions tömb jön vissza, akkor hosszabb prefixre se jön találat, ugrás a köv szóra
                        if (completions.Length == 0)
                        {
                            while (pos < line.Length && charMap[pos] != -1)
                                pos++;
                            break;
                        }

                        // leghosszabb completion keresése
                        for (int i = 0; i < completions.Length; i++)
                        {
                            // csak azokat szúrjuk be, amelyik hosszabb a prefixnél
                            if (line.Substring(pos).StartsWith(completions[i]) && completions[i].Length > prefixLen)
                            {
                                if (chosenCompletion == null)
                                {
                                    chosenCompletion = completions[i];
                                    chosenCompletionIndex = i;
                                }
                                else if (chosenCompletion.Length < completions[i].Length)
                                {
                                    chosenCompletion = completions[i];
                                    chosenCompletionIndex = i;
                                }
                            }
                        }

                        if (chosenCompletion == null)
                            prefixLen++;
                    }

                    if (chosenCompletion != null)
                    {
                        try
                        {
                            for (int i = 0; i < chosenCompletion.Length; i++)
                            {
                                if (charMap[pos + i] == 0)
                                    charMap[pos + i] = 1;
                            }
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            //Logger.Create(Path.GetDirectoryName(evalPath)).Log(
                            logger.Log(
                                string.Format("------\nTarget string: {0}\nMatch: {1}\n------", line, chosenCompletion));
                            //savedStroke = 0;
                            return 0.0f;
                        }

                        savedStroke += chosenCompletion.Length - prefixLen - chosenCompletionIndex;

                        pos += chosenCompletion.Length;
                        // lehet, hogy a completion egy szónak a közepén fejeződött be, a maradékot átugorjuk
                        while (pos < line.Length && charMap[pos] != -1)
                            pos++;
                    }
                }

                savedRatio = ((float)savedStroke) / charMap.Count(f => f != -1);
            }

            //if (savedRatio > 1)
            //    Debugger.Break();

            return savedRatio;
        }

        //private int findPosByDictionary(string str, Dictionary<char, List<int>> dict, int offset, string target)
        //{
        //    if (!dict.ContainsKey(str[0])) return -1;
        //    foreach (int i in dict[str[0]])
        //        if (i >= offset && target.Substring(i + 1).StartsWith(str))
        //            return i;
        //    return -1;
        //}
    }

    class EvalResult
    {
        public ulong EvalSentenceCount;

        public float AvgKeyStrokeSave;

        public float AvgCoverage;

        public float AvgSentenceLength;
    }
}
