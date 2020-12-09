using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoaderFundHolders
{
    class Edgar
    {
        public List<Fund> funds; //*Список всех фондов
        public List<Fund> fundsuniq;//*Список фондов - представителей семейства
        public Tools tools = new Tools();
        public Label statusbar;

        public string baseLink = "https://www.sec.gov";

        public string xmlPathR = "Funds/Edgar/";
        public string PathF = "Funds/EdgarF/";
        public bool overwrite = false;

        public int numberForms = 12;
        public int _I = 1;

        public string[] forms = { "N-Q", "N-CSR", "N-CSRS" }; //*Типы документов, которые нужно парсить


        public Ex ex = new Ex();


        public Dictionary<string, Fund> _funds = new Dictionary<string, Fund>();

        public HtmlNodeCollection Tables { get; set; }
        public string bondPrevName = null;

        public Edgar(int _numberForms = 1, bool fromserver = false)
        {
            statusbar = null;
            funds = new List<Fund>();

            if (!fromserver)
            {
                GetFunds();
            }
            else
            {
                funds = tools.getFundsFromServer();
            }


            makefuniq();

            numberForms = _numberForms;
        }

        public void GetFunds()
        {
            string path = Directory.GetCurrentDirectory();
            string fileName = "Source/All/_FundFamilies.xml";
            path = Path.Combine(path, fileName);

            tools.GetXML(path, "funds", XmlEachFund);
        }
        public void XmlEachFund(string xml)
        {
            Fund fund = new Fund(xml);

            if (fund.name != null)
                fund.mfname();

            funds.Add(fund);
        }
        public void makefuniq()
        {
            fundsuniq = new List<Fund>();

            foreach (Fund fund in funds)
            {
                if (fund.name == null || fundsuniq.Find(x => x.fname == fund.fname) == null)
                {
                    fundsuniq.Add(fund);
                }
            }
        }
        //*Функция, которая обрабатывает html перед тем как передать его на разбор в htmlagilitypack
        public string htmlMakeAll(string html)
        {
            html = Regex.Replace(html, @"<tr>[\n\t\s]*</tr>", @"</tr><tr>", RegexOptions.IgnoreCase);

            return html;
        }
        //*Функция, которая обрабатывает html перед тем как передать его на разбор в htmlagilitypack
        public string htmlMakeDocs(string html)
        {
            int linesCount = html.Split('\r').Length;
            if (linesCount < 20)
            {
                html = Regex.Replace(html, @"</div>", m => string.Format(@"{0}" + '\n', m.Value));
            }

            /*html = Regex.Replace(html, @"<DOCUMENT>[\s\S]*<HTML[^>]*>", @"<HTML>", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"</BODY>[\s\S]*</DOCUMENT>", @"</BODY></HTML>", RegexOptions.IgnoreCase);*/

            html = Regex.Replace(html, @"<sup.*?</sup>", @"", RegexOptions.IgnoreCase);
            return html;
        }
        //*Функция, которая просматривает обновления в системе Edgar
        public void updateFunds()
        {
            string link = "http://www.sec.gov/cgi-bin/current.pl?q1=5&q2=1&q3=";
            foreach (string form in forms)
            {
                HtmlNode body = tools.BodyNodeS(tools.Load(link + form));
                if (body != null)
                {
                    HtmlNode pre = body.SelectSingleNode(".//pre");
                    if (pre != null)
                    {
                        HtmlNodeCollection formLinks = pre.SelectNodes(".//a");
                        if (formLinks != null)
                        {
                            foreach (HtmlNode alink in formLinks)
                            {
                                if (alink.InnerText == form)
                                {
                                    string llink = baseLink + alink.GetAttributeValue("href", "");
                                    HtmlNode bodyTc = tools.BodyNodeS(tools.Load(llink), htmlMakeAll);
                                    if (bodyTc != null)
                                    {

                                        List<Fund> uFunds = selectUTickers(bodyTc);
                                        if (uFunds.Count > 0)
                                        {
                                            foreach (Fund fund in uFunds)
                                            {
                                                fund.formName = form;
                                                LoadFormAll(fund, bodyTc, llink, true);
                                                //SAVE
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        //*Функция, которая ищет тиккеры, по которым появились новые отчеты
        public List<Fund> selectUTickers(HtmlNode body)
        {
            List<Fund> tickers = new List<Fund>();
            HtmlNodeCollection tds = body.SelectNodes(".//div[@id='seriesDiv']//tr[@class='contractRow']//td[position() = 4]");

            if (tds != null)
            {
                foreach (HtmlNode td in tds)
                {
                    var tval = td.InnerText.Trim().ToLower();

                    Fund fund = funds.Find(x => x.ticker.ToLower() == tval);
                    if (fund != null)
                    {
                        tickers.Add(fund);
                    }
                }
            }
            return tickers;
        }
        public void MakeFunds(Action<Fund> make)
        {
            if (funds != null)
            {
                int i = 0;
                foreach (Fund fund in fundsuniq)
                {
                    i++;
                    bool download = true;

                    download = ((download && !checkFile(fund)) || overwrite);

                    if (download)
                    {
                        //*Массив обработанных фондов, сейчас не используется
                        if (!_funds.ContainsKey(fund.ticker))
                        {
                            //Console.WriteLine(fund.ticker);
                            _funds.Add(fund.ticker, fund);
                        }

                        if (statusbar != null)
                        {
                            statusbar.Text = "Status: " + fund.ticker + " (" + i + "/" + fundsuniq.Count + ")";
                            Application.DoEvents();
                        }


                        if (fund.ticker.ToLower() != "prn")
                        {
                            make(fund);
                        }

                        //SAVE
                    }

                }
            }
            else
            {
                Console.Write("0005");
            }
        }
        public bool checkFile(Fund fund)
        {
            string d = Directory.GetCurrentDirectory();
            string nameOfFile = d + "\\Funds\\Edgar\\errors\\" + fund.ticker + ".xml";

            return System.IO.File.Exists(nameOfFile) || Directory.Exists(d + "\\Funds\\Edgar\\" + fund.ticker);
        }
        public string takeClassContract(HtmlNode body)
        {
            HtmlNodeCollection identInfoc = body.SelectNodes(".//p[@class='identInfo']/strong");

            var re = new Regex(@"C[0-9]+");

            if (identInfoc != null)
            {
                foreach (HtmlNode identInfo in identInfoc)
                {

                    Match cc = re.Match(identInfo.InnerText);

                    if (cc != null && cc.Success)
                    {
                        return identInfo.InnerText;
                    }
                }
            }

            return null;
        }
        public void MakeFamilies()
        {
            if (funds != null)
            {
                foreach (Fund fund in funds)
                {
                    fund.name = null;
                }

                XmlDocument document;
                string xmlPath = null;

                tools.MakeXml(Path.Combine(Directory.GetCurrentDirectory(), "Funds/Edgar/_fundfamily.xml"), out document, out xmlPath, "funds-list");

                int i = 0;

                foreach (Fund fund in funds)
                {
                    i++;

                    statusbar.Text = "Status: " + fund.ticker + " (" + i + "/" + funds.Count + ")";

                    if (fund.ticker.ToLower() != "prn")
                    {
                        if (fund.name == null)
                        {
                            MakeFamily(fund);
                        }
                        else
                        {
                            Console.WriteLine("bonus : {0} - {1}", fund.ticker, fund.name);
                        }


                        if (fund.name != null)
                        {
                            XmlNode fundsXml = document.CreateElement("funds");
                            document.DocumentElement.AppendChild(fundsXml);


                            tools.createNode(document, fundsXml, "FundTicker", fund.ticker);
                            tools.createNode(document, fundsXml, "Name", fund.name);

                            document.Save(xmlPath);
                        }


                    }

                }
            }
            else
            {
                Console.Write("0005");
            }
        }
        public void MakeFamily(Fund fund)
        {
            string urlForms = baseLink + "/cgi-bin/browse-edgar?CIK=" + fund.ticker + "&owner=exclude&action=getcompany&count=200&Find=Search";

            HtmlNode body = tools.BodyNodeS(tools.Load(urlForms), htmlMakeAll);

            string familyName = "";
            bool finded = false;

            if (body == null)
            {
                Console.WriteLine("{0} - NOT FOUND IN EDGAR", fund.ticker);
                fund.makeFailXml("NOT FOUND IN EDGAR", null, xmlPathR);
                //errors.Error("0008");
                fund.status = 1;
            }
            else
            {
                fund.classcontract = takeClassContract(body);

                HtmlNode a = body.SelectSingleNode(".//p[@class='identInfo']/a[position()=2]");
                if (a != null)
                {
                    string link = baseLink + a.GetAttributeValue("href", "");
                    Console.WriteLine(link);

                    HtmlNode bodyL = tools.BodyNodeS(tools.LoadWeb(link), htmlMakeAll);

                    if (bodyL != null)
                    {
                        HtmlNodeCollection trs = bodyL.SelectNodes(".//table[@summary]//tr");
                        if (trs != null)
                        {
                            foreach (HtmlNode tr in trs)
                            {
                                HtmlNodeCollection tds = tr.SelectNodes(".//td");


                                if (tds != null)
                                {
                                    int t = 0;

                                    foreach (HtmlNode td in tds)
                                    {
                                        HtmlNode aa = td.SelectSingleNode("./a");

                                        if (!finded)
                                        {
                                            if (aa != null)
                                            {
                                                //Console.WriteLine(aa.GetAttributeValue("class", ""));
                                                if (aa.GetAttributeValue("class", "") == "")
                                                {
                                                    familyName = aa.InnerText;
                                                }
                                            }

                                            if (td.InnerText == fund.classcontract)
                                            {
                                                finded = true;

                                                if (familyName != "" && tds[t + 1] != null)
                                                {
                                                    setName(fund, familyName, tds[t + 1].InnerText);

                                                }

                                            }
                                        }

                                        if (t == 4)
                                        {
                                            Fund fundBonus = funds.Find(x => x.ticker.ToLower() == tds[t].InnerText.ToLower().Trim());


                                            if (fundBonus != null && familyName != "")
                                            {
                                                setName(fundBonus, familyName, tds[t - 1].InnerText);

                                            }
                                        }

                                        t++;

                                    }
                                }

                            }
                        }

                    }
                }

            }
        }
        public void setName(Fund fund, string name, string _suffix)
        {
            string suffix = Regex.Replace(_suffix, @"class|shares|" + name, @"", RegexOptions.IgnoreCase).Trim();
            suffix = Regex.Replace(suffix, @"[ ]{2,}", @" ", RegexOptions.IgnoreCase).Trim();

            List<string> aname = name.Split(' ').ToList();
            List<string> asuffix = suffix.Split(' ').ToList();
            List<string> newsuffix = new List<string>();

            foreach (string psuffix in asuffix)
            {
                if (aname.Find(x => x == psuffix) == null)
                {
                    newsuffix.Add(psuffix);
                }
                else
                {
                    newsuffix.Clear();
                }
            }

            fund.name = name + ";" + Regex.Replace(string.Join(" ", newsuffix), @"-|", @"", RegexOptions.IgnoreCase).Trim();
        }
        public void MakeFund(Fund fund)
        {
            //Console.WriteLine("Ticker: {0} - {1}", fund.ticker, _I);
            _I++;
            int currentNum = 1;
            int _cNum = currentNum;

            string urlForms = baseLink + "/cgi-bin/browse-edgar?CIK=" + fund.ticker + "&owner=exclude&action=getcompany&count=200&Find=Search";
            //string urlForms = "http://www.sec.gov/cgi-bin/browse-edgar?company=blackrock+global+allocation&owner=exclude&action=getcompany";
            // Console.WriteLine(urlForms);

            //*Вход в edgar на страницу с отчетами для этого фонда
            HtmlNode body = tools.BodyNodeS(tools.Load(urlForms), htmlMakeAll);

            if (body == null)
            {
                Console.WriteLine("{0} - NOT FOUND IN EDGAR", fund.ticker);
                fund.makeFailXml("NOT FOUND IN EDGAR", null, xmlPathR);
                //errors.Error("0008");
                fund.status = 1;
            }
            else
            {
                fund.classcontract = takeClassContract(body);

                do
                {
                    //*Ищем необходимую форму
                    string urlForm = FindNQ(fund, body, currentNum);
                    fund.companyName = FindCompanyName(body);
                    try
                    {
                        body = LoadFormAll(fund, body, urlForm, true);
                    }
                    catch (OutOfMemoryException e)
                    {
                        Console.WriteLine("{0} - OutOfMemory", fund.ticker);
                    }
                    currentNum++;
                }
                while (currentNum <= numberForms);
            }
        }
        public HtmlNode LoadFormAll(Fund fund, HtmlNode body, string urlForm, bool makeXmlAll = false)
        {
            // Console.WriteLine("LoadForm");

            fund.netAssetsFromEdgar = 0;

            List<string> names = new List<string>();
            List<Caption> captions = new List<Caption>();
            //*Вторая страница Edgar для конкретного отчета. 
            HtmlNode bodyForm = tools.BodyNodeS(tools.Load(urlForm), htmlMakeAll);

            if (bodyForm == null)
            {
                return body;
            }
            else
            {
                //*Берем обязательно даты, они нужны для сохранения
                fund.periodOfReport = FindPeriodOfReport(bodyForm);
                fund.fillingDate = FindFillingDate(bodyForm);
                fund.dateEffectiveness = FindDateEffectiveness(bodyForm);

                fund.source = FindUrlDocument(bodyForm, fund.formName);
                fund.nameFromEdgar = FindFullNameClc(bodyForm, fund);

                /*if (FindFullName(bodyForm, fund) == fund.nameFromEdgar)
                {
                    Console.WriteLine("{0} - CHECKED", fund.ticker);
                    //fund.makeFailXml("NO FAIL", captions, xmlPathR);
                    return null;
                }*/

                Console.WriteLine("FUND : {0}", fund.ticker);

                //*Берем названия фондов, которые так же находятся в данном отчете
                GetFullNames(bodyForm, fund, names, captions);

                if (fund.source != null)
                {
                    if (fund.nameFromEdgar != null)
                    {
                        //*Документ, который нужно распарсить
                        string doc = tools.Load(fund.source);

                        //*Определяем тип файла
                        string[] url = fund.source.Split('/');
                        string[] nameOfFile = url[url.Length - 1].Split('.');
                        string ext = nameOfFile[nameOfFile.Length - 1];
                        //Console.WriteLine(ext);

                        if (ext == "html" || ext == "htm")
                        {
                            // fund.saveSource(s, xmlPathR);
                            HtmlNode bodyDocument = tools.BodyNodeS(doc, htmlMakeDocs);


                            if (bodyDocument != null)
                            {
                                //*Сумма всех позиций с сайта morningstar
                                fund.TotalAssetsMorning();
                                Document document = new Document(bodyDocument);

                                document.Parse(fund, captions);

                                //*Вторая попытка
                                if (document.holdings.Count == 0)
                                {
                                    //Console.WriteLine("Try Again");
                                    //*Сжимаем заголовки
                                    zNames(captions, fund);
                                    document.Parse(fund, captions);
                                }

                                if (document.holdings.Count > 0)
                                {
                                    //*Трансормируем набор позиций из словаря в список и назначаем фонду
                                    tools.transformToList(fund, document.holdings);
                                    fund.sumHoldings();
                                    //*Сохранение
                                    fund.MakeXml(xmlPathR);
                                }
                                else
                                {
                                    Console.WriteLine("{0} - HOLDINGS FAIL", fund.ticker);
                                    fund.makeFailXml("HOLDINGS FAIL", captions, xmlPathR);
                                }

                                fund.remove();
                            }
                            else
                            {
                                Console.WriteLine("{0} - HTML FAIL", fund.ticker);
                                fund.makeFailXml("HTML FAIL", captions, xmlPathR);
                                //errors.Error("0012");
                                //*ошибка с html функция_для_кривого_html(fund, captions);
                                //captions - полные названия фондов в этом документе. Взято с предыдущей страницы edgar
                                //fund.sumHoldings(); fund.MakeXml(xmlPathR);   - для сохранения

                            }
                        }
                        else
                        {
                            Console.WriteLine("{0} - TXT FAIL", fund.ticker);
                            fund.makeFailXml("TXT FAIL", captions, xmlPathR);
                            //NOTE - файл(doc) - txt. функция_для_txt(fund, captions);
                        }



                    }
                    else
                    {

                    }
                }
                else
                {
                    Console.Write("0013");
                    fund.status = 2;
                }
            }
            return body;
        }

        //*Функция, которая ищет названия текущего фонда
        public string FindFullName(HtmlNode BodyNodeS, Fund fund)
        {
            bool previous = false;
            string name = null;


            HtmlNode tableFile = BodyNodeS.SelectSingleNode(".//table[@class='tableFile']");

            if (tableFile != null)
            {
                foreach (HtmlNode tr in tableFile.SelectNodes(".//tr"))
                {
                    HtmlNodeCollection tds = BodyNodeS.SelectNodes(".//td");
                    if (tds != null)
                    {
                        foreach (HtmlNode td in tds)
                        {

                            if (td.GetAttributeValue("class", "") == "seriesCell" && tools.ClearSpace(td.InnerText) != "")
                            {
                                name = td.InnerText;
                                previous = true;
                            }

                            if (previous == true)
                            {
                                if (td.InnerText == fund.ticker && name != null)
                                {
                                    return name;
                                }
                            }
                        }
                    }
                }

            }

            //if (fund.nameFromEdgar != null) name = fund.nameFromEdgar;

            return name;
        }
        public string FindFullNameClc(HtmlNode BodyNodeS, Fund fund)
        {
            bool previous = false;
            string name = null;


            HtmlNode tableFile = BodyNodeS.SelectSingleNode(".//table[@class='tableFile']");

            if (tableFile != null)
            {
                foreach (HtmlNode tr in tableFile.SelectNodes(".//tr"))
                {
                    HtmlNodeCollection tds = BodyNodeS.SelectNodes(".//td");
                    if (tds != null)
                    {
                        foreach (HtmlNode td in tds)
                        {

                            if (td.GetAttributeValue("class", "") == "seriesCell" && tools.ClearSpace(td.InnerText) != "")
                            {
                                name = td.InnerText;
                                previous = true;
                            }

                            if (previous == true)
                            {

                                if (td.GetAttributeValue("class", "") == "classContract" && name != null)
                                {
                                    HtmlNode a = td.SelectSingleNode(".//a");

                                    if (a != null && a.InnerText == fund.classcontract) return name;
                                }

                            }
                        }
                    }
                }

            }

            if (fund.nameFromEdgar != null) name = fund.nameFromEdgar;

            return name;
        }
        //*Функция, которая ищет названия всех фондов и заполняет список заголовков
        public void GetFullNames(HtmlNode BodyNodeS, Fund fund, List<string> names, List<Caption> captions)
        {

            HtmlNodeCollection nms = BodyNodeS.SelectNodes(".//td[@class='seriesCell'][normalize-space()!='&nbsp;']");
            if (nms != null)
            {
                foreach (HtmlNode n in nms)
                {
                    bool first = false;
                    if (fund.nameFromEdgar == n.InnerText)
                    {
                        first = true;
                    }

                    string captiontext = n.InnerText;


                    names.Add(n.InnerText);
                    //*Добавляем заголовок
                    captions.Add(new Caption(n.InnerText, tools.ClearSpaceCaption(captiontext).ToLower(), first));
                }
            }
        }
        //*Функция, которая обрезает одинаковую часть у всех заголовков
        public void zNames(List<Caption> captions, Fund fund)
        {

            int i = zNamesFindI(captions, captions.First().compressednamenozip);

            if (i == 0)
                zNamesFindI2(captions, tools.ClearSpaceCaption(fund.companyName).ToLower());

            foreach (Caption caption in captions)
            {

                if (caption.izip != 0)
                {
                    caption.compressedname = caption.compressedname.Substring(caption.izip);
                }

                if (caption.compressedname.Length > 10)
                    caption.compressedname = Regex.Replace(caption.compressedname, @"fund$", @"", RegexOptions.IgnoreCase);

                caption.compressedname = Regex.Replace(caption.compressedname, @"inc$", @"", RegexOptions.IgnoreCase);

            }
        }
        public int zNamesFindI(List<Caption> captions, string basic)
        {
            bool end = false;
            int i = 0;

            if (captions.Count > 1)
                do
                {
                    if (i < basic.Length)
                    {
                        char r = basic[i];
                        foreach (Caption caption in captions)
                        {
                            if (caption.compressedname[i] != r)
                            {
                                caption.izip = i;
                                end = true;
                            }
                        }
                        if (end != true) i++;
                    }
                    else
                    {
                        end = true;
                    }
                }
                while (end == false);

            return i;
        }
        public int zNamesFindI2(List<Caption> captions, string basic)
        {

            int I = 0;

            if (captions.Count > 1)
            {

                foreach (Caption caption in captions)
                {
                    int i = 0;

                    bool end = false;
                    do
                    {
                        char r = basic[i];
                        if (i > caption.compressedname.Length - 10 || caption.compressedname[i] != r)
                        {
                            end = true;
                            caption.izip = i;

                            if (i > I) I = i;
                        }
                        i++;
                    }
                    while (end == false);
                }
            }

            return I;
        }
        public string FindCompanyName(HtmlNode BodyNodeS)
        {
            HtmlNode name = BodyNodeS.SelectSingleNode(".//span[@class='companyName']");
            if (name != null)
            {
                return name.InnerText.Split('#')[0];
            }

            return null;
        }
        //*Функция, которая находит нужную форму, num - номер формы (считаются только нужные типы отчетов) от начала таблицы с формами
        public string FindNQ(Fund fund, HtmlNode BodyNodeS, int num = 1)
        {
            int _num = 1;
            bool next = false;
            string url = null;
            HtmlNode series = BodyNodeS.SelectSingleNode(".//div[@id='seriesDiv']");

            if (series != null)
            {

                foreach (HtmlNode table in series.SelectNodes(".//table"))
                {
                    foreach (HtmlNode tr in table.SelectNodes(".//tr"))
                    {
                        HtmlNodeCollection tds = BodyNodeS.SelectNodes(".//td");
                        if (tds != null)
                        {
                            foreach (HtmlNode td in tds)
                            {
                                if (next == true)
                                {
                                    HtmlNode a = td.SelectSingleNode(".//a");

                                    if (url != null && td.GetAttributeValue("class", "") == "")
                                    {
                                        fund.forms++;
                                        return url;
                                    }

                                    if (a != null)
                                    {
                                        if (tools.ClearStrNameNew(a.InnerText) == "Documents")
                                        {
                                            url = baseLink + a.GetAttributeValue("href", "");
                                        }

                                    }
                                }
                                else
                                {
                                    if (td.InnerText == "N-Q")
                                    {

                                        fund.formName = "N-Q";
                                        if (_num == num)
                                        {
                                            next = true;
                                        }
                                        _num++;

                                    }
                                    if (td.InnerText == "N-CSRS")
                                    {
                                        fund.formName = "N-CSRS";
                                        if (_num == num)
                                        {
                                            next = true;
                                        }
                                        _num++;
                                    }
                                    if (td.InnerText == "N-CSR")
                                    {
                                        fund.formName = "N-CSR";
                                        if (_num == num)
                                        {
                                            next = true;
                                        }
                                        _num++;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return null;
        }
        //*Функция, которая находит адрес документа
        public string FindUrlDocument(HtmlNode BodyNodeS, string formName)
        {
            bool previous = false;
            string url = null;

            HtmlNode tableFile = BodyNodeS.SelectSingleNode(".//table[@class='tableFile']");

            if (tableFile != null)
            {
                foreach (HtmlNode tr in tableFile.SelectNodes(".//tr"))
                {

                    HtmlNodeCollection tds = BodyNodeS.SelectNodes(".//td");
                    if (tds != null)
                    {
                        foreach (HtmlNode td in tds)
                        {

                            HtmlNode a = td.SelectSingleNode(".//a");
                            if (a != null)
                            {
                                url = baseLink + a.GetAttributeValue("href", "");
                                previous = true;
                            }

                            if (previous == true)
                            {
                                if (td.InnerText == formName && url != null)
                                {
                                    return url;
                                }
                            }
                        }
                    }
                }

            }

            return url;
        }
        //*Функции, которые находят на странице даты публикации отчета
        public string FindPeriodOfReport(HtmlNode body)
        {
            HtmlNode node = body.SelectSingleNode(".//div[@class='formContent']/div[@class='formGrouping'][position()=2]/div[@class='info'][position()=1]");
            if (node != null)
            {
                return node.InnerText.Trim();
            }
            else
                return null;

        }
        public string FindFillingDate(HtmlNode body)
        {
            HtmlNode node = body.SelectSingleNode(".//div[@class='formContent']/div[@class='formGrouping'][position()=2]/div[@class='info'][position()=2]");
            if (node != null)
            {
                return node.InnerText.Trim();
            }
            else
                return null;

        }
        public string FindDateEffectiveness(HtmlNode body)
        {
            HtmlNode node = body.SelectSingleNode(".//div[@class='formContent']/div[@class='formGrouping'][position()=2]/div[@class='info'][position()=3]");
            if (node != null)
            {
                return node.InnerText.Trim();
            }
            else
                return null;

        }

        public void errorStats()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Funds/Edgar/errors");


            string[] files = Directory.GetFiles(path, "*.xml");

            int all = 0;
            int txt = 0;
            int hld = 0;
            int html = 0;
            int nfe = 0;

            foreach (string file in files)
            {
                all++;
                string f = tools.openFileD(file);

                if (f.Contains("TXT FAIL")) txt++;
                if (f.Contains("NOT FOUND")) nfe++;
                if (f.Contains("HOLDINGS FAIL")) hld++;
                if (f.Contains("HTML FAIL")) html++;
            }

            Console.WriteLine("ALL {0}", all);
            Console.WriteLine("______________________________");
            Console.WriteLine("NFE {0}", nfe);
            Console.WriteLine("HTML {0}", html);
            Console.WriteLine("HLD {0}", hld);
            Console.WriteLine("TXT {0}", txt);
        }

    }
}
