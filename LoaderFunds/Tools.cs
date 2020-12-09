using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Globalization;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace LoaderFundHolders
{
    public class Tools
    {
        public DataTable CommandExecutor(string SQL)
        {
            SqlConnection cn;
            cn = new SqlConnection();
            cn.ConnectionString = @"Data Source=srv-rxdb-01;Initial Catalog=Rixtrema.401K;User Id=DeveloperUser;Password=Zaq1Xsw2;MultipleActiveResultSets=True;Connection Timeout=199;";
            cn.Open();

            DataSet ds = new DataSet();
            System.Data.SqlClient.SqlCommand comm = new SqlCommand(SQL, cn);
            comm.CommandTimeout = 10800 * 2;

            SqlDataAdapter adapter = new SqlDataAdapter(comm);
            try
            {

                adapter.Fill(ds);

            }
            catch (Exception ex)
            {
            }
            if (ds.Tables.Count == 0) return null;

            return ds.Tables[0];
        }


        //* Валюты
        public string cur = "(pen|lkr|krw|rsd|php|myr|idr|inr|huf|ghs|brl|plc|uyu|myr|nok|aud|zar|sgd|eur|sek|jpy|hkd|cad|gbp|chf|usd|pln|mxn)+";
        //* Maturity регулярное выражение
        public string maturityReg = @"[0-9]+[/-—]+[0-9]+[/-—]+[0-9]+";
        //* Coupon регулярное выражение
        public string couponReg = @"[0-9]+\.[0-9]+[ %]*\b";

        //* В этом классе помойка из вспомогательных функций. В основном загрузки файлов по url и работа со строками и регулярными выражениями
        public Tools()
        {

        }

        public void LoadFileAsync(string url, string path)
        {
            WebClient web = new WebClient();
            Uri uri = new Uri(url);
            web.DownloadFileAsync(uri, path);
        }
        public string LoadFile(string url, string path)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();

            FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter write = new StreamWriter(file);
            StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
            int b;
            for (int i = 0; ; i++)
            {
                b = stream.ReadByte();
                if (b == -1) break;
                write.Write((char)b);
            }
            write.Close();
            file.Close();

            string fileString = streamReader.ReadToEnd();
            streamReader.Close();

            return fileString;
        }
        public long mrngValue(string value)
        {
            string val = value.Trim();
            long result = 0;

            val = Regex.Replace(val, @",", @"", RegexOptions.IgnoreCase);
            Match mm = Regex.Match(val, @"[0-9.]*");
            Match c = Regex.Match(val, @"[a-zA-Z]+");

            double Number;
            if (Double.TryParse(mm.Value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out Number))
            {
                // Console.WriteLine(Number);
                int coef = 1;
                if (c.Value == "mil") coef = 1000000;
                if (c.Value == "bil") coef = 1000000000;
                result = Convert.ToInt64(Number * coef);
            }

            return result;
        }
        public string TrimGarbage(string str)
        {
            return Regex.Replace(str, @"^[^a-zA-Z0-9\(\)]+|[^a-zA-Z0-9\(\)]+$", @"", RegexOptions.IgnoreCase);
        }
        public string ClearSpace(string str)
        {
            str = Regex.Replace(str, @"&[^;]*;", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"[^0-9a-zA-Z]*", @"", RegexOptions.IgnoreCase);

            return str;
        }

        public string ClearSpaceTable(string str)
        {
            str = Regex.Replace(str, @"[^0-9a-zA-Z]+", @"", RegexOptions.IgnoreCase);
            return str;
        }
        public string ClearSpaceCaption(string str)
        {
            str = Regex.Replace(str, @"/[a-zA-Z]+/", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"\([^(]*\)", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"and|inc$|&[^&]+?;", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"[^0-9a-zA-Z]*", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&", @"", RegexOptions.IgnoreCase);

            return str;
        }
        public string ClearFromNumbers(string str)
        {
            str = Regex.Replace(str, @"&[^;]*;", @"", RegexOptions.IgnoreCase);
            return Regex.Replace(str, @"[^a-zA-Z]*", @"", RegexOptions.IgnoreCase);
        }
        public string clear(HtmlNode node)
        {
            if (node == null) return null;
            string t = node.InnerText.Trim();
            t = Regex.Replace(t, @"\n", " ", RegexOptions.IgnoreCase);
            return t;
        }
        public string clear(string node)
        {
            string t = node.Trim();
            t = Regex.Replace(t, @"\n", " ", RegexOptions.IgnoreCase);
            return t;
        }
        public string clear(string str, string rg, out string strcopy, int max = 0)
        {
            bool _if = true;
            string value = null;
            strcopy = str;
            var rvalue = new Regex(rg);
            Match valueM = rvalue.Match(str.ToLower());

            if (max != 0 && valueM.Captures.Count >= max)
            {
                _if = false;
            }

            if (_if == true && valueM.Success)
            {
                value = valueM.Value.Trim();
                strcopy = rvalue.Replace(str.ToLower(), @"").Trim();
            }


            return value;
        }
        public string ClearStrPString(string str)
        {
            return Regex.Replace(str, @"$", @"", RegexOptions.IgnoreCase);
        }
        public string ClearStrNum(string str)
        {
            str = Regex.Replace(str, @"(&[^;]*;|[eur $%])*", @"", RegexOptions.IgnoreCase);
            return str;
        }
        public string ClearStrNameNew(string str)
        {
            str = Regex.Replace(str.Trim(), @"&nbsp;", @" ", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&amp;", @"&", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&mdash;|&ndash;|&#151;|&#8211;|[--]+", @"—", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&[^;]*;", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"\n|  ", @" ", RegexOptions.IgnoreCase);

            str = Regex.Replace(str, @"[^0-9a-zA-Z ,.\/\(\)%$/—]*", @"", RegexOptions.IgnoreCase);

            return str.Trim();
        }
        public string ClearStrName(string str)
        {
            str = Regex.Replace(str, @"\(.*?\)", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&mdash;|&ndash;|&#151;", @"—", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(&[^;]*;|[^0-9a-zA-Z ,.])*", @"", RegexOptions.IgnoreCase);

            return str.Trim();
        }
        public string ClearSpacepBonds(string str, bool bond, string cur)
        {
            if (bond)
            {
                str = Regex.Replace(str, @"\([^\)]*\)|(\(0[^s]+?s\)|&[^;]*;|[^0-9a-zA-Z\u2010\u2013\u2014])*", @"", RegexOptions.IgnoreCase);
                str = Regex.Replace(str, @"|reg s|note|senior|bond", @"", RegexOptions.IgnoreCase);

                return str;
            }
            else
                return ClearSpace(str);
        }
        public string ClearValue(string str)
        {
            str = Regex.Replace(str.Trim(), @"&nbsp;", @" ", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&amp;", @"&", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&mdash;|&ndash;|&#150;|&#151;|&#8211;|[--]+", @"—", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&[^;]*;", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"\n|  ", @" ", RegexOptions.IgnoreCase);

            str = Regex.Replace(str, @"[^0-9a-zA-Z ,.\/\(\)%$/—]*", @"", RegexOptions.IgnoreCase);

            return str;
        }
        public bool isCoupon(string value)
        {

            string cp = couponReg;
            var re = new Regex(cp);

            Match couponCandidat = re.Match(value);

            if (couponCandidat == null || !couponCandidat.Success) return false;

            double couponCandidatN;
            extendTryParse(couponCandidat.Value, out couponCandidatN);

            return couponCandidatN < 10 && ClearSpace(value).Length < 40;
        }
        public bool isMaturityOnly(string value)
        {
            string mat = maturityReg;
            var re = new Regex(mat);
            bool match = re.IsMatch(value);
            clear(value, mat, out value);
            return match && ClearSpace(value).Length < 15;
        }
        public bool findKeyInValueEx(string value)
        {
            var re = new Regex(@"[0-9]+[ %]+|^\([^\)\(]+\)$");
            return re.IsMatch(value.Trim());
        }
        public string clearFromMC(string str)
        {
            string add = "";
            string strcopy = str;

            clear(strcopy, maturityReg, out strcopy, 2);
            clear(strcopy, couponReg, out strcopy, 2);

            add = clear(strcopy, @"series|class|[0-9]+[a-zA-Z]{1}", out strcopy);

            if (strcopy.Trim().Length > 1) return (strcopy + " " + add).Trim();

            return null;
        }
        public bool isMaturityAlso(string value)
        {
            var re = new Regex(@"[0-9]+/[0-9]+/[0-9]+");
            return re.IsMatch(value) && ClearSpace(value).Length < 40;
        }
        public bool isNumberWP(string value)
        {
            var re = new Regex(@"[0-9]+\.[0-9]{0,3}[ %]*$");
            return re.IsMatch(value.Trim());
        }
        public string GetMatchingDdValue(HtmlNode dtNode)
        {
            var found = dtNode.SelectSingleNode("following-sibling::*[1]");
            return found == null ? "" : found.InnerText;
        }
        public bool Number(string numberTry)
        {
            double _Number;
            return extendTryParse(numberTry, out _Number);
        }
        public bool extendTryParse(string numberTry, out double _Number)
        {
            if (numberTry == null)
            {
                _Number = 0;
                return false;
            }
            double Number;
            bool minus = false;

            var re = new Regex(@"\([0-9.]+");
            if (re.IsMatch(numberTry) == true)
            {
                numberTry = StrRemoveAll(numberTry, "(");
                numberTry = StrRemoveAll(numberTry, ")");

                minus = true;
            }
            bool isInt = Double.TryParse(numberTry, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out Number);

            _Number = Number;
            if (minus == true)
            {
                _Number = -_Number;
            }

            return isInt;
        }
        public string currency(string str, out string _out)
        {
            string _str = str;
            string value = clear(str.Trim(), cur, out _str);

            if (Regex.Replace(_str, @"[0-9,]+", @"", RegexOptions.IgnoreCase).Length < 3 && value != "")
            {
                _out = _str.Trim();
                return value;
            }
            else
            {
                _out = str;
                return null;
            }
        }
        public string currency(string str)
        {
            string value = clear(str.Trim(), cur, out str);

            str = Regex.Replace(str, @"[0-9,]+", @"", RegexOptions.IgnoreCase);

            if (str.Length < 3 && value != "")
            {
                return value;
            }
            else
            {
                return null;
            }
        }
        public string StrRemove(string str, string substr)
        {
            int n = str.IndexOf(substr);
            if (n > -1)
            {
                str = str.Remove(n, substr.Length);
            }

            return str;
        }
        public string StrRemoveAll(string str, string substr)
        {
            string tempStr = str;

            do
            {
                tempStr = str;
                str = StrRemove(str, substr);
            }
            while (str != tempStr);

            return str;
        }
        public string LoadFile(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();

            StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));

            string fileString = streamReader.ReadToEnd();
            streamReader.Close();
            return fileString;
        }
        public HtmlNode WaitNode(HtmlNode bodyNode, string xPath)
        {
            HtmlNode ret = null;
            do
            {
                ret = bodyNode.SelectSingleNode(xPath);
            }
            while (ret == null);

            return ret;
        }
        public HtmlNodeCollection WaitNodes(HtmlNode bodyNode, string xPath)
        {
            HtmlNodeCollection ret = null;
            do
            {
                ret = bodyNode.SelectNodes(xPath);
            }
            while (ret == null);

            return ret;
        }
        public HtmlNode BodyNodeS(string html, Func<string, string> htmlMake = null, string xPath = ".//body")
        {
            if (html != null)
            {
                try
                {
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                    htmlDoc.OptionFixNestedTags = true;

                    htmlDoc.OptionAutoCloseOnEnd = false;
                    htmlDoc.OptionCheckSyntax = false;
                    htmlDoc.OptionFixNestedTags = false;

                    var rebody = new Regex(@"</body>");
                    var rehtml = new Regex(@"</html>");

                    var refbody = new Regex(@"<body[^>]*>");
                    var refhtml = new Regex(@"<html[^>]*>");

                    if (rebody.IsMatch(html) != true)
                    {
                        html = html + "\n</body>";
                    }
                    if (rehtml.IsMatch(html) != true)
                    {
                        html = html + "\n</html>";
                    }
                    if (refbody.IsMatch(html) != true)
                    {
                        html = "<body>\n" + html;
                    }
                    if (refhtml.IsMatch(html) != true)
                    {
                        html = "<html>\n" + html;
                    }

                    if (htmlMake != null)
                    {
                        html = htmlMake(html);
                    }


                    htmlDoc.LoadHtml(html);

                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        Console.WriteLine("error html");
                        foreach (HtmlParseError error in htmlDoc.ParseErrors)
                        {
                            Console.WriteLine("{0}, {1}", error.Line, error.Reason);
                        }
                    }
                    else
                    {
                        if (htmlDoc.DocumentNode != null)
                        {
                            HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode(xPath);
                            if (bodyNode != null)
                            {
                                return bodyNode;
                            }
                        }
                    }
                }
                catch
                {
                    html = null;
                    //Console.WriteLine("Сильно большой файл html");
                }

            }
            else
            {
                //Console.WriteLine("html==null");
            }

            return null;
        }
        public HtmlNode BodyNodeJSON(string json, Func<string, string> htmlMake = null, string xPath = ".//body")
        {

            try
            {
                dynamic js = JsonConvert.DeserializeObject(json);
                if (js.html != null)
                {
                    string html = js.html;
                    return BodyNode(html, htmlMake, xPath);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }
        public HtmlNode BodyNode(string html, Func<string, string> htmlMake = null, string xPath = ".//body")
        {
            if (html != null)
            {
                try
                {
                    HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

                    if (htmlMake != null)
                    {
                        html = htmlMake(html);
                    }
                    html = Regex.Replace(html, @"<!--.*?-->", @"", RegexOptions.IgnoreCase);

                    var rebody = new Regex(@"</body>");
                    var rehtml = new Regex(@"</html>");

                    var refbody = new Regex(@"<body>");
                    var refhtml = new Regex(@"<html>");

                    if (rebody.IsMatch(html) != true)
                    {
                        html = html + "\n</body>";
                    }
                    if (rehtml.IsMatch(html) != true)
                    {
                        html = html + "\n</html>";
                    }
                    if (refbody.IsMatch(html) != true)
                    {
                        html = "<body>\n" + html;
                    }
                    if (refhtml.IsMatch(html) != true)
                    {
                        html = "<html>\n" + html;
                    }

                    htmlDoc.OptionAutoCloseOnEnd = false;
                    htmlDoc.OptionCheckSyntax = false;
                    htmlDoc.OptionFixNestedTags = false;

                    HtmlNode.ElementsFlags.Remove("option");
                    HtmlNode.ElementsFlags.Remove("img");
                    HtmlNode.ElementsFlags.Remove("link");

                    try
                    {
                        htmlDoc.LoadHtml(html);
                    }
                    catch (Exception e)
                    {
                        html = null;
                        return null;
                    }


                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        foreach (HtmlParseError error in htmlDoc.ParseErrors)
                        {
                            Console.WriteLine(error.SourceText);
                            Console.WriteLine(error.Reason);
                            Console.WriteLine(error.Line);
                        }
                    }
                    else
                    {
                        if (htmlDoc.DocumentNode != null)
                        {
                            HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode(xPath);
                            if (bodyNode != null)
                            {
                                return bodyNode;
                            }
                        }

                    }
                }
                catch
                {
                    html = null;
                }

            }
            else
            {
                //Console.WriteLine("html==null");
            }

            return null;
        }
        public bool NodeInNode(HtmlNode nodeParent, HtmlNode nodeChild)
        {
            while (nodeChild.Name != "body" && nodeChild.ParentNode != null)
            {
                nodeChild = nodeChild.ParentNode;

                if (nodeChild.Line == nodeParent.Line && nodeChild.Name == nodeParent.Name) return true;
            };

            return false;
        }
        public int lettersBetweenTags(HtmlNode nodeOne, HtmlNode nodeTwo)
        {
            int letters = 0;

            HtmlNode next = nodeOne.NextSibling;

            while (next != null && next.Line < nodeTwo.Line && NodeInNode(next, nodeTwo))
            {

                if (next.Name != "#text")
                {
                    letters = letters + ClearSpaceTable(next.InnerText).Length;
                    //Console.WriteLine("LETTERS {0} - {1}, {2}, {3}", letters, next.Name, next.Line, nodeTwo.Line);
                }
                next = next.NextSibling;

            }

            return letters;
        }
        public void MakeXml(string _path, out XmlDocument document, out string xmlPath, string head = "head")
        {
            string path = Directory.GetCurrentDirectory();
            string fileName = String.Format(_path);
            xmlPath = System.IO.Path.Combine(path, fileName);

            System.IO.File.Delete(xmlPath);
            Console.WriteLine(xmlPath);

            XmlTextWriter textWritter = new XmlTextWriter(xmlPath, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement(head);
            textWritter.WriteEndElement();
            textWritter.Close();

            document = new XmlDocument();
            document.Load(xmlPath);
        }
        public void MakeXmlNode(XmlDocument document, XmlNode parent, string name, string value)
        {
            if (value != null)
            {
                XmlNode nodeXml = document.CreateElement(name);
                nodeXml.InnerText = value;
                parent.AppendChild(nodeXml);
            }
        }
        public string LoadWeb(string url)
        {
            using (var myWebClient = new WebClient())
            {
                myWebClient.Headers["User-Agent"] = "MOZILLA/5.0 (WINDOWS NT 6.1; WOW64) APPLEWEBKIT/537.1 (KHTML, LIKE GECKO) CHROME/21.0.1180.75 SAFARI/537.1";

                string page = myWebClient.DownloadString(url);

                return page;
            }

        }
        public string date()
        {
            string curDate = DateTime.Now.ToShortDateString();
            string[] date = curDate.Split(new char[] { '.' });

            curDate = date[2] + "-" + date[1] + "-" + date[0];

            return curDate;
        }
        public void openFile(string _path, Fund fund = null, Func<string, Fund, string> lineMake = null)
        {
            string path = Directory.GetCurrentDirectory();
            string fileName = String.Format(_path);

            int counter = 0;
            string line;

            System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.Combine(path, fileName));
            while ((line = file.ReadLine()) != null)
            {
                if (lineMake != null)
                {
                    lineMake(line, fund);
                }
                counter++;
            }

            file.Close();
            System.Console.ReadLine();
        }
        public string openFile(string _path)
        {
            string path = Directory.GetCurrentDirectory();
            string fileName = String.Format(_path);

            int counter = 0;
            string line;
            string rez = "";

            System.IO.StreamReader file = new System.IO.StreamReader(System.IO.Path.Combine(path, fileName));

            rez = file.ReadToEnd();
            file.Close();

            return rez;
        }
        public string openFileD(string _path)
        {
            string fileName = String.Format(_path);

            int counter = 0;
            string line;
            string rez = "";

            System.IO.StreamReader file = new System.IO.StreamReader(fileName);

            rez = file.ReadToEnd();
            file.Close();

            return rez;
        }
        public string Load(string url, bool success = false)
        {
            string _html = null;
            string html = null;
            if (url != null)
            {
                do
                {
                    try
                    {
                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                        WebResponse webResponse = httpWebRequest.GetResponse();
                        Stream responseStream = webResponse.GetResponseStream();

                        StreamReader stream = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));

                        _html = stream.ReadToEnd();
                        stream.Close();
                        if (_html != null)
                        {
                            html = _html;
                            success = true;
                        }

                    }
                    catch (IOException e)
                    {
                        return null;
                    }
                    catch (OutOfMemoryException e)
                    {
                        return null;
                    }
                    catch (WebException e)
                    {
                        return null;
                    }
                }
                while (success == false);
            }

            return html;
        }
        public int tdsCount(HtmlNodeCollection tds)
        {
            if (tds == null) return 0;

            int c = tds.Count;

            for (int i = c - 1; i >= 0; i--)
            {
                HtmlNode td = tds[i];
                if (ClearSpace(td.InnerText.Trim()) != "") return i + 1;
            }

            return c;
        }
        public string htmlDelete(string html, int startLine, int EndLine)
        {

            string[] _html = html.Split('\n');
            string newHtml = "";
            int i = 1;
            foreach (string h in _html)
            {
                if (i > startLine && i < EndLine) ;
                else
                {
                    newHtml = newHtml + '\n' + h;

                }
                i++;
            }

            return newHtml;
        }
        public string htmlTake(string html, int startLine, int EndLine)
        {

            string[] _html = html.Split('\n');
            string newHtml = "<html><body>";
            int i = 1;

            foreach (string h in _html)
            {
                if (i >= startLine && i <= EndLine)
                {
                    newHtml = newHtml + '\n' + h;
                }

                i++;
            }

            return newHtml + "</body></html>";
        }
        public string htmlDeleteE(string html, int startLine, int EndLine)
        {

            string[] _html = html.Split('\n');
            string newHtml = "";
            int j = 1;
            int l = _html.Length;
            for (int i = l - 1; i > 0; i--)
            {
                string h = _html[i];
                if (j > startLine && j < EndLine) ;
                else
                {
                    newHtml = newHtml + '\n' + h;
                }
                j++;
            }

            return newHtml;
        }
        public List<Fund> GetFunds(string _path)
        {
            List<Fund> funds = new List<Fund>();
            string path = Directory.GetCurrentDirectory();
            string fileName = _path;
            path = System.IO.Path.Combine(path, fileName);

            using (XmlReader xml = XmlReader.Create(path))
            {
                while (xml.Read())
                {
                    switch (xml.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xml.Name == "funds")
                            {
                                Fund fund = new Fund(xml.ReadOuterXml());
                                funds.Add(fund);
                            }
                            break;
                    }
                }
            }
            return funds;
        }
        public string GetXML(string source, string nodeName, Action<string> Each = null)
        {
            using (XmlReader xml = XmlReader.Create(source))
            {
                while (xml.Read())
                {
                    switch (xml.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xml.Name == nodeName)
                            {
                                if (Each != null)
                                {
                                    Each(xml.ReadOuterXml());
                                }
                            }
                            break;
                    }
                }
            }
            return null;
        }
        public string GetXMLNode(string source, string nodeName)
        {
            try
            {
                using (XmlReader xml = XmlReader.Create(source))
                {
                    while (xml.Read())
                    {
                        if (xml.NodeType == XmlNodeType.Element && xml.Name == nodeName)
                        {
                            return xml.ReadOuterXml();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
        public int monthNumber(string mm)
        {
            mm = mm.Substring(0, 3).ToLower();
            String[] mms = new String[] { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sem", "oct", "nov", "dec" };


            return Array.IndexOf(mms, mm) + 1;
        }
        public void Union(Dictionary<string, Holding> d1, Dictionary<string, Holding> d2)
        {
            foreach (KeyValuePair<string, Holding> kvp in d2)
            {
                if (!d1.ContainsKey(kvp.Key))
                {
                    d1.Add(kvp.Key, kvp.Value);
                }
            }
        }
        public void transformToList(Fund fund, Dictionary<string, Holding> holdingList)
        {
            if (holdingList.Count > 0)
            {
                foreach (KeyValuePair<string, Holding> h in holdingList)
                {
                    fund.holdings.Add(h.Value);
                }
                holdingList.Clear();
            }
        }
        public List<Fund> getFundsFromServer()
        {
            List<Fund> funds = new List<Fund>();
            DateTime now = DateTime.Now;
            string postData = "Action=GETALLFUNDS&Login=maximgrishkov%40yandex.ru&Password=3333";

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://rixtrema.net/RixtremaWS401k/AJAXFCT.aspx");

            wr.Timeout = int.MaxValue;
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";
            wr.ContentLength = postData.Length;

            StreamWriter writer = new StreamWriter(wr.GetRequestStream());
            try
            {
                writer.Write(postData);
            }
            catch (Exception ex) { throw (ex); }
            finally { writer.Close(); }


            HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string respText = reader.ReadToEnd();

            double seconds = (DateTime.Now - now).TotalSeconds;
            Console.WriteLine(seconds);

            if (!respText.ToLower().Contains("success")) throw new Exception(respText);
            else
            {

                try
                {
                    dynamic js = JsonConvert.DeserializeObject(respText);
                    if (js.FCT == null) return null;
                    if (js.FCT.fund == null) return null;
                    int i = 0;
                    foreach (dynamic jsfund in js.FCT.fund)
                    {
                        i++;
                        if (jsfund.Ticker != null && jsfund.Name != null)
                        {
                            Fund fund = new Fund();
                            fund.ticker = jsfund.Ticker;
                            fund.name = jsfund.Name;
                            fund.fname = jsfund.FamilyName;

                            funds.Add(fund);
                        }
                    }
                    Console.WriteLine(i);

                    return funds;
                }
                catch
                {
                    return null;
                }
            }
        }
        public void createSource(string path, List<Fund> funds)
        {
            XmlDocument document;

            this.MakeXml(path, out document, out path);
            XmlNode fundsXml = document.CreateElement("funds-list");
            document.DocumentElement.AppendChild(fundsXml);

            foreach (Fund fund in funds)
            {
                XmlNode fundXml = createNode(document, fundsXml, "funds");

                createNode(document, fundXml, "FundTicker", fund.ticker);
            }

            document.Save(path);
        }
        public XmlNode createNode(XmlDocument document, XmlNode parentNode, string nodeName, string value = null)
        {
            XmlNode node = document.CreateElement(nodeName);

            if (value != null)
                node.InnerText = value;

            parentNode.AppendChild(node);

            return node;
        }
        public void makeFile(string data, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(data);
                }
            }
        }

        public string request(string postData)
        {
            DateTime now = DateTime.Now;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://rixtrema.net/RixtremaWS401k/AJAXFCT.aspx");

            //Console.WriteLine(postData);

            wr.Timeout = int.MaxValue;
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";
            wr.ContentLength = postData.Length;

            StreamWriter writer = new StreamWriter(wr.GetRequestStream());
            try
            {
                writer.Write(postData);
            }
            catch (Exception ex) { throw (ex); }
            finally { writer.Close(); }


            HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string respText = reader.ReadToEnd();



            double seconds = (DateTime.Now - now).TotalSeconds;

            return respText;
        }
        public string saveLogs(object data, string alias)
        {
            string postData = "action=CREATEEVENT&Login=maximgrishkov@yandex.ru&Password=3333&actor=maximgrishkov@yandex.ru&Milestone=" + alias + "&qaData=" + Newtonsoft.Json.JsonConvert.SerializeObject(data);

            return this.request(postData);
        }

        public void uploadFile(string eid, string filename, string filePath)
        {
            string uri = "https://rixtrema.net/RixtremaWS401k/AJAXFCT.aspx?Action=UPLOADQAFILE&EventID=" + eid + "&FileName=" + filename + "&Login=astarodubtsev%40rixtrema.com&Password=alex";
            using (WebClient client = new WebClient())
            {
                client.UploadFile(uri, filePath);
            }
        }
    }
}


