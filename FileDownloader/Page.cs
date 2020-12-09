using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace PlansParser.FileDownloader
{
    public class _Page
    {
        protected string urlIp = "http://yandex.ru/internet/";

        [FindsBy(How = How.CssSelector, Using = ".client__item.client__item_type_ipv4")]
        protected IWebElement ipElement;
        protected int clearWait { get; set; }
        protected IWebDriver d { get; set; }
        protected IJavaScriptExecutor js { get; set; }

        public _Page()
        {
            PageFactory.InitElements(this.d, this);
        }

        public _Page(IWebDriver drv, int cW)
        {
            this.d = drv;
            this.clearWait = cW;
            PageFactory.InitElements(this.d, this);
        }

        public _Page(IWebDriver drv, int cW, IJavaScriptExecutor jS)
            : this(drv, cW)
        {
            this.js = jS;
        }

        public string ip()
        {
            string _ip = "0.0.0.0";

            this.d.Navigate().GoToUrl(this.urlIp);

            _ip = this.ipElement.Text;

            return _ip;
        }

        public void Refresh()
        {
            this.js.ExecuteScript("javascript:localStorage.clear();");
            this.d.Manage().Cookies.DeleteAllCookies();
        }
    }

    public class Page : _Page
    {
        protected string url = "https://12.158.148.210/portal/app/disseminate?execution=e13s2";

        [FindsBy(How = How.Id, Using = "form")]
        protected IWebElement form;

        [FindsBy(How = How.Id, Using = "accountId")]
        protected IWebElement accountId;

        [FindsBy(How = How.Id, Using = "password")]
        protected IWebElement password;

        [FindsBy(How = How.Id, Using = "form:loginbtn")]
        protected IWebElement loginbtn;

        [FindsBy(How = How.CssSelector, Using = "#menuContainer [href='disseminate']")]
        protected IWebElement refresh;

        [FindsBy(How = How.CssSelector, Using = ".ui-dialog")]
        protected IWebElement dialog;

        public Page() : base()
        {
        }

        public Page(IWebDriver drv, int cW) : base(drv, cW)
        {
        }

        public Page(IWebDriver drv, int cW, IJavaScriptExecutor jS) : base(drv, cW, jS)
        {
        }

        public void Go(string url)
        {
            this.d.Navigate().GoToUrl(url);
            Login();
        }

        public void Login()
        {
            if (this.d.IsElementPresent(this.accountId, 1))
            {
                //this.accountId.eprint
                this.Go(this.url);
            }
        }

        public void Refresh()
        {

            this.d.Manage().Cookies.DeleteAllCookies();

            refresh.Click();
        }
    }

    public class SearchPage : Page
    {
        public SearchPage(IWebDriver drv, int cW) : base(drv, cW)
        {
        }

        public SearchPage(IWebDriver drv, int cW, IJavaScriptExecutor jS) : base(drv, cW, jS)
        {
        }

        [FindsBy(How = How.Id, Using = "sponsorName")]
        protected IWebElement sponsorName;

        [FindsBy(How = How.Id, Using = "ackId")]
        protected IWebElement ackId;

        [FindsBy(How = How.Id, Using = "form:nextbtn")]
        protected IWebElement nextButton;

        public void searchBySponsor(string sponsorName)
        {
            try
            {
                this.sponsorName.eprint(sponsorName);
                this.nextButton.Click();
            }
            catch
            {
                this.Refresh();
                this.searchBySponsor(sponsorName);
            }
        }

        public void searchById(string id)
        {
            try
            {
                this.ackId.eprint(id);
                this.nextButton.Click();
            }
            catch
            {
                this.Refresh();
                this.searchById(id);
            }
        }


    }

    public class FormPage : Page
    {
        private const string docSelector = "form:filingTreeTable_node_0";

        public FormPage(IWebDriver drv, int cW) : base(drv, cW)
        {
        }

        public FormPage(IWebDriver drv, int cW, IJavaScriptExecutor jS) : base(drv, cW, jS)
        {
        }

        [FindsBy(How = How.Id, Using = docSelector)]
        protected IWebElement firstDocument;
        [FindsBy(How = How.Id, Using = docSelector + "_0")]
        protected IWebElement firstDocument_mainPart;

        public string TakeFirst(string name, string downloadDir, string tempDownloadDir)
        {
            string pdfPath = null;

            IWebElement expand = this.firstDocument.FindElement(By.CssSelector(".ui-treetable-toggler"));

            expand.Click();
            //this.WebDriver.jsClick(expand, this.js);

            if (this.d.IsElementPresent(firstDocument_mainPart, this.clearWait))
            {
                int i = 1;

                while (this.d.IsElementPresent(By.Id(docSelector + "_" + i)))
                {
                    IWebElement docPart = this.d.FindElement(By.Id(docSelector + "_" + i));
                    if (docPart.Text != null && docPart.Text.ToLower().Contains("attach"))
                    {
                        IWebElement expandAttachments = docPart.FindElement(By.CssSelector(".ui-treetable-toggler"));
                        expandAttachments.Click();

                        if (this.d.IsElementPresent(this.d.FindElement(By.Id(docSelector + "_" + i + "_0")),
                            this.clearWait))
                        {

                            int j = 0;

                            while (this.d.IsElementPresent(By.Id(docSelector + "_" + i + "_" + j)))
                            {
                                IWebElement attPart = this.d.FindElement(By.Id(docSelector + "_" + i + "_" + j));
                                if (attPart.Text != null &&
                                    (attPart.Text.ToLower().Contains("assets") ||
                                     attPart.Text.ToLower().Contains("held")))
                                {
                                    IWebElement link =
                                        attPart.FindElement(
                                            By.Id("form:filingTreeTable:0_" + i + "_" + j + ":documentLnk"));
                                    link.Click();

                                    this.d.Wait_IsNotVisible(dialog, 20);
                                    Thread.Sleep(3000);
                                    var newFileName =
                                        Directory.GetFiles(tempDownloadDir)
                                            .Where(x => x.Contains("filing"))
                                            .Select(x => new FileInfo(x))
                                            .OrderByDescending(x => x.CreationTimeUtc)
                                            .FirstOrDefault();
                                    WaitFileDownloaded(newFileName);

                                    if (newFileName != null)
                                    {
                                        newFileName.Refresh();
                                        string newFile = newFileName.FullName;
                                        if (!newFileName.Exists)
                                        {
                                            newFile = newFile.TrimEnd(".crdownload".ToCharArray());
                                        }

                                        File.Move(newFile, Path.Combine(downloadDir, name + ".pdf"));
                                        pdfPath = newFile;
                                    }

                                    this.Refresh();

                                    return pdfPath ?? "ERRORTIMEOUT";
                                }
                                j++;
                            }

                            break;
                        }
                    }

                    i++;
                }
                ;
            }

            this.Refresh();

            return pdfPath;
        }

        private void WaitFileDownloaded(FileInfo newFileName)
        {
            int tryCount = 0;

            while (true)
            {
                tryCount++;
                if (newFileName.Name.EndsWith("crdownload"))
                {
                    tryCount++;
                    newFileName.Refresh();
                    if (newFileName.Exists)
                        Thread.Sleep(2000);
                    else
                    {
                        return;
                    }

                }
                else
                {
                    long size = 0;
                    while (true)
                    {
                        newFileName.Refresh();
                        if (size == newFileName.Length)
                            return;
                        Thread.Sleep(2000);
                        newFileName.Refresh();
                        size = newFileName.Length;
                    }
                }

                if (tryCount > 150) return;
            }
        }

        public class GooglePage : _Page
        {
            protected string url = "https://www.google.ru/";

            public GooglePage(IWebDriver drv, int cW) : base(drv, cW)
            {
            }

            public GooglePage(IWebDriver drv, int cW, IJavaScriptExecutor jS) : base(drv, cW, jS)
            {
            }

            [FindsBy(How = How.Id, Using = "lst-ib")]
            protected IWebElement searchField;

            [FindsBy(How = How.CssSelector, Using = "button.lsb")]
            protected IWebElement searchGo;

            [FindsBy(How = How.Id, Using = "resultStats")]
            protected IWebElement resultStats;

            [FindsBy(How = How.Id, Using = "rcnt")]
            protected IWebElement resultsContainer;

            public void Go()
            {
                Console.WriteLine(this.url);
                this.d.Navigate().GoToUrl(this.url);
            }

            public void search(string key)
            {
                this.searchField.eprint(key);
                this.searchGo.Click();

                this.d.Wait_Visible(this.resultStats, this.clearWait);
            }

            public List<SearchResult> getSearchResult()
            {
                List<SearchResult> searchResults = new List<SearchResult>();

                Console.WriteLine("getSearchResult");

                if (this.resultsContainer != null)
                {
                    ReadOnlyCollection<IWebElement> results = resultsContainer.FindElements(By.CssSelector("div.g"));

                    foreach (IWebElement result in results)
                    {
                        SearchResult searchResult = new SearchResult();

                        try
                        {
                            IWebElement caption = result.FindElement(By.CssSelector(".rc h3.r a"));

                            searchResult.caption = caption.Text;
                        }
                        catch
                        {

                        }

                        try
                        {
                            IWebElement link = result.FindElement(By.CssSelector("cite"));

                            searchResult.link = link.Text;
                        }
                        catch
                        {

                        }

                        try
                        {
                            IWebElement text = result.FindElement(By.CssSelector(".rc .s span.st"));

                            searchResult.text = text.Text;
                        }
                        catch
                        {

                        }

                        if (!searchResult.isEmpty())
                        {
                            searchResults.Add(searchResult);
                        }

                    }
                }

                return searchResults;
            }
        }

        public class BingPage : _Page
        {
            protected string url = "https://www.bing.com/";
            protected string urlPreferences = "http://www.bing.com/account/general";

            protected string settings_search_location = "New York, New York, United States";

            public BingPage(IWebDriver drv, int cW) : base(drv, cW)
            {
            }

            public BingPage(IWebDriver drv, int cW, IJavaScriptExecutor jS) : base(drv, cW, jS)
            {
            }

            [FindsBy(How = How.Id, Using = "sb_form_q")]
            protected IWebElement searchField;

            [FindsBy(How = How.Id, Using = "sb_form_go")]
            protected IWebElement searchGo;

            [FindsBy(How = How.CssSelector, Using = "#b_tween .sb_count")]
            protected IWebElement resultStats;

            [FindsBy(How = How.Id, Using = "b_results")]
            protected IWebElement resultsContainer;

            [FindsBy(How = How.Id, Using = "id_sc")]
            protected IWebElement preferences;

            [FindsBy(How = How.Id, Using = "geoname")]
            protected IWebElement geoname;

            [FindsBy(How = How.Id, Using = "sv_btn")]
            protected IWebElement settingsSave;

            [FindsBy(How = How.CssSelector, Using = ".b_no")]
            protected IWebElement noRes;

            /*[FindsBy(How = How.CssSelector, Using = "#region-section .b_vPanel a")]
            protected IWebElement countryRegion;
            */


            public void setPreferences()
            {
                //this.preferences.Click();

                this.Go(urlPreferences);
                try
                {
                    geoname.SendKeys(settings_search_location);
                    this.d.hClick(settingsSave);
                }
                catch
                {

                }
                Thread.Sleep(1000);

                this.Go("http://bing.com/account/action?scope=web&setmkt=en-US&setplang=en-us&setlang=en-us&FORM=W5WA");

            }

            public void Go(string url = null)
            {
                if (url == null) url = this.url;

                Console.WriteLine(url);
                this.d.Navigate().GoToUrl(url);
            }

            public bool search(string key)
            {
                this.searchField.eprint(key);
                this.searchGo.Click();

                try
                {
                    this.d.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
                    this.d.Wait_Visible(this.resultStats, 2);
                    this.d.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(20));

                    Console.WriteLine("RESSTATS {0}", this.d.IsElementPresent(By.CssSelector(".b_no")));

                    if (this.d.IsElementPresent(By.CssSelector(".b_no")))

                        return false;

                    return true;
                }
                catch
                {
                    return false;
                }

            }

            public List<SearchResult> getSearchResult()
            {
                List<SearchResult> searchResults = new List<SearchResult>();

                if (this.resultsContainer != null)
                {
                    ReadOnlyCollection<IWebElement> results = resultsContainer.FindElements(By.CssSelector("li.b_algo"));

                    foreach (IWebElement result in results)
                    {
                        SearchResult searchResult = new SearchResult();

                        try
                        {
                            IWebElement caption = result.FindElement(By.CssSelector("h2"));

                            searchResult.caption = caption.Text;
                        }
                        catch
                        {

                        }

                        try
                        {
                            IWebElement link = result.FindElement(By.CssSelector("cite"));

                            searchResult.link = link.Text;
                        }
                        catch
                        {

                        }

                        try
                        {
                            IWebElement text = result.FindElement(By.CssSelector(".b_caption p"));

                            searchResult.text = text.Text;
                        }
                        catch
                        {

                        }

                        if (!searchResult.isEmpty())
                        {
                            searchResults.Add(searchResult);
                        }

                    }
                }

                return searchResults;
            }
        }

        public class SearchResult
        {
            public string caption { get; set; }
            public string text { get; set; }
            public string link { get; set; }

            public SearchResult()
            {

            }

            public bool isEmpty()
            {
                return this.caption == null && this.text == null && this.link == null;
            }

            public double rating()
            {
                double rating = 0.5;

                if (this.link != null)
                {
                    rating = 1;

                    if (this.link.ToLower().Contains("morningstar")) rating = 1.5;

                    if (this.link.ToLower().Contains("fundresearch")) rating = 1.2;

                    if (this.link.ToLower().Contains("finance.yahoo")) rating = 1.5;

                    if (this.link.ToLower().Contains("mutualfundstore")) rating = 1.2;

                    if (this.link.ToLower().Contains("bloomberg")) rating = 1.5;

                }

                return rating;
            }

            public List<string> findTickers(string key = null)
            {
                List<string> not = new List<string>();

                if (key != null)
                {

                    not = key.Split(' ').ToList();
                }

                List<string> tickers = new List<string>();

                //Шаблон Тиккера
                string template = @"[A-Z]{3,5}|[A-Z0-9]{6,10}";

                //Ищем тиккер в заголоке, ссылке, тексте
                if (this.caption != null) addToListUniq(tickers, findInString(this.caption, template, not));
                if (this.link != null) addToListUniq(tickers, findInString(this.link, template, not));
                if (this.text != null) addToListUniq(tickers, findInString(this.text, template, not));

                return tickers;
            }

            public List<string> findInString(string str, string template, List<string> not)
            {
                List<string> matches = new List<string>();

                Regex expression = new Regex(template);
                var results = expression.Matches(str);

                foreach (Match match in results)
                {
                    // not использовался для того, чтобы исключить слова, находящиеся в запросе из результатов.

                    /*if (not.Find(x => x.ToLower() == match.Value.ToLower()) == null)
                    {*/
                    if (matches.Find(x => x == match.Value) == null)
                    {
                        matches.Add(match.Value);

                    }
                    // }
                }

                return matches;
            }

            //Функция добавляет элемент в список, если его там нет
            public List<string> addToListUniq(List<string> list1, List<string> list2)
            {
                foreach (string s in list2)
                {
                    if (list1.Find(x => x == s) == null)
                    {
                        list1.Add(s);
                    }
                }

                return list1;
            }
        }
    }
}
