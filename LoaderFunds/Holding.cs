using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;

namespace LoaderFundHolders
{
    public class Holding
    {
        public string name { get; set; }
        public string tname { get; set; }
        public string shares { get; set; }
        public string value { get; set; }
        public double _shares { get; set; }
        public double _value { get; set; }
        public string identifier { get; set; }
        public string ticker { get; set; }
        public string tna { get; set; }
        public string couponRate { get; set; }
        public string maturityDate { get; set; }
        public string percentOfNetAssets { get; set; }
        public string faceAmount { get; set; }
        public string principalAmount { get; set; }
        public string parvalue { get; set; }
        public string currency { get; set; }
        public string contracts { get; set; }
        public string country { get; set; }
        public string industry { get; set; }
        public string notionalamount { get; set; }
        public string type { get; set; }

        public Tools tools = new Tools();

        public Holding()
        {

        }
        public Holding(string _name, string _percentOfNetAssets)
        {
            name = _name;
            percentOfNetAssets = _percentOfNetAssets;
        }


        public Holding(string _name, string _shares, string _value)
        {
            name = _name;
            shares = _shares;
            value = _value;
        }
        public Holding(string _name, double __shares, double __value)
        {
            name = _name;
            _shares = __shares;
            _value = __value;
            shares = _shares.ToString();
            value = _value.ToString();
        }

        public bool check()
        {
            if (name != null && (_shares != 0 || faceAmount != null || principalAmount != null || parvalue != null || notionalamount != null || contracts != null) && _value != 0) return true;

            return false;
        }
        public string key()
        {
            string key = "";
            if (name != null) key = key + name;
            if (country != null) key = key + country;
            if (currency != null) key = key + currency;
            if (couponRate != null) key = key + couponRate;
            if (maturityDate != null) key = key + maturityDate;

            return key;
        }

        public Holding(string _name, string _shares, string _value, string _identifier, string _tna, string _couponRate, string _maturityDate)
        {
            name = _name;
            shares = _shares;
            value = _value;
            identifier = _identifier;
            tna = _tna;
            couponRate = _couponRate;
            maturityDate = _maturityDate;
        }
        public bool isNotBond(string val)
        {
            return val.ToLower().Contains("repurchase") == true;
        }
        public void transformbund()
        {
            //if (maturityDate != null || couponRate != null) return;

            string _name = name;

            if (!isNotBond(_name))
            {
                if (maturityDate == null)
                {
                    maturityDate = tools.clear(_name, "[0-9]+/[0-9]+/[0-9]+[^A-Za-z0-9]+[0-9]+/[0-9]+/[0-9]+", out _name, 2);

                    if (maturityDate == null)
                    {
                        maturityDate = tools.clear(_name, "[0-9]+/[0-9]+/[0-9]+", out _name, 2);
                    }
                }

                if (couponRate == null)
                {
                    couponRate = tools.clear(_name, @"[0-9]+\.[0-9]+[%]*[^A-Za-z0-9]+[0-9]+\.[0-9]+[%]*\b", out _name, 2);

                    if (couponRate == null)
                    {
                        couponRate = tools.clear(_name, @"[0-9]+\.[0-9]+[%]*\b", out _name, 2);
                    }

                    if (couponRate != null && maturityDate == null)
                    {
                        maturityDate = tools.clear(_name, "[0-9—]+$", out _name);
                    }
                }

                _name = Regex.Replace(_name, @"due|callable", @"", RegexOptions.IgnoreCase);
                _name = Regex.Replace(_name, @",|[^A-Za-z0-9./)]+$", @"", RegexOptions.IgnoreCase);

                if (couponRate == null || maturityDate == null)
                {
                    _name = name;
                }
            }

            _name = Regex.Replace(_name, @" {2,}", @" ", RegexOptions.IgnoreCase);
            name = _name.Trim();
        }
        public void Print()
        {
            Console.WriteLine("___holding___");
            if (identifier != null)
            {
                Console.WriteLine("Identifier, {0}", identifier);
            }

            if (ticker != null)
            {
                Console.WriteLine("Ticker, {0}", ticker);
            }

            if (name != null)
            {
                Console.WriteLine("Name, {0}", name);
            }

            if (shares != null)
            {
                Console.WriteLine("Shares, {0}", shares);
            }

            if (_shares != 0)
            {
                Console.WriteLine("_Shares, {0}", _shares);
            }

            if (value != null)
            {
                Console.WriteLine("Value, {0}", value);
            }

            if (_value != 0)
            {
                Console.WriteLine("_Value, {0}", _value);
            }


            if (tna != null)
            {
                Console.WriteLine("TNA, {0}", tna);
            }

            if (couponRate != null)
            {
                Console.WriteLine("CouponRate, {0}", couponRate);
            }

            if (maturityDate != null)
            {
                Console.WriteLine("MaturityDate, {0}", maturityDate);
            }

            if (percentOfNetAssets != null)
            {
                Console.WriteLine("PercentOfNetAssets, {0}", percentOfNetAssets);
            }

            if (faceAmount != null)
            {
                Console.WriteLine("FaceAmount, {0}", faceAmount);
            }

            if (industry != null)
            {
                Console.WriteLine("Industry, {0}", industry);
            }

            if (currency != null)
            {
                Console.WriteLine("Currency, {0}", currency);
            }

            if (country != null)
            {
                Console.WriteLine("Country, {0}", country);
            }
            Console.WriteLine("___endholding___");

        }
        public void prepareToSave()
        {
            if (_shares != 0)
            {
                shares = _shares.ToString();
            }
            if (_value != 0)
            {
                value = _value.ToString();
            }

            if (shares == null)
            {
            }

            if (value == null)
            {
                if (percentOfNetAssets != null)
                {
                    value = Regex.Replace(percentOfNetAssets, @"%", @"", RegexOptions.IgnoreCase);
                }
            }
        }
        public void AddToXML(XmlDocument document, XmlNode holdingsXml)
        {
            prepareToSave();
            XmlNode holdingXml = document.CreateElement("holding");
            holdingsXml.AppendChild(holdingXml);

            if (identifier != null)
            {
                XmlNode identifierXml = document.CreateElement("identifier");
                identifierXml.InnerText = identifier;
                holdingXml.AppendChild(identifierXml);
            }

            if (ticker != null)
            {
                XmlNode tickerXml = document.CreateElement("ticker");
                tickerXml.InnerText = ticker;
                holdingXml.AppendChild(tickerXml);
            }

            if (name != null)
            {
                XmlNode nameXml = document.CreateElement("securityName");
                nameXml.InnerText = name;
                holdingXml.AppendChild(nameXml);
            }

            if (type != null)
            {
                XmlNode typeXml = document.CreateElement("type");
                typeXml.InnerText = type;
                holdingXml.AppendChild(typeXml);
            }

            if (faceAmount != null)
            {
                XmlNode faceAmountXml = document.CreateElement("faceAmount");
                faceAmountXml.InnerText = faceAmount;
                holdingXml.AppendChild(faceAmountXml);
            }

            if (parvalue != null)
            {
                XmlNode parvalueXml = document.CreateElement("parvalue");
                parvalueXml.InnerText = parvalue;
                holdingXml.AppendChild(parvalueXml);
            }

            if (principalAmount != null)
            {
                XmlNode principalAmountXml = document.CreateElement("principalAmount");
                principalAmountXml.InnerText = principalAmount;
                holdingXml.AppendChild(principalAmountXml);
            }

            if (notionalamount != null)
            {
                XmlNode notionalamountXml = document.CreateElement("notionalAmount");
                notionalamountXml.InnerText = notionalamount;
                holdingXml.AppendChild(notionalamountXml);
            }

            if (contracts != null)
            {
                XmlNode contractsXml = document.CreateElement("contracts");
                contractsXml.InnerText = contracts;
                holdingXml.AppendChild(contractsXml);
            }

            if (shares != null)
            {
                XmlNode sharesXml = document.CreateElement("shares");
                sharesXml.InnerText = shares;
                holdingXml.AppendChild(sharesXml);
            }
            else
            {
                if (_shares != 0)
                {
                    XmlNode sharesXml = document.CreateElement("shares");
                    sharesXml.InnerText = _shares.ToString();
                    holdingXml.AppendChild(sharesXml);
                }
            }

            if (value != null)
            {
                XmlNode valueXml = document.CreateElement("value");
                valueXml.InnerText = value;
                holdingXml.AppendChild(valueXml);
            }
            else
            {
                XmlNode valueXml = document.CreateElement("value");
                valueXml.InnerText = _value.ToString();
                holdingXml.AppendChild(valueXml);
            }

            if (tna != null)
            {
                XmlNode tnaXml = document.CreateElement("tna");
                tnaXml.InnerText = tna;
                holdingXml.AppendChild(tnaXml);
            }

            if (couponRate != null)
            {
                XmlNode couponRateXml = document.CreateElement("couponRate");
                couponRateXml.InnerText = couponRate;
                holdingXml.AppendChild(couponRateXml);
            }

            if (maturityDate != null)
            {
                XmlNode maturityDateXml = document.CreateElement("maturityDate");
                maturityDateXml.InnerText = maturityDate;
                holdingXml.AppendChild(maturityDateXml);
            }

            if (percentOfNetAssets != null)
            {
                XmlNode percentOfNetAssetsXml = document.CreateElement("percentOfNetAssets");
                percentOfNetAssetsXml.InnerText = percentOfNetAssets;
                holdingXml.AppendChild(percentOfNetAssetsXml);
            }

            if (currency != null)
            {
                XmlNode currencyXml = document.CreateElement("currency");
                currencyXml.InnerText = currency;
                holdingXml.AppendChild(currencyXml);
            }

            if (country != null)
            {
                XmlNode countryXml = document.CreateElement("country");
                countryXml.InnerText = country;
                holdingXml.AppendChild(countryXml);
            }

            if (industry != null)
            {
                XmlNode industryXml = document.CreateElement("industry");
                industryXml.InnerText = industry;
                holdingXml.AppendChild(industryXml);
            }

        }
    }
}
