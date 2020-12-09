using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace PlansParser
{
    // http://knab.ws/blog/index.php?/archives/10-CSV-file-parser-and-writer-in-C-Part-2.html

    public class CSVParser
    {
        public static char Delimiter = ',';
        public static DataTable CSVToDataTable(string FileName, bool headers, int RowsToParse, int HeaderLineNum)
        {
            FileStream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            return Parse(stream, headers, RowsToParse, HeaderLineNum);
        }
        public static DataTable Parse(Stream strm, bool headers, int RowsToParse, int HeaderLineNum)
        {
            int aHeaderLineNum = HeaderLineNum;
            DataTable table = new DataTable();

            TextReader stream = new StreamReader(strm);

            // Get Delimiter row
            String hdr = stream.ReadLine();

            // Skip lines before header
            while (aHeaderLineNum > 1)
            {
                hdr = stream.ReadLine();
                aHeaderLineNum--;
            }

            List<int> lst = new List<int>();
            Delimiter = ',';
            int commapos = hdr.IndexOf(','); if (commapos != -1) lst.Add(commapos);
            int semicolonpos = hdr.IndexOf(';'); if (semicolonpos != -1) lst.Add(semicolonpos);
            int fencepos = hdr.IndexOf('|'); if (fencepos != -1) lst.Add(fencepos);
            int tabpos = hdr.IndexOf('\t'); if (tabpos != -1) lst.Add(tabpos);

            // First found delimiter will be CSV delimiter
            if (lst.Count > 0) Delimiter = hdr[lst.Min()];


            //stream.Close();
            strm.Seek(0, SeekOrigin.Begin);
            stream = new StreamReader(strm);

            CsvStream csv = new CsvStream(stream, Delimiter);
            string[] row = csv.GetNextRow();

            // Skip lines before header
            aHeaderLineNum = HeaderLineNum;
            while (aHeaderLineNum > 1)
            {
                row = csv.GetNextRow();
                aHeaderLineNum--;
            }

            if (row == null)
                return null;
            if (headers)
            {
                foreach (string header in row)
                {
                    if (header != null && header.Length > 0 && !table.Columns.Contains(header))
                        table.Columns.Add(header.Trim(), typeof(string));
                    else
                        table.Columns.Add(GetNextColumnHeader(table), typeof(string));
                }
                row = csv.GetNextRow();
            }

            int counter = RowsToParse;
            while (row != null)
            {
                while (row.Length > table.Columns.Count)
                    table.Columns.Add(GetNextColumnHeader(table), typeof(string));
                table.Rows.Add(row);
                row = csv.GetNextRow();

                if (RowsToParse > 0)
                {
                    counter--;
                    if (counter == 0) break;
                }
            }

            stream.Close();

            return table;
        }

        private static string GetNextColumnHeader(DataTable table)
        {
            int c = 1;
            while (true)
            {
                string h = "Column" + c++;
                if (!table.Columns.Contains(h))
                    return h;
            }
        }


        private class CsvStream
        {
            private TextReader stream;
            private char Delim;

            public CsvStream(TextReader s, char Delimiter)
            {
                Delim = Delimiter;
                stream = s;
            }

            public string[] GetNextRow()
            {
                ArrayList row = new ArrayList();
                while (true)
                {
                    string item = GetNextItem();
                    if (item == null)
                        return row.Count == 0 ? null : (string[])row.ToArray(typeof(string));
                    row.Add(item);
                }
            }

            private bool EOS = false;
            private bool EOL = false;

            private string GetNextItem()
            {
                if (EOL)
                {
                    // previous item was last in line, start new line
                    EOL = false;
                    return null;
                }

                bool quoted = false;
                char quoteChar = '0';
                bool predata = true;
                bool postdata = false;
                StringBuilder item = new StringBuilder();

                while (true)
                {
                    char c = GetNextChar(true);
                    if (EOS)
                        return item.Length > 0 ? item.ToString() : null;

                    if ((postdata || !quoted) && c == Delim)
                        // end of item, return
                        return item.ToString();

                    if ((predata || postdata || !quoted) && (c == '\x0A' || c == '\x0D'))
                    {
                        // we are at the end of the line, eat newline characters and exit
                        EOL = true;
                        if (c == '\x0D' && GetNextChar(false) == '\x0A')
                            // new line sequence is 0D0A
                            GetNextChar(true);
                        return item.ToString();
                    }

                    if (predata && c == ' ')
                        // whitespace preceeding data, discard
                        continue;

                    if (predata && c == '"')
                    {
                        // quoted data is starting
                        quoted = true;
                        quoteChar = c;
                        predata = false;
                        continue;
                    }
                    if (predata && c == '\'')
                    {
                        // quoted data is starting
                        quoted = true;
                        quoteChar = c;
                        predata = false;
                        continue;
                    }
                    if (predata)
                    {
                        // data is starting without quotes
                        predata = false;
                        item.Append(c);
                        continue;
                    }

                    if (c == '"' && quoted && quoteChar == c)
                    {
                        if (GetNextChar(false) == '"')
                            // double quotes within quoted string means add a quote       
                            item.Append(GetNextChar(true));
                        else
                            // end-quote reached
                            postdata = true;
                        continue;
                    }
                    if (c == '\'' && quoted && quoteChar == c)
                    {
                        if (GetNextChar(false) == '\'')
                            // double quotes within quoted string means add a quote       
                            item.Append(GetNextChar(true));
                        else
                            // end-quote reached
                            postdata = true;
                        continue;
                    }
                    // all cases covered, character must be data
                    item.Append(c);
                }
            }

            private char[] buffer = new char[4096];
            private int pos = 0;
            private int length = 0;

            private char GetNextChar(bool eat)
            {
                if (pos >= length)
                {
                    length = stream.ReadBlock(buffer, 0, buffer.Length);
                    if (length == 0)
                    {
                        EOS = true;
                        return '\0';
                    }
                    pos = 0;
                }
                if (eat)
                    return buffer[pos++];
                else
                    return buffer[pos];
            }
        }
    }
}