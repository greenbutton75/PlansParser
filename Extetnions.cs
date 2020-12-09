using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PlansParser
{
    public static class Extetnions
    {
        private static readonly Dictionary<string, Regex> Regexes = new Dictionary<string, Regex>();
        private static readonly object lockObject = new object();

        private static Regex GetRegexWithCache(string pattern, RegexOptions regexOptions)
        {
            Regex regex;
            var options = RegexOptions.Compiled;

            if (regexOptions == RegexOptions.IgnoreCase)
            {
                pattern += "(?i)";
                options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            }

            lock (lockObject)
            {
                if (!Regexes.TryGetValue(pattern, out regex))
                {
                    regex = new Regex(pattern, options);
                    Regexes.Add(pattern, regex);
                }
            }

            return regex;
        }
        public static bool DigitsOnly(this string s)
        {
            int len = s.Length;
            for (int i = 0; i < len; ++i)
            {
                char c = s[i];
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }


        public static string ReplaceWholeWord(this string original, string wordToFind, string replacement, RegexOptions regexOptions = RegexOptions.None)
        {
            string pattern = string.Format(@"\b{0}\b", wordToFind);
            return GetRegexWithCache(pattern, regexOptions).Replace(original, replacement);
        }

        public static bool ContainsWholeWord(this string original, string wordToFind, RegexOptions regexOptions = RegexOptions.None)
        {
            string pattern = string.Format(@"\b{0}\b", wordToFind);
            return GetRegexWithCache(pattern, regexOptions).IsMatch(original);
        }

        public static bool EndsWithsWholeWord(this string original, string wordToFind, RegexOptions regexOptions = RegexOptions.None)
        {
            string pattern = string.Format(@"\b{0}$", wordToFind);
            return GetRegexWithCache(pattern, regexOptions).IsMatch(original);

        }
        public static string Between(this string value, string a, string b)
        {
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        }
        public static string Before(this string value, string a)
        {
            int posA = value.IndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        }
        public static string After(this string value, string a)
        {
            int posA = value.LastIndexOf(a);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        }
        public static void AddIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key)) dict.Add(key, value);
        }
        public static void AddIfNotExist<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) list.Add(item);
        }
        public static void AddIfNotExist<T>(this HashSet<T> list, T item)
        {
            if (!list.Contains(item)) list.Add(item);
        }

        public static IEnumerable<IList<T>> Batch<T>(this IEnumerable<T> source, int size)
        {
            T[] bucket = null;
            var count = 0;
            size = size < 1 ? 1 : size;
            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new T[size];

                bucket[count++] = item;

                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
                yield return bucket.Take(count).ToList();
        }

        public static string Description<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : source.ToString();
        }

        public static void Invoke(this Form form, Action method)
        {
            form.Invoke(new MethodInvoker(delegate
             {
                 method();
             }));
        }

        public static string TakeFirstNLetters(this String str, int symbolsCount)
        {
            return new string(str.Take(symbolsCount).ToArray());
        }

        public static void WriteCSV(this DataTable dt, string filePath)
        {
            dt.WriteCSV(filePath, ";");
        }
        public static void WriteCSV(this DataTable dt, string filePath, string delimiter, bool withCaption = true)
        {
            // Unload DT -> CSV
            System.IO.File.WriteAllText(filePath, DataTableToCSVString(dt, delimiter, withCaption));
        }

        private static string DataTableToCSVString(DataTable dt, string delimiter, bool withCaption = true)
        {
            string res = "";
            // DT -> CSV
            if (dt != null)
            {
                StringBuilder sb = new StringBuilder();

                if (withCaption)
                {
                    string[] columnNames = dt.Columns.Cast<DataColumn>().
                                                      Select(column => column.ColumnName).
                                                      ToArray();
                    sb.AppendLine(string.Join(delimiter, columnNames));
                }

                foreach (DataRow row in dt.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => field.ToString()).
                                                    ToArray();
                    sb.AppendLine(string.Join(delimiter, fields));
                }

                res = sb.ToString();
            }
            return res;
        }
    }

}