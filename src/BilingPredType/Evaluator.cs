using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilingPredType
{
    public class Evaluator
    {
        private readonly string srcPath, trgPath;

        private readonly Engine engine;

        public Evaluator(string srcPath, string trgPath, Engine engine)
        {
            this.srcPath = srcPath;
            this.trgPath = trgPath;
            this.engine = engine;
        }

        public EvalResult Evaluate()
        {
            ulong sumSentenceLen = 0;
            float sumCoverage = 0.0f, sumKeyStrokeSave = 0.0f;
            uint lineCnt = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            using (var srcReader = new StreamReader(srcPath, Encoding.UTF8))
            using (var trgReader = new StreamReader(trgPath, Encoding.UTF8))
            {
                string srcLine, trgLine;

                while ((srcLine = srcReader.ReadLine()) != null &&
                       (trgLine = trgReader.ReadLine()) != null)
                {
                    lineCnt++;

                    sumSentenceLen += (ulong)trgLine.Length;

                    if ((lineCnt % 200) == 0)
                        Console.WriteLine(lineCnt);

                    LookupHit[] hits = engine.Lookup(srcLine);

                    //sumCoverage += calcCoverage(trgLine, hits);
                    sumKeyStrokeSave += calcKeyStrokeSave(trgLine, hits);
                }
            }

            sw.Stop();

            return new EvalResult()
                {
                    EvalTime = sw.Elapsed,
                    //AvgCoverage = sumCoverage / lineCnt,
                    AvgKeyStrokeSave = sumKeyStrokeSave / lineCnt,
                    AvgSentenceLength = ((float)sumSentenceLen) / lineCnt,
                    SentenceCount = lineCnt
                };
        }

        //private LookupHit[] getHits(string srcLine)
        //{
        //    LookupHit[] hits = engine.Lookup(srcLine);
        //    return hits;
        //}

        //private Dictionary<string, string[]> lookupCache = new Dictionary<string, string[]>();

        Dictionary<string, string[]> lookupCache = new Dictionary<string, string[]>();

        private float calcKeyStrokeSave(string trgLine, LookupHit[] hits)
        {
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
            List<int> charMap = new List<int>(trgLine.Length);
            // initialize the mapping
            foreach (var ch in trgLine)
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

                lookupCache.Clear();

                while (!lineEnd)
                {
                    // megkeres a következő értelmes karaktert
                    while (pos < trgLine.Length && charMap[pos] == -1)
                        pos++;

                    // ha a sor végére értünk, kilép a ciklusból
                    if (pos >= trgLine.Length)
                        break;

                    // a completionba beletartozik a prefix is

                    int prefixLen = 1;
                    string chosenCompletion = null;
                    int chosenCompletionIndex = -1;
                    while (chosenCompletion == null)
                    {
                        // a sor végére értünk
                        if ((pos + prefixLen) >= trgLine.Length)
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

                        string prefix = trgLine.Substring(pos, prefixLen);

                        string[] completions;
                        if (!lookupCache.TryGetValue(prefix, out completions))
                        {
                            completions = hits.Where(eh => eh.TranslationHint.StartsWith(prefix) &&
                                                             eh.TranslationHint.Length > prefixLen)
                                                .OrderByDescending(eh => eh.Score)
                                                .Take(6)
                                                .Select(eh => eh.TranslationHint)
                                                .ToArray();
                            lookupCache.Add(prefix, completions);
                        }

                        // ha üres completions tömb jön vissza, akkor hosszabb prefixre se jön találat, ugrás a köv szóra
                        if (completions.Length == 0)
                        {
                            while (pos < trgLine.Length && charMap[pos] != -1)
                                pos++;
                            break;
                        }

                        // leghosszabb completion keresése
                        for (int i = 0; i < completions.Length; i++)
                        {
                            // csak azokat szúrjuk be, amelyik hosszabb a prefixnél
                            if (trgLine.Substring(pos).StartsWith(completions[i]))
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
                            //    string.Format("------\nTarget string: {0}\nMatch: {1}\n------", line, chosenCompletion));
                            //savedStroke = 0;
                            return 0.0f;
                        }

                        savedStroke += chosenCompletion.Length - prefixLen - chosenCompletionIndex;

                        pos += chosenCompletion.Length;
                        // lehet, hogy a completion egy szónak a közepén fejeződött be, a maradékot átugorjuk
                        while (pos < trgLine.Length && charMap[pos] != -1)
                            pos++;
                    }
                }

                savedRatio = ((float)savedStroke) / charMap.Count(f => f != -1);
            }

            //if (savedRatio > 1)
            //    Debugger.Break();

            return savedRatio;
        }

        private float calcCoverage(string trgLine, LookupHit[] hits)
        {
            // csere | szóközre, mert úgy egyszerűbb és target oldalon a coverage szempontjából mindegy
            trgLine = trgLine.Replace('|', ' ');

            float covRatio = 0.0f;

            const string ignoreChar = " ";

            List<int> charMap = new List<int>(trgLine.Length);

            foreach (char ch in trgLine)
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

            Dictionary<char, List<int>> positions = new Dictionary<char, List<int>>();

            int pos = 0;
            while ((pos = trgLine.IndexOf(' ', pos)) >= 0)
            {
                if (trgLine.Length > pos + 1)
                {
                    if (!positions.ContainsKey(trgLine[pos + 1]))
                        positions[trgLine[pos + 1]] = new List<int>();
                    positions[trgLine[pos + 1]].Add(pos);
                }
                pos++;
            }

            if (characterCount > 0)
            {
                //LookupHit[] hits = engine.Lookup(srcLine);

                foreach (LookupHit hit in hits)
                {
                    // iterate through all the matches for this hit in the target
                    int offset = 0;
                    pos = 0;

                    //while ((pos = targetString.IndexOf(" " + hit.TranslationHint, offset)) >= 0)
                    while ((pos = findPosByDictionary(hit.TranslationHint, positions, offset, trgLine)) >= 0)
                    {
                        // map the letters for this hit
                        // +1 is because of the extra space
                        for (int j = pos + 1; j < pos + hit.TranslationHint.Length + 1; j++)
                        {
                            if (charMap[j] == 0)
                            {
                                charMap[j] = 1;
                            }
                        }
                        // navigate to the start of the next word
                        offset = pos + hit.TranslationHint.Length + 1;
                        // skipping the rest of the word which was not covered by the hit
                        int nextStartPos = 0;
                        if ((nextStartPos = trgLine.IndexOf(' ', offset)) >= 0)
                        {
                            offset = nextStartPos;
                        }
                    }
                }

                covRatio = (float)charMap.Count(f => f == 1) / charMap.Count(f => f != -1);
            }

            return covRatio;
        }

        private int findPosByDictionary(string str, Dictionary<char, List<int>> dict, int offset, string target)
        {
            if (!dict.ContainsKey(str[0])) return -1;
            foreach (int i in dict[str[0]])
                if (i >= offset && target.Substring(i + 1).StartsWith(str))
                    return i;
            return -1;
        }
    }

    public class EvalResult
    {
        public TimeSpan EvalTime;
        public uint SentenceCount;
        public float AvgSentenceLength;
        public float AvgKeyStrokeSave;
        public float AvgCoverage;
    }
}
