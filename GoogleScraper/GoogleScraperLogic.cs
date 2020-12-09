using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GoogleScraper_Parser;
using Newtonsoft.Json;

namespace PlansParser.GoogleScraper
{
    class GoogleScraperLogic
    {
        private readonly PlansDownloaderForm _form;
        private readonly string _keywordsFilePath;
        private readonly string _scraperFilePath;
        private string SearchEngine = "bing";
        private readonly string _fileDir;
        private readonly string _tmpDir;
        private const string ScraperResultFileName = "scraper_output.json";
        private string _resultFileName = "GoogleScraper_results.csv";
        private string _failedQueriesFileName = "";

        private readonly HashSet<string> _oldKeys = new HashSet<string>();
        private FileInfo[] _tmpKewordsFiles;

        public GoogleScraperLogic(string fileDir, PlansDownloaderForm form, string keywordsFilePath)
        {
            _form = form;
            _keywordsFilePath = keywordsFilePath;
            _fileDir = fileDir;
            _scraperFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Drivers", "GoogleScraper.exe");
            _tmpDir = _fileDir + "_tmpkwrds";
        }

        private readonly Dictionary<string, Dictionary<string, double>> _matches = new Dictionary<string, Dictionary<string, double>>();


        public void StartProcessing()
        {
            try
            {
                _resultFileName = _keywordsFilePath.Replace(".", "_Result.");
                _failedQueriesFileName = _keywordsFilePath.Replace(".", "_Failed.");

                _form.SetProgressLabel("Preparing keywords...");
                PrepareKeywords();

                if (_tmpKewordsFiles.Length < 1)
                {
                    MessageBox.Show("Keywords temporary folder is empty");
                    return;
                }

                _form.SetProgressBarInit(_tmpKewordsFiles.Length);
                int i = 1;
                foreach (FileInfo file in _tmpKewordsFiles)
                {
                    _form.SetProgressBarStep(i);
                    _form.SetProgressLabel("Scraping (" + i + "/" + _tmpKewordsFiles.Length + ")...");
                    if (!RunGoogleScraper(file))
                    {
                        continue;
                    }
                    _form.SetProgressLabel("Parsing results (" + i + "/" + _tmpKewordsFiles.Length + ")...");
                    Parse();
                    i++;
                }

                _form.SetProgressLabel("File was parsed succesfully.");
            }
            catch (Exception ex)
            {
                errorHandler(ex.Message + " / " + ex.StackTrace);
            }
        }

        private void PrepareKeywords()
        {
            DirectoryInfo di;

            if (Directory.Exists(_tmpDir))
            {
                di = new DirectoryInfo(_tmpDir);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
            else
            {
                Directory.CreateDirectory(_tmpDir);
            }

            List<string> chunk = new List<string>();
            string tmpFilename;

            string[] lines = File.ReadAllLines(_keywordsFilePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].ToLower().EndsWith(" fund")) //if str ends with " fund", do " fund fund". This is not mistake. 
                {
                    lines[i] += " fund";
                }
                else if (!Regex.IsMatch(lines[i].ToLower(), @"\bfund\b"))
                {
                    lines[i] += " fund";
                }
                chunk.Add(lines[i]);

                if (chunk.Count == 1000)
                {
                    tmpFilename = "_tmp_" + (i + 1) + ".txt";
                    File.WriteAllLines(_tmpDir + "/" + tmpFilename, chunk);
                    chunk.Clear();
                }

            }
            tmpFilename = "_tmp_" + (lines.Length + 1) + ".txt";
            File.WriteAllLines(_tmpDir + "/" + tmpFilename, chunk);

            di = new DirectoryInfo(_tmpDir);
            _tmpKewordsFiles = di.GetFiles();
        }

        private bool RunGoogleScraper(FileInfo file)
        {
            string arguments = " -m http --keyword-file \"" + file.FullName + "\" --num-workers 10  --output-filename \"" + _fileDir + ScraperResultFileName + "\" --search-engines \"" + SearchEngine + "\""; // bing
            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo(_scraperFilePath, arguments);
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);

            process.WaitForExit();
            var exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                MessageBox.Show("Google Scraper was exited with code " + exitCode);
                return false;
            }

            //clear cache
            if (File.Exists(_fileDir + "google_scraper.db"))
            {
                File.Delete(_fileDir + "google_scraper.db");
            }

            if (Directory.Exists(_fileDir + ".scrapecache"))
            {
                Directory.Delete(_fileDir + ".scrapecache", true);
            }
            return true;
        }

        private void Parse()
        {
            _matches.Clear();
            LoadOldKeysFromFile();

            string fileContent;
            using (StreamReader strReader = new StreamReader(_fileDir + ScraperResultFileName))
            {
                fileContent = strReader.ReadToEnd();
            }

            List<ScraperQuery> list = JsonConvert.DeserializeObject<List<ScraperQuery>>(fileContent);

            foreach (ScraperQuery query in list)
            {
                if (_oldKeys.Contains(query.query))
                {
                    Log("Query \"" + query.query + "\" skipped because of duplicate");
                    continue;
                }

                if (query.status != "successful" || query.results.Count == 0)
                {
                    if (query.status != "successful")
                    {
                        Log(query.query, "failed_queries.log");
                    }
                    else if (query.results.Count == 0)
                    {
                        Log(query.query, "empty_queries.log");
                    }

                    AddToFailedQueriesFile(query);
                    continue;
                }

                ParseTickersMatches(query);

            }

            WriteResultsToFile();
        }

        private void AddToFailedQueriesFile(ScraperQuery query)
        {
            string queryStr = query.query;
            if (queryStr.ToLower().EndsWith(" fund"))
            {
                queryStr = queryStr.Substring(0, queryStr.Length - 5);
            }
            File.AppendAllText(_failedQueriesFileName, queryStr + Environment.NewLine);
        }

        private void LoadOldKeysFromFile()
        {
            string fullPath = _resultFileName;
            if (!File.Exists(fullPath))
                return;

            try
            {
                using (StreamReader sr = new StreamReader(fullPath))
                {
                    string currentLine;
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        int pos = currentLine.IndexOf("|");
                        if (pos > -1)
                        {
                            _oldKeys.Add(currentLine.Substring(0, pos));

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DialogResult dr = MessageBox.Show(ex.Message + Environment.NewLine + "Try again?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (dr == DialogResult.Yes)
                {
                    LoadOldKeysFromFile();
                }
                else
                {
                    throw new Exception("Old results file is exists, but locked by another program. Please close it and try again.");
                }
            }

        }

        private void WriteResultsToFile()
        {
            var csv = new StringBuilder();
            if (!File.Exists(_resultFileName))
            {
                csv.AppendLine("Name|Ticker|Rank");
            }

            foreach (var queryMatch in _matches)
            {
                string query = queryMatch.Key;
                if (query.ToLower().EndsWith(" fund"))
                {
                    query = query.Substring(0, query.Length - 5);
                }

                foreach (var match in queryMatch.Value)
                {
                    csv.AppendLine(string.Format("{0}|{1}|{2}", query, match.Key, match.Value));
                }
            }

            File.AppendAllText(_resultFileName, csv.ToString());
        }

        private void ParseTickersMatches(ScraperQuery query)
        {
            Regex expression = new Regex(@"[A-Z]{3,5}|[A-Z0-9]{6,10}");

            Dictionary<string, double> queryMatches = new Dictionary<string, double>();
            if (_matches.ContainsKey(query.query))
            {
                queryMatches = _matches[query.query];
            }
            else
            {
                _matches.Add(query.query, queryMatches);
            }

            int i = query.results.Count + 1;

            foreach (ScraperResult item in query.results)
            {

                string searchStr = item.snippet + " " + item.title;
                double linkRating = 1;
                //baidu returns links to themselves, that looks like http://www.baidu.com/link?url=LbVBBXNIgC71iKD1YJXa3zFf4... (~ 40 symbols URL), thats not useful for search
                if (!item.link.Contains("baidu.com"))
                {
                    searchStr += " " + item.link;
                    linkRating = GetLinkRating(item.link);
                }
                // i = relevance by desc
                linkRating -= (double)1 / i;

                var results = expression.Matches(searchStr);

                foreach (var match in results)
                {
                    string key = match.ToString();
                    if (queryMatches.ContainsKey(key))
                    {
                        queryMatches[key] += linkRating;
                    }
                    else
                    {
                        queryMatches.Add(key, linkRating);
                    }
                }
                i--;
            }
        }

        public static double GetLinkRating(string link)
        {
            double rating = 0.5;

            if (link != null)
            {
                rating = 1;

                if (link.ToLower().Contains("morningstar")) rating = 1.5;
                if (link.ToLower().Contains("fundresearch")) rating = 1.2;
                if (link.ToLower().Contains("finance.yahoo")) rating = 1.5;
                if (link.ToLower().Contains("mutualfundstore")) rating = 1.2;
                if (link.ToLower().Contains("bloomberg")) rating = 1.5;
            }

            return rating;
        }

        private void Log(string msg)
        {
            File.AppendAllText(_fileDir + "/googlescrapper.log", DateTime.Now + " : " + msg + Environment.NewLine);
        }

        private void Log(string msg, string fileName)
        {
            File.AppendAllText(_fileDir + "/" + fileName, DateTime.Now + " : " + msg + Environment.NewLine);
        }

        private void errorHandler(string error)
        {
            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
