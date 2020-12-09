using System;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace PlansParser.FileDownloader
{
    public class PlanLoader
    {
        private readonly string _tempDownloadDir;

        private readonly string _url = Properties.Settings.Default.DownloadPdfUrl;
        private readonly int clearWait = 20;
        private readonly string _downloadDir;
        public Process TorProcess { get; set; }

        private IWebDriver _webDriver;
        private IJavaScriptExecutor _js;

        public PlanLoader(string dir)
        {
            _tempDownloadDir = Path.Combine(dir, "Temp", Guid.NewGuid().ToString());
            if (!Directory.Exists(_tempDownloadDir))
                Directory.CreateDirectory(_tempDownloadDir);

            _downloadDir = Path.Combine(dir, "pdf");
            if (!Directory.Exists(_downloadDir))
                Directory.CreateDirectory(_tempDownloadDir);
        }


        public void CreateTor()
        {
            TorProcess = new Process
            {
                StartInfo =
                    {
                        FileName = Properties.Settings.Default.TorBrowserPath,
                        Arguments = "-n",
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
            };

            TorProcess.Start();
        }


        public IWebDriver SetupBrowser(bool i2p = false)
        {
            string driversPath = Path.Combine(Directory.GetCurrentDirectory(), "Drivers");
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);

            if (i2p)
            {
                CreateTor();

                chromeOptions.AddArguments("--proxy-server=socks5://127.0.0.1:9150");
            }

            chromeOptions.AddUserProfilePreference("download.default_directory", _tempDownloadDir);
            _webDriver = new ChromeDriver(driversPath, chromeOptions);
            _webDriver.Manage().Window.Maximize();
            _js = (IJavaScriptExecutor)_webDriver;
            _webDriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(7));
            return _webDriver;
        }

        public void TeardownBrowser()
        {
            try
            {
                if (this.TorProcess != null) this.TorProcess.Kill();
                _webDriver.Quit();
            }
            catch (Exception)
            {
            }
        }

        public void DownloadFile(string id, string sponsorName)
        {
            string pdfFile = CheckFile(sponsorName, _downloadDir, "pdf");

            if (pdfFile == null)
            {
                if (_webDriver == null)
                {
                    SetupBrowser();
                    Page page = new Page(_webDriver, clearWait, _js);
                    page.Go(_url);
                }

                try
                {
                    SearchPage searchPage = new SearchPage(_webDriver, clearWait, _js);
                    searchPage.searchById(id);

                    try
                    {
                        FormPage formPage = new FormPage(_webDriver, clearWait, _js);
                        pdfFile = formPage.TakeFirst(sponsorName, _downloadDir, _tempDownloadDir);

                        if (pdfFile == "ERRORTIMEOUT")
                        {
                            TeardownBrowser();
                            _webDriver = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Download file problem. Ex: " + ex);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Download file problem. Ex: " + ex);
                    TeardownBrowser();
                    _webDriver = null;
                }
            }
        }

        public string CheckFile(string nameOfFile, string directiory, string ext)
        {
            nameOfFile = Path.Combine(directiory, nameOfFile);
            nameOfFile = nameOfFile + "." + ext;

            bool fileExist = File.Exists(nameOfFile);

            if (fileExist)
                return nameOfFile;
            return null;
        }
    }
}
