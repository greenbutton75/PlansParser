using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace LoaderFundHolders
{
    public class Interval
    {
        public int offset = 20;
        public int from { get; set; }
        public int to { get; set; }
        public bool prtf { get; set; }
        public int prtfline { get; set; }
        public int appendix { get; set; }

        public int end = 0;

        public List<Table> Tables;
        public Dictionary<string, Holding> holdings = new Dictionary<string, Holding>();
        public Tools tools = new Tools();
        public Table lastHoldingTable;
        public Interval(int _from, int _to)
        {
            from = _from;
            to = _to;
        }
        public void print()
        {
            Console.WriteLine("from: {0}", from);
            Console.WriteLine("to: {0}", to);
            Console.WriteLine("prtf: {0}", prtf);
        }
        public void inInterval(List<int> list, string key)
        {
            int line = list.Find(x => from <= x + offset && to > x + offset);

            if (key == "prtf")
            {
                prtfline = line;

                if (prtfline != 0) prtf = true;

            }

            if (key == "appendix") appendix = line;
        }
        //* Находим html таблицы, которые лежат в интервале и создаем объекты Table
        public void FindTablesInInterval(HtmlNode node)
        {
            HtmlNodeCollection tables = node.SelectNodes(".//table");
            Tables = new List<Table>();

            if (tables != null)
            {
                int I = 0;
                foreach (HtmlNode table in tables)
                {
                    int _to = to;
                    if (appendix != 0 && appendix < to) _to = appendix;

                    if (table.Line >= Math.Max(prtfline, from) && table.Line < _to)
                    {
                        Table _table = new Table(table);
                        if (_table.trs != null)
                        {
                            Tables.Add(_table);

                            if (I > 0)
                            {
                                _table.prev = Tables[I - 1];
                                Tables[I - 1].next = _table;
                            }

                            I++;
                        }
                    }
                }
            }
        }

        public void Parse(HtmlNode body)
        {
            int tablesAfterH = 0;
            bool tablesAfterHInd = false;

            print();
            FindTablesInInterval(body);
            //* Основной вопрос в каждой итерации цикла - продолжается ли таблица. Пытаемся на него ответить с помощью всеких тестов и исловий.
            foreach (Table table in Tables)
            {
                if (tablesAfterH > 5 && tablesAfterHInd == false)
                {
                    end++;
                    tablesAfterHInd = true;
                }

                if (holdings.Count > 0 && tools.lettersBetweenTags(lastHoldingTable.node, table.node) > 1000)
                {
                    //Console.WriteLine("ENDLett {0}", tools.lettersBetweenTags(lastHoldingTable.node, table.node));
                    end = end + 2;
                }

                //Console.WriteLine("END {0}", end);

                if (end > 1) return;

                if (table.prev == null ||
                    table.prev.portfolio != true ||
                    table.prev.totalCaption == true ||
                    (table.prev.trs.Count > 10 && table.prev.holdings.Count == 0) ||
                    tools.lettersBetweenTags(table.prev.node, table.node) > 100)
                {
                    /*if (table.prev == null) Console.WriteLine("R 1");
                    else
                    {
                        if (table.prev.portfolio != true) Console.WriteLine("R 2");
                        if (table.prev.trs.Count > 10 && table.prev.holdings.Count == 0) Console.WriteLine("R 3");
                        if (tools.lettersBetweenTags(table.prev.node, table.node) > 100) Console.WriteLine("R 4");
                    }
                     if(table.prev != null) Console.WriteLine("TableAnal {0}", tools.lettersBetweenTags(table.prev.node, table.node ));*/


                    table.tAnalysis();
                }
                else
                {
                    table.continueTable(table.prev);
                }

                //WriteLine("Table {0} {1} : {2} - {3} % {4}", table.portfolio, table.node.Line, from, to, prtfline);

                if (table.portfolio)
                {
                    table.Parse();
                    if (table.holdings.Count > 0)
                    {
                        lastHoldingTable = table;
                        //* После парсинга таблицы добавляем найденные позиции в список позиций для интервала
                        tools.Union(holdings, table.holdings);
                    }
                    if (holdings.Count > 0)
                    {
                        if (table.totalCaption == true)
                        {
                            end++;
                        }
                    }
                }
                else
                {
                    if (holdings.Count > 0)
                    {
                        tablesAfterH++;
                    }
                }
            }
        }
    }
    public class IntervalComparer : IComparer<Interval>
    {
        public int Compare(Interval x, Interval y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // Если x == null и y == null, то они равны 
                    return 0;
                }
                else
                {
                    // Если x == null и y != null, то у больше 
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
                    if (x.prtf != y.prtf)
                    {
                        if (x.prtf == true && y.prtf == false) return -1;
                        else return 1;
                    }
                    else
                    {
                        int retval = x.from.CompareTo(y.from);
                        return retval;
                    }
                }
            }
        }
    }
}
