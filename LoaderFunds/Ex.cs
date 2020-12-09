using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace LoaderFundHolders
{
    public class Ex
    {
        public HashSet<string> exes;
        public HashSet<string> countries;
        public HashSet<string> industries;
        public List<string> types;

        public HashSet<string> exesGood;
        public HashSet<string> garbage;
        public HashSet<string> exesPure;
        public HashSet<string> exesAppendix;

        public Ex()
        {
            exes = new HashSet<string>();
            string pathe = Directory.GetCurrentDirectory();
            string fileNamee = "Source/Edgar/ex.xml";
            pathe = Path.Combine(pathe, fileNamee);
            GetXML(pathe, exes, "ex");

            countries = new HashSet<string>();
            string pathc = Directory.GetCurrentDirectory();
            string fileNamec = "Source/Edgar/countries.xml";
            pathc = Path.Combine(pathc, fileNamec);
            GetXML(pathc, countries, "country");

            industries = new HashSet<string>();
            string pathi = Directory.GetCurrentDirectory();
            string fileNamei = "Source/Edgar/industry.xml";
            pathi = Path.Combine(pathi, fileNamei);
            GetXML(pathi, industries, "industry");

            exesGood = new HashSet<string>();
            string patheg = Directory.GetCurrentDirectory();
            string fileNameeg = "Source/Edgar/eg.xml";
            patheg = Path.Combine(patheg, fileNameeg);
            GetXML(patheg, exesGood, "eg");

            garbage = new HashSet<string>();
            string pathegr = Directory.GetCurrentDirectory();
            string fileNameegr = "Source/Edgar/garbage.xml";
            pathegr = Path.Combine(pathegr, fileNameegr);
            GetXML(pathegr, garbage, "eg");

            exesPure = new HashSet<string>();
            string pathegpe = Directory.GetCurrentDirectory();
            string fileNameep = "Source/Edgar/exesPure.xml";
            pathegpe = Path.Combine(pathegpe, fileNameep);
            GetXML(pathegpe, exesPure, "ex");

            exesAppendix = new HashSet<string>();
            string pathegap = Directory.GetCurrentDirectory();
            string fileNameap = "Source/Edgar/exesAppendix.xml";
            pathegap = Path.Combine(pathegap, fileNameap);
            GetXML(pathegap, exesAppendix, "ex");

            types = new List<string>();
            string pathegtypes = Directory.GetCurrentDirectory();
            string fileNametypes = "Source/Edgar/types.xml";
            pathegtypes = Path.Combine(pathegtypes, fileNametypes);
            GetXML(pathegtypes, types, "type");

        }
        public string clear(string str)
        {
            return (new Regex(@"&[^;]*;|[ ,.]*", RegexOptions.None)).Replace(str.Trim().ToLower(), @"");
        }
        public bool pure(string str)
        {
            bool exin = false;
            string hstr = clear(str);
            foreach (string ex in exesPure)
            {
                if (hstr.Contains(ex))
                {
                    return true;
                }
            }
            return exin;
        }
        public bool garbagePure(string str)
        {
            bool exin = false;
            string hstr = clear(str);
            foreach (string ex in garbage)
            {
                if (hstr.Contains(ex))
                {
                    return true;
                }
            }
            return exin;
        }
        public void GetXML(string source, HashSet<string> hashset, string nodename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(source);
            foreach (XmlNode node in doc.SelectNodes(".//" + nodename))
            {
                string nodetext = clear(node.InnerText);
                if (!hashset.Contains(node.InnerText))
                {
                    hashset.Add(clear(nodetext));
                }
            }
        }
        public void GetXML(string source, List<string> list, string nodename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(source);
            foreach (XmlNode node in doc.SelectNodes(".//" + nodename))
            {
                string nodetext = clear(node.InnerText);
                if (!list.Contains(node.InnerText))
                {
                    list.Add(clear(nodetext));
                }
            }
        }
    }
}
