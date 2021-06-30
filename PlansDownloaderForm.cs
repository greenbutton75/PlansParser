using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Flurl;
using Flurl.Http;
using HtmlAgilityPack;
using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;
using PlansParser.FileDownloader;
using PlansParser.GoogleScraper;
using PlansParser.Properties;
using ZipFile = System.IO.Compression.ZipFile;
using LoaderFundHolders;

namespace PlansParser
{
    public partial class PlansDownloaderForm : Form
    {
        private readonly QualityControl _qualityControl;
        static object locker = new object();
        List<string> providers = new List<string>();

        public PlansDownloaderForm()
        {
            InitializeComponent();
            Constants.BaseFolderTextBox = BaseSourceFolderDirTextBox;
            _qualityControl = new QualityControl();
            Logger.SetAdditionalOutputForLogger(s => this.Invoke(() => txtFunds.AppendText(s + Environment.NewLine)));
            FlurlHelper.SetAdditionalOutputForLogger(s => prgLabel.Text = s.ToString());
            BaseSourceFolderDirTextBox.Text = Settings.Default.BaseDirFolder;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            #region providers
            providers.Add("mass mu");
            providers.Add("se!");
            providers.Add("etfs");
            providers.Add("rowe");

            providers.Add("r6");
            providers.Add("r-6");
            providers.Add("r5");
            providers.Add("r-5");
            providers.Add("r4");
            providers.Add("r-4");
            providers.Add("r3");
            providers.Add("r-3");
            providers.Add("r2");
            providers.Add("r-2");
            providers.Add("r1");
            providers.Add("r-1");

            providers.Add("2010");
            providers.Add("2015");
            providers.Add("2020");
            providers.Add("2025");
            providers.Add("2030");
            providers.Add("2035");
            providers.Add("2040");
            providers.Add("2045");
            providers.Add("2050");
            providers.Add("2055");
            providers.Add("2060");
            providers.Add("2065");
            providers.Add("2070");
            providers.Add("2075");
            providers.Add("2080");
            providers.Add("2085");
            providers.Add("2090");
            providers.Add("2095");

            providers.Add("flexpath");
            providers.Add("parnass");
            providers.Add("avenu");
            providers.Add("doubleline");
            providers.Add("odysse");
            providers.Add("baird");
            providers.Add("champla");
            providers.Add("babson");
            providers.Add("davenpo");
            providers.Add("diamond");
            providers.Add("dreihaus ");
            providers.Add("fidelity");
            providers.Add("northern");
            providers.Add("morley");
            providers.Add("emerald");
            providers.Add("fairpo");
            providers.Add("state str");
            providers.Add("oppenh");
            providers.Add("jpmorg");
            providers.Add("blackro");
            providers.Add("black ro");
            providers.Add("vang");
            providers.Add("loomis");
            providers.Add("pimco");
            providers.Add("goldman");
            providers.Add("jennis");
            providers.Add("cref");
            providers.Add("abbet");
            providers.Add("invesco");
            providers.Add("tiaa");
            providers.Add("federated");
            providers.Add("hancock");
            providers.Add("janus");
            providers.Add("american century");
            providers.Add("american funds");
            providers.Add("principal");
            providers.Add("retirement");
            providers.Add("prudential");
            providers.Add("MFS");
            providers.Add("DFA");
            providers.Add("WFA ");
            providers.Add("Wells Fargo");
            providers.Add("Templeton");
            providers.Add("Harbor");
            providers.Add("Oakmark");
            providers.Add("Alliance Bernstein");
            providers.Add("AB ");
            providers.Add("AllianzGI");
            providers.Add("NFJ");
            providers.Add("ALPS ");
            providers.Add("Altegris");
            providers.Add("Am Beacon");
            providers.Add("American Beacon");
            providers.Add("BMO ");
            providers.Add("BNY ");
            providers.Add("Bridge Builder");
            providers.Add("Calamos");
            providers.Add("Calvert");
            providers.Add("Catalyst");
            providers.Add("ClearBridge");
            providers.Add("Cohen");
            providers.Add("Columbia");
            providers.Add("Davis");
            providers.Add("Delaware");
            providers.Add("Deutsche");
            providers.Add("Direxion");
            providers.Add("Dodge");
            providers.Add("Drey");
            providers.Add("Dunham");
            providers.Add("Eagle");
            providers.Add("Eaton Vance");
            providers.Add("EV ");
            providers.Add("AmericaFirst");
            providers.Add("Franklin");
            providers.Add("Fred Alger");
            providers.Add("Gabelli");
            providers.Add("GAMCO");
            providers.Add("Glenmede");
            providers.Add("GMO ");
            providers.Add("Great West");
            providers.Add("Guggenheim");
            providers.Add("GuideStone");
            providers.Add("Hartford");
            providers.Add("Henderson");
            providers.Add("Highland");
            providers.Add("iShares");
            providers.Add("Ivy ");
            providers.Add("Legg Mason");
            providers.Add("LVIP");
            providers.Add("MainStay");
            providers.Add("Manning");
            providers.Add("MassMutual");
            providers.Add("morgan stanley");
            providers.Add("Nationwide");
            providers.Add("Natixis");
            providers.Add("Neuberger");
            providers.Add("Nuveen");
            providers.Add("Oak Ridge");
            providers.Add("PACE ");
            providers.Add("Perkins");
            providers.Add("Pioneer");
            providers.Add("PNC ");
            providers.Add("PowerShare");
            providers.Add("ProFunds");
            providers.Add("Praxis");
            providers.Add("ProShares");
            providers.Add("Putnam");
            providers.Add("Quaker");
            providers.Add("RidgeWorth");
            providers.Add("Royce");
            providers.Add("Russell");
            providers.Add("Rydex");
            providers.Add("Saratoga");
            providers.Add("Schwab");
            providers.Add("SEI ");
            providers.Add("Sentinel");
            providers.Add("SPDR Barclays");
            providers.Add("State Farm");
            providers.Add("Sterling Capital");
            providers.Add("SunAmerica");
            providers.Add("TCW ");
            providers.Add("Thornburg");
            providers.Add("TETON");
            providers.Add("Thrivent");
            providers.Add("Touchstone");
            providers.Add("Transamerica");
            providers.Add("US Global Investors");
            providers.Add("VALIC ");
            providers.Add("Van Eck");
            providers.Add("Vantagepoint");
            providers.Add("Victory");
            providers.Add("Virtus");
            providers.Add("Voya");
            providers.Add("VY ");
            providers.Add("Waddell Reed");
            providers.Add("Wasatch");
            providers.Add("Wells Fargo");
            providers.Add("Westcore");
            providers.Add("Western Asset");
            providers.Add("William Blair");
            providers.Add("WisdomTree");
            providers.Add("Aberdeen");
            providers.Add("American Beacon");
            providers.Add("American Century");
            providers.Add("AMG ");
            providers.Add("AQR ");
            providers.Add("Aquila");
            providers.Add("Artisan");
            providers.Add("Ashmore");
            providers.Add("ASTON");
            providers.Add("Cavanal Hill");
            providers.Add("Waddell & Reed");
            providers.Add("First Investors");
            providers.Add("Great-West");
            providers.Add("American Independ");
            providers.Add("Strategic Adviser");
            providers.Add("AllianceBernstein");
            providers.Add("City National");
            providers.Add("Hotchkis & Wiley");
            providers.Add("etf ");
            providers.Add("bernst");
            providers.Add("amcent");
            providers.Add("matthew");
            providers.Add("stadion");
            providers.Add("metropolitan");
            providers.Add("retiresmar");
            providers.Add("lifestyle");
            providers.Add("harding");
            providers.Add("lazard");

            providers.Add("spdr");
            providers.Add("neub");
            providers.Add("freedom ");
            providers.Add("wilmingt");
            providers.Add("welling");
            providers.Add("clrbrg");
            providers.Add("bnym");
            providers.Add("ballie");
            providers.Add("allianz");
            providers.Add("axa ");
            providers.Add("beacon");
            providers.Add("century");
            providers.Add("amfund");
            providers.Add("ss russ");


            #endregion providers

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.BaseDirFolder = BaseSourceFolderDirTextBox.Text;
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }


        private async void DownloadGowPlansListButton_Click(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                var baseFolder = BaseSourceFolderDirTextBox.Text;
                if (Regex.IsMatch(baseFolder.Trim(), "[0-9]{3,}$"))
                {
                    baseFolder = new FileInfo(baseFolder).DirectoryName;
                }

                BaseSourceFolderDirTextBox.Text = Path.Combine(baseFolder, "Plans_" + DateTime.Now.ToString("yyyyMMdd"));

                Properties.Settings.Default.BaseDirFolder = BaseSourceFolderDirTextBox.Text;
                Properties.Settings.Default.Save();

                prgLabel.Text = "Downloading PlansList...";

                var curYear = DateTime.Now.Year - 1;
                var govAskaUrl = "http://askebsa.dol.gov/FOIA%20Files/{0}/All/"; // Settings.Default.GovAskaUrl;
                var f5500ArchiveName = "F_5500_{0}_All.zip";
                var fSchHAllZip = "F_SCH_H_{0}_All.zip";

                if (DateTime.Now.Month != 1) // for the Jan there is no data for current year!
                {
                    await DownloadPlansList(string.Format(f5500ArchiveName, curYear), string.Format(fSchHAllZip, curYear), string.Format(govAskaUrl, curYear));
                }
                await DownloadPlansList(string.Format(f5500ArchiveName, curYear - 1), string.Format(fSchHAllZip, curYear - 1), string.Format(govAskaUrl, curYear - 1));

                prgLabel.Text = "Ready...";

                if (AutoStartLoadNewPlans.Checked)
                    AddPlanLoad_Click(sender, e);
            }
            catch (Exception exception)
            {
                prgLabel.Text = "Error download 5500 file!";
                Logger.Log("Error download 5500 file. Ex:" + exception);
            }
        }

        private async Task DownloadPlansList(string f5500ArchiveName, string fSchHAllZip, string govAskaUrl)
        {
            var f5500ArchivePath = Path.Combine(BaseSourceFolderDirTextBox.Text, f5500ArchiveName);
            var fSchHAllArchivePath = Path.Combine(BaseSourceFolderDirTextBox.Text, fSchHAllZip);

            if (File.Exists(f5500ArchivePath)) File.Delete(f5500ArchivePath);
            if (File.Exists(fSchHAllArchivePath)) File.Delete(fSchHAllArchivePath);


            if (Properties.Settings.Default.NeedTorBrowser)
            {
                var planLoader = new PlanLoader("");

                var driver = planLoader.SetupBrowser(false);

                Thread.Sleep(3000);
                driver.Navigate().GoToUrl(govAskaUrl + f5500ArchiveName);
                Thread.Sleep(3000);
                driver.download(f5500ArchiveName, BaseSourceFolderDirTextBox.Text);

                driver.Navigate().GoToUrl(govAskaUrl + fSchHAllZip);
                Thread.Sleep(3000);
                driver.download(fSchHAllZip, BaseSourceFolderDirTextBox.Text);

                planLoader.TeardownBrowser();
            }
            else
            {
                var tasks = new List<Task>
                {
                    (govAskaUrl + f5500ArchiveName).DownloadFileAsync(BaseSourceFolderDirTextBox.Text),
                    (govAskaUrl + fSchHAllZip).DownloadFileAsync(BaseSourceFolderDirTextBox.Text)
                };

                await Task.WhenAll(tasks);
            }

            var f5500FileName = new FileInfo(ZipExtractToDirectory(f5500ArchivePath));
            var fSchHfIleName = new FileInfo(ZipExtractToDirectory(fSchHAllArchivePath));

            var destF5500FileName = Path.Combine(Constants.BaseDir, f5500FileName.Name);
            if (File.Exists(destF5500FileName)) File.Delete(destF5500FileName);
            File.Move(f5500FileName.FullName, destF5500FileName);

            var destfSchHfIleName = Path.Combine(Constants.BaseDir, fSchHfIleName.Name);
            if (File.Exists(destfSchHfIleName)) File.Delete(destfSchHfIleName);
            File.Move(fSchHfIleName.FullName, destfSchHfIleName);
        }

        private async void AddPlanLoad_Click(object sender, EventArgs e)
        {
            LoadNewPlansLogic loadNewPlansLogic = new LoadNewPlansLogic(BaseSourceFolderDirTextBox.Text, prgLine,
                prgLabel, _qualityControl);

            await loadNewPlansLogic.AddPlanLoad();

            if (AutoStartDawnloadPdfPlans.Checked)
                DownloadGovPlansPdfButton_Click(sender, e);
        }

        private string ZipExtractToDirectory(string filePath)
        {
            using (ZipArchive archive = ZipFile.Open(filePath, ZipArchiveMode.Read))
            {
                var destinationDirectoryName = filePath.TrimEnd(".zip".ToCharArray()) + "Extracted";
                if (Directory.Exists(destinationDirectoryName)) Directory.Delete(destinationDirectoryName, true);
                archive.ExtractToDirectory(destinationDirectoryName);
                return Path.Combine(destinationDirectoryName,
                    new FileInfo(filePath).Name.TrimEnd(".zip".ToCharArray()) + ".csv");
            }
        }

        private async void DownloadGovPlansPdfButton_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> rows = new Dictionary<string, string>();
            var path = BaseSourceFolderDirTextBox.Text + @"\_DownloadPDF.csv";
            var pathRead = BaseSourceFolderDirTextBox.Text + @"\DownloadedPDFFile.csv";

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            HashSet<string> readPDF = new HashSet<string>();


            if (!File.Exists(pathRead))
            {
                File.Create(pathRead);
            }
            try
            {
                string[] linesRead = File.ReadAllLines(pathRead, Encoding.UTF8);
                foreach (string line in linesRead)
                {
                    readPDF.AddIfNotExist(line.Before("."));
                }
            }
            catch { }


            foreach (string line in lines)
            {

                if (!readPDF.Contains(line.After(","))) { rows.Add(line.Before(","), line.After(",")); }
            }
            int cnt = 1;

            int delay = Convert.ToInt32(textBox1.Text);
            int processes = Convert.ToInt32(textBox3.Text);

            //foreach (var item in rows)
            Task task = Task.Factory.StartNew(delegate
            {
                Parallel.ForEach(rows, new ParallelOptions { MaxDegreeOfParallelism = processes }, item => // VVK MaxDegreeOfParallelism=4
                {
                    using (var webClient = new WebClient())
                    {
                        try
                        {
                            Thread.Sleep(delay);  // Request Rate Limit Exceeded   You have exceeded the request rate EFAST2 allows. You will be blocked from submitting subsequent requests to EFAST2 until the excessive rate behavior has subdued. You may restart request submissions, at a slower request rate, after an allotted wait time has elapsed.
                            string data = webClient.DownloadString("https://rixtrema.com/accopinion/process.php?mode=ACKID&reqdate=&ackid=" + item.Key + "&reqname=" + item.Value);

                            txtFunds.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                if (data.Contains("\"code\":0,\"message\":\"\""))
                                {
                                    txtFunds.AppendText("\r\n" + cnt.ToString() + " - " + item.Key + " ==== OK");
                                    Application.DoEvents();
                                }
                                else
                                {
                                    txtFunds.AppendText("\r\n" + cnt.ToString() + " - " + item.Key + " ==== " + data);
                                    Application.DoEvents();
                                }
                            });

                            lock (locker) { cnt++; }
                        }
                        catch { }
                    }

                    /*
                     * NEAUDIO","info":{"ACKID":"2020-06-24","EIN":"931060262","PN":"001","RECEIVED":"Dec 31, 2019","PLANNAME":"BLACKSTONE AUDIO 401K PROFIT SHARING PLAN","YEAREND":"20200624132415NAL0001205219001"},"accOp":"\/home\/extrem24\/public_html\/accopinion\/out\/accop\/BLACKSTONEAUDIO.pdf","sched":"\/home\/extrem24\/public_html\/accopinion\/out\/sched\/BLACKSTONEAUDIO.pdf"}
2 - 20200624132522NAL0002399601001 ==== {"code":0,"message":"","req":"MARYVILLECOLLEGE","info":{"ACKID":"2020-06-24","EIN":"620475691","PN":"001","RECEIVED":"Dec 31, 2019","PLANNAME":"MARYVILLE COLLEGE DEFINED CONTRIBUTION RETIREMENT","YEAREND":"20200624132522NAL0002399601001"},"accOp":"\/home\/extrem24\/public_html\/accopinion\/out\/accop\/MARYVILLECOLLEGE.pdf","sched":"\/home\/extrem24\/public_html\/accopinion\/out\/sched\/MARYVILLECOLLEGE.pdf"}
3 - 20200624110429NAL0002839857001 ==== {"code":0,"message":"","req":"OTTOCANDIESLLC","info":{"ACKID":"2020-06-24","EIN":"720399596","PN":"001","RECEIVED":"Dec 31, 2019","PLANNAME":"Otto Candies, LLC Retirement Savings Plan","YEAREND":"20200624110429NAL0002839857001"},"accOp":"\/home\/extrem24\/public_html\/accopinion\/out\/accop\/OTTOCANDIESLLC.pdf","sched":"\/home\/extrem24\/public_html\/accopinion\/out\/sched\/OTTOCANDIESLLC.pdf"}
4 - 20200624132826NAL0001208339001 ==== {"code":0,"message":"","req":"HAMILTONPARKERCOMPANYLLC_417387","info":{"ACKID":"2020-06-24","EIN":"264038680","PN":"001","RECEIVED":"Dec 31, 2019","PLANNAME":"Hamilton Parker Company, LLC Profit Sharing Plan And Trust","YEAREND":"20200624132826NAL0001208339001"},"accOp":"\/home\/extrem24\/public_html\/accopinion\/out\/accop\/HAMILTONPARKERCOMPANYLLC_417387.pdf","sched":"\/home\/extrem24\/public_html\/accopinion\/out\/sched\/HAMILTONPARKERCOMPANYLLC_417387.pdf"}
5 - 20200604123412NAL0012150819001 ==== {"code":14,"message":"AckId has no Attachments","req":"DOORCONTROLSUSAINC","info":{},"accOp":null,"sched":null}
                     * */

                });

                txtFunds.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtFunds.AppendText("\r\n" + "READY");
                    Application.DoEvents();
                });

            });



            /*
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            prgLabel.Text = "Downloading Gov PlansPdf...";
            var directoryWithPdf = Constants.PdfFolder;
            if (!Directory.Exists(directoryWithPdf))
                Directory.CreateDirectory(directoryWithPdf);

            var startPdfFileCount = Directory.GetFiles(directoryWithPdf).Length;
            var records = GetRecordsFromDownloadPdfFile(BaseSourceFolderDirTextBox.Text + @"\_DownloadPDF.csv").ToList();

            prgLine.Maximum = records.Count;
            prgLine.Value = 0;

            var threads = ThreadHelper.RunAsyncMultiThreadAction(ProccssBathDownloadPdfPlans, records,
                Properties.Settings.Default.DownloadGovPlansPdfThreadCount);

            await ThreadHelper.WaitAllThreadAsync(threads);

            stopwatch.Stop();
            var data =
                new
                {
                    Total = prgLine.Maximum,
                    Success = Directory.GetFiles(directoryWithPdf).Length - startPdfFileCount
                };
            _qualityControl.CreateEvent(QualityActionsAliases.UPDFUND_LOADFROMGOV, data.ToJson(), stopwatch.Elapsed);

            File.WriteAllText(Constants.IgnoreAskIdFilePath,
                string.Join("\r\n", _ingonreAskIdDictionary.Select(x => x.Key + ',' + x.Value)) + "\r\n");
            prgLabel.Text = "RunMappingSql Success";

            this.Invoke(() => prgLabel.Text = "Ready");

            if (AutoStartConvertToXls.Checked)
                ConvertPdfToXml_Click(sender, e);
                */
        }

        private void ProccssBathDownloadPdfPlans(object obj)
        {
            var idToSponsorNameDictionary = (IList<KeyValuePair<string, string>>)obj;
            PlanLoader planLoader = new PlanLoader(Constants.BaseDir);

            try
            {
                foreach (var idToSponsorNameMap in idToSponsorNameDictionary)
                {
                    try
                    {
                        planLoader.DownloadFile(idToSponsorNameMap.Key, idToSponsorNameMap.Value);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e);
                    }
                    finally
                    {
                        this.Invoke(() => { prgLine.Value++; });
                    }

                    if (!File.Exists(Path.Combine(Constants.PdfFolder, idToSponsorNameMap.Value + ".pdf")))
                    {
                        _ingonreAskIdDictionary.AddIfNotExist(idToSponsorNameMap.Key, 0);
                        _ingonreAskIdDictionary[idToSponsorNameMap.Key]++;
                    }
                }
            }
            finally
            {
                planLoader.TeardownBrowser();

            }
        }

        readonly Dictionary<string, int> _ingonreAskIdDictionary = new Dictionary<string, int>();

        public IEnumerable<KeyValuePair<string, string>> GetRecordsFromDownloadPdfFile(string csvSourceFilePath)
        {
            var ignoreAskIdfileName = Constants.IgnoreAskIdFilePath;
            if (!File.Exists(ignoreAskIdfileName)) File.Create(ignoreAskIdfileName);
            var fs = new FileStream(ignoreAskIdfileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fs))
            {
                CsvReader ignoreCsvReader = new CsvReader(sr, false, ',');

                while (ignoreCsvReader.ReadNextRecord())
                {
                    var askId = ignoreCsvReader[0];
                    if (string.IsNullOrEmpty(askId))
                    {
                        continue;
                    }

                    int retryCount;
                    if (int.TryParse(ignoreCsvReader[1], out retryCount))
                    {
                        _ingonreAskIdDictionary.AddIfNotExist(askId, retryCount);
                    }
                }
            }

            CsvReader csv = new CsvReader(new StreamReader(csvSourceFilePath), false, ',');
            int idPosition = 0;
            int sponsorNamePosition = 1;
            while (csv.ReadNextRecord())
            {
                var askId = csv[idPosition];
                if (askId == null)
                {
                    continue;
                }
                Application.DoEvents();
                if (_ingonreAskIdDictionary.ContainsKey(askId) &&
                    _ingonreAskIdDictionary[askId] >= Properties.Settings.Default.DownloadPdfMaxRetryCount)
                {
                    continue;
                }

                yield return new KeyValuePair<string, string>(askId, csv[sponsorNamePosition]);
            }
        }

        private async void ConvertPdfToXml_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!Directory.Exists(Constants.PdfFolder))
            {
                prgLabel.Text = "Invalid path to folder with pdf";
            }

            var pdfs = Directory.EnumerateFileSystemEntries(Constants.PdfFolder, "*.pdf").ToList();
            prgLine.Maximum = pdfs.Count;
            prgLabel.Text = "Start convert Pdf to Xlsx";
            prgLine.Value = 0;

            var threads = ThreadHelper.RunAsyncMultiThreadAction(RunAbbyForListPdf, pdfs,
                Properties.Settings.Default.FineCmdThreadCount);

            await ThreadHelper.WaitAllThreadAsync(threads);

            stopwatch.Stop();
            var qaData = new { Total = prgLine.Maximum, Success = prgLine.Value }.ToJson();
            _qualityControl.CreateEvent(QualityActionsAliases.UPDFUND_CONVERTPDFTOXLS, qaData, stopwatch.Elapsed);

            this.Invoke(new MethodInvoker(delegate
            {
                prgLine.Value = prgLine.Maximum;
                prgLabel.Text = string.Format("Finish convert Pdf to Xlsx. Success {0} of {1}", prgLine.Value,
                    prgLine.Maximum);
            }));

            if (AutoStartConvertToCsv.Checked)
                ConvertXlsToCsvButton_Click(sender, e);
        }

        private void RunAbbyForListPdf(object pdfsObject)
        {
            var pdfs = (IList<string>)pdfsObject;

            foreach (var pdf in pdfs)
            {
                var pdfInfo = new FileInfo(pdf);

                try
                {
                    Retry.Do(() =>
                    {

                        var xlsFolder = Constants.XlsFolder;
                        if (!Directory.Exists(xlsFolder))
                            Directory.CreateDirectory(xlsFolder);
                        var resultFile = Path.Combine(xlsFolder, pdfInfo.Name.TrimEnd(pdfInfo.Extension.ToCharArray()) + ".xlsx");
                        if (!File.Exists(resultFile))
                        {


                            var
                                arguments = string.Format("\"{0}\" \"{1}\" /lang english /out \"{2}\" /quit",
                                    Settings.Default.PathToFineCmd, pdfInfo.FullName, resultFile);

                            using (var cmd = new Process
                            {
                                StartInfo =
                                {
                                    StandardOutputEncoding = Encoding.UTF8,
                                    FileName = "cmd.exe",
                                    RedirectStandardInput = true,
                                    RedirectStandardOutput = true,
                                    CreateNoWindow = true,
                                    UseShellExecute = false
                                }
                            })
                            {
                                cmd.Start();
                                cmd.StandardInput.WriteLine(@"Chcp 1251");
                                cmd.StandardInput.WriteLine(arguments);
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                var timeout = TimeSpan.FromMinutes(5);
                                cmd.WaitForExit(timeout.Milliseconds);
                                var task = Task.Run(() => cmd.StandardOutput.ReadToEnd());
                                if (!task.Wait(timeout))
                                {
                                    var fineReaderProces = Process.GetProcessesByName("FineReader").OrderBy(x => x.StartTime).FirstOrDefault();

                                    if (fineReaderProces != null && fineReaderProces.StartTime < DateTime.Now.Add(-timeout))
                                        fineReaderProces.Kill();


                                    throw new Exception("Warn: File " + pdfInfo.FullName + " not processed. Try again");

                                }
                            }
                        }

                        if (File.Exists(resultFile))
                        {
                            this.Invoke(() =>
                            {
                                prgLine.Value++;
                                prgLabel.Text = string.Format("File {0} processed", pdfInfo.Name);
                            });
                        }
                        else
                        {
                            throw new Exception("Warn: File " + pdfInfo.FullName + " not processed. Try again");
                        }
                    }, TimeSpan.FromSeconds(5));

                }
                catch (Exception e)
                {
                    this.Invoke(() => prgLabel.Text = string.Format("File {0} not processed", pdfInfo.Name));
                    Logger.Log("Error. Fail process pdf file " + pdf + "ex: " + e);
                }
            }
        }

        private async void ConvertXlsToCsvButton_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var xlsxToCsvConverter = new XlsxToCsvConverter(this, txtFunds, prgLine, prgLabel);
            var threads = xlsxToCsvConverter.ReadPlanXlsxFromFolder(Constants.XlsFolder);

            await ThreadHelper.WaitAllThreadAsync(threads);

            stopwatch.Stop();

            var qaData = new { Total = prgLine.Maximum, Success = prgLine.Value }.ToJson();
            _qualityControl.CreateEvent(QualityActionsAliases.UPDFUND_ConvertXsToCsv, qaData, stopwatch.Elapsed);
            prgLabel.Text = "Ready";

        }

        private async void LoadBigAssDataButton_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread.CurrentThread.CurrentCulture = new CultureInfo(1033);

            var files = Directory.GetFiles(Constants.CsvFolder, "*.csv", SearchOption.TopDirectoryOnly);

            prgLine.Value = 0;
            prgLine.Maximum = files.Length;

            var tasks = new List<Task>();

            int totalNewRecordInBigAssData = 0;

            foreach (string file in files)
            {
                var bigAssDataDtos = GetBigAssDataFromFolderWithCsv(file);
                bool isFirstButchInFile = true;
                foreach (var newPlans in bigAssDataDtos.Batch(10))
                {
                    if (isFirstButchInFile)
                    {
                        await SendBigAssDataToServer(newPlans, true);
                        isFirstButchInFile = false;
                    }
                    else
                    {
                        totalNewRecordInBigAssData += newPlans.Count;
                        if (newPlans.Count == 0)
                        {
                            continue;
                        }
                        tasks.Add(SendBigAssDataToServer(newPlans, false));
                    }
                }
            }

            await Task.WhenAll(tasks);
            var qaData = new { TotalFiles = prgLine.Maximum, TotalNewRecords = totalNewRecordInBigAssData }.ToJson();
            _qualityControl.CreateEvent(QualityActionsAliases.UPDFUND_LOADBIGASSDATA, qaData, stopwatch.Elapsed);

            prgLabel.Text = "Ready";

            if (AutoStartRunMappingSql.Checked)
                RunMappingSqlButton_Click(sender, e);
        }

        readonly SemaphoreSlim _bigAssThrottler = new SemaphoreSlim(5);

        private async Task SendBigAssDataToServer(IList<BigAssDataDto> bigAssDataDtos,
            bool isFirstStartForFile)
        {
            try
            {
                await _bigAssThrottler.WaitAsync();
                var data = new
                {
                    isFirstStartForFile = isFirstStartForFile ? 1 : 0,
                    SponsorName = string.Join("||", bigAssDataDtos.Select(x => x.SponsorName)),
                    Name1 = string.Join("||", bigAssDataDtos.Select(x => x.Name1)),
                    Name2 = string.Join("||", bigAssDataDtos.Select(x => x.Name2)),
                    Year = string.Join("||", bigAssDataDtos.Select(x => x.Year)),
                    Value = string.Join("||", bigAssDataDtos.Select(x => x.Value)),
                    ETF = string.Join("||", bigAssDataDtos.Select(x => x.ETF)),
                    Class = string.Join("||", bigAssDataDtos.Select(x => x.Class)),
                    ClassPosition = string.Join("||", bigAssDataDtos.Select(x => x.ClassPosition)),
                };

                var httpResponseMessage = await FlurlHelper.RunActionOnRixtremaWsWithRetry("AddBigAssData", data);

                if (isFirstStartForFile)
                {
                    prgLine.Value++;
                }

                if (httpResponseMessage == null || httpResponseMessage.FCT.Result != "Success")
                {
                    var errorString = string.Format("Error process AddBigAssData action. Response: {0}",
                        ObjectExtentions.ToJson(httpResponseMessage));
                    Logger.Log(errorString);
                    prgLabel.Text = errorString;
                }
                else
                {
                    prgLabel.Text = string.Format("AddBigAssData for part Success. {0} of {1}", prgLine.Value,
                        prgLine.Maximum);
                }
            }
            catch (Exception e)
            {
                var errorString = string.Format("Error process AddBigAssData action. Ex: {0}", e);
                Logger.Log(errorString);
                prgLabel.Text = errorString;
            }
            finally
            {
                _bigAssThrottler.Release();
            }
        }

        private class BigAssDataDto
        {
            public string SponsorName { get; set; }
            public string Name1 { get; set; }
            public string Name2 { get; set; }
            public string Year { get; set; }
            public string Value { get; set; }
            public string ETF { get; set; }
            public string Class { get; set; }
            public string ClassPosition { get; set; }

        }

        private IEnumerable<BigAssDataDto> GetBigAssDataFromFolderWithCsv(string file)
        {
            prgLabel.Text = Path.GetFileName(file);
            Application.DoEvents();

            string sponsorName = Path.GetFileNameWithoutExtension(file);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(1033);

            var lines = File.ReadAllLines(file);
            bool fileWithRecords = false;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] items = lines[i].Split("|".ToCharArray(), StringSplitOptions.None);

                if (items[0].Length > 500) items[0] = items[0].Substring(0, 500);
                if (items[1].Length > 500) items[1] = items[1].Substring(0, 500);

                double value;
                if (!double.TryParse(items[2].Replace(",", "."), out value) || value > 1600000000) continue;

                fileWithRecords = true;
                yield return new BigAssDataDto
                {
                    SponsorName = sponsorName.TakeFirstNLetters(100),
                    Name1 = items[0],
                    Name2 = items[1],
                    Year = items[3],
                    Value = items[2],
                    ETF = items[4],
                    Class = items[5],
                    ClassPosition = items[6]
                };
            }

            if (!fileWithRecords)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    var problemDir = Constants.ProblemXlsDir;
                    if (!Directory.Exists(problemDir))
                        Directory.CreateDirectory(problemDir);
                    File.Copy(file, Path.Combine(problemDir, fileInfo.Name), true);
                    Logger.Log(string.Format("Warn. In file not exist valid records. FileName: {0}", file));
                }
                catch (Exception e)
                {
                    Logger.Log("Info: " + e);
                }
            }
        }

        private async void RunMappingSqlButton_Click(object sender, EventArgs e)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                prgLabel.Text = "RunMappingSql starting...";

                var httpResponseMessage = await FlurlHelper.RunActionOnRixtremaWsWithRetry("RunMappingSql");

                if (httpResponseMessage.FCT.Result != "Success")
                {
                    var errorString = string.Format("Error process RunMappingSql action. Response: {0}",
                        ObjectExtentions.ToJson(httpResponseMessage));
                    Logger.Log(errorString);
                    prgLabel.Text = "Error process RunMappingSql action.";
                }
                else
                {
                    List<dynamic> newPlans = httpResponseMessage.FCT.NotMappedStrings;
                    File.WriteAllText(Constants.KeyWordsFilePath,
                        string.Join("\r\n", newPlans.Select(x => x.NotMappedString)) + "\r\n");
                    prgLabel.Text = "RunMappingSql Success";
                    var qaData = new { TotalNewNotMappedNames = newPlans.Count }.ToJson();
                    _qualityControl.CreateEvent(QualityActionsAliases.UPDFUND_RunMappingSql, qaData, stopwatch.Elapsed);


                }
            }
            catch (Exception ex)
            {
                var errorString = string.Format("Error process RunMappingSql action. Ex: {0}", ex);
                Logger.Log(errorString);
                prgLabel.Text = "Error process RunMappingSql action.";
            }
        }

        private void GoogleScraperButton_Click(object sender, EventArgs e)
        {
            UiShowProcessing(true);
            var googleScraper = new GoogleScraperLogic(BaseSourceFolderDirTextBox.Text, this, Constants.KeyWordsFilePath);
            googleScraper.StartProcessing();
            UiShowProcessing(false);


        }

        private void UiShowProcessing(bool processing)
        {
            Cursor.Current = (processing) ? Cursors.WaitCursor : Cursors.Default;
        }

        public void SetProgressLabel(string label)
        {
            prgLine.Text = label;
        }

        public void SetProgressBarInit(int max)
        {
            prgLine.Minimum = 0;
            prgLine.Maximum = max;
        }

        public void SetProgressBarStep(int step)
        {
            prgLine.Value = step;
        }

        private async void LoadSearchMappingsButton_Click(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await FlurlHelper.RunActionOnRixtremaWsWithRetry("GetAllTickerToFullNamePair");
            List<dynamic> tickerToFullNameList = result.FCT.TickerToFullNamePairs;
            var tckrs = tickerToFullNameList
                .Select(x => ((string)x.TickerToFullNamePair).Split(new[] { "||" }, StringSplitOptions.None))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]);

            /*-------------------------------------------------------------------*/
            string srcFile = Constants.GoogleScraperResultFileName; // @"C:\RXUtilities\Funds_Process\1\_tickers.csv"
            string resFile = srcFile.Replace(".csv", "Res.csv");
            bool hasBaseTicker = false;
            DataTable searchRes = CSVParser.CSVToDataTable(srcFile, true, 0, 0);
            // Add empty last one

            if (searchRes.Columns.Contains("BaseTicker")) hasBaseTicker = true;

            if (hasBaseTicker)
                searchRes.Rows.Add("EmptyName", "EmptyTicker", "EmptyBaseTicker", "0");
            else
                searchRes.Rows.Add("EmptyName", "EmptyTicker", "0");


            prgLine.Maximum = searchRes.Rows.Count;
            prgLine.Value = 0;
            prgLabel.Text = "Load Search Mappings...";


            // TODO for Dmitry
            // first line with headers - Name|Ticker|Rank - fixed in parser
            // find out why last block of text is ignored - fixed in 8753,8790
            // find out why some lines are broken and not joined with DB lines - solved by removing unicode chars in DB

            var nameToTickerResultDict = new Dictionary<string, string>();


            string currName = "";
            string baseTicker = "";
            string resolvedTicker = "";
            double resolvedRank = 0;
            double resolvedDistanse = 999999;

            var totalMappedNames = 0;
            var totalNotMappedNames = 0;
            foreach (DataRow row in searchRes.Rows)
            {
                var name = row["Name"].ToString();
                prgLine.Value++;
                if (name == "") continue;
                name = name.Replace(";", " ").Replace(":", " ").Replace("-", " ").Replace(".", " ").Replace(",", " ");
                Regex regex = new Regex("[ ]{2,}");
                name = regex.Replace(name, " ").Trim();
                var Ticker = row["Ticker"].ToString();
                double Rank;
                if (!double.TryParse(row["Rank"].ToString(), out Rank))
                    continue;

                Application.DoEvents();

                if (currName == name)
                {
                    if (tckrs.ContainsKey(Ticker))
                    {
                        string FundName = tckrs[Ticker];
                        double Distanse = LevenshteinDistance.ComputeCaseInsensitive(name, FundName);
                        if (Distanse < resolvedDistanse || (Distanse == resolvedDistanse && Rank > resolvedRank))
                        {
                            string sYearOrig = Regex.Match(name, @"\b(20)\d{2}\b", RegexOptions.None).ToString();
                            string sYearFundName = Regex.Match(FundName, @"\b(20)\d{2}\b", RegexOptions.None).ToString();

                            if (sYearOrig == sYearFundName)
                            {
                                if (!LevenshteinDistance.AtLeastOneWordMatches(name, FundName)) continue;

                                resolvedTicker = Ticker;
                                resolvedDistanse = Distanse;
                                resolvedRank = Rank;
                                if (hasBaseTicker) baseTicker = row["BaseTicker"].ToString();
                            }
                        }
                    }
                }

                if (currName != name)
                {
                    // write ResolvedTicker to DB
                    if (currName != "")
                    {
                        var baseTickerInfo = "";
                        if (hasBaseTicker)
                        {
                            string baseTickerName = "";
                            if (tckrs.ContainsKey(baseTicker)) baseTickerName = tckrs[baseTicker];
                            baseTickerInfo = baseTicker + "\t" + baseTickerName + "\t";
                        }
                        if (tckrs.ContainsKey(resolvedTicker))
                        {
                            File.AppendAllText(resFile,
                                baseTickerInfo + resolvedTicker + "\t" + currName + "\t" + tckrs[resolvedTicker] + "\t" +
                                resolvedDistanse.ToString() + "\r\n");
                            // Save resolved tickers to DB
                            nameToTickerResultDict.AddIfNotExist(currName, resolvedTicker);
                        }
                        else
                        {
                            File.AppendAllText(resFile, baseTickerInfo + resolvedTicker + "\t" + currName + "\t\t\r\n");
                            // Save unresolved tickers to DB
                            nameToTickerResultDict.AddIfNotExist(currName, "");
                        }
                    }

                    currName = name;
                    resolvedTicker = "";
                    resolvedRank = 0;
                    resolvedDistanse = 999999;

                    if (tckrs.ContainsKey(Ticker))
                    {
                        string FundName = tckrs[Ticker];
                        double Distanse = LevenshteinDistance.ComputeCaseInsensitive(name, FundName);
                        if (Distanse < resolvedDistanse || (Distanse == resolvedDistanse && Rank > resolvedRank))
                        {
                            string sYearOrig = Regex.Match(name, @"\b(20)\d{2}\b", RegexOptions.None).ToString();
                            string sYearFundName = Regex.Match(FundName, @"\b(20)\d{2}\b", RegexOptions.None).ToString();
                            if (sYearOrig == sYearFundName)
                            {
                                if (!LevenshteinDistance.AtLeastOneWordMatches(name, FundName)) continue;

                                resolvedTicker = Ticker;
                                resolvedDistanse = Distanse;
                                resolvedRank = Rank;
                                if (hasBaseTicker) baseTicker = row["BaseTicker"].ToString();
                            }
                        }
                    }
                }

                if (nameToTickerResultDict.Count > 100)
                {
                    await FlurlHelper.RunActionOnRixtremaWsWithRetry("AddMappingSearch", new
                    {
                        Tickers = string.Join(",", nameToTickerResultDict.Values),
                        FullNames = string.Join(",", nameToTickerResultDict.Keys)
                    });
                    totalMappedNames += nameToTickerResultDict.Keys.Count(x => x != "");
                    totalNotMappedNames += nameToTickerResultDict.Values.Count(x => x == "");

                    nameToTickerResultDict.Clear();
                }
            }

            await FlurlHelper.RunActionOnRixtremaWsWithRetry("AddMappingSearch", new
            {
                Tickers = string.Join(",", nameToTickerResultDict.Values),
                FullNames = string.Join(",", nameToTickerResultDict.Keys)
            });
            totalMappedNames += nameToTickerResultDict.Keys.Count(x => x != "");
            totalNotMappedNames += nameToTickerResultDict.Values.Count(x => x == "");

            var qaData =
                new
                {
                    Total = prgLine.Maximum,
                    totalMappedNames = totalMappedNames,
                    totalNotMappedNames = totalNotMappedNames
                }.ToJson();
            _qualityControl.CreateEvent(QualityActionsAliases.UPDFUND_LoadNewMappingSearch, qaData, stopwatch.Elapsed);

            prgLabel.Text = "Ready";

            // if (AutoStartFillBigAssDataTmp.Checked)
            //    FillBigAssDataTMP_Click(sender, e);
        }

        private async void FillBigAssDataTMP_Click(object sender, EventArgs e)
        {
            prgLabel.Text = "Start FillBigAssDataTMP";

            await FlurlHelper.RunActionOnRixtremaWsWithRetry("FillBigAssDataTMP");

            prgLabel.Text = "Ready";
        }

        private async void DownloadLoansButton_Click(object sender, EventArgs e)
        {
            var curThread = Thread.CurrentThread;
            curThread.CurrentCulture = new CultureInfo(1033);

            prgLabel.Text = "Downloading PlansList...";

            var govAskaUrl = Properties.Settings.Default.GovAskaUrl;
            var f5500ArchiveName = "F_5500_SF_2015_All.zip";
            var fSchHAllZip = "F_SCH_H_2015_All.zip";
            var fSchIAllZip = "F_SCH_I_2015_All.zip";

            var f5500ArchivePath = Path.Combine(ToolsBaseFolderTextBox.Text, f5500ArchiveName);
            var fSchHAllArchivePath = Path.Combine(ToolsBaseFolderTextBox.Text, fSchHAllZip);
            var fSchIAllArchivePath = Path.Combine(ToolsBaseFolderTextBox.Text, fSchIAllZip);

            if (File.Exists(f5500ArchivePath)) File.Delete(f5500ArchivePath);
            if (File.Exists(fSchHAllArchivePath)) File.Delete(fSchHAllArchivePath);
            if (File.Exists(fSchIAllArchivePath)) File.Delete(fSchIAllArchivePath);

            var tasks = new List<Task>
            {
                (govAskaUrl + f5500ArchiveName).DownloadFileAsync(ToolsBaseFolderTextBox.Text),
                (govAskaUrl + fSchIAllZip).DownloadFileAsync(ToolsBaseFolderTextBox.Text),
                (govAskaUrl + fSchHAllZip).DownloadFileAsync(ToolsBaseFolderTextBox.Text)
            };

            await Task.WhenAll(tasks);

            var f5500FileName = new FileInfo(ZipExtractToDirectory(f5500ArchivePath));
            var fSchHfIleName = new FileInfo(ZipExtractToDirectory(fSchHAllArchivePath));
            var fSchIfIleName = new FileInfo(ZipExtractToDirectory(fSchIAllArchivePath)); // 

            var destF5500FileName = Path.Combine(ToolsBaseFolderTextBox.Text, f5500FileName.Name);
            if (File.Exists(destF5500FileName)) File.Delete(destF5500FileName);
            File.Move(f5500FileName.FullName, destF5500FileName);

            var destfSchHfIleName = Path.Combine(ToolsBaseFolderTextBox.Text, fSchHfIleName.Name);
            if (File.Exists(destfSchHfIleName)) File.Delete(destfSchHfIleName);
            File.Move(fSchHfIleName.FullName, destfSchHfIleName);

            var destfSchIfIleName = Path.Combine(ToolsBaseFolderTextBox.Text, fSchIfIleName.Name);
            if (File.Exists(destfSchIfIleName)) File.Delete(destfSchIfIleName);
            File.Move(fSchIfIleName.FullName, destfSchIfIleName);



            var valuesSchI = ReadFielsFromFile(destfSchIfIleName,
                "SCH_I_PLAN_NUM",
                "SCH_I_EIN",
                "SMALL_TOT_ASSETS_EOY_AMT",
                "SMALL_MORTG_PARTCP_EOY_IND",
                "SMALL_MORTG_PARTCP_EOY_AMT");

            var valuesF5500 = ReadFielsFromFile(destF5500FileName,
                "SF_PLAN_NUM",
                "SF_SPONS_EIN",
                "SF_TOT_ASSETS_EOY_AMT",
                "SF_PARTCP_LOANS_IND",
                "SF_PARTCP_LOANS_EOY_AMT");



            var valuesSchH = ReadFielsFromFile(destfSchHfIleName,
                "SCH_H_PN",
                "SCH_H_EIN",
                "TOT_ASSETS_EOY_AMT",
                "PARTCP_LOANS_EOY_AMT");


            using (StreamWriter file = new StreamWriter("Result.csv", false))
            {

                foreach (var value in valuesF5500.Concat(valuesSchI).Concat(valuesSchH))
                {
                    try
                    {
                        double totAssets = 0;
                        string lastField = "0";
                        foreach (var keyValuePair in value)
                        {
                            if (keyValuePair.Key.Contains("TOT_ASSETS_EOY"))
                            {
                                double.TryParse(keyValuePair.Value, out totAssets);
                                continue;
                            }

                            if (keyValuePair.Key == "PARTCP_LOANS_EOY_AMT")
                            {
                                if (string.IsNullOrEmpty(keyValuePair.Value))
                                {
                                    file.Write(2 + ";");
                                }
                                else
                                {
                                    file.Write(1 + ";");
                                }
                            }
                            lastField = keyValuePair.Value;
                            file.Write(keyValuePair.Value + ";");
                        }

                        if (totAssets == 0 || string.IsNullOrEmpty(lastField))
                        {
                            file.Write(0);
                        }
                        else
                        {
                            file.Write(double.Parse(lastField) / totAssets * 100);
                        }


                        file.WriteLine();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }

        private IEnumerable<Dictionary<string, string>> ReadFielsFromFile(string fileName, params string[] headers)
        {
            using (CsvReader csv = new CsvReader(new StreamReader(fileName), true, ','))
                while (csv.ReadNextRecord())
                {
                    yield return headers.ToDictionary(header => header, header => csv[header]);
                }
        }




        private void ParseSubAccButton_Click(object sender, EventArgs e)
        {
            //    SaveSubAccToDb(ToolsBaseFolderTextBox.Text);
        }

        private void DetectEmptyXlsxButton_Click(object sender, EventArgs e)
        {
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (!Directory.Exists(Constants.CsvFolder))
            {
                Directory.CreateDirectory(Constants.CsvFolder);
            }
            else
            {
                MessageBox.Show(Constants.CsvFolder + " already exists!");
                return;
            }
            if (!Directory.Exists(Constants.ProblemXlsDir))
            {
                Directory.CreateDirectory(Constants.ProblemXlsDir + "_total");
            }
            else
            {
                MessageBox.Show(Constants.ProblemXlsDir + "_total" + " already exists!");
                return;
            }

            // ----------------------- Get largest CSVs

            Dictionary<string, long> csvAcroDict = new Dictionary<string, long>();

            var csvAcro = Directory.GetFiles(Constants.CsvFolder + "_acro", "*.csv", SearchOption.TopDirectoryOnly);
            foreach (var fileName in csvAcro)
            {
                FileInfo fi = new FileInfo(fileName);
                csvAcroDict.AddIfNotExist(fi.Name, fi.Length);
            }

            Dictionary<string, long> csvFrDict = new Dictionary<string, long>();

            var csvFr = Directory.GetFiles(Constants.CsvFolder + "_fr", "*.csv", SearchOption.TopDirectoryOnly);
            foreach (var fileName in csvFr)
            {
                FileInfo fi = new FileInfo(fileName);
                csvFrDict.AddIfNotExist(fi.Name, fi.Length);
            }

            foreach (var item in csvAcroDict)
            {
                if (!csvFrDict.ContainsKey(item.Key) || csvFrDict[item.Key] <= item.Value)
                {
                    // Take largest or unique
                    File.Copy(Constants.CsvFolder + "_acro\\" + item.Key, Constants.CsvFolder + "\\" + item.Key);
                }
            }

            foreach (var item in csvFrDict)
            {
                if (!csvAcroDict.ContainsKey(item.Key) || csvAcroDict[item.Key] < item.Value)
                {
                    // Take unique from  csvFrDict
                    File.Copy(Constants.CsvFolder + "_fr\\" + item.Key, Constants.CsvFolder + "\\" + item.Key);
                }
            }

            // ----------------------- Get problem PDFs
            HashSet<string> csvFiles = new HashSet<string>();
            var csv = Directory.GetFiles(Constants.CsvFolder, "*.csv", SearchOption.TopDirectoryOnly);
            foreach (var fileName in csv)
            {
                csvFiles.AddIfNotExist(Path.GetFileNameWithoutExtension(fileName));
            }

            var problemAcro = Directory.GetFiles(Constants.ProblemXlsDir + "_acro\\pdf", "*.pdf", SearchOption.TopDirectoryOnly);
            foreach (var fileName in problemAcro)
            {
                if (!csvFiles.Contains(Path.GetFileNameWithoutExtension(fileName)))
                    File.Copy(fileName, Constants.ProblemXlsDir + "_total\\" + Path.GetFileName(fileName), true);
            }
            var problemFr = Directory.GetFiles(Constants.ProblemXlsDir + "_fr\\pdf", "*.pdf", SearchOption.TopDirectoryOnly);
            foreach (var fileName in problemFr)
            {
                if (!csvFiles.Contains(Path.GetFileNameWithoutExtension(fileName)))
                    File.Copy(fileName, Constants.ProblemXlsDir + "_total\\" + Path.GetFileName(fileName), true);
            }

            MessageBox.Show("ready!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Выбираем файлы из accop, которых нет в PDF и сразу их добавляем в PDF

            string accopDir = @"C:\RXUtilities\Funds_Process\Plans\Load\accop";
            string pdfDir = @"C:\RXUtilities\Funds_Process\Plans\Load\pdf";
            int i = 0;

            var accopDirFiles = Directory.GetFiles(accopDir, "*.pdf", SearchOption.TopDirectoryOnly);
            foreach (var fileName in accopDirFiles)
            {
                if (!File.Exists(pdfDir + "\\" + Path.GetFileName(fileName)))
                {
                    File.Copy(fileName, pdfDir + "\\" + Path.GetFileName(fileName));
                    i++;
                }
            }

            MessageBox.Show("ready! copied " + i.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string accopDir = @"C:\RXUtilities\Funds_Process\Plans\Load\accop";
            string pdfDir = @"C:\RXUtilities\Funds_Process\Plans\Load\pdf";
            string Accop_PDF_To_OCR = Constants.BaseDir + "\\Accop_PDF_To_OCR";
            int i = 0;

            List<string> PDF_with_positions = File.ReadAllLines(Constants.BaseDir + "\\PDF_with_positions.txt").ToList();

            if (!Directory.Exists(Accop_PDF_To_OCR))
            {
                Directory.CreateDirectory(Accop_PDF_To_OCR);
            }


            var problem = Directory.GetFiles(Constants.ProblemXlsDir + "_total", "*.pdf", SearchOption.TopDirectoryOnly);
            foreach (var fileName in problem)
            {
                if (!PDF_with_positions.Contains(Path.GetFileName(fileName).ToUpper()))
                {
                    if (File.Exists(accopDir + "\\" + Path.GetFileName(fileName)))
                    {
                        File.Copy(accopDir + "\\" + Path.GetFileName(fileName), pdfDir + "\\" + Path.GetFileName(fileName), true);
                        File.Copy(accopDir + "\\" + Path.GetFileName(fileName), Accop_PDF_To_OCR + "\\" + Path.GetFileName(fileName), true);
                        i++;
                    }
                }
            }
            MessageBox.Show("ready! copied " + i.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string dirML = @"C:\RXUtilities\Funds_Process\ML\\";

            File.Delete(dirML + "prvDifference.csv");
            File.Delete(dirML + "res.txt");
            File.Delete(dirML + "prv.csv");
            File.Delete(dirML + "prvTest.txt");
            File.Delete(dirML + "prvTestOrig.txt");

            Tools tools = new Tools();

            // выгрузка для обучения C:\RXUtilities\Funds_Process\ML\prv.csv
            DataTable dt = tools.CommandExecutor("select UPPER(Ticker)  Ticker, LOWER(Name)Name from tMappingSearch ms where    Len(Name) < 170   and not Ticker is null order by CONVERT(VARCHAR(32), HashBytes('MD5', name), 2)");
            dt.WriteCSV(dirML + "prv.csv", ",", false);

            //Выгрузка для распознавания C:\RXUtilities\Funds_Process\ML\prvTest.txt
            dt = tools.CommandExecutor("select Name from tMappingSearch ms where Ticker is null order by CONVERT(VARCHAR(32), HashBytes('MD5', name), 2)");
            dt.WriteCSV(dirML + "prvTest.txt", "\t", false);

            MessageBox.Show("ready!");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //C:\RXUtilities\Funds_Process\ML\prvDifference.csv
            //C:\RXUtilities\resultMSNoTicker.csv

            //C:\RXUtilities\resultMSEmpty.csv


            //string MLFile = @"D:\Work\Riskostat\prvDifference.csv";
            //string PatternFile = @"D:\Work\Riskostat\resultMSNoTicker.csv";
            //string EmptyFile = @"D:\Work\Riskostat\resultMSEmpty.csv";
            //string ResultFile = @"D:\Work\Riskostat\result.sql";

            string MLFile = @"C:\RXUtilities\Funds_Process\ML\prvDifference.csv";
            string PatternFile = @"C:\RXUtilities\resultMSNoTicker.csv";
            string EmptyFile = @"C:\RXUtilities\resultMSEmpty.csv";
            string ResultFile = Constants.BaseDir + @"\result.sql";
            //Constants.BaseDir

            List<string> res = new List<string>();
            DataTable dtML =CSVParser.CSVToDataTable(MLFile, false, 0, 0);
            DataTable dtPattern = CSVParser.CSVToDataTable(PatternFile, false, 0, 0);
            DataTable dtEmpty = CSVParser.CSVToDataTable(EmptyFile, false, 0, 0);

            Dictionary<string, string> MLDict = new Dictionary<string, string>();
            foreach (DataRow row in dtML.Rows)
            {
                if (row[0].ToString() != "")
                {
                    string key = row[0].ToString() + row[3].ToString().ToLower();
                    MLDict.Add(key, row[1].ToString());
                }
            }
            foreach (DataRow row in dtPattern.Rows)
            {
                if (row[0].ToString() != "")
                {
                    string key = row[0].ToString() + row[2].ToString().ToLower();
                    if (MLDict.ContainsKey(key))
                    {
                        res.Add($"update tMappingSearch set Checked=1, Ticker='{row[0].ToString()}' where Name='{row[2].ToString().Replace ("'","''")}'  \t\t-- {row[1].ToString()}");
                    }
                }
            }

            res.Add(""); res.Add(""); res.Add(""); res.Add("-- EMPTY --");
            
            foreach (DataRow row in dtEmpty.Rows)
            {
                res.Add($"update tMappingSearch set Checked=1, Ticker='' where Name='{row[0].ToString().Replace("'", "''")}'");
            }
            System.IO.File.AppendAllLines(ResultFile, res);

            MessageBox.Show("ready!");

        }

        private void button6_Click(object sender, EventArgs e)
        {

            string csvPath = Constants.CsvFolder;
            //string csvPath = @"D:\Work\Riskostat\Corrections\csv";
            string csvPathNoProv = csvPath+"\\noprov";

            if (!Directory.Exists(csvPathNoProv)) Directory.CreateDirectory(csvPathNoProv);

            var problem = Directory.GetFiles(csvPath, "*.csv", SearchOption.TopDirectoryOnly);
            foreach (var fileName in problem)
            {
                string text = File.ReadAllText(fileName).ToLower ();
                bool hasProv = false;

                foreach (var item in providers)
                {
                    if (text.Contains(item.ToLower()))
                    {
                        hasProv = true;
                        break;
                    }
                }

                if (!hasProv) File.Move(fileName, csvPathNoProv + "\\" + Path.GetFileName ( fileName));

            }
            MessageBox.Show("ready!");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string EmptyFile = @"C:\RXUtilities\resultMSEmpty.csv";
            string EmptyFile_woProv = @"C:\RXUtilities\resultMSEmpty_woProv.csv";
            string EmptyFile_wProv = @"C:\RXUtilities\resultMSEmpty_wProv.csv";
            List<string> woProv = new List<string>();
            List<string> wProv = new List<string>();

            string[] lines = File.ReadAllLines(EmptyFile);

            foreach (var text in lines)
            {
                bool hasProv = false;
                foreach (var item in providers)
                {
                    if (text.ToLower().Contains(item.ToLower()))
                    {
                        hasProv = true;
                        break;
                    }
                }
                if (hasProv) wProv.Add(text);
                if (!hasProv) woProv.Add(text);
            }

            System.IO.File.AppendAllLines(EmptyFile_woProv, woProv.ToArray());
            System.IO.File.AppendAllLines(EmptyFile_wProv, wProv.ToArray());

        }
    }
};