using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoaderFundHolders
{
    public class Caption
    {
        public string name { get; set; }

        public bool first { get; set; }
        public string compressedname { get; set; }
        public string compressednamenozip { get; set; }
        public bool nozip { get; set; }
        public int izip { get; set; }
        public List<int> lines { get; set; }

        public Caption()
        {

        }

        public Caption(string _name, string _compressedname, bool _first)
        {
            name = _name;
            compressedname = _compressedname;
            compressednamenozip = _compressedname;
            nozip = false;
            first = _first;
            lines = new List<int>();
        }
        public void AddLine(int _lines)
        {
            if (!lines.Contains(_lines))
                lines.Add(_lines);
        }

        public void print()
        {
            Console.WriteLine(name);
            Console.WriteLine(compressedname);
            foreach (int line in lines)
            {
                Console.WriteLine(line);
            }
        }
    }
}
