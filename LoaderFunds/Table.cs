using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Forms;

namespace LoaderFundHolders
{
    public class Document
    {
        //*тело документа
        public HtmlNode body { get; set; }
        //*Список интервалов, соответсвующий фондам, которые присутствуют в документе
        public List<Interval> intervals { get; set; }
        //*Список заголовков(названий) фондов, присутствующих в документе
        List<Caption> captions { get; set; }

        //*Номера строк,  в которых найдены ключевые слова(Пример: Portfolio investments), указывающие на то, что далее пойдет раздел с портфолио фонда 
        List<int> prtfinLines = new List<int>();
        //*Номера строк,  после которых идут приложения. Найдены по заголовку APPENDIX. Там могут быть позициилевых фондов
        List<int> appendixLines = new List<int>();

        public Dictionary<string, Holding> holdings = new Dictionary<string, Holding>();

        //*Тэги, в которых будем искать заголовки
        public string[] nameTags = { "p", "font", "b", "span", "div", "td" };
        //*Не используется возможно
        public string[] fakeCaptions = { };
        //*Если длина значимых символов в имени фонда меньше в coefCpt раз, чем длина текстовых символов в теге, то игнорируем этот тег (там может быть просто абзац с текстом, в котором встретилось название фонда)
        public int coefCpt = 8;

        public int trForCpt = 20;

        public Tools tools = new Tools();
        public Ex ex = new Ex();

        public Document(HtmlNode _body)
        {
            body = _body;
            intervals = new List<Interval>();
        }
        //*Функция, которая парсит документ
        public void Parse(Fund fund, List<Caption> _captions)
        {
            captions = _captions;
            if (FindCaptions() && FindIntervals())
            {
                intervals = sortInrervals(intervals);

                foreach (Interval interval in intervals)
                {
                    // Console.WriteLine("Interval Parse");
                    interval.Parse(body);


                    if (interval.holdings.Count > holdings.Count)
                    {
                        holdings = interval.holdings;
                    }
                }

            }
            else
            {
                Console.WriteLine("{0} - CAPTION FAIL", fund.ticker);
                // errors.Error("0009");
                fund.status = 3;
            }
        }
        //*Функция, которая составляет интервалы, после того как нашли заголовки
        public bool FindIntervals()
        {
            intervals.Clear();
            Caption firstCaption = captions.Find(x => x.first == true);

            if (firstCaption != null)
            {
                //*Создаем интервалы, началом которых будут номера строк тегов, в которых был найден первый заголовок.
                //*Конец интервала - номер строки следующего в документе найденного заголовка другого фонда. 
                foreach (int line in firstCaption.lines)
                {
                    bool inintervalyet = false;

                    foreach (Interval interval in intervals)
                    {
                        if (line > interval.from && line < interval.to)
                        {
                            inintervalyet = true;
                        }
                    }

                    if (inintervalyet == false)
                    {
                        int to = 10000000;
                        foreach (Caption secondCaption in captions)
                        {
                            int secondLine = secondCaption.lines.Find(x => x > line);
                            if (secondLine != 0)
                            {
                                if (to > secondLine)
                                    to = secondLine;
                            }
                        }

                        foreach (Interval interval in intervals)
                        {
                            if (to == interval.to)
                            {
                                inintervalyet = true;
                            }

                            if (line == interval.to)
                            {
                                interval.to = to;
                                inintervalyet = true;
                            }
                        }

                        if (inintervalyet == false)
                        {
                            Interval interval = new Interval(line, to);
                            intervals.Add(interval);
                        }
                    }
                }

                foreach (Interval interval in intervals)
                {
                    //*смотрим есть ли в интервале приложения, либо текст указывающий на portfolio investments
                    interval.inInterval(prtfinLines, "prtf");
                    interval.inInterval(appendixLines, "appendix");
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        //*Функция, которая находит заголовки в отчете
        public bool FindCaptions()
        {
            bool add = false;
            //*слабый или сильный метод проверки
            bool poor = true;

            Caption firstCaption = captions.Find(x => x.first == true);
            if (firstCaption != null)
            {
                //*Цикл в котором решаем каким методом проверки будем пользоваться
                foreach (Caption caption in captions)
                {
                    // Console.WriteLine(caption.compressedname);
                    caption.lines.Clear();
                    if (caption != firstCaption)
                    {
                        //*Если есть заголовок, который включает в себя заголовок нужного фонда - то выбираем жесткий метод проверки (равенство строк)
                        if (caption.compressedname.Contains(firstCaption.compressedname))
                        {
                            poor = false;
                            break;
                        }
                    }
                }
                for (int i = 0; i < nameTags.Length; i++)
                {
                    HtmlNodeCollection tags = body.SelectNodes(".//" + nameTags[i]);
                    if (tags != null)
                    {
                        foreach (HtmlNode tag in tags)
                        {
                            Application.DoEvents();

                            Match mm = Regex.Match(tag.InnerHtml, @"href=");
                            if (!mm.Success)
                            {
                                //*оставляем только буквы и сравниваем по ним
                                string candidat = tools.ClearSpaceCaption(tag.InnerText).ToLower();
                                string candidatextend = candidat + tools.ClearSpaceCaption(tools.GetMatchingDdValue(tag)).ToLower();
                                string candidatNofund = candidat;

                                if (!poor)
                                {
                                    if (candidatNofund.Length > 10)
                                        candidatNofund = Regex.Replace(candidatNofund, @"fund$", @"", RegexOptions.IgnoreCase);

                                    candidatNofund = Regex.Replace(candidatNofund, @"inc$", @"", RegexOptions.IgnoreCase);
                                }

                                if (candidat != "")
                                {
                                    foreach (Caption caption in captions)
                                    {
                                        bool _if;


                                        if (poor)
                                        {
                                            //*слабая проверка
                                            _if = poorcpt(candidat, caption, coefCpt);

                                            //ACAAX
                                            //_if = poorcpt(candidat, caption.compressedname, coefCpt) && (!candidat.Contains("funds") || caption.compressedname.Contains("funds"));
                                        }
                                        else
                                        {
                                            //*жесткая проверка
                                            _if = (candidat == caption.compressednamenozip)
                                                || (candidatextend == caption.compressednamenozip)
                                                || (candidatNofund == caption.compressedname)
                                                || (candidat == caption.compressedname);
                                        }

                                        if (_if)
                                        {
                                            HtmlNode tg = tag;
                                            //*Тэг с заголовком может лежать в таблице с позициями, узнаем это 
                                            tg = ParentTable(tg);

                                            if (tg != null)
                                            {
                                                //*Если в таблице - то проверяем лежат ли в этой таблице другие заголовки (как например в таблице с оглавлениями и прочим)
                                                if (tg.Name.ToLower() == "table" && checkTableForAnotherCaption(tg, caption))
                                                {
                                                    tg = null;
                                                }

                                                if (tg != null && !caption.lines.Exists(x => x == tg.Line))
                                                {
                                                    //*Добавляем номер найденной строки в объект "заголовок"
                                                    caption.AddLine(tg.Line);
                                                    if (caption.first == true)
                                                    {
                                                        add = true;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //*Аналогичным почти способом ищем строки, указывающие на приложения и на portfolioinvestments
                                    if (!prtfinLines.Contains(tag.Line))
                                    {
                                        foreach (string prtf in ex.exesGood)
                                        {
                                            if (!prtfinLines.Contains(tag.Line) && poorcpt(candidat, prtf, coefCpt))
                                            {
                                                HtmlNode tg = tag;
                                                tg = ParentTable(tg);
                                            }
                                        }
                                    }
                                    //*Аналогичным почти способом ищем строки, указывающие на приложения и на portfolioinvestments
                                    if (!appendixLines.Contains(tag.Line))
                                    {
                                        foreach (string appendix in ex.exesAppendix)
                                        {
                                            if (!appendixLines.Contains(tag.Line) && poorcpt(candidat, appendix, coefCpt))
                                            {
                                                appendixLines.Add(tag.Line);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (Caption caption in captions)
                {
                    //*Отсортируем по возрастанию найденные строки
                    caption.lines.Sort();
                }

                if (prtfinLines.Count > 0)
                {
                    prtfinLines.Sort();
                }
            }

            return add;
        }
        //*Функция, которая проверяет есть ли в данной таблице заголовок отличный от переданного в аргументы
        public bool checkTableForAnotherCaption(HtmlNode table, Caption _caption)
        {
            HtmlNodeCollection tds = table.SelectNodes(".//td");
            if (tds != null)
            {
                foreach (HtmlNode td in tds)
                {
                    string candidat = tools.ClearSpaceCaption(td.InnerText).ToLower();
                    foreach (Caption caption in captions)
                    {
                        if (caption.compressedname != _caption.compressedname && !_caption.compressedname.Contains(caption.compressedname))
                        {
                            if (poorcpt(candidat, caption, 50))
                            {
                                Console.WriteLine("INTABLE: {0}, {1}", _caption.compressedname, caption.compressedname);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        //*Функция, которая сравнивает строку с названием фонда
        public bool poorcpt(string candidat, Caption caption, int coefCpt)
        {
            return candidat.Contains(caption.compressedname) && tools.ClearFromNumbers(candidat).Length < caption.compressednamenozip.Length * coefCpt;
        }
        public bool poorcpt(string candidat, string compressedname, int coefCpt)
        {
            return candidat.Contains(compressedname) && tools.ClearFromNumbers(candidat).Length < compressedname.Length * coefCpt;
        }
        //*Функция, которая определяет находится ли выше этой строки в данной таблице заголовок колонки (shares, value...)
        public bool CaptionInUp(HtmlNode table, HtmlNode tr)
        {
            Table _tbl = new Table(table);

            while (tr != null)
            {
                if (tr.Name.ToLower() == "tr" && _tbl.findKeyInTr(tr) == true) return true;
                tr = tr.PreviousSibling;
            }

            return false;
        }
        //*Функция, которая проверяет есть ли в строке(предположительно в строке) цифровые значения
        public bool NumbersInRow(HtmlNode tag)
        {
            string tg = tag.Name;
            while (tg != "tr" && tag.ParentNode != null)
            {
                tag = tag.ParentNode;
                tg = tag.Name.ToLower();
            };

            if (tg == "tr")
            {
                HtmlNodeCollection tds = tag.SelectNodes(".//td");
                if (tds != null)
                {
                    foreach (HtmlNode td in tds)
                    {
                        double Number;
                        string numberTry = tools.ClearStrNum(td.InnerText);
                        bool isInt = tools.extendTryParse(td.InnerText, out Number);

                        if (isInt) return true;
                    }
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        //*Функция, которая смотрит находится ли текущий тэг в какой либо таблице
        public HtmlNode ParentTable(HtmlNode tag)
        {
            HtmlNode originaltag = tag;
            HtmlNode _tr = null;
            string tg = tag.Name;
            while (tg != "body" && tg != "table" && tag.ParentNode != null)
            {
                tag = tag.ParentNode;
                tg = tag.Name.ToLower();

                if (tg.ToLower() == "tr" && _tr == null) _tr = tag;

                if (tg.ToLower() == "tr" && tag.PreviousSibling != null)
                {
                    HtmlNode tr = tag.PreviousSibling;
                    int i = 0;
                    while (tr != null)
                    {
                        tr = tr.PreviousSibling;
                        i++;

                        if (i > trForCpt || NumbersInRow(originaltag) == true)
                            return null;
                    }
                }
            };

            if (tg == "table")
            {
                //|| CaptionInUp(tag, _tr)
                if (NumbersInRow(originaltag) == true)
                {
                    return null;
                }
                else
                {
                    // Console.WriteLine("TAG IN TABLE : {0} {1}", originaltag.Line, originaltag.InnerText);
                    return tag;
                }
            }

            else return originaltag;
        }
        //*Функция, которая сортирует интервалы по признаку: встретились ли ей ключевые слова portfolio investments и по возраствнию from
        public List<Interval> sortInrervals(List<Interval> intervals)
        {
            IntervalComparer ic = new IntervalComparer();
            intervals.Sort(ic);
            List<Interval> newIntervals = new List<Interval>();
            List<Interval> tail = new List<Interval>();

            if (intervals.Exists(x => x.prtf == true))
            {
                Interval iprev = null;
                foreach (Interval icur in intervals)
                {
                    if (icur.prtf == true)
                    {
                        newIntervals.Add(icur);
                        iprev = icur;
                    }
                    else
                        if (iprev != null)
                    {
                        if (icur.from >= iprev.from)
                        {
                            newIntervals.Add(icur);
                            iprev = icur;
                        }
                        else
                        {
                            tail.Add(icur);
                        }
                    }


                }

                if (tail.Count > 0)
                {
                    foreach (Interval t in tail)
                    {
                        newIntervals.Add(t);
                    }
                }
            }
            else
            {
                return intervals;
            }

            return newIntervals;
        }
    }
    //*Это объект заголовка в таблице (shares, value), везде далее Ключа.
    public class Key : ICloneable
    {
        //*Ключевой значение 
        public string value { get; set; }
        public int ratio { get; set; }
        //*Исключения - пример share -> sharesholder
        public string exclude { get; set; }
        //*Позиция в таблице. Все пустые ячейки игнорируются
        public float index { get; set; }
        //*Позиция в таблице. Все ячейки учитываются 
        public float indexbp { get; set; }
        //*Временный буфер для индексов
        public float indexTemp { get; set; }
        public float indexBpTemp { get; set; }
        //*Значимость ключа
        public bool major { get; set; }
        //*Ключи, имеющие равные group - синонимы
        public int group { get; set; }

        public Key(string _value, bool _major, int _group = 0, string _exclude = null, int _ratio = 1)
        {
            value = _value;
            major = _major;
            exclude = _exclude;
            ratio = _ratio;

            group = _group;

            indexTemp = -1;
            indexBpTemp = -1;
        }
        public Key(Key other)
        {
            value = other.value;
            major = other.major;
            exclude = other.exclude;
        }
        public void indexesFromTemp()
        {
            if (indexTemp != -1)
            {
                index = indexTemp;
                indexTemp = -1;
            }

            if (indexBpTemp != -1)
            {
                indexbp = indexBpTemp;
                indexBpTemp = -1;
            }
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class Table
    {
        public Dictionary<string, Key> keys = new Dictionary<string, Key>();


        public Tools tools = new Tools();
        //public Errors errors = new Errors();
        public Ex ex = new Ex();
        public int upcounts = 0;

        //*Позиции, найденные в этой таблице
        public Dictionary<string, Holding> holdings = new Dictionary<string, Holding>();
        //*Строки таблицы
        public HtmlNodeCollection trs;
        //*Массив ключей
        public List<Key> dataCols { get; set; }
        //*Флаг, определяется наличием портфолио
        public bool portfolio { get; set; }
        public HtmlNode node { get; set; }

        public Table prev = null;
        public Table next = null;

        //*Разделы таблицы (Пример Australia - 5.5% - если встретим такое - значит всем нижележащим позициям будем присваивать Австралию как страну, пока не дойдем до total) 
        public string country = null;
        public string industry = null;
        public string type = null;

        //*Индекс заполненности таблицы. Количество всех ячеек в строке делим на количество непустых
        public bool occupancy = false;
        public double occupancyIndex = 0.7;

        public int mtdsCount = 0;
        public bool totalCaption = false;

        public int backTable = 0;

        public void printDataCols()
        {
            Console.WriteLine("__________________");
            foreach (Key dataCol in dataCols)
            {

                Console.WriteLine("{0}: index - {1}, indexbp - {2}", dataCol.value, dataCol.index, dataCol.indexbp);

            }
            Console.WriteLine("________end_______");
        }

        public Table(HtmlNode table)
        {
            node = table;
            dataCols = new List<Key>();
            portfolio = false;

            trs = node.SelectNodes(".//tr");

            //*Сюда должны поместить все возможные заголовки, которые могут встреться в таблице с портфолио

            keys.Add("name", new Key("name", false));
            keys.Add("share", new Key("share", true, 1, "shareholder"));
            keys.Add("parvalue", new Key("parvalue", true, 1));
            keys.Add("principal", new Key("principal", true, 1));
            keys.Add("cost", new Key("cost", false));
            keys.Add("faceamount", new Key("faceamount", true, 1));
            keys.Add("percentageof", new Key("percentageof", false, 3));
            keys.Add("netassets", new Key("netassets", false, 3));
            keys.Add("ofnet", new Key("ofnet", false, 3));
            keys.Add("coupon", new Key("coupon", false, 2));
            keys.Add("interestrate", new Key("interestrate", false, 2));
            keys.Add("maturity", new Key("maturity", false));
            keys.Add("ratings", new Key("ratings", false));
            keys.Add("industry", new Key("industry", false));
            keys.Add("country", new Key("country", false));
            keys.Add("value", new Key("value", true, 0, "parvalue"));
            keys.Add("currency", new Key("currency", true));
            keys.Add("contracts", new Key("contracts", true, 1));
            keys.Add("notionalamount", new Key("notionalamount", true, 1));

        }

        public void tAnalysis()
        {
            findDataCols();
            prtfTable();
        }
        //*Функция, которая определяет портфолийная таблица перед нами или нет. Он является портфолийной, если встретятся в ней два значащих Ключа. Кроме этого максимально количество непустых ячеек должно быть меньше 7 
        private bool prtfTable()
        {
            int major = 0;
            foreach (Key key in dataCols)
            {
                if (key.major == true) major++;
            }

            if (major == 2 && mtdsCount < 7)
                portfolio = true;
            else
                portfolio = false;

            return portfolio;
        }
        //*Функция, которая находит Ключи в таблице
        public bool findDataCols()
        {
            Application.DoEvents();
            HtmlNodeCollection trs = node.SelectNodes(".//tr");
            if (trs == null) return false;

            foreach (HtmlNode tr in trs)
            {
                if (findKeyInTr(tr) != false) return true;
            }

            return false;
        }
        //*Функция, которая находит Ключи в строке
        public bool findKeyInTr(HtmlNode tr)
        {
            HtmlNodeCollection tds = tr.SelectNodes(".//td");
            if (tds == null) return false;

            int numTd = 0;
            int dnumTd = 0;
            int tdsCount = trLength(tr);
            int tdsCountBp = tools.tdsCount(tds);

            List<string> cpts = new List<string>();

            if (tdsCount > mtdsCount) mtdsCount = tdsCount;

            foreach (HtmlNode td in tds)
            {
                Key fkey = findKeyInValue(td.InnerText);

                if (fkey != null)
                {
                    //*Если уже встречался такой ключ в таблице, значет это не та таблица
                    if (cpts.Find(x => x == fkey.value) != null)
                    {
                        dataCols.Clear();
                        return false;
                    }

                    fkey.index = ((float)(dnumTd + 1) / tdsCount);
                    fkey.indexbp = ((float)(numTd + 1) / tdsCountBp);
                    //Console.WriteLine("numTd: {0} / tds.Count :{1}  = {2}", numTd, tds.Count, fkey.indexbp);
                    cpts.Add(fkey.value);
                    renewDatacols(fkey);
                }
                else
                {
                    //*Если в строке в какой-нибудь ячейке  есть только цифры - игнорируем и не добавляем ключ для этой таблицы
                    if (prtfTable() || tools.Number(tools.ClearFromNumbers(td.InnerText.ToLower())))
                    {
                        return true;
                    }
                }

                if (tools.ClearSpace(td.InnerText.Trim()) != "") dnumTd++;

                numTd = numTd + td.GetAttributeValue("rowspan", 1);
            }

            return false;
        }
        //*Облегченная предыдущая функция, только для проверки
        public bool findKeyInTrLight(HtmlNode tr)
        {
            HtmlNodeCollection tds = tr.SelectNodes(".//td");
            if (tds == null) return false;
            foreach (HtmlNode td in tds)
            {
                Key fkey = findKeyInValue(td.InnerText);

                if (fkey != null)
                {
                    //Console.WriteLine(td.InnerText);
                    return true;
                }
            }

            return false;
        }
        //*Функция определяющая является ли строка ключом
        public Key findKeyInValue(string innerText, int maxLength = 50)
        {
            if (tools.ClearSpaceCaption(innerText).Length < maxLength)
            {
                string value = tools.ClearFromNumbers(innerText.ToLower());

                if (tools.Number(value))
                {
                    return null;
                }
                else
                {
                    foreach (KeyValuePair<string, Key> kvp in keys)
                    {
                        if (value.Contains(kvp.Value.value) && (kvp.Value.exclude == null || !value.Contains(kvp.Value.exclude)))
                        {
                            return kvp.Value;
                        }
                    }
                }
            }

            return null;
        }
        //*Функция которая продолжает предыдущую таблицу. Копирует в новыю таблицу все ключи (столбцы) если она содержала портфолио
        public void continueTable(Table _table)
        {
            bool inheritance = true;

            if (_table.node.NextSibling != null && _table.node.NextSibling.Line + 100 < node.Line) inheritance = false;

            if (inheritance == true)
            {
                //Console.WriteLine("TableAnalCont");
                // Console.WriteLine("name {0}, nLine {1} line :{2}", _table.node.NextSibling.Name, _table.node.NextSibling.Line, node.Line);
                portfolio = _table.portfolio;

                if (portfolio == true)
                {
                    type = _table.type;
                    dataCols = _table.dataCols;
                }

                prtfTable();
            }
            else
            {
            }
        }
        //*Парсинг таблицы по строкам
        public void Parse()
        {
            if (trs == null || portfolio == false) return;
            foreach (HtmlNode tr in trs)
            {
                try
                {
                    Holding holding = ParseRow(tr);
                    if (holding != null)
                    {
                        if (!holdings.ContainsKey(holding.key()))
                        {
                            holdings.Add(holding.key(), holding);
                        }
                        else
                        {
                            // Console.WriteLine("fail");
                        }
                    }
                }
                catch (StackOverflowException)
                {
                    Console.WriteLine("StackOverflowException");
                }

            }
        }
        //*Количество только непустых ячеек
        public int trLength(HtmlNode tr)
        {
            int length = 0;
            HtmlNodeCollection tds = tr.SelectNodes(".//td");
            if (tds == null) return 0;

            foreach (HtmlNode td in tds)
            {
                if (tools.ClearSpace(td.InnerText.Trim()) != "") length++;
            }

            return length;
        }
        //* Примеры стран, индустрии и типов позиций есть в xml файлах - их пытаемся найти в таблице
        public string takeCountry(string value)
        {
            value = tools.ClearSpaceCaption(value);
            value = Regex.Replace(value, @"[^a-zA-Z]*", @"", RegexOptions.IgnoreCase);

            if (ex.countries.Contains(ex.clear(value)))
            {
                return value;
            }

            return null;
        }

        public string takeIndustry(string value)
        {
            value = tools.ClearSpaceCaption(value);
            value = Regex.Replace(value, @"[^a-zA-Z]*", @"", RegexOptions.IgnoreCase);

            if (ex.industries.Contains(ex.clear(value)))
            {
                return value;
            }

            return null;
        }

        public string takeType(string value)
        {
            value = tools.ClearSpaceCaption(value);
            value = Regex.Replace(value, @"[^a-zA-Z]*", @"", RegexOptions.IgnoreCase);

            foreach (string type in ex.types)
            {
                if (ex.clear(value).Contains(type)) return type;
            }

            return null;
        }
        //* Функция которая определяет содержатся ли число в ячейке
        public bool numbersInTds(HtmlNodeCollection tds)
        {
            foreach (HtmlNode td in tds)
            {
                string value = tools.ClearValue(td.InnerText.Trim());
                double number;
                string numberTry = tools.ClearStrNum(value);

                if (tools.extendTryParse(numberTry, out number))
                {
                    return true;
                }
            }
            return false;
        }
        //* Функция, которая парсит строку.
        public Holding ParseRow(HtmlNode tr, Holding holding = null)
        {
            //* В эту функцию так же можно вернуться снизу, чтобы добрать необходимую часть в названии позиции
            bool frombottom = false;
            bool bad = false;

            List<Key> dataColsinTr = new List<Key>(dataCols);
            HtmlNodeCollection tds = tr.SelectNodes(".//td");
            int tdsCount = trLength(tr);
            int tdsCountBp = tools.tdsCount(tds);

            //* Снизу всегда приходим с объектом holding
            if (holding == null)
            {
                backTable = 0;
                holding = new Holding();
            }
            else frombottom = true;

            //* Бывают фэйковые строки, игнорируем, проходим либо вниз, либо вверх в соответствие с исходным направлением
            if (tds == null || tdsCount < 1)
            {
                if (frombottom == true && tr.PreviousSibling != null)
                {
                    return ParseRow(tr.PreviousSibling, holding);
                }
                else
                {
                    //if (tds == null) return null;
                }
            }

            int col = 0;
            int i = 0;

            if ((frombottom == true && findKeyInTrLight(tr) == true) || tds == null)
            {
                //Console.WriteLine("findKeyInTr MB");
            }
            else
                foreach (HtmlNode td in tds)
                {
                    bool addDataCols = false;
                    string value = td.InnerText.Trim();



                    if (tools.ClearSpace(value) != "")
                    {
                        value = tools.ClearValue(value);

                        //* Ближайший ключ
                        Key closestKey = null;
                        //* Проверяем сразу содержит ли ячейка валюту
                        string currency = tools.currency(value, out value);
                        double number;
                        //* Определяем текст ли в ячейке или число
                        string numberTry = tools.ClearStrNum(value);
                        bool isnumber = tools.extendTryParse(numberTry, out number);

                        //* Индекс ячейки - учитывает возможные пустые ячейки
                        float index = ((float)(col + 1) / tdsCount);
                        //* Точный Индекс ячейки - не учитывает возможные пустые ячейки
                        float indexbp = ((float)(i + 1) / tdsCountBp);

                        float findIndex = index;

                        //* Если снизу или в таблице мало пустот или мы нашли больше 3 ключей используем точный индекс
                        if (frombottom == true || occupancy == true || dataCols.Count > 3) findIndex = indexbp;

                        //* Проверим возможно мы в заголовке находимся
                        closestKey = findKeyInValue(value, 20);

                        string _country = takeCountry(value);
                        string _industry = takeIndustry(value);

                        if (numbersInTds(tds) == false)
                        {
                            string _type = takeType(value);

                            if (_country != null)
                            {
                                country = _country;
                            }
                            if (_industry != null)
                            {
                                industry = _industry;
                            }
                            if (_type != null)
                            {
                                type = _type;
                            }
                        }
                        else
                        {
                            if (tools.ClearSpaceCaption(value.ToLower()).Contains("netassets"))
                            {
                                totalCaption = true;
                                //Console.WriteLine("TOTAL");
                            }
                        }

                        if (country != null && value.Contains("total"))
                        {
                            country = null;
                        }
                        if (industry != null && value.Contains("total"))
                        {
                            industry = null;
                        }
                        if (industry != null && value.Contains("total") && takeType(value) != null)
                        {
                            type = null;
                        }

                        //* Исли мы наткнулись на заголовок
                        if (closestKey != null && numbersInTds(tds) == false && tools.findKeyInValueEx(value) == false)
                        {
                            Console.WriteLine("findKeyInValue {0} : {1}, {2}", closestKey.value, index, indexbp);
                            closestKey.index = index;
                            closestKey.indexbp = indexbp;
                            renewDatacols(closestKey, true);
                            checkOccupancy(tdsCount);
                        }
                        else
                        {
                            if (tdsCount >= 3 || frombottom == true)
                            {
                                if (currency != null)
                                {
                                    closestKey = keys["currency"];
                                    if (value.Length > 0)
                                    {
                                        addDataCols = true;
                                    }
                                }

                                if (value.Length > 0)
                                {

                                    //Console.WriteLine("VALUE: {0}", value);
                                    //* Работа с числами, ищем Ключ из числовых ключей
                                    if (isnumber == true)
                                    {
                                        if (frombottom == true && index > indexKey("name")) break;

                                        //* Если похоже на величину с процентом идем сюда
                                        if (tools.isNumberWP(numberTry))
                                        {
                                            closestKey = closest(findIndex, new int[] { 3, 2 });

                                            if (closestKey == null)
                                            {
                                                if (tools.isCoupon(value)) closestKey = keys["coupon"];
                                                else
                                                {
                                                    //  Console.WriteLine("BADPOINT");
                                                    bad = true;
                                                }
                                            }
                                        }

                                        else
                                        {
                                            closestKey = closest(findIndex, new string[] { "share", "value", "principal", "cost", "faceamount", "parvalue", "notionalamount", "contracts" });

                                            //Console.WriteLine("valueNumber: {0}, {1}, {2}", value, closestKey.value, findIndex);

                                            if (closestKey == null)
                                            {

                                                //* Если не нашли ключ, то ошибка
                                                bad = true;
                                                // Console.WriteLine("NoShareValue");
                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //* Есть список всякого мусора, смотрим, чтобы в него не попали
                                        if (exesString(value)) { bad = true;/* Console.WriteLine("exesString {0}", value);*/ }
                                        else
                                        {
                                            //* Если мы уже нашли столбец в котором находятся имена, то ищем ближайший ключ,
                                            //* часто в таблицах нет заголовка Name или securityName, поэтому ключ с именем можно определить только исходя из данных
                                            if (dataCols.Exists(x => x.value == "name"))
                                            {
                                                closestKey = closest(findIndex, new string[] { "ratings", "industry", "country", "name", "maturity", "coupon" });
                                            }

                                            //* Если не нашли ключ - анализируем данные
                                            if (closestKey == null)
                                            {
                                                // Console.WriteLine("closestKey == null");
                                                if (ex.countries.Contains(ex.clear(value)))
                                                {
                                                    closestKey = keys["country"];
                                                    addDataCols = true;
                                                }
                                                else
                                                {
                                                    if (ex.industries.Contains(ex.clear(value)))
                                                    {

                                                        closestKey = keys["industry"];
                                                        addDataCols = true;
                                                    }
                                                    else
                                                    {

                                                        if (dataCols.Exists(x => x.value == "name"))
                                                        {
                                                            if (frombottom == true && tdsCount == 1)
                                                            {
                                                                closestKey = keys["name"];
                                                            }
                                                            else
                                                            {
                                                                if (frombottom != true)
                                                                {
                                                                    //* Так как ключ имя в другой колонке - здесь ошибка
                                                                    bad = true;
                                                                    // Console.WriteLine("NameExist, {0}, {1}", index, value);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //  Console.WriteLine("valueNull: {0}", value);
                                                            closestKey = keys["name"];
                                                            addDataCols = true;
                                                        }
                                                    }
                                                }

                                            }
                                            //* Все равно анализируем данные, так как в одной колонке могут находится разные типы данных - 
                                            //* В столбце с именем часто находятся coupon и maturity
                                            else
                                            {
                                                //Console.WriteLine("value: {0}, {1}, {2}", value, closestKey.value, findIndex);
                                                if (closestKey.value != "country" && closestKey.value != "industry" && closestKey.value != "ratings")
                                                {
                                                    if (closestKey.value != "maturity" && tools.clearFromMC(value) == null)
                                                    {
                                                        if (frombottom == true) { break; }

                                                        if (Math.Round(index) == indexKey("name"))
                                                        {
                                                        }
                                                        else
                                                        {
                                                            closestKey = keys["maturity"];
                                                            addDataCols = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (tools.isMaturityOnly(value))
                                                        {
                                                            closestKey = keys["maturity"];
                                                            //addDataCols = true;
                                                        }
                                                        else
                                                        {
                                                            //* Если дошли до сюда - то ключ - имя
                                                            closestKey = keys["name"];
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                //* Если нет ошибок и мы определили тип ключа
                                if (bad == false && closestKey != null)
                                {
                                    //* Если нашли ключ - добавляем его
                                    if (addDataCols && frombottom != true)
                                    {
                                        closestKey.index = index;
                                        closestKey.indexbp = indexbp;

                                        renewDatacols(closestKey);
                                    }
                                    else
                                    {
                                        dataColsinTr.Add(closestKey);
                                    }

                                    //* Если мы пришли снизу - нас интересует только название
                                    if (frombottom != true)
                                    {
                                        //* Заполняем holdings попутно в некоторых местах снова проверяя данные

                                        //* Сначала только числовые
                                        if (closestKey.value == "share" && isnumber == true) holding._shares = number;
                                        if (closestKey.value == "contracts" && isnumber == true) holding.contracts = number.ToString();
                                        if (closestKey.value == "notionalamount" && isnumber == true) holding.notionalamount = number.ToString();
                                        if (closestKey.value == "parvalue" && isnumber == true) holding.parvalue = number.ToString();
                                        if (closestKey.value == "faceamount" && isnumber == true) holding.faceAmount = number.ToString();
                                        if (closestKey.value == "principal" && isnumber == true) holding.principalAmount = number.ToString();
                                        if (closestKey.value == "value" && isnumber == true) holding._value = number;

                                        if ((closestKey.value == "maturity" || tools.isMaturityAlso(value)) && isnumber == false && holding.maturityDate == null)
                                        {
                                            holding.maturityDate = tools.clear(value, tools.maturityReg, out value);
                                            holding.tname = value;
                                        }

                                        if ((closestKey.value == "percentageof" || closestKey.value == "ofnet" || closestKey.value == "netassets") && isnumber == true && number < 100)
                                        {
                                            holding.percentOfNetAssets = number.ToString();
                                        }
                                        else
                                        {
                                            if ((closestKey.value == "interestrate" || closestKey.value == "coupon" || tools.isCoupon(value)) && holding.couponRate == null && closestKey.value != "percentageof")
                                            {
                                                if (isnumber == true && number < 10)
                                                {
                                                    holding.couponRate = number.ToString();
                                                }
                                                else
                                                    if (tools.isCoupon(value))
                                                {
                                                    holding.couponRate = tools.clear(value, tools.couponReg, out value);
                                                    value = tools.TrimGarbage(value);
                                                    holding.tname = value;
                                                }

                                            }
                                        }


                                        if (closestKey.value == "currency" && isnumber == false)
                                        {
                                            if (currency != null)
                                            {
                                                holding.currency = currency;
                                            }
                                            else
                                            {
                                                string cur = tools.currency(value);
                                                if (cur != null) holding.currency = cur;
                                            }

                                        }
                                        if (closestKey.value == "country" && isnumber == false)
                                        {
                                            holding.country = value.Trim();
                                        }
                                        if (closestKey.value == "industry" && isnumber == false)
                                        {
                                            if (holding.industry == null)
                                                holding.industry = value.Trim();
                                            else
                                                holding.industry = value.Trim() + " " + holding.industry;
                                        }
                                    }


                                    if (closestKey.value == "name" && isnumber == false)
                                    {
                                        value = tools.ClearStrNameNew(value);

                                        //* В имени может тоже содержаться coupon, maturity - разносим их по нужным полям
                                        if (holding.maturityDate != null && holding.couponRate != null)
                                        {
                                            value = tools.clearFromMC(value);
                                        }

                                        if (value != null && value.Length != 0)
                                        {
                                            //* Если снизу, то имя соединяем с тем что взяли с последующей строки
                                            if (holding.name == null)
                                            {
                                                holding.name = tools.ClearStrNameNew(value);
                                            }
                                            else
                                            {
                                                if (frombottom == true)
                                                {
                                                    holding.name = value + " " + holding.name;

                                                }
                                                else
                                                {
                                                    //* Если имя уже есть - то ошибка
                                                    bad = true;
                                                    //Console.WriteLine("BadName - {0}", value);
                                                }

                                            }

                                            if (frombottom == true)
                                            {

                                                if (holding.name != null && holding.tname != null)
                                                    holding.name += " " + holding.tname;
                                            }
                                        }
                                    }

                                    // Console.WriteLine("VALUE(CL), {0}", value);

                                    //* Если все ок - можно актуализировать позицию ключа
                                    tempIndexes(closestKey.value, index, indexbp);
                                }
                                else
                                {
                                    if (frombottom == true)
                                    {
                                    }
                                }
                            }
                        }
                        col++;
                    }
                    i = i + td.GetAttributeValue("rowspan", 1);
                }

            if (frombottom == true)
            {
                //
            }
            if (bad == true)
            {
                //Console.WriteLine("BAD");
            }
            /* 
            printDataCols();
            holding.Print();*/



            //* Если нужно вверх подняться - идем наверх
            if (up(holding))
            {
                upcounts++;
                //* Если мы находимся в самой верхней строке - идем в таблицу выше
                if (tr.PreviousSibling != null)
                {
                    return ParseRow(tr.PreviousSibling, holding);
                }
                else
                {
                    return prevTable(holding);
                }
            }
            else
                //* Проверяем была ли ошибка и удовлетворяет ли позиция нашим требованиям. Если да - сохраняем ее
                if (holding.check() && !bad)
            {
                if (holding.country == null && country != null)
                    holding.country = country;

                if (holding.industry == null && industry != null)
                    holding.industry = industry;

                if (frombottom != true)
                {
                    // Console.WriteLine("oc: {0}", (double)tdsCount / (double)tds.Count);
                    /*if (((double)tdsCount / (double)tdsCountBp) > occupancyIndex) 
                        occupancy = true;
                    else 
                        occupancy = false;*/


                    //* Уточняем индексы всех ключей
                    clarifyIndexes(holding);
                }
                else
                {
                    //* Только удаляем неактуальные индексы
                    deleteIndexes(holding);
                }

                //* Проверяем заполненность таблицы
                checkOccupancy(tdsCount);



                if (holding.type == null && type != null)
                    holding.type = type.ToLower();

                //* Еще раз ищем coupon maturity в имени
                holding.transformbund();
                // holding.Print();

                //* Распарсили позицию
                return holding;
            }
            else
            {
                return null;
            }
        }
        public void checkOccupancy(int tdsCount)
        {
            if ((double)tdsCount / (double)dataCols.Count > occupancyIndex)
                occupancy = true;
            else
                occupancy = false;
        }
        //* Уточнение и удаление индексов - две функции, суть которых в том, 
        //* что если позиция имеет конкретный ключ - мы актуализируем его позицию (так как могли появиться пустые ячейки и тд)
        //* Если нет ключа в позиции - удаляем его из datacols
        public void deleteIndexes(Holding holding)
        {
            if (holding.name == null) deleteIndex("name");
            if (holding._shares == 0) deleteIndex("share");
            if (holding.principalAmount == null) deleteIndex("principal");
            if (holding.faceAmount == null) deleteIndex("faceamount");
            if (holding._value == 0) deleteIndex("value");
            if (holding.couponRate == null) { deleteIndex("coupon"); deleteIndex("interestrate"); }
            if (holding.maturityDate == null) deleteIndex("maturity");
            if (holding.percentOfNetAssets == null) { deleteIndex(3); }
            if (holding.currency == null) deleteIndex("currency");
            if (holding.industry == null) deleteIndex("industry");
            if (holding.country == null) deleteIndex("country");
            if (holding.parvalue == null) deleteIndex("parvalue");
            if (holding.notionalamount == null) deleteIndex("notionalamount");
            if (holding.contracts == null) deleteIndex("contracts");
        }
        public void clarifyIndexes(Holding holding)
        {
            if (holding.name != null) clarifyIndexes("name"); else deleteIndex("name");
            if (holding._shares != 0) clarifyIndexes("share"); else deleteIndex("share");
            if (holding.principalAmount != null) clarifyIndexes("principal"); else deleteIndex("principal");
            if (holding.faceAmount != null) clarifyIndexes("faceamount"); else deleteIndex("faceamount");
            if (holding._value != 0) clarifyIndexes("value"); else deleteIndex("value");
            if (holding.couponRate != null) { clarifyIndexes("coupon"); clarifyIndexes("interestrate"); } else { deleteIndex("coupon"); deleteIndex("interestrate"); }
            if (holding.maturityDate != null) clarifyIndexes("maturity"); else deleteIndex("maturity");
            if (holding.percentOfNetAssets != null) { clarifyIndexes(3); } else { deleteIndex(3); }
            if (holding.currency != null) clarifyIndexes("currency"); else deleteIndex("currency");
            if (holding.industry != null) clarifyIndexes("industry"); else deleteIndex("industry");
            if (holding.country != null) clarifyIndexes("country"); else deleteIndex("country");
            if (holding.parvalue != null) clarifyIndexes("parvalue"); else deleteIndex("parvalue");
            if (holding.notionalamount != null) clarifyIndexes("notionalamount"); else deleteIndex("notionalamount");
            if (holding.contracts != null) clarifyIndexes("contracts"); else deleteIndex("contracts");
        }
        public void tempIndexes(string name, float _indexTemp, float _indexBpTemp)
        {
            Key key = dataCols.Find(x => x.value == name);
            if (key != null)
            {
                key.indexTemp = _indexTemp;
                key.indexBpTemp = _indexBpTemp;


            }
        }
        public void clarifyIndexes(string name)
        {
            Key key = dataCols.Find(x => x.value == name);
            if (key != null)
            {
                //Console.WriteLine("indexsNew ({0}) : {1}, {2}", name, key.indexTemp, key.indexBpTemp);
                key.indexesFromTemp();
                renewDatacols(key);
            }
        }
        public void clarifyIndexes(int group)
        {
            List<Key> keys = dataCols.FindAll(x => x.group == group);
            if (keys != null)
            {
                foreach (Key key in keys)
                {
                    key.indexesFromTemp();
                    renewDatacols(key);
                }
            }
        }
        public void deleteIndex(string name)
        {
            Key key = dataCols.Find(x => x.value == name);
            if (key != null)
            {
                dataCols.Remove(key);
            }
        }
        public void deleteIndex(int group)
        {
            List<Key> keys = dataCols.FindAll(x => x.group == group);
            if (keys != null)
            {
                foreach (Key key in keys)
                {
                    dataCols.Remove(key);
                }
            }
        }
        //* Выбор предыдущей таблицы. Максимум 5 таблиц, чтобы избежать бесконечных циклов
        public Holding prevTable(Holding holding)
        {
            if (backTable == 5) return null;

            Table _prev = prev; ;

            for (int i = 0; i < backTable; i++)
            {
                _prev = _prev.prev;
            }

            if (_prev == null || _prev.node == null) return null;

            HtmlNode trPrev = _prev.node.SelectSingleNode(".//tr[last()]");
            if (trPrev != null)
            {
                backTable++;
                return ParseRow(trPrev, holding);
            }
            else
                return null;
        }
        //* Проверка - нужно ли вверх. В общих словах - если есть все, но нет имени, или оно короткое - идем наверх
        public bool up(Holding holding)
        {


            if ((holding._shares == 0 && holding.faceAmount == null && holding.principalAmount == null && holding.parvalue == null && holding.contracts == null && holding.notionalamount == null)
                || holding._value == 0 || upcounts == 100)
            {
                upcounts = 0;
                return false;
            }


            if (holding.industry != null && holding.name == null) return true;
            if (holding.maturityDate != null && holding.name == null) return true;
            if (holding.couponRate != null && holding.name == null) return true;
            if (holding.name != null)
            {
                if (tools.ClearStrName(holding.name).Length <= 3)
                {
                    return true;
                }
                else
                {
                    var re = new Regex(@"^[^(]+?\)[^)]*$");
                    if (re.IsMatch(holding.name))
                        return true;
                }

            }

            upcounts = 0;
            return false;
        }

        public bool exesString(string value)
        {
            value = ex.clear(value);
            var re = new Regex("^total");


            return ex.pure(value) || ex.exes.Contains(value) || ((re.IsMatch(value.ToLower()) && value.Length > 10));
        }
        public void renewDatacols(float index, string value)
        {
            if (tools.ClearSpaceCaption(value).Length < 20) return;

            foreach (KeyValuePair<string, Key> kvp in keys)
            {
                if (!dataCols.Contains(kvp.Value) && value.Contains(kvp.Value.value) && (kvp.Value.exclude == null || !value.Contains(kvp.Value.exclude)))
                {
                    kvp.Value.index = index;
                    renewDatacols(kvp.Value);
                }
            }
        }
        //* Добавляем в нужное место ключ
        public void renewDatacols(Key key, bool bp = false)
        {
            int index = -1;

            Key simKey = dataCols.Find(x => x.value == key.value);
            bool rem = dataCols.Remove(simKey);

            if (bp == false)
                index = dataCols.FindIndex(x => x.index >= key.index);
            else
            {
                if (key.group != 0)
                {
                    int groupIndex = dataCols.FindIndex(x => key.group == x.group);

                    if (groupIndex != -1)
                    {
                        dataCols[groupIndex] = key;
                        return;
                    }
                }


                index = dataCols.FindIndex(x => x.indexbp >= key.indexbp);
                key.index = key.indexbp;
            }

            //Console.WriteLine("INDEX {0}, key.index {1}, key.indexbp {2}", index, key.index, key.indexbp);

            if (index != -1)
            {
                if (key.index == dataCols[index].index)
                {
                    if (key.group != 0 && dataCols[index].group == key.group)
                    {
                        dataCols[index] = key;
                    }
                    else
                    {
                        if (key.indexbp > dataCols[index].indexbp)
                        {
                            if (index + 1 < dataCols.Count)
                            {
                                dataCols.Insert(index + 1, key);
                            }
                            else
                            {
                                dataCols.Add(key);
                            }
                        }

                        if (key.indexbp < dataCols[index].indexbp)
                        {
                            dataCols.Insert(index, key);
                        }

                        if (key.indexbp == dataCols[index].indexbp)
                        {
                            dataCols[index] = key;
                        }
                    }
                }
                else
                {
                    dataCols.Insert(index, key);
                }
            }
            else
            {
                dataCols.Add(key);
            }


            //printDataCols();
        }

        public Key closest(double Index, int[] groups)
        {
            List<string> vals = new List<string>();

            foreach (int group in groups)
            {
                List<Key> _keys = new List<Key>();

                foreach (KeyValuePair<string, Key> k in keys)
                {
                    if (k.Value.group == group) _keys.Add(k.Value);
                }

                foreach (Key key in _keys)
                {
                    vals.Add(key.value);
                }
            }

            return closest(Index, vals.ToArray());
        }
        //* Нахождение ближайшего ключа по его позиции в массиве
        public Key closest(double Index, string[] vals)
        {
            int countCols = dataCols.Count;

            if (!dataCols.Exists(x => x.value == "name"))
            {
                countCols++;
            }

            Index = Index * countCols - 1;

            string closer = vals[0];
            foreach (string val in vals)
            {
                int indexCloser = indexKey(closer);
                int indexVal = indexKey(val);

                if (indexCloser == -1)
                {
                    //Console.WriteLine("indexCloser == -1 {0}",closer);
                    closer = val;
                }

                else
                {
                    if (indexVal != -1)
                    {
                        if (Math.Abs(indexCloser - Index) > Math.Abs(indexVal - Index))
                            closer = val;
                    }
                }
            }

            if (indexKey(closer) > -1 && Math.Abs(indexKey(closer) - Index) < 1)
                return keys[closer];
            else
            {
                //Console.WriteLine("index {0} - {1}...", Index, vals[0]);
                // printDataCols();
                return null;
            }
        }

        public int indexKey(string value)
        {
            Key simKey = dataCols.Find(x => x.value == value);

            if (simKey == null) return -1;
            else
                return dataCols.IndexOf(simKey);
        }
    }
}
