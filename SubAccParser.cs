using PlansParser;
using RixtremaWS;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ScenariosMaker
{
    class SubAccParser
    {
        private readonly ToolStripProgressBar prgLine;
        private readonly ToolStripStatusLabel prgLabel;

        public SubAccParser(ToolStripProgressBar prgLine, ToolStripStatusLabel prgLabel)
        {
            this.prgLine = prgLine;
            this.prgLabel = prgLabel;
        }

        public void GetExtraInfoForFound(string foundName)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(string.Format("https://www.sec.gov/cgi-bin/series?company={0}&sc=companyseries", foundName.Replace(" ", "+")));


        }


        public void ParseSubAccFromFolder(string folderPath)
        {

            foreach (var file in Directory.GetFiles(folderPath, "*.xlsx", SearchOption.TopDirectoryOnly))
            {
                var result = ParseSubAccFromFile(file);

                using (var fileWriter = new StreamWriter("result.csv"))
                {
                    result = result.OrderByDescending(x => x.Values.Sum(y => y.Count)).ToList();
                    var allreadyAddedSubAcc = new HashSet<string>();
                    fileWriter.Write("{0},{1},{2}", file, result.FirstOrDefault().Values.Sum(y => y.Count), result.Sum(x => x.Values.Sum(y => y.Count)));
                }
            }


        }


        private List<Dictionary<string, List<string>>> ParseSubAccFromFile(string file)
        {

            var removeSpecCharRegexp = new Regex(@"[^a-z0-9 ]", RegexOptions.Compiled);

            var endOfWord = @"\b";
            var startOfWord = @"\b";
            var wordsToRemove = "fund,class,the".Split(',')
                     .Select(x => new Regex(startOfWord + x + endOfWord, RegexOptions.Compiled))
                     .ToList();

            var removeDuplicateCharRegexp = new Regex(@"[ ]{2,}", RegexOptions.Compiled);

            var removeValInBktRegexp = new Regex(@"\(.*\)", RegexOptions.Compiled);
            string[] tikerMarkWordsArr = { "GROWTH", "CAPITAL", "INCOME", "INVESTMENT", "ALLOCATION", "VALUE", "EQUITY", "INDEX", "INTERNATIONAL", "TOTAL", "SMALL", "INSTITUTIONAL",
                                            "INFLATION", "MARKET", "SELECT", "RETURN", "FINANCIAL", "ASSET", "CORPORATE", "FUND","FUNDS", "ENHANCED", "CONVERTIBLE", "RETIREMENT", "MODERATE",
                                            "BOND", "SHORT", "INVESTORS", "STOCK", "HEALTH", "BALANCED", "GLOBAL", "INSIGHTS", "GOVERNMENT", "EMERGING", "WORLD", "HEALTHCARE", "TREASURY",
                                            "INFO", "REAL", "RESERVES", "MARKETS", "ENERGY", "TECHNOLOGY", "CASH", "RESOURCES", "COMPANY", "LONG", "TERM", "APPRECIATION","HIGH",
                                            "LARGE","MID", "Portfolio"};


            var tickerMarkWordsArrRegex = tikerMarkWordsArr
                     .Select(x => new Regex(startOfWord + x.ToLower() + endOfWord, RegexOptions.Compiled))
                     .ToList();


            var resultIssuers = new List<Dictionary<string, List<string>>>();
            var comboList = new Dictionary<string, List<string>>();

            var probabilityMass = 0;
            string detectedIssuer = null;
            string possibleIsser = null;
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                var dt = XlsxToCsvConverter.XlsxToDataTable(stream, false, 0, 0);
                prgLine.Maximum = dt.Rows.Count;

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        prgLine.Increment(1);

                        Application.DoEvents();
                        var row = dt.Rows[i][j];
                        string rawVal = row.ToString().Trim();
                        string val = rawVal.ToLower();
                        var tickerMarksInVal = tickerMarkWordsArrRegex.Where(regex => regex.IsMatch(val)).ToList();
                        val = removeValInBktRegexp.Replace(val, "");
                        val = removeSpecCharRegexp.Replace(val, "");
                        val = wordsToRemove.Aggregate(val, (current, regex) => regex.Replace(current, ""));
                        val = removeDuplicateCharRegexp.Replace(val, "").Trim();
                        if (string.IsNullOrEmpty(val))
                        {
                            continue;
                        }

                        if (val.Length <= 100)
                        {
                            if (rawVal.Length >= 4 && rawVal.Any(char.IsLetter) && rawVal.All(x => !char.IsLetter(x) || char.IsUpper(x)))
                            {
                                possibleIsser = detectedIssuer ?? rawVal;
                                probabilityMass = probabilityMass > 1 ? probabilityMass : 1;

                            }


                            if (tickerMarksInVal.Count > 0)
                            {
                                possibleIsser = detectedIssuer ?? possibleIsser ?? "";
                                probabilityMass = probabilityMass > 2 ? probabilityMass : 2;
                                comboList.AddIfNotExist(possibleIsser, new List<string>());
                                comboList[possibleIsser].Add(string.Format("{0},{1},{2}", i, j, val));
                                continue;
                            }

                            if (probabilityMass > 0)
                            {
                                probabilityMass--;

                                long longVal;
                                if (val.Length > 5 && !long.TryParse(val, out longVal))
                                {
                                    comboList.AddIfNotExist(detectedIssuer ?? possibleIsser, new List<string>());
                                    comboList[detectedIssuer ?? possibleIsser].Add(string.Format("{0},{1},{2}", i, j, val));
                                }

                                continue;
                            }
                        }

                        if (comboList.Count >= 2 || comboList.Values.Sum(x => x.Count) >= 5)
                        {
                            resultIssuers.Add(comboList);
                        }

                        comboList = new Dictionary<string, List<string>>();
                        detectedIssuer = null;
                        possibleIsser = null;
                        probabilityMass = 0;
                    }

                    // startNewColumn
                    if (comboList.Count >= 2 || comboList.Values.Sum(x => x.Count) >= 5)
                    {
                        resultIssuers.Add(comboList);
                    }

                    comboList = new Dictionary<string, List<string>>();
                    detectedIssuer = null;
                    possibleIsser = null;
                    probabilityMass = 0;
                }


                //finalize 
                if (comboList.Count >= 2 || comboList.Values.Sum(x => x.Count) >= 5)
                {
                    resultIssuers.Add(comboList);
                }
            }

            using (var fileWriter = new StreamWriter(file + "_result.csv"))
            {
                resultIssuers = resultIssuers.OrderByDescending(x => x.Keys.Count).ToList();
                var allreadyAddedSubAcc = new HashSet<string>();

                foreach (var dict in resultIssuers)
                {
                    var uniqList = dict.Values.SelectMany(x => x).Where(value => allreadyAddedSubAcc.Add(value.Split(',')[2])).ToList();
                    if (uniqList.Count() < 5)
                        continue;

                    fileWriter.WriteLine("New Combo. KeyCount {0}, ValueCount {1}", dict.Keys.Count, dict.Values.Sum(x => x.Count));

                    foreach (var issuerToValListPair in dict)
                    {
                        foreach (var value in issuerToValListPair.Value)
                        {
                            fileWriter.WriteLine("{0},{1}", issuerToValListPair.Key, value);
                        }
                    }

                    fileWriter.WriteLine();
                }
            }

            return resultIssuers;
        }
    }
}
