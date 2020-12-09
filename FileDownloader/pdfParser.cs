using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PlansParser.FileDownloader
{
    public class pdfParser
    {
        protected string file;
        protected List<string> pdf;
        protected string path = "ParsedPlans";
        protected char tab = '\t';
        public List<Position> positions { get; set; }

        public void parse()
        {
            List<pdfPage> pages = new List<pdfPage>();
            pdfPage pdfpage;

            for (int i = 0; i < pdf.Count; i++)
            {
                string page = pdf[i]; Console.WriteLine(page);

                if (i == 0)
                {

                    pdfpage = new pdfPage(page);
                }
                else
                {
                    pdfpage = new pdfPage(page, pages[i - 1]);
                }

                pages.Add(pdfpage);
                bool end = pdfpage.parse(positions);

                if (pdfpage.positions != null && pdfpage.positions.Count > 0)
                {
                    positions.AddRange(pdfpage.positions);
                }

                if (end) break;

            }

            clearPositions();

            if (this.positions.Count > 0) save();
        }
        public void print()
        {
            foreach (Position position in positions)
            {
                if (position.valid) position.print();
            }
        }
        public void save()
        {
            StreamWriter SW = new StreamWriter(new FileStream(this.path + ".csv", FileMode.Create, FileAccess.Write));

            foreach (Position position in positions)
            {
                SW.Write(position.addToCsv());
            }

            SW.Close();
        }
        public void clearPositions()
        {
            int positionsWithCost = 0;

            foreach (Position position in positions)
            {
                if (position.cost != 0) positionsWithCost++;

                if (position.identity != null) position.identity = Regex.Replace(position.identity, @",", @", ", RegexOptions.IgnoreCase);
                if (position.description != null) position.description = Regex.Replace(position.description, @",", @", ", RegexOptions.IgnoreCase);
            }

            if (positionsWithCost < positions.Count / 10)
            {
                foreach (Position position in positions)
                {
                    if (position.cost != 0) position.valid = false;
                }
            }
        }
        public List<string> xlsToText(string path)
        {
            List<string> _pdf = new List<string>();

            Console.WriteLine("PATH: {0}", path);

            string ConnectionString = String.Format(
                  "Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties=\"Excel 8.0;HDR=No\";Data Source={0}", path);

            DataSet ds = new DataSet("EXCEL");
            OleDbConnection cn = new OleDbConnection(ConnectionString);
            cn.Open();

            // Получаем списко листов в файле
            DataTable schemaTable =
                cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                        new object[] { null, null, null, "TABLE" });

            foreach (DataRow list in schemaTable.Rows)
            {
                string pdfList = "";

                string sheet = (string)list.ItemArray[2];
                // Выбираем все данные с листа
                string select = String.Format("SELECT * FROM [{0}]", sheet);
                OleDbDataAdapter ad = new OleDbDataAdapter(select, cn);
                ad.Fill(ds);
                DataTable tb = ds.Tables[0];

                // Показать данные с листа
                foreach (DataRow row in tb.Rows)
                {
                    string str = "";
                    int i = 1;
                    foreach (object col in row.ItemArray)
                    {
                        str = col.ToString();

                        if (i == row.ItemArray.Count())
                            str = str + "\n";
                        else
                            str = str + tab;

                        i++;
                    }

                    pdfList = pdfList + str;
                }

                _pdf.Add(pdfList);
            }

            return _pdf;
        }

        public List<string> txtToText(string uri)
        {
            string[] lines = System.IO.File.ReadAllLines(uri, Encoding.Default);

            List<string> _pdf = new List<string>();
            string currentList = "";

            foreach (string line in lines)
            {
                if (line == "\f")
                {
                    _pdf.Add(currentList);
                    currentList = "";
                }
                else
                    currentList = currentList + line + '\n';
            }

            return _pdf;
        }
    }
    public class pdfPage
    {
        protected string[] strings;
        protected bool refreshed = false;

        protected static string nreg = @"[\.,0-9]+";

        public bool table = false;
        public bool caption = false;
        public int mparam = 2;
        public List<Position> positions { get; set; }
        public List<Key> keys { get; set; }
        public List<Key> findedThereKeys { get; set; }

        public Key[] iniKeys = { new Key("description"), new Key("identity"), new Key("cost", true, @"^" + nreg + "$"), new Key("value", true, "^" + nreg + "$") };
        public string[] formKeyWords = { "shedule", "ofassets", "4i" };
        public string lastKey;
        public pdfPage(string page)
        {
            this.strings = page.Split('\n');
            positions = new List<Position>();
            keys = new List<Key>();
            findedThereKeys = new List<Key>();
        }

        public pdfPage(string page, pdfPage pdfpage)
            : this(page)
        {
            if (pdfpage.table)
            {
                //this.keys = pdfpage.keys;
                this.table = pdfpage.table;
                this.caption = pdfpage.caption;
            }
        }

        public bool parse(List<Position> allPositions)
        {
            bool end = false;

            if (table || tableInPage())
            {
                Position position = new Position();

                foreach (string str in this.strings)
                {
                    if (clearSpace(str) != "" && !ignoreString(str))
                    {
                        string value = clearValue(str);
                        if (table == true || tableInPage())
                        {
                            Console.WriteLine("VALUE {0}", value);
                            orderKeys();
                            //printKeys();

                            if (!FindKeysInString(str) && !checkABCDE(value))
                            {
                                Console.WriteLine("VALUE2 {0}", value);
                                if (maybeNew(value, this.keys, position))
                                {

                                    //position.print();
                                    position.migration();

                                    if (position.check())
                                    {
                                        position = savePosition(position);
                                        lastKey = null;
                                    }
                                    else
                                    {
                                        position.clear();
                                    }


                                }

                                List<string> numbers = gentleNumbersRemove(value, out value);

                                // Console.WriteLine("VALUEWITHOUTNUMBERS {0}", value);

                                if (clearSpace(value).Length > 4)
                                {
                                    //Console.WriteLine("VALUETOISTRING {0} {1}", value, position.ineed(keys));
                                    if (position.ineed(keys) == "identity")
                                    {
                                        position.identity = value;
                                        lastKey = "identity";
                                    }
                                    else if (position.ineed(keys) == "description")
                                    {
                                        position.description = value;
                                        lastKey = "description";
                                    }
                                    else
                                    {
                                        //Console.WriteLine("NONEED");
                                    }
                                }

                                if (numbers.Count <= 3 && numbers.Count > 0)
                                {
                                    foreach (string _n in numbers)
                                    {
                                        //Console.WriteLine("NUMBER {0}" , _n);
                                        double number;

                                        var reYear = new Regex("20[0-9]{2}");

                                        if (reYear.IsMatch(_n))
                                        {
                                            value = value + " " + _n;
                                        }
                                        else
                                        {
                                            if (extendTryParse(_n, out number))
                                            {

                                                string need = position.ineed(keys, true);

                                                if (checkTotal(number, allPositions, value, position))
                                                {
                                                    end = true;
                                                    break;
                                                }

                                                if (positions.Count > 0 && positions.Last() != null && positions.Last().value / number > 1000 && number < 100)
                                                {
                                                    break;
                                                }

                                                if (need != null)
                                                {
                                                    setNumber(position, number, need);
                                                    lastKey = need;
                                                }

                                            }
                                        }
                                    }
                                }

                                if (!end && position.check())
                                {
                                    position = savePosition(position);
                                    lastKey = null;
                                }

                                if (end) break;
                            }

                            else
                            {
                                position.clear();
                            }


                            if (majorKeys())
                                table = true;

                            if (majorKeys(findedThereKeys) && !refreshed)
                            {
                                refreshed = true;
                                if (positions.Count < 5)
                                {
                                    positions.Clear();
                                    position = new Position();
                                }
                            }
                        }
                    }
                }

                //?
                position.migration();
                if (position.check())
                {
                    position = savePosition(position);
                }
            }

            return end;
        }
        //position
        //
        public Position savePosition(Position position)
        {
            Position copyPosition = new Position(position);
            positions.Add(copyPosition);
            return new Position();
        }
        public void printPositions()
        {
            foreach (Position position in positions)
            {
                position.print();
            }
        }
        public bool setNumber(Position position, double number, string destination)
        {
            if (position[destination] == null || (position[destination].ToString() == null || position[destination].ToString() == "0" || position[destination].ToString() == "") && indexKey(destination) > -1)
            {
                position[destination] = number;

                return true;
            }
            return false;
        }
        public bool FindKeysInString(string str)
        {
            bool find = false;
            foreach (Key ikey in iniKeys)
            {
                if (str.ToLower().Contains(ikey.value))
                {
                    find = true;
                    addKey(ikey);
                    addKey(ikey, findedThereKeys);
                }
            }

            return find;
        }
        public bool tableInPage()
        {
            foreach (string str in this.strings)
            {
                string strcompressed = clearSpaceCaption(str);
                foreach (string keyWord in formKeyWords)
                {
                    if (strcompressed.ToLower().Contains(keyWord)) caption = true;
                }
            }

            return caption;
        }
        public bool checkTotal(double number, List<Position> allPositions, string activeString, Position currentPosition)
        {
            if (allPositions.Count + positions.Count > 3 && (
                        currentPosition.findStr("total")
                    || activeString.ToLower().Contains("total")
                    || currentPosition.findStr("assets")
                    || activeString.ToLower().Contains("assets")
                    )
                )
            {
                double sum = 0;
                foreach (Position position in allPositions)
                {
                    sum = sum + position.value;
                }

                foreach (Position position in positions)
                {
                    sum = sum + position.value;
                }

                if (sum * .8 < number && number < sum * 1.2)
                {
                    return true;
                }
            }

            return false;
        }

        //keys
        //
        public void orderKeys(List<Key> _keys = null)
        {
            if (_keys == null) _keys = this.keys;

            KeyComparer kc = new KeyComparer();
            _keys.Sort(kc);
        }
        public bool maybeNew(string value, List<Key> _keys = null, Position position = null)
        {
            if (_keys == null) _keys = this.keys;

            if (this.lastKey == null) return false;

            if (_keys.Count == 0) return false;

            int indexOfLastKey = indexKey(this.lastKey);

            if (_keys.Count == indexOfLastKey + 1) return true;

            int nextIndex = _keys.FindIndex(indexOfLastKey, x => x.major == true);

            if (nextIndex == -1) return false;

            Key nextKey = _keys[nextIndex];
            Key firstKey = _keys[0];

            var reNext = new Regex(nextKey.reg);
            var reFirst = new Regex(firstKey.reg);

            //Console.WriteLine("MAYBENEW {0} {1} {2}-{3} {4} {5}", nextIndex, value, !reNext.IsMatch(value), nextKey.reg, reFirst.IsMatch(value), this.lastKey);

            if (!reNext.IsMatch(value) && reFirst.IsMatch(value))
            {

                if (position != null)
                {
                    Position checkposition = new Position(position);
                    checkposition.migration();

                    if (checkposition.check()) return true;
                }

            }

            return false;
        }
        public bool majorKeys(List<Key> _keys = null)
        {
            if (_keys == null) _keys = this.keys;

            int _mparam = 0;
            foreach (Key ikey in _keys)
            {
                if (ikey.major) _mparam++;
            }

            if (_mparam < mparam) return false;
            else return true;
        }
        public void addKey(Key key, List<Key> _keys = null)
        {
            if (_keys == null) _keys = this.keys;

            Key fkey = keys.Find(x => x.value == key.value);

            if (fkey != null)
            {
                if (key.value == "cost" || key.value == "value") return;

                keys.Remove(fkey);
            }

            keys.Add(key);
        }
        public int indexKey(Key key, List<Key> _keys = null)
        {
            if (_keys == null) _keys = this.keys;

            return keys.FindIndex(x => x.value == key.value);
        }
        public int indexKey(string key, List<Key> _keys = null)
        {
            if (_keys == null) _keys = this.keys;

            return keys.FindIndex(x => x.value == key);
        }
        public void printKeys(List<Key> _keys = null)
        {
            if (_keys == null) _keys = this.keys;

            Console.WriteLine("__keys__");



            foreach (Key key in keys)
            {
                Console.WriteLine("key: {0}", key.value);
            }

            Console.WriteLine("");
        }

        ///helpers
        ///
        public bool checkABCDE(string str)
        {
            var re = new Regex(@"\([abcde]+\)");
            return re.IsMatch(str);
        }
        public List<string> gentleNumbersRemove(string str, out string strcopy)
        {
            str = str.Trim();
            strcopy = str;

            if (keys.Count > 0)
            {
                List<Key> numberKeys = keys.FindAll(x => x.number == true);
                List<string> result = new List<string>();

                foreach (Key numberKey in numberKeys)
                {
                    int indexNotNumberKey = keys.FindIndex(x => x.number == false);
                    int indexThisKey = keys.FindIndex(x => x.value == numberKey.value);
                    string thisReg = nreg;

                    if (indexThisKey > indexNotNumberKey)
                    {
                        thisReg = @"\b" + thisReg + "$";
                    }
                    else
                    {
                        thisReg = @"^" + thisReg + "\b";
                    }

                    string value = findAndRemoveOne(strcopy, thisReg, out strcopy, @"[\.,]+");

                    if (value != null) result.Add(value);

                }

                return result;
            }
            else
            {
                return findAndRemove(str, nreg, out strcopy);
            }
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
        public string StrRemove(string str, string substr)
        {
            int n = str.IndexOf(substr);
            if (n > -1)
            {
                str = str.Remove(n, substr.Length);
            }

            return str;
        }
        public List<string> findAndRemove(string str, string rg, out string strcopy, string rgFail = null)
        {
            List<string> values = new List<string>();
            strcopy = str;
            var rvalue = new Regex(rg);
            Match match = rvalue.Match(str.ToLower());

            if (match.Success)
            {
                strcopy = rvalue.Replace(str.ToLower(), @"").Trim();
            }

            while (match.Success)
            {
                values.Add(match.Value.Trim());

                match = match.NextMatch();
            }

            return values;
        }
        public string findAndRemoveOne(string str, string rg, out string strcopy, string rgFail = null)
        {
            string value = null;
            strcopy = str;

            var rvalue = new Regex(rg);
            Match match = rvalue.Match(str.ToLower());

            if (match.Success)
            {
                if (rgFail != null)
                {
                    var rFail = new Regex(rgFail);
                    if (rFail.IsMatch(match.Value))
                        return null;
                }

                strcopy = rvalue.Replace(str.ToLower(), @"").Trim();
                value = match.Value;
            }

            return value;
        }
        public bool ignoreString(string str)
        {
            string[] ignores = { "in the plan", "erisa", "party in" };

            foreach (string ignore in ignores)
            {
                if (str.ToLower().Contains(ignore))
                {
                    return true;
                }
            }

            return false;

        }
        public string clearValue(string str)
        {
            str = Regex.Replace(str, @"&nbsp;", @" ", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&amp;", @"&", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&mdash;|&ndash;|&#150;|&#151;|&#8211;|[--]+", @"—", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"&[^;]*;", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"\n|  ", @" ", RegexOptions.IgnoreCase);

            str = Regex.Replace(str, @"[^0-9a-zA-Z \/\(\)/—\.,]*", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @", ", @",", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @",", @"", RegexOptions.IgnoreCase);

            return str.Trim();
        }
        public string clearSpace(string str)
        {
            str = Regex.Replace(str, @"&[^;]*;", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"[^0-9a-zA-Z]*", @"", RegexOptions.IgnoreCase);

            return str;
        }
        public string clearSpaceCaption(string str)
        {
            str = Regex.Replace(str, @"/[a-zA-Z]+/", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"\([^(]*\)", @"", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"and|inc$|&[^&]*?;|[^0-9a-zA-Z]*", @"", RegexOptions.IgnoreCase);

            return str;
        }
    }
    public class Key
    {
        public string value { get; set; }
        public string reg { get; set; }
        public bool major { get; set; }
        public bool number { get; set; }
        public int arrange { get; set; }
        public Key(string val, bool _number = false, string _reg = @"[\s\S]+")
        {
            this.reg = _reg;
            this.value = val;
            this.number = _number;
            if (val == "description")
            {
                arrange = 2;
                major = false;
            }
            if (val == "identity")
            {
                arrange = 1;
                major = true;
            }
            if (val == "cost")
            {
                arrange = 3;
                major = false;
            }
            if (val == "value")
            {
                arrange = 4;
                major = true;
            }
        }
    }
    public class KeyComparer : IComparer<Key>
    {
        public int Compare(Key x, Key y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                // Если x != null... 
                // 
                if (y == null)
                // ...и y == null, то x больше. 
                {
                    return 1;
                }
                else
                {
                    if (x.arrange < y.arrange)
                    {
                        return -1;
                    }

                    if (x.arrange > y.arrange)
                    {
                        return 1;
                    }

                    return 0;

                }
            }
        }
    }
    public class Position
    {
        public string description { get; set; }
        public string identity { get; set; }
        public double cost { get; set; }
        public double value { get; set; }
        public bool valid { get; set; }

        public Position()
        {
            this.valid = true;
        }
        public bool findStr(string str)
        {
            if (this.identity != null && this.identity.ToLower().Contains(str)) return true;
            if (this.description != null && this.description.ToLower().Contains(str)) return true;

            return false;
        }
        public void clear()
        {
            this.identity = null;
            this.description = null;
            this.cost = 0;
            this.value = 0;
        }
        public Position(Position _position)
            : this()
        {
            this.description = _position.description;
            this.identity = _position.identity;
            this.cost = _position.cost;
            this.value = _position.value;
        }
        public bool check()
        {
            return (this.value != 0 && ((this.identity != null && this.identity != "") || (this.description != null && this.description != "")));
        }
        public bool isEmpty()
        {
            return (value == 0 && cost == 0 && (identity == null || identity == "") && (description == null || description == ""));
        }
        public void migration()
        {
            if (value == 0 && cost != 0)
            {
                value = cost;
                cost = 0;
            }
            if (identity == null && description != null)
            {
                identity = description;
            }
        }
        public object this[string key]
        {
            get
            {
                var prop = GetType().GetProperties();
                var p = prop.FirstOrDefault(x => x.Name == key);
                if (p == null)
                    return null;
                return p.GetValue(this, null);
            }
            set
            {
                var prop = GetType().GetProperties();
                var p = prop.FirstOrDefault(x => key == x.Name);
                if (p == null)
                    return;
                p.SetValue(this, value, null);
            }
        }
        public string ineed(List<Key> keys, bool numbersonly = false)
        {
            if (keys.Count > 0)
            {
                foreach (Key key in keys)
                {
                    if (!numbersonly || key.value == "cost" || key.value == "value")
                    {
                        if (this[key.value] == null || (this[key.value].ToString() == null || this[key.value].ToString() == "0" || this[key.value].ToString() == "")) return key.value;
                    }
                }
            }


            return null;
        }
        public void print()
        {
            Console.WriteLine("____");
            Console.WriteLine("identity: -{0}-", this.identity);
            Console.WriteLine("description: {0}", this.description);
            Console.WriteLine("cost: -{0}-", this.cost);
            Console.WriteLine("value: -{0}-", this.value);
            Console.WriteLine("");
        }
        public string addToCsv()
        {

            string r = "";
            if (this.description == null)
                this.description = "";

            r = r + this.identity + ";" + this.description + ";" + this.cost.ToString() + ";" + this.value.ToString() + "\n";

            return r;
        }
    }
}
