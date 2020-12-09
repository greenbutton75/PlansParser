using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;


namespace LoaderFundHolders
{
    [XmlRoot("funds")]
    public class Fund
    {
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlElement("FundTicker")]
        public string ticker { get; set; }
        [XmlElement("Name")]
        public string name { get; set; }
        public string classcontract { get; set; }
        public string fname { get; set; }
        public string periodOfReport { get; set; }
        public string dateEffectiveness { get; set; }
        public string fillingDate { get; set; }
        public int forms { get; set; }
        public string link { get; set; }
        public string phone { get; set; }
        public string address { get; set; }

        public string source { get; set; }
        public List<Holding> holdings { get; set; }
        public Tools tools = new Tools();
        public long netAssetsFromMorningStar { get; set; }
        public double netAssetsFromEdgar { get; set; }
        public string nameFromEdgar { get; set; }
        public string formName { get; set; }
        public string manager { get; set; }
        public string indexTicker { get; set; }
        public int status { get; set; }
        public int ctable { get; set; }
        public string expenseRatio { get; set; }
        public bool end { get; set; }
        public string statusM { get; set; }
        public string cash { get; set; }
        public string stocks { get; set; }
        public string bonds { get; set; }
        public string other { get; set; }
        public string companyName { get; set; }
        public string prospectus { get; set; }
        public List<KeyValuePair<string, string>> FeesExpensesMs { get; set; }
        public List<KeyValuePair<string, string>> FeesExpenses { get; set; }
        public List<KeyValuePair<string, string>> FundOverview { get; set; }
        public List<KeyValuePair<string, string[]>> history { get; set; }
        public List<KeyValuePair<string, string[]>> historyT { get; set; }
        public Fund()
        {
            newLists();
        }
        ~Fund()
        {
            remove();
        }
        public void remove()
        {
            ctable = 0;
            holdings.Clear();
        }

        public Fund(string _name, string _link, string _ticker)
        {
            ticker = _ticker;
            link = _link;
            name = _name;
            newLists();
            ctable = 0;
        }
        public Fund(string _name, string _link)
        {
            link = _link;
            name = _name;
            newLists();
        }

        public Fund(string xml)
        {
            forms = 0;
            LoadXml(xml);

            newLists();
        }
        public void newLists()
        {
            holdings = new List<Holding>();
        }

        public void LoadXml(string xml)
        {
            XmlSerializer mySerializer = new XmlSerializer(this.GetType());
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                object obj = mySerializer.Deserialize(ms);
                foreach (PropertyInfo p in obj.GetType().GetProperties())
                {
                    PropertyInfo p2 = this.GetType().GetProperty(p.Name);
                    if (p2 != null && p2.CanWrite)
                    {
                        p2.SetValue(this, p.GetValue(obj, null), null);
                    }
                }
            }
        }
        public void sumHoldings()
        {
            double sum = 0;

            foreach (Holding holding in holdings)
            {
                sum += holding._value;
            }

            netAssetsFromEdgar = sum;
        }
        public void transformbunds()
        {
            foreach (Holding holding in holdings)
            {
                holding.transformbund();
            }
        }
        public void saveSource(string s, string path)
        {
            string folder = ticker + "/source";
            string folderName = path + folder;
            DirectoryInfo drInfo = new DirectoryInfo(folderName);

            if (!drInfo.Exists)
            {
                drInfo.Create();
            }

            string pthS = folderName + "/" + dateEffectiveness + ".html";

            StreamWriter SW = new StreamWriter(new FileStream(pthS, FileMode.Create, FileAccess.Write));
            SW.Write(s);
            SW.Close();
        }
        public void makeFailXml(string error, List<Caption> captions, string path)
        {
            XmlDocument document;
            string xmlPath;

            string folderName = path + "errors";

            DirectoryInfo drInfo = new DirectoryInfo(folderName);

            if (!drInfo.Exists)
            {
                drInfo.Create();
            }

            string xmlPathR = folderName + "/" + ticker + ".xml";
            System.IO.File.Delete(xmlPathR);
            tools.MakeXml(xmlPathR, out document, out xmlPath);

            XmlNode fundXml = document.CreateElement("funds");
            document.DocumentElement.AppendChild(fundXml);
            AddToXML(document, fundXml);

            XmlNode ErrorNode = createNode(document, fundXml, "Error", error);

            if (captions != null && captions.Count > 0)
            {
                Caption firstC = captions.Find(x => x.first == true);
                if (firstC != null)
                {
                    XmlNode MainCaptionsNode = createNode(document, fundXml, "MainCaption", firstC.name);

                    XmlNode CaptionsNode = createNode(document, fundXml, "OtherFundsCaptions");
                    foreach (Caption caption in captions)
                    {
                        if (caption.first == false)
                            createNode(document, CaptionsNode, "caption", caption.name);
                    }
                }
            }


            document.Save(xmlPath);

        }
        public void MakeToXml(string path, Action<XmlDocument, XmlNode, Fund> AddToXML = null)
        {
            string folder = ticker;
            XmlDocument document;
            string xmlPath;

            DirectoryInfo drInfo = new DirectoryInfo(path);

            if (!drInfo.Exists)
            {
                drInfo.Create();
            }

            string pth = path + ticker + ".xml";
            string p = Directory.GetCurrentDirectory();
            string fileName = String.Format(pth);
            string xmlPathR = System.IO.Path.Combine(p, fileName);
            System.IO.File.Delete(xmlPathR);
            tools.MakeXml(xmlPathR, out document, out xmlPath);

            XmlNode fundsXml = document.CreateElement("funds");
            document.DocumentElement.AppendChild(fundsXml);

            AddToXML(document, fundsXml, this);
            document.Save(xmlPath);
        }
        public void MakeXml(string path, bool edg = true)
        {
            string folder = ticker;
            XmlDocument document;
            string xmlPath;

            if (edg == true)
            {
                statusM = "undefined";

                if (netAssetsFromEdgar != 0)
                {
                    if (netAssetsFromMorningStar == 0)
                    {
                        statusM = "morning0";
                    }
                    else
                        if (netAssetsFromMorningStar != 0 && ((double)netAssetsFromMorningStar / netAssetsFromEdgar < 0.5 || (double)netAssetsFromMorningStar / netAssetsFromEdgar > 2))
                    {
                        statusM = "morningnoteq";
                    }
                    else
                    {
                        statusM = "morningeq";
                    }

                }
                string folderName = path + folder;


                DirectoryInfo drInfo = new DirectoryInfo(folderName);

                if (!drInfo.Exists)
                {
                    drInfo.Create();
                }

                string xmlPathR = folderName + "/" + dateEffectiveness + ".xml";
                System.IO.File.Delete(xmlPathR);
                tools.MakeXml(xmlPathR, out document, out xmlPath);

                XmlNode fundsXml = document.CreateElement("funds");
                document.DocumentElement.AppendChild(fundsXml);

                AddToXML(document, fundsXml);
                document.Save(xmlPath);
            }
            else
            {
                DirectoryInfo drInfo = new DirectoryInfo(path);

                if (!drInfo.Exists)
                {
                    drInfo.Create();
                }

                string pth = path + ticker + ".xml";
                string p = Directory.GetCurrentDirectory();
                string fileName = String.Format(pth);
                string xmlPathR = System.IO.Path.Combine(p, fileName);
                System.IO.File.Delete(xmlPathR);
                tools.MakeXml(xmlPathR, out document, out xmlPath);

                XmlNode fundsXml = document.CreateElement("funds");
                document.DocumentElement.AppendChild(fundsXml);

                AddToXML(document, fundsXml);
                document.Save(xmlPath);
            }


        }
        public void TotalAssetsMorning()
        {
            string url = "http://quotes.morningstar.com/fund/c-header?&t=XNAS:" + ticker + "&region=usa&culture=en-US&version=RET&cur=&test=QuoteiFrame";
            netAssetsFromMorningStar = 0;
            HtmlNode bodyForm = tools.BodyNodeS(tools.Load(url), null, ".//table");
            if (bodyForm != null)
            {
                HtmlNode TotalAssets = bodyForm.SelectSingleNode(".//span[@vkey='TotalAssets']");
                if (TotalAssets != null)
                {
                    string val = TotalAssets.InnerText.Trim();
                    Match mm = Regex.Match(val, @"[0-9.]*");

                    Match c = Regex.Match(val, @"[a-zA-Z]+");

                    double Number;
                    if (Double.TryParse(mm.Value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out Number))
                    {
                        int coef = 1;
                        if (c.Value == "mil") coef = 1000000;
                        if (c.Value == "bil") coef = 1000000000;
                        netAssetsFromMorningStar = Convert.ToInt64(Number * coef);
                    }
                }
            }
        }
        public string getFeesExpenses(string key)
        {
            if (FeesExpenses != null)
            {
                KeyValuePair<string, string> kvp = FeesExpenses.Find(x => x.Key == key);

                return kvp.Value;

            }
            return null;
        }
        public string getFundOverview(string key)
        {
            if (FeesExpenses != null)
            {
                KeyValuePair<string, string> kvp = FundOverview.Find(x => x.Key == key);

                return kvp.Value;

            }
            return null;
        }
        public bool filterFE()
        {
            if (FundOverview != null && FeesExpenses != null)
            {
                double annualreportexpenserationet;
                bool db = tools.extendTryParse(getFeesExpenses("annualreportexpenserationet"), out annualreportexpenserationet);

                double morningstarrating;
                db = tools.extendTryParse(getFundOverview("morningstarrating"), out morningstarrating) && db;

                string max12b1fee = getFeesExpenses("max12b1fee");

                //Console.WriteLine("{0}, {1}, {2}, {3}", db, annualreportexpenserationet, max12b1fee, morningstarrating);

                if (db == true && annualreportexpenserationet < 1.25 && max12b1fee == "n/a" && morningstarrating >= 3)
                {
                    return true;
                }
            }

            return false;
        }
        public void beforeSave()
        {
            if (FeesExpenses == null || FeesExpenses.Count > 0)
            {
                if (FeesExpensesMs != null && FeesExpensesMs.Count > 0) FeesExpenses = FeesExpensesMs;
            }
        }
        public void mfname()
        {
            string[] words = name.Split(new char[] { ';' });
            fname = words[0];
        }
        public XmlNode AddToXML(XmlDocument document, XmlNode fundsXml, bool all = true)
        {
            XmlNode fundXml = document.CreateElement("fund");
            fundsXml.AppendChild(fundXml);

            beforeSave();

            if (name != null && name != "")
            {
                XmlNode nameXml = document.CreateElement("name");
                nameXml.InnerText = Regex.Replace(name, @"&amp;", @"&", RegexOptions.IgnoreCase); ;
                fundXml.AppendChild(nameXml);
            }


            if (ticker != null)
            {
                XmlNode tickerXml = document.CreateElement("ticker");
                tickerXml.InnerText = ticker;
                fundXml.AppendChild(tickerXml);
            }

            if (netAssetsFromMorningStar != 0)
            {
                XmlNode NetAssetsFromMorningStarXml = document.CreateElement("netAssetsFromMorningStar");
                NetAssetsFromMorningStarXml.InnerText = netAssetsFromMorningStar.ToString();
                fundXml.AppendChild(NetAssetsFromMorningStarXml);
            }

            if (companyName != null)
            {
                XmlNode companyNameXml = document.CreateElement("companyName");
                companyNameXml.InnerText = companyName;
                fundXml.AppendChild(companyNameXml);
            }

            if (phone != null)
            {
                XmlNode phoneXml = document.CreateElement("phone");
                phoneXml.InnerText = phone;
                fundXml.AppendChild(phoneXml);
            }
            if (address != null)
            {
                XmlNode addressXml = document.CreateElement("address");
                addressXml.InnerText = address;
                fundXml.AppendChild(addressXml);
            }

            if (FeesExpenses != null && FeesExpenses.Count > 0)
            {
                XmlNode FeesExpensesNode = createNode(document, fundXml, "FeesExpenses");
                foreach (KeyValuePair<string, string> kvp in FeesExpenses)
                {
                    createNode(document, FeesExpensesNode, kvp.Key, kvp.Value);
                }
            }


            /*if (FeesExpensesMs != null && FeesExpensesMs.Count > 0)
            {
                XmlNode FeesExpensesNode = createNode(document, fundXml, "FeesExpensesMs");
                foreach (KeyValuePair<string, string> kvp in FeesExpensesMs)
                {
                    createNode(document, FeesExpensesNode, kvp.Key, kvp.Value);
                }
            }*/


            if (FundOverview != null && FundOverview.Count > 0)
            {
                XmlNode FundOverviewNode = createNode(document, fundXml, "FundOverview");
                foreach (KeyValuePair<string, string> kvp in FundOverview)
                {
                    createNode(document, FundOverviewNode, kvp.Key, kvp.Value);
                }
            }

            if (history != null && history.Count > 0)
            {
                XmlNode HistoryNode = createNode(document, fundXml, "History");
                foreach (KeyValuePair<string, string[]> hvp in history)
                {
                    XmlNode DataNode = createNode(document, HistoryNode, "Data");
                    createNode(document, DataNode, "Date", hvp.Key);
                    if (hvp.Value[0] != "") createNode(document, DataNode, "Closed", hvp.Value[0]);
                    if (hvp.Value[1] != "") createNode(document, DataNode, "AdjClosed", hvp.Value[1]);
                }
            }

            if (prospectus != null)
            {
                createNode(document, fundXml, "prospectus", prospectus);
            }


            if (all == true)
            {
                if (cash != null)
                {
                    createNode(document, fundXml, "cash", cash);
                }

                if (stocks != null)
                {
                    createNode(document, fundXml, "stocks", stocks);
                }
                if (bonds != null)
                {
                    createNode(document, fundXml, "bonds", bonds);
                }
                if (other != null)
                {
                    createNode(document, fundXml, "other", other);
                }
                if (link != null)
                {
                    XmlNode linkXml = document.CreateElement("link");
                    linkXml.InnerText = link;
                    fundXml.AppendChild(linkXml);
                }

                if (expenseRatio != null)
                {
                    XmlNode expenseRatioXml = document.CreateElement("expenseRatio");
                    expenseRatioXml.InnerText = expenseRatio;
                    fundXml.AppendChild(expenseRatioXml);
                }

                if (statusM != null)
                {
                    XmlNode statusMXml = document.CreateElement("statusM");
                    statusMXml.InnerText = statusM;
                    fundXml.AppendChild(statusMXml);
                }

                if (manager != null)
                {
                    XmlNode managerXml = document.CreateElement("manager");
                    managerXml.InnerText = manager;
                    fundXml.AppendChild(managerXml);
                }


                if (formName != null)
                {
                    XmlNode formNameXml = document.CreateElement("formName");
                    formNameXml.InnerText = formName;
                    fundXml.AppendChild(formNameXml);
                }

                if (source != null)
                {
                    XmlNode sourceXml = document.CreateElement("source");
                    sourceXml.InnerText = source;
                    fundXml.AppendChild(sourceXml);
                }

                if (nameFromEdgar != null)
                {
                    XmlNode nameFromEdgarXml = document.CreateElement("nameFromEdgar");
                    nameFromEdgarXml.InnerText = nameFromEdgar;
                    fundXml.AppendChild(nameFromEdgarXml);
                }

                if (netAssetsFromEdgar != 0)
                {
                    XmlNode NetAssetsFromEdgarXml = document.CreateElement("netAssetsFromEdgar");
                    NetAssetsFromEdgarXml.InnerText = netAssetsFromEdgar.ToString();
                    fundXml.AppendChild(NetAssetsFromEdgarXml);
                }



                if (indexTicker != null)
                {
                    XmlNode indexTickerXml = document.CreateElement("indexTicker");
                    indexTickerXml.InnerText = indexTicker;
                    fundXml.AppendChild(indexTickerXml);
                }



                if (holdings.Count > 0)
                {
                    XmlNode holdingsXml = document.CreateElement("holdings");
                    fundXml.AppendChild(holdingsXml);

                    foreach (Holding holding in holdings)
                    {
                        holding.AddToXML(document, holdingsXml);
                    }
                }

                if (id != null)
                {
                    XmlNode idXml = document.CreateElement("id");
                    idXml.InnerText = id;
                    fundXml.AppendChild(idXml);
                }

                if (periodOfReport != null)
                {
                    XmlNode periodXml = document.CreateElement("periodOfReport");
                    periodXml.InnerText = periodOfReport;
                    fundXml.AppendChild(periodXml);
                }


                if (dateEffectiveness != null)
                {
                    XmlNode dateEffectivenessXml = document.CreateElement("dateEffectiveness");
                    dateEffectivenessXml.InnerText = dateEffectiveness;
                    fundXml.AppendChild(dateEffectivenessXml);
                }



                if (fillingDate != null)
                {
                    XmlNode fillingDateXml = document.CreateElement("fillingDate");
                    fillingDateXml.InnerText = fillingDate;
                    fundXml.AppendChild(fillingDateXml);
                }
            }



            return fundXml;
        }
        public XmlNode createNode(XmlDocument document, XmlNode parentNode, string nodeName, string value = null)
        {
            XmlNode node = document.CreateElement(nodeName);

            if (value != null)
                node.InnerText = value;

            parentNode.AppendChild(node);

            return node;
        }
        public void Print()
        {
            if (ticker != null) Console.WriteLine("Ticker: {0}", ticker);
            if (name != null) Console.WriteLine("Name: {0}", name);
            if (link != null) Console.WriteLine("Link: {0}", link);
            if (source != null) Console.WriteLine("Source: {0}", source);
            if (id != null) Console.WriteLine("Id: {0}", id);

            /* if (periodOfReport.Length > 0)
             {
                 foreach (string period in periodOfReport)
                 {
                     if (period != null)
                     {
                         Console.WriteLine("PeriodOfReport: {0}", period);
                     }

                 }

             }
             if (dateEffectiveness.Length > 0)
             {
                 foreach (string date in dateEffectiveness)
                 {
                     if (date != null)
                     {
                         Console.WriteLine("DateEffectiveness: {0}", date);
                     }
                 }
             }*/


            if (holdings != null)
            {
                foreach (Holding holding in holdings)
                {
                    holding.Print();
                }
            }

            Console.WriteLine("___________");
        }
    }
}
