using System;
using System.Collections.Generic;
using System.Linq;

namespace PlansParser.GoogleScraper
{
    /// <summary>
    /// Contains approximate string matching
    /// </summary>
    static class LevenshteinDistance
    {
        static readonly HashSet<string> NoiseWords = new HashSet<string>(new[] { "GROWTH", "CAPITAL", "INCOME", "INVESTMENT", "ALLOCATION", "VALUE", "EQUITY", "INDEX", "INTERNATIONAL", "TOTAL", "SMALL", "INSTITUTIONAL",
            "INFLATION", "MARKET", "SELECT", "RETURN", "FINANCIAL", "ASSET", "CORPORATE", "FUNDS", "ENHANCED", "CONVERTIBLE", "RETIREMENT", "MODERATE",
            "BOND", "SHORT", "INVESTORS", "STOCK", "HEALTH", "BALANCED", "GLOBAL", "INSIGHTS", "GOVERNMENT", "EMERGING", "WORLD", "HEALTHCARE", "TREASURY",
            "INFO", "REAL", "RESERVES", "MARKETS", "ENERGY", "TECHNOLOGY", "CASH", "RESOURCES", "COMPANY", "LONG", "TERM", "APPRECIATION", "AND", "THE" ,"HIGH",
            "LARGE","MID"});
        public static bool AtLeastOneWordMatches(string s1, string s2)
        {
            char[] splitpattern = new char[] { ' ', '-', '&', '\\', '/', '(', ')', '[', ']', '{', '}', '\t', ',', '.', ':', ';' };

            bool res = false;
            s1 = s1.ToUpper();
            s2 = s2.ToUpper();

            List<string> splitS1 = s1.Split(splitpattern, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length > 2).ToList();
            var first = new HashSet<string>(splitS1);

            List<string> splitS2 = s2.Split(splitpattern, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length > 2).ToList();
            var second = new HashSet<string>(splitS2);

            //общие слова в 1м и 2м именах
            var intersect = first.Intersect(second);

            //общие слова со словарем
            var intersectDict = intersect.Intersect(NoiseWords);

            //если общих слов меньше 2 или все общие слова из словаря, то плохо
            res = !(intersect.Count() < 2 || intersect.Count() == intersectDict.Count());


            return res;
        }

        public static int ComputeCaseInsensitive(string s, string t)
        {
            return Compute(s.ToLower(), t.ToLower());
        }
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
