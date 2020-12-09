using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OfficeOpenXml;

namespace PlansParser
{
    public class XlsxToCsvConverter
    {
        private readonly Form _fundsForm;
        private readonly TextBox _txtFunds;
        private readonly ToolStripProgressBar _prgLine;
        private readonly ToolStripStatusLabel _prgLabel;

        public XlsxToCsvConverter(Form fundsForm, TextBox txtFundsBox, ToolStripProgressBar progressBar, ToolStripStatusLabel prgLabel)
        {
            this._fundsForm = fundsForm;
            this._prgLabel = prgLabel;
            this._prgLine = progressBar;
            this._txtFunds = txtFundsBox;
        }

        public List<Thread> ReadPlanXlsxFromFolder(string folderWithPlans)
        {
            string[] files = Directory.GetFiles(folderWithPlans, "*.xlsx", SearchOption.TopDirectoryOnly);
            _prgLine.Value = 0;
            _prgLine.Maximum = files.Length;

            var threads = ThreadHelper.RunAsyncMultiThreadAction(ReadPlanXlsxFromFiles, files, Properties.Settings.Default.XlsToCsvConverThreadCount);

            return threads;
        }

        private void ReadPlanXlsxFromFiles(object obj)
        {
            var files = (IList<string>)obj;
            foreach (var file in files)
            {
                try
                {
                    _fundsForm.Invoke((MethodInvoker)delegate { _prgLabel.Text = Path.GetFileName(file); });
                    ReadPlanXLSXFromFile(file);
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }

            try
            {
                if (_prgLine.Value == _prgLine.Maximum)
                {
                    _fundsForm.BeginInvoke((MethodInvoker)delegate { _prgLabel.Text = "Ready"; });
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public static DataTable XlsxToDataTable(Stream stream, bool headers, int RowsToParse, int HeaderLineNum)
        {
            var returnDT = new DataTable();

            // Open and read the XLSX file.
            using (var package = new ExcelPackage(stream))
            {
                try
                {
                    var workBook = package.Workbook;
                    if (workBook != null)
                    {
                        if (workBook.Worksheets != null && workBook.Worksheets.Count > 0)
                        {
                            // Get the first worksheet
                            foreach (var sheet in workBook.Worksheets)
                            {
                                if (sheet.Hidden == eWorkSheetHidden.Visible)
                                {
                                    for (int colNumber = 1; colNumber <= sheet.Dimension.End.Column; colNumber++)
                                    {
                                        returnDT.Columns.Add("Col" + returnDT.Columns.Count);
                                    }

                                    // Read all contents
                                    for (int rowNumber = 1; rowNumber <= sheet.Dimension.End.Row; rowNumber++)
                                    {
                                        var row = returnDT.Rows.Add();
                                        for (int colNumber = 1; colNumber <= sheet.Dimension.End.Column; colNumber++)
                                        {
                                            try
                                            {
                                                object cellValue = sheet.Cells[rowNumber, colNumber].Value;
                                                if (cellValue != null)
                                                    row[colNumber - 1] = cellValue.ToString().Replace("\n", "").Replace("\r", "");
                                            }
                                            catch { }
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            if (headers)
            {
                foreach (DataColumn col in returnDT.Columns)
                {
                    if (!String.IsNullOrEmpty(returnDT.Rows[HeaderLineNum - 1][col.ColumnName].ToString()))
                    {
                        string newName = returnDT.Rows[HeaderLineNum - 1][col.ColumnName].ToString().Trim();
                        if (!returnDT.Columns.Contains(newName)) { col.ColumnName = newName; }
                    }
                }

                for (int i = 0; i < HeaderLineNum; i++)
                {
                    returnDT.Rows.RemoveAt(0);
                }
            }

            if (returnDT.Rows.Count > 0 && RowsToParse > 0)
            {
                while (returnDT.Rows.Count > RowsToParse)
                {
                    returnDT.Rows.RemoveAt(returnDT.Rows.Count - 1);
                }
            }

            return returnDT;
        }

        public void ReadPlanXLSXFromFile(string file)
        {
            DataTable dt;
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                dt = XlsxToDataTable(stream, false, 0, 0);
            DataTable dtFragment = new DataTable();
            DataTable dtResult = new DataTable();


            dtResult.Columns.Add("Name", typeof(String));
            dtResult.Columns.Add("Name2", typeof(String));
            dtResult.Columns.Add("Value", typeof(String));
            dtResult.Columns.Add("Year", typeof(String));
            dtResult.Columns.Add("ETF", typeof(String));
            dtResult.Columns.Add("Class", typeof(String));
            dtResult.Columns.Add("ClassPosition", typeof(String));


            bool isAllocAssetsType = false;
            bool TableStart = false;
            int legendChance = 0;
            bool LegendStart = false;
            Dictionary<string, string> legendMap = new Dictionary<string, string>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int valueHeaderCount = 0;

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string val = dt.Rows[i][j].ToString().Trim();
                    string valLow = val.ToLower();

                    // || valLow.Contains("(a)") || valLow.Contains("(e)")
                    var isBorrower = (valLow.Contains("borrower") &&
                                      (valLow.Length < 200 || (valLow.Contains("cost") || valLow.Contains("value"))));

                    if (valLow.Contains("fair value") || isBorrower || (valLow.Contains("identity") && valLow.Contains("of") && valLow.Contains("issue")) ||
                       (valLow.Contains("current") && valLow.Contains("value")) && valLow.Length < 400)
                    {
                        TableStart = true;
                        break; // skip line
                    }

                    if (valLow.Contains("ALLOCATION OF PLAN ASSETS".ToLower()))
                    {
                        isAllocAssetsType = true;
                    }

                    if (isAllocAssetsType && valLow.Contains("value"))
                    {
                        valueHeaderCount++;
                    }

                    if (valueHeaderCount == 3)
                    {
                        TableStart = true;
                    }

                    if (valLow == "INVESTMENT OPTION".ToLower())
                    {
                        legendChance++;
                    }

                    if (legendChance > 0 && valLow == "legend")
                    {
                        LegendStart = true;
                    }

                    if (valLow.Contains("total mutual") || valLow.Contains("total investments") ||
                        valLow.Contains("total assets") || valLow == "total" || i == dt.Rows.Count - 1)
                    {
                        TableStart = false;
                        break; // skip line
                    }

                    if (LegendStart)
                    {
                        string privRowValue = null;
                        for (int jj = 0; jj < dt.Columns.Count; jj++)
                        {
                            string col = dt.Rows[i][jj].ToString().Replace("*", "").Trim();
                            if (col != "")
                            {
                                if (privRowValue != null)
                                {
                                    legendMap[privRowValue.ToLower()] = col.ToLower();
                                    privRowValue = null;
                                    continue;
                                }

                                privRowValue = col;
                            }
                        }

                        break; // skip line
                    }


                    if (TableStart)
                    {
                        string line = "";
                        List<string> list = new List<string>();
                        for (int jj = 0; jj < dt.Columns.Count; jj++) // All columns after the first
                        {
                            string col = dt.Rows[i][jj].ToString().Replace("*", "").Trim();
                            if (col != "") list.Add(col);
                        }

                        List<string> itemarray = list.ToList();
                        if (list.Count < 1) continue;
                        if (list.Count == 1)
                        {
                            itemarray =
                                list[0].Split("  ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        if (itemarray.Count > 1)
                        {
                            if (dtFragment.Columns.Count < itemarray.Count)
                            {
                                var addColl = itemarray.Count - dtFragment.Columns.Count;
                                for (int ii = 0; ii < addColl; ii++)
                                {
                                    dtFragment.Columns.Add();
                                }
                            }

                            dtFragment.Rows.Add(itemarray.ToArray());
                        }

                        break; // skip line
                    }

                }
            }

            if (dtFragment.Rows.Count > 0)
            {
                ReadFragment(dtFragment, legendMap, ref dtResult);
            }

            if (dtResult.Rows.Count > 0)
            {
                var fileInfo = new FileInfo(file.Replace(".xlsx", ".csv").Replace(".XLSX", ".csv"));
                var resultDir = Constants.CsvFolder;

                if (!Directory.Exists(resultDir))
                {
                    Directory.CreateDirectory(resultDir);
                }

                var filePath = Path.Combine(resultDir, fileInfo.Name);

                dtResult.WriteCSV(filePath, "|");

                _fundsForm.Invoke(() => _prgLine.Increment(1));
            }
            else
            {
                _fundsForm.BeginInvoke((MethodInvoker)delegate { _txtFunds.AppendText(file + "\r\n"); });

                try
                {
                    var fileInfo = new FileInfo(file);
                    var problemDir = Constants.ProblemXlsDir;

                    if (!Directory.Exists(problemDir))
                        Directory.CreateDirectory(problemDir);
                    var pdfProblemFolder = Path.Combine(problemDir, "pdf");
                    if (!Directory.Exists(pdfProblemFolder))
                        Directory.CreateDirectory(pdfProblemFolder);

                    File.Copy(
                        file,
                        Path.Combine(problemDir, fileInfo.Name),
                        true);

                    File.Copy(
                        Path.Combine(Constants.PdfFolder, fileInfo.Name.Replace(".xlsx", ".pdf").Replace(".XLSX", ".pdf")),
                        Path.Combine(pdfProblemFolder, fileInfo.Name.Replace(".xlsx", ".pdf").Replace(".XLSX", ".pdf")),
                        true);
                }
                catch (Exception ex)
                {
                    Logger.Log("Info: " + ex);
                }
            }
        }

        private void ReadFragment(DataTable dtFragment, Dictionary<string, string> legendMap, ref DataTable dtResult)
        {
            var digitParrern = @"[0-9]*[.,]*[0-9]*\s*";
            Regex rgx = new Regex(digitParrern + "shares of", RegexOptions.Compiled);
            Regex rgx1 = new Regex(digitParrern + "shares", RegexOptions.Compiled);
            Regex rgx2 = new Regex(@"[ ]{1,}", RegexOptions.Compiled);
            Regex rgx3 = new Regex(@"\b\d{4}\b", RegexOptions.Compiled);
            Regex rgx4 = new Regex(digitParrern + "units of", RegexOptions.Compiled);
            Regex rgx5 = new Regex(digitParrern + "units", RegexOptions.Compiled);

            for (int i = 0; i < dtFragment.Rows.Count; i++)
            {
                string iName1 = "";
                string iName2 = "";
                string iYear = "";
                double iValue = double.MinValue;

                for (int j = 0; j < dtFragment.Columns.Count; j++)
                {
                    string val = dtFragment.Rows[i][j].ToString().Replace("*", "").Trim().Replace("\n", " ");
                    if (string.IsNullOrEmpty(val)) continue;
                    double tmp_iValue;
                    string doubleVal = val.Replace("$", "").Replace(",", "").Replace(".", "").Replace("*", "").Replace("-", "").Replace(" ", "").Trim();
                    if (doubleVal.DigitsOnly() && !doubleVal.StartsWith("0"))
                    {
                        doubleVal = val.Replace("$", "").Replace("*", "").Replace("-", "").Replace(" ", "").Trim();
                        if (doubleVal.Contains(".") && !doubleVal.Contains(",") && doubleVal.Length > (doubleVal.LastIndexOf(".") + 3)) doubleVal = doubleVal.Replace(".", ",");
                        if (doubleVal.Contains(".")) doubleVal = doubleVal.Replace(",", "");
                        else if (doubleVal.Contains(","))
                        {
                            int delim_last = doubleVal.LastIndexOf(",");
                            if (delim_last + 3 < doubleVal.Length)
                            {
                                doubleVal = doubleVal.Substring(0, delim_last + 4) + "." + doubleVal.Substring(delim_last + 4);
                                doubleVal = doubleVal.Replace(",", "");
                                doubleVal = doubleVal.TrimEnd('.');
                            }
                        }

                        if (double.TryParse(doubleVal, out tmp_iValue) && tmp_iValue != 0) { iValue = tmp_iValue; continue; }
                    }

                    if (val.Length < 4) continue;
                    /*-------------------------------------------------------------------*/
                    if (iName1 == "")
                    {
                        iName1 = val;
                        if (rgx3.IsMatch(iName1))
                        {
                            iYear = rgx3.Match(iName1).Value.Trim();
                        }

                        continue;
                    }

                    if (iName2 == "")
                    {
                        iName2 = val;
                        if (rgx3.IsMatch(iName2))
                        {
                            iYear = rgx3.Match(iName2).Value.Trim();
                        }

                        continue;
                    }
                    /*-------------------------------------------------------------------*/
                }

                if (iValue == double.MinValue || iName1 == "") continue;

                iName1 = iName1.ToLower().Trim();
                iName2 = iName2.ToLower().Trim();

                if (legendMap.Count > 0)
                {

                    if (string.IsNullOrEmpty(iName2) && !string.IsNullOrEmpty(iName1))
                    {
                        iName2 = legendMap.TryGetValue(iName1, out iName2) ? iName2 : "";
                    }

                    if (string.IsNullOrEmpty(iName1) && !string.IsNullOrEmpty(iName2))
                    {
                        iName1 = legendMap.TryGetValue(iName2, out iName1) ? iName1 : "";
                    }

                    var tmp = iName1.TrimStart('1');
                    iName1 = iName2;
                    iName2 = tmp;
                }
                /*-------------------------------------------------------------------*/
                #region IGNORE

                if (iName1.Contains("corporate stock")) continue;
                if (iName2.Contains("corporate stock")) continue;

                if (iName1.Contains("common stock")) continue;
                if (iName2.Contains("common stock")) continue;

                if (iName1.Contains("corporate security")) continue;
                if (iName2.Contains("corporate security")) continue;

                /*-------------------------------------------------------------------*/
                iName1 = iName1.Replace("|", " ");
                iName2 = iName2.Replace("|", " ");

                if (iName1.Replace(" ", "") == "registeredinvestmentcompany") iName1 = "";
                if (iName2.Replace(" ", "") == "registeredinvestmentcompany") iName2 = "";

                if (iName1.Replace(" ", "") == "mutualfund") iName1 = "";
                if (iName2.Replace(" ", "") == "mutualfund") iName2 = "";

                if (iName1.Replace(" ", "") == "investment") iName1 = "";
                if (iName2.Replace(" ", "") == "investment") iName2 = "";

                iName1 = iName1.Replace("fidelity investments", "");
                iName2 = iName2.Replace("fidelity investments", "");

                iName1 = iName1.Replace("john hancock, usa", "");
                iName2 = iName2.Replace("john hancock, usa", "");

                iName1 = iName1.Replace("lincoln national life insurance company", "");
                iName2 = iName2.Replace("lincoln national life insurance company", "");

                iName1 = iName1.Replace("transamerica financial life insurance company", "");
                iName2 = iName2.Replace("transamerica financial life insurance company", "");

                iName1 = iName1.Replace("wells fargo, n.a.", "");
                iName2 = iName2.Replace("wells fargo, n.a.", "");

                iName1 = iName1.Replace("fidelity investments", "");
                iName2 = iName2.Replace("fidelity investments", "");

                iName1 = iName1.Replace("wilmington trust", "");
                iName2 = iName2.Replace("wilmington trust", "");

                iName1 = iName1.Replace("principal life insurance company", "");
                iName2 = iName2.Replace("principal life insurance company", "");

                iName1 = iName1.Replace("ing life insurance and annuity co", "");
                iName2 = iName2.Replace("ing life insurance and annuity co", "");

                iName1 = iName1.Replace("john hancock usa", "");
                iName2 = iName2.Replace("john hancock usa", "");

                iName1 = iName1.Replace("voya retirement insurance and annuity company", "");
                iName2 = iName2.Replace("voya retirement insurance and annuity company", "");

                iName1 = iName1.Replace("woodt rust bank n.a.", "");
                iName2 = iName2.Replace("woodt rust bank n.a.", "");

                iName1 = iName1.Replace("fidelity funds", "");
                iName2 = iName2.Replace("fidelity funds", "");

                iName1 = iName1.Replace("mid-atlantic trust company", "");
                iName2 = iName2.Replace("mid-atlantic trust company", "");

                iName1 = iName1.Replace("mn life insurance co.", "");
                iName2 = iName2.Replace("mn life insurance co.", "");

                iName1 = iName1.Replace("td ameritrade trust company", "");
                iName2 = iName2.Replace("td ameritrade trust company", "");

                iName1 = iName1.Replace("john hancock life ins co", "");
                iName2 = iName2.Replace("john hancock life ins co", "");

                iName1 = iName1.Replace("john hancock life ins c", "");
                iName2 = iName2.Replace("john hancock life ins c", "");

                iName1 = iName1.Replace("great-west life & annuity co.", "");
                iName2 = iName2.Replace("great-west life & annuity co.", "");

                iName1 = iName1.Replace("john hancock life insurance company", "");
                iName2 = iName2.Replace("john hancock life insurance company", "");

                iName1 = iName1.Replace("john hancock insurance co", "");
                iName2 = iName2.Replace("john hancock insurance co", "");

                //iName1 = iName1.Replace("common/collective trust", "");
                //iName2 = iName2.Replace("common/collective trust", "");

                iName1 = iName1.Replace("nationwide life insurance co.", "");
                iName2 = iName2.Replace("nationwide life insurance co.", "");

                iName1 = iName1.Replace("nationwide life insurance", "");
                iName2 = iName2.Replace("nationwide life insurance", "");

                iName1 = iName1.Replace("john hancock life insurance company (u.s.a.)", "");
                iName2 = iName2.Replace("john hancock life insurance company (u.s.a.)", "");

                iName1 = iName1.Replace("wells fargo, n.a.", "");
                iName2 = iName2.Replace("wells fargo, n.a.", "");

                iName1 = iName1.Replace("registered", "");
                iName2 = iName2.Replace("registered", "");

                iName1 = iName1.Replace("value of int in regist invest co", "");
                iName2 = iName2.Replace("value of int in regist invest co", "");

                iName1 = iName1.Replace("voya retirement insurance & annuit", "");
                iName2 = iName2.Replace("voya retirement insurance & annuit", "");

                iName1 = iName1.Replace("voya retirement insurance and annuitv co.", "");
                iName2 = iName2.Replace("voya retirement insurance and annuitv co.", "");

                iName1 = iName1.Replace("voya retirement ins & annuity co", "");
                iName2 = iName2.Replace("voya retirement ins & annuity co", "");

                iName1 = iName1.Replace("voya retirement", "");
                iName2 = iName2.Replace("voya retirement", "");

                iName1 = iName1.Replace("value of int. in reg. invest co.", "");
                iName2 = iName2.Replace("value of int. in reg. invest co.", "");

                iName1 = iName1.Replace("value of int. in ins co. general acct.", "");
                iName2 = iName2.Replace("value of int. in ins co. general acct.", "");

                iName1 = iName1.Replace("registered investment company", "");
                iName2 = iName2.Replace("registered investment company", "");

                iName1 = iName1.Replace("insurance company general", "");
                iName2 = iName2.Replace("insurance company general", "");

                iName1 = iName1.Replace("pooled separate accounts", "");
                iName2 = iName2.Replace("pooled separate accounts", "");

                iName1 = iName1.Replace("life insurance company", "");
                iName2 = iName2.Replace("life insurance company", "");

                iName1 = iName1.Replace("common and preferred stock", "");
                iName1 = iName1.Replace("other investments", "");
                iName1 = iName1.Replace("various rates and maturities", "");
                iName1 = iName1.Replace("common equity securities", "");
                iName1 = iName1.Replace("exchange traded", "");
                iName1 = iName1.Replace("guaranteed interest", "");
                iName1 = iName1.Replace("interest in investment companies", "");
                iName1 = iName1.Replace("guaranteed interest account", "");
                iName1 = iName1.Replace("investment company", "");
                iName1 = iName1.Replace("investment co", "");
                //iName1 = iName1.Replace("collective trust", "");
                iName1 = iName1.Replace("schedule of assets (held at end of year)", "");
                iName1 = iName1.Replace("investment companies", "");
                //iName1 = iName1.Replace("common collective trust", "");
                iName2 = iName2.Replace("page 2 of 2", "");


                iName2 = iName2.Replace("transamerica life insurance company", "");
                iName2 = iName2.Replace("nationwide life insurance", "");
                iName2 = iName2.Replace("john hancock life insurance co", "");
                iName2 = iName2.Replace("sentry life insurance company", "");
                iName2 = iName2.Replace("nationwide life insurance company", "");
                iName2 = iName2.Replace("hartford life insurance company", "");
                iName2 = iName2.Replace("john hancock life insurance", "");
                iName2 = iName2.Replace("john hancock life insurance co", "");
                iName2 = iName2.Replace("lincoln national life insurance company", "");
                iName2 = iName2.Replace("transamerica life insurance co", "");
                iName2 = iName2.Replace("american united life ins co", "");
                iName2 = iName2.Replace("transamerica life insurance co", "");
                iName2 = iName2.Replace("lincoln national life insurance co", "");
                iName2 = iName2.Replace("massachusetts mutual life insurance company", "");
                iName2 = iName2.Replace("nationwide life ins co", "");
                iName2 = iName2.Replace("variable annuity life insurance company", "");
                iName2 = iName2.Replace("hartford life insurance co", "");
                iName2 = iName2.Replace("transamerica financial life ins co", "");
                iName2 = iName2.Replace("john hancock life insurance co (usa)", "");
                iName2 = iName2.Replace("voya life insurance and annuity company", "");
                iName2 = iName2.Replace("mass mutual life insurance company", "");
                iName2 = iName2.Replace("mutual of america life insurance company", "");
                iName2 = iName2.Replace("massmutual life insurance company", "");
                iName2 = iName2.Replace("hartford life insurance co", "");
                iName2 = iName2.Replace("john hancock life ins co", "");
                iName2 = iName2.Replace("nationwide life insurance co", "");
                iName2 = iName2.Replace("massachusetts mutual life insurance co", "");
                iName2 = iName2.Replace("transamerica life insurance", "");
                iName2 = iName2.Replace("lincoln national life ins co", "");
                iName2 = iName2.Replace("ameritas life insurance corporate ", "");
                iName2 = iName2.Replace("the variable annuity life insurance company", "");
                iName2 = iName2.Replace("principal life insurance co", "");
                iName2 = iName2.Replace("principal life insurance co", "");
                iName2 = iName2.Replace("metropolitan life insurance company", "");
                iName2 = iName2.Replace("cmfg life insurance company", "");
                iName2 = iName2.Replace("voya life insurance & annuity co", "");
                iName2 = iName2.Replace("transamerica life ins co", "");
                iName2 = iName2.Replace("minnesota life insurance company", "");
                iName2 = iName2.Replace("life insurance company", "");
                iName2 = iName2.Replace("nationwide life ins co", "");
                iName2 = iName2.Replace("john hancock life insurance co (usa )", "");
                iName2 = iName2.Replace("variable annuity life insurance co", "");
                iName2 = iName2.Replace("john hancock life ins company", "");
                iName2 = iName2.Replace("john hancock life ins co usa", "");
                iName2 = iName2.Replace("the lincoln national life insurance co", "");
                iName2 = iName2.Replace("metlife insurance company", "");
                iName2 = iName2.Replace("john hancock life ins", "");
                iName2 = iName2.Replace("transamerica financial life ins co", "");
                iName2 = iName2.Replace("axa equitable life insurance co", "");
                iName2 = iName2.Replace("variable annuity life insurance co", "");
                iName2 = iName2.Replace("john hancocklife insurance company (usa )", "");
                iName2 = iName2.Replace("sentry life insurance company of new york", "");
                iName2 = iName2.Replace("lincoln national life insurance", "");
                iName2 = iName2.Replace("voya life insurance & annuity company", "");
                iName2 = iName2.Replace("john hancock life insurance (usa )", "");
                iName2 = iName2.Replace("the nationwide life insurance company", "");
                iName2 = iName2.Replace("variable annuity life ins co", "");
                iName2 = iName2.Replace("hartford life insurance", "");
                iName2 = iName2.Replace("john hancocklife insurance company", "");
                iName2 = iName2.Replace(" john hancock life insurance co", "");
                iName2 = iName2.Replace("united of omaha life insurance company", "");
                iName2 = iName2.Replace("transamerica life ins co", "");
                iName2 = iName2.Replace("mutual of america life insurance co", "");
                iName2 = iName2.Replace("john hankcock life insurance company (usa )", "");
                iName2 = iName2.Replace("transamerica financial life insurance co", "");
                iName2 = iName2.Replace("american united life insurance co", "");
                iName2 = iName2.Replace("mass mutual life insurance co", "");
                iName2 = iName2.Replace(" transamerica life insurance co", "");
                iName2 = iName2.Replace("the hartford life insurance company", "");
                iName2 = iName2.Replace("variable life insurance company", "");
                iName2 = iName2.Replace("metropolitan life insurance co", "");
                iName2 = iName2.Replace("lincoln national life insurance co", "");
                iName2 = iName2.Replace("the lincoln national life insurance company", "");
                iName2 = iName2.Replace("john hancock life ins c (usa )", "");
                iName2 = iName2.Replace("minnesota life insurance co", "");
                iName2 = iName2.Replace("transamerica financial life ins", "");
                iName2 = iName2.Replace("massmutual life ins", "");
                iName2 = iName2.Replace("american united life insurance company", "");
                iName2 = iName2.Replace("voya life ins & annuity", "");
                iName2 = iName2.Replace("great west life ins co", "");
                iName2 = iName2.Replace("voya life insurance & annuity co", "");
                iName2 = iName2.Replace("lincoln life insurance", "");
                iName2 = iName2.Replace("nationwide life insurance cmpy", "");
                iName2 = iName2.Replace("great-west life insurance co", "");
                iName2 = iName2.Replace("john hancock life insurance co (usa)", "");
                iName2 = iName2.Replace("transmerica financial life insurance company", "");
                iName2 = iName2.Replace("transamerica financial life insurance co", "");
                iName2 = iName2.Replace("the hartford life insurance co", "");
                iName2 = iName2.Replace("great west life ins & annuity", "");
                iName2 = iName2.Replace("voya life insurance company", "");
                iName2 = iName2.Replace("hartford life insurace company", "");
                iName2 = iName2.Replace("john hancock life insurance co usa", "");
                iName2 = iName2.Replace("transamerica financial life ins co", "");
                iName2 = iName2.Replace("transamerica life insurance company - sub-accounts", "");
                iName2 = iName2.Replace("hartford life ins ", "");
                iName2 = iName2.Replace("ameritas life insurance corporate", "");
                iName2 = iName2.Replace("great-west life ins", "");
                iName2 = iName2.Replace("massmutual life ins company", "");
                iName2 = iName2.Replace("voya life insurance & co", "");
                iName2 = iName2.Replace("massmutual life insurance co", "");
                iName2 = iName2.Replace("massmutual life insurance co", "");
                iName2 = iName2.Replace("massachusetts mutual life ins co", "");
                iName2 = iName2.Replace("transamerica financial life insuran", "");
                iName2 = iName2.Replace("transamerica life insurance comp", "");
                iName2 = iName2.Replace("great-west life insurance company", "");
                iName2 = iName2.Replace("american united life insurance company", "");
                iName2 = iName2.Replace("metlife insurance company - separate account", "");
                iName2 = iName2.Replace("mass mutual life ins co", "");
                iName2 = iName2.Replace("voya life ins & annuity co", "");
                iName2 = iName2.Replace("company massachusetts mutual life insurance", "");
                iName2 = iName2.Replace("united of omaha life ins co", "");
                iName2 = iName2.Replace("hartford life ins co", "");
                iName2 = iName2.Replace("jackson national life insurance company", "");
                iName2 = iName2.Replace("hardford life insurance co", "");
                iName2 = iName2.Replace("transamerica financial life insurance com", "");
                iName2 = iName2.Replace("lincoln nat'l life insurance co", "");
                iName2 = iName2.Replace("principal life ins co", "");
                iName2 = iName2.Replace("mass mutual life ins company", "");
                iName2 = iName2.Replace("john hancock life ins co", "");
                iName2 = iName2.Replace("voya life insurance", "");
                iName2 = iName2.Replace("principal life ins company", "");
                iName2 = iName2.Replace("massachusetts mutual life insurance", "");
                iName2 = iName2.Replace("metlife insurance company of connecticut", "");
                iName2 = iName2.Replace("john hancock life insurance co (usa)", "");
                iName2 = iName2.Replace("voya life insurance and annuity company", "");
                iName2 = iName2.Replace("lincoln national life insurance co", "");
                iName2 = iName2.Replace("american united life ins co", "");
                iName2 = iName2.Replace("hartford life insurance compa", "");
                iName2 = iName2.Replace("life insurance policy", "");
                iName2 = iName2.Replace("mass mutual life ins company", "");
                iName2 = iName2.Replace("pacific life insurance co", "");
                iName2 = iName2.Replace("principal life insurance c", "");
                iName2 = iName2.Replace("voya life insurance & annuity", "");
                iName2 = iName2.Replace("minnesota life ins co", "");
                iName2 = iName2.Replace("nationwide life insurance company", "");
                iName2 = iName2.Replace("voya financial life insurance and annuity", "");
                iName2 = iName2.Replace("john hancock life ins co of new york", "");
                iName2 = iName2.Replace("lincoln financial life insurance co", "");
                iName2 = iName2.Replace("voya life insurance and annuity co", "");
                iName2 = iName2.Replace("transamerica life insurance company", "");
                iName2 = iName2.Replace("transamerica financial life insurance", "");
                iName2 = iName2.Replace("john hancock life insu rance com pany", "");

                iName1 = iName1.Replace("life insurance co", "");
                iName2 = iName2.Replace("life insurance co", "");

                iName1 = iName1.Replace("life ins co", "");
                iName2 = iName2.Replace("life ins co", "");

                iName1 = iName1.Replace("life insurance", "");
                iName2 = iName2.Replace("life insurance", "");

                #endregion
                /*-------------------------------------------------------------------*/
                if (iName2.Replace(" ", "").StartsWith(iName1.Replace(" ", ""))) iName1 = "";
                if (iName1.Replace(" ", "").StartsWith(iName2.Replace(" ", ""))) iName2 = "";
                /*-------------------------------------------------------------------*/
                if (iName1.Replace("%", "").Replace("$", "").Replace(",", "").Replace(".", "").Replace("*", "").Replace(" ", "").Trim().DigitsOnly()) iName1 = "";
                if (iName2.Replace("%", "").Replace("$", "").Replace(",", "").Replace(".", "").Replace("*", "").Replace(" ", "").Trim().DigitsOnly()) iName2 = "";
                /*-------------------------------------------------------------------*/
                iName1 = rgx.Replace(iName1, @"");
                iName2 = rgx.Replace(iName2, @"");
                iName1 = rgx1.Replace(iName1, @"");
                iName2 = rgx1.Replace(iName2, @"");
                iName1 = rgx4.Replace(iName1, @"");
                iName2 = rgx4.Replace(iName2, @"");
                iName1 = rgx5.Replace(iName1, @"");
                iName2 = rgx5.Replace(iName2, @"");
                iName1 = rgx2.Replace(iName1, @" ");
                iName2 = rgx2.Replace(iName2, @" ");
                /*-------------------------------------------------------------------*/
                if (iName1.Trim() == "" && iName2.Trim() == "") continue;
                if (!(iName1.Trim() + iName2.Trim()).Contains(" ")) continue;
                /*-------------------------------------------------------------------*/
                string iETF = "";
                if ((iName1 + iName2).Contains("etf")) iETF = "ETF";
                /*-------------------------------------------------------------------*/
                iName1 = NormazileFundName(iName1, true).Trim();
                iName2 = NormazileFundName(iName2, true).Trim();
                /*-------------------------------------------------------------------*/
                string iClass = "";
                int iClassPosition = 0;
                if ((iName1 + " " + iName2).ContainsWholeWord("admin")) { iClass = "admin"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("admiral")) { iClass = "admiral"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("individual")) { iClass = "individual"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("institutional")) { iClass = "institutional"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("r1")) { iClass = "r1"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("r2")) { iClass = "r2"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("r3")) { iClass = "r3"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("r4")) { iClass = "r4"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("r5")) { iClass = "r5"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("retail")) { iClass = "retail"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("retl")) { iClass = "retl"; iClassPosition = 0; }
                if ((iName1 + " " + iName2).ContainsWholeWord("r-inst")) { iClass = "r-inst"; iClassPosition = 0; }


                if (iName1.EndsWithsWholeWord("4") || iName2.EndsWithsWholeWord("4")) { iClass = "4"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("5") || iName2.EndsWithsWholeWord("5")) { iClass = "5"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("a") || iName2.EndsWithsWholeWord("a")) { iClass = "a"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("aaa") || iName2.EndsWithsWholeWord("aaa")) { iClass = "aaa"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("advantage") || iName2.EndsWithsWholeWord("advantage")) { iClass = "advantage"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("advntg") || iName2.EndsWithsWholeWord("advntg")) { iClass = "advntg"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("advisory") || iName2.EndsWithsWholeWord("advisory")) { iClass = "advisory"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("b") || iName2.EndsWithsWholeWord("b")) { iClass = "b"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("b3") || iName2.EndsWithsWholeWord("b3")) { iClass = "b3"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("c") || iName2.EndsWithsWholeWord("c")) { iClass = "c"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("c1") || iName2.EndsWithsWholeWord("c1")) { iClass = "c1"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("cap") || iName2.EndsWithsWholeWord("cap")) { iClass = "cap"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("cii") || iName2.EndsWithsWholeWord("cii")) { iClass = "cii"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("class i") || iName2.EndsWithsWholeWord("class i")) { iClass = "class i"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("class ii") || iName2.EndsWithsWholeWord("class ii")) { iClass = "class ii"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("class iii") || iName2.EndsWithsWholeWord("class iii")) { iClass = "class iii"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("common") || iName2.EndsWithsWholeWord("common")) { iClass = "common"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("cons") || iName2.EndsWithsWholeWord("cons")) { iClass = "cons"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("d") || iName2.EndsWithsWholeWord("d")) { iClass = "d"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("e") || iName2.EndsWithsWholeWord("e")) { iClass = "e"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("f") || iName2.EndsWithsWholeWord("f")) { iClass = "f"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("fi") || iName2.EndsWithsWholeWord("fi")) { iClass = "fi"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("fid") || iName2.EndsWithsWholeWord("fid")) { iClass = "fid"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("fl") || iName2.EndsWithsWholeWord("fl")) { iClass = "fl"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("growth") || iName2.EndsWithsWholeWord("growth")) { iClass = "growth"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("h") || iName2.EndsWithsWholeWord("h")) { iClass = "h"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i") || iName2.EndsWithsWholeWord("i")) { iClass = "i"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-0") || iName2.EndsWithsWholeWord("i-0")) { iClass = "i-0"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i1") || iName2.EndsWithsWholeWord("i1")) { iClass = "i1"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-10") || iName2.EndsWithsWholeWord("i-10")) { iClass = "i-10"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-15") || iName2.EndsWithsWholeWord("i-15")) { iClass = "i-15"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i2") || iName2.EndsWithsWholeWord("i2")) { iClass = "i2"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-2") || iName2.EndsWithsWholeWord("i-2")) { iClass = "i-2"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-25") || iName2.EndsWithsWholeWord("i-25")) { iClass = "i-25"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i3") || iName2.EndsWithsWholeWord("i3")) { iClass = "i3"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-30") || iName2.EndsWithsWholeWord("i-30")) { iClass = "i-30"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-35") || iName2.EndsWithsWholeWord("i-35")) { iClass = "i-35"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-5") || iName2.EndsWithsWholeWord("i-5")) { iClass = "i-5"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("i-50") || iName2.EndsWithsWholeWord("i-50")) { iClass = "i-50"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ia") || iName2.EndsWithsWholeWord("ia")) { iClass = "ia"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ib") || iName2.EndsWithsWholeWord("ib")) { iClass = "ib"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii") || iName2.EndsWithsWholeWord("ii")) { iClass = "ii"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii-10") || iName2.EndsWithsWholeWord("ii-10")) { iClass = "ii-10"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii-15") || iName2.EndsWithsWholeWord("ii-15")) { iClass = "ii-15"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii-25") || iName2.EndsWithsWholeWord("ii-25")) { iClass = "ii-25"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii-30") || iName2.EndsWithsWholeWord("ii-30")) { iClass = "ii-30"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii-35") || iName2.EndsWithsWholeWord("ii-35")) { iClass = "ii-35"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii-5") || iName2.EndsWithsWholeWord("ii-5")) { iClass = "ii-5"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ii-50") || iName2.EndsWithsWholeWord("ii-50")) { iClass = "ii-50"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii") || iName2.EndsWithsWholeWord("iii")) { iClass = "iii"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii-10") || iName2.EndsWithsWholeWord("iii-10")) { iClass = "iii-10"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii-15") || iName2.EndsWithsWholeWord("iii-15")) { iClass = "iii-15"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii-25") || iName2.EndsWithsWholeWord("iii-25")) { iClass = "iii-25"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii-30") || iName2.EndsWithsWholeWord("iii-30")) { iClass = "iii-30"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii-35") || iName2.EndsWithsWholeWord("iii-35")) { iClass = "iii-35"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii-5") || iName2.EndsWithsWholeWord("iii-5")) { iClass = "iii-5"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iii-50") || iName2.EndsWithsWholeWord("iii-50")) { iClass = "iii-50"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("institutional plus") || iName2.EndsWithsWholeWord("institutional plus")) { iClass = "institutional plus"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("international") || iName2.EndsWithsWholeWord("international")) { iClass = "international"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("inv") || iName2.EndsWithsWholeWord("inv")) { iClass = "inv"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("investment") || iName2.EndsWithsWholeWord("investment")) { iClass = "investment"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("investor") || iName2.EndsWithsWholeWord("investor")) { iClass = "investor"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ip") || iName2.EndsWithsWholeWord("ip")) { iClass = "ip"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ir") || iName2.EndsWithsWholeWord("ir")) { iClass = "ir"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("is") || iName2.EndsWithsWholeWord("is")) { iClass = "is"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("iss") || iName2.EndsWithsWholeWord("iss")) { iClass = "iss"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("j") || iName2.EndsWithsWholeWord("j")) { iClass = "j"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("k") || iName2.EndsWithsWholeWord("k")) { iClass = "k"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("l") || iName2.EndsWithsWholeWord("l")) { iClass = "l"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("m") || iName2.EndsWithsWholeWord("m")) { iClass = "m"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("mgmt") || iName2.EndsWithsWholeWord("mgmt")) { iClass = "mgmt"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("management") || iName2.EndsWithsWholeWord("management")) { iClass = "management"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("morgan") || iName2.EndsWithsWholeWord("morgan")) { iClass = "morgan"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("n") || iName2.EndsWithsWholeWord("n")) { iClass = "n"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("o") || iName2.EndsWithsWholeWord("o")) { iClass = "o"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("open") || iName2.EndsWithsWholeWord("open")) { iClass = "open"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("p") || iName2.EndsWithsWholeWord("p")) { iClass = "p"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("pfd") || iName2.EndsWithsWholeWord("pfd")) { iClass = "pfd"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ppt") || iName2.EndsWithsWholeWord("ppt")) { iClass = "ppt"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("prem") || iName2.EndsWithsWholeWord("prem")) { iClass = "prem"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("premier") || iName2.EndsWithsWholeWord("premier")) { iClass = "premier"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("premium") || iName2.EndsWithsWholeWord("premium")) { iClass = "premium"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("q") || iName2.EndsWithsWholeWord("q")) { iClass = "q"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("r") || iName2.EndsWithsWholeWord("r")) { iClass = "r"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("r6") || iName2.EndsWithsWholeWord("r6")) { iClass = "r6"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("rsv") || iName2.EndsWithsWholeWord("rsv")) { iClass = "rsv"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("rtl") || iName2.EndsWithsWholeWord("rtl")) { iClass = "rtl"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("s") || iName2.EndsWithsWholeWord("s")) { iClass = "s"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("sel") || iName2.EndsWithsWholeWord("sel")) { iClass = "sel"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("select") || iName2.EndsWithsWholeWord("select")) { iClass = "select"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("service") || iName2.EndsWithsWholeWord("service")) { iClass = "service"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("ss") || iName2.EndsWithsWholeWord("ss")) { iClass = "ss"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("std") || iName2.EndsWithsWholeWord("std")) { iClass = "std"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("t") || iName2.EndsWithsWholeWord("t")) { iClass = "t"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("tr") || iName2.EndsWithsWholeWord("tr")) { iClass = "tr"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("u") || iName2.EndsWithsWholeWord("u")) { iClass = "u"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("w") || iName2.EndsWithsWholeWord("w")) { iClass = "w"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("y") || iName2.EndsWithsWholeWord("y")) { iClass = "y"; iClassPosition = 1; }
                if (iName1.EndsWithsWholeWord("z") || iName2.EndsWithsWholeWord("z")) { iClass = "z"; iClassPosition = 1; }
                /*-------------------------------------------------------------------*/

                if (!(iName2 + iName1).Contains("%"))
                {
                    dtResult.Rows.Add(iName2, iName1, iValue, iYear, iETF, iClass, iClassPosition);
                }
            }
        }


        private string NormazileFundName(string name, bool toLower)
        {
            Dictionary<string, string> ReplaceList = new Dictionary<string, string>();

            string ret = name;
            if (toLower) ret = name.ToLower();
            /*-------------------------------------------------------------------*/
            #region Replacement Words List

            if (ReplaceList.Count <= 0)
            {
                ReplaceList.AddIfNotExist("registered investment company", "");
                ReplaceList.AddIfNotExist("pooled separate accounts", "");
                ReplaceList.AddIfNotExist("pooled separate account", "");
                ReplaceList.AddIfNotExist("sep acct", "");
                ReplaceList.AddIfNotExist("separate acct", "");
                ReplaceList.AddIfNotExist("separate account", "");
                ReplaceList.AddIfNotExist("separate acct", "");
                ReplaceList.AddIfNotExist("sa", "");

                ReplaceList.AddIfNotExist("mutual funds:", "");
                ReplaceList.AddIfNotExist("mutual funds", "");
                ReplaceList.AddIfNotExist("mutual fund:", "");
                ReplaceList.AddIfNotExist("mutual fund", "");
                ReplaceList.AddIfNotExist("fidelity investments", "");
                ReplaceList.AddIfNotExist("john hancock, usa", "");
                ReplaceList.AddIfNotExist("transamerica financial life insurance company", "");
                ReplaceList.AddIfNotExist("ameritas life insurance corporate", "");
                ReplaceList.AddIfNotExist("wells fargo, n.a.", "");
                ReplaceList.AddIfNotExist("fidelity investments", "");
                ReplaceList.AddIfNotExist("wilmington trust", "");
                ReplaceList.AddIfNotExist("principal life insurance company", "");
                ReplaceList.AddIfNotExist("ing life insurance and annuity co", "");
                ReplaceList.AddIfNotExist("hartford life insurance co", "");
                ReplaceList.AddIfNotExist("john hancock usa", "");
                ReplaceList.AddIfNotExist("voya retirement insurance and annuity company", "");
                ReplaceList.AddIfNotExist("woodt rust bank n.a.", "");
                ReplaceList.AddIfNotExist("fidelity funds", "");
                ReplaceList.AddIfNotExist("mid-atlantic trust company", "");
                ReplaceList.AddIfNotExist("mn life insurance co.", "");
                ReplaceList.AddIfNotExist("td ameritrade trust company", "");
                ReplaceList.AddIfNotExist("john hancock life ins co", "");
                ReplaceList.AddIfNotExist("great-west life & annuity co.", "");
                ReplaceList.AddIfNotExist("john hancock life insurance company", "");
                ReplaceList.AddIfNotExist("john hancock insurance co", "");
                //ReplaceList.AddIfNotExist("common/collective trust", "");
                ReplaceList.AddIfNotExist("nationwide life insurance co.", "");
                ReplaceList.AddIfNotExist("john hancock life insurance company (u.s.a.)", "");
                ReplaceList.AddIfNotExist("wells fargo, n.a.", "");
                ReplaceList.AddIfNotExist("registered", "");


                ReplaceList.AddIfNotExist("hq bond", "High Quality Bond");
                ReplaceList.AddIfNotExist("vngrd", "Vanguard");
                ReplaceList.AddIfNotExist("hlth", "Health");
                ReplaceList.AddIfNotExist("MM Sel", "Multi-Manager Select");
                ReplaceList.AddIfNotExist("Eq-Wtd", "Equally-Wtd");
                ReplaceList.AddIfNotExist("Low Vol", "Low Volatility");
                ReplaceList.AddIfNotExist("Lw Vol", "Low Volatility");
                ReplaceList.AddIfNotExist("Dur", "Duration");
                ReplaceList.AddIfNotExist("Allc", "Allocation");
                ReplaceList.AddIfNotExist("Alloc", "Allocation");
                ReplaceList.AddIfNotExist("MC Growth", "Mid Cap Growth");
                ReplaceList.AddIfNotExist("Invmt", "Investment");
                ReplaceList.AddIfNotExist("Invt", "Investment");
                ReplaceList.AddIfNotExist("Innovs", "Innovations");
                ReplaceList.AddIfNotExist("MultiCp", "Multi Cap");
                ReplaceList.AddIfNotExist("Infras", "Infrastructure");
                ReplaceList.AddIfNotExist("Infra", "Infrastructure");
                ReplaceList.AddIfNotExist("Intl", "International");
                ReplaceList.AddIfNotExist("INTL", "International");
                ReplaceList.AddIfNotExist("Internatl", "International");
                ReplaceList.AddIfNotExist("Intrntl", "International");
                ReplaceList.AddIfNotExist("Mkt Neut St", "Market Neutral Strategy");
                ReplaceList.AddIfNotExist("Em Mkts", "Emerging Markets");
                ReplaceList.AddIfNotExist("Emg Mkts", "Emerging Markets");
                ReplaceList.AddIfNotExist("Em Mk", "Emerging Markets");
                ReplaceList.AddIfNotExist("EM", "Emerging Markets");
                ReplaceList.AddIfNotExist("Emerg", "Emerging");
                ReplaceList.AddIfNotExist("Mkts", "Markets");
                ReplaceList.AddIfNotExist("Mrkt", "Markets");
                ReplaceList.AddIfNotExist("Mkt", "Market");
                ReplaceList.AddIfNotExist("Alt", "Alternative");
                ReplaceList.AddIfNotExist("Fxd Inc", "Fixed Income");
                ReplaceList.AddIfNotExist("Val", "Value");
                ReplaceList.AddIfNotExist("wd", "World");
                ReplaceList.AddIfNotExist("SCG", "Small Cap Growth");
                ReplaceList.AddIfNotExist("MC Growth", " Mid Cap Growth");
                ReplaceList.AddIfNotExist("RealRet", "Real Retirement");
                ReplaceList.AddIfNotExist("Sel", "Select");
                ReplaceList.AddIfNotExist("Eq", "Equity");
                ReplaceList.AddIfNotExist("Eqty", "Equity");
                ReplaceList.AddIfNotExist("Mat Obligs", "Maturity Obligations");
                ReplaceList.AddIfNotExist("Inst", "Institutional");
                ReplaceList.AddIfNotExist("Instl", "Institutional");
                ReplaceList.AddIfNotExist("Sm Cp", "Small Cap");
                ReplaceList.AddIfNotExist("Sm Cap", "Small Cap");
                ReplaceList.AddIfNotExist("SmCp", "Small Cap");
                ReplaceList.AddIfNotExist("Sm-Cp", "Small Cap");
                ReplaceList.AddIfNotExist("Small Cp", "Small Cap");
                ReplaceList.AddIfNotExist("SmlCp", "Small Cap");
                ReplaceList.AddIfNotExist("Gl", "Global");
                ReplaceList.AddIfNotExist("Glbl", "Global");
                ReplaceList.AddIfNotExist("Large Cp", "Large Cap");
                ReplaceList.AddIfNotExist("LgCp", "Large Cap");
                ReplaceList.AddIfNotExist("Lg Cp", "Large Cap");
                ReplaceList.AddIfNotExist("Lg-Cp", "Large Cap");
                ReplaceList.AddIfNotExist("Lg-Cap", "Large Cap");
                ReplaceList.AddIfNotExist("Lg Cap", "Large Cap");
                ReplaceList.AddIfNotExist("LgCap", "Large Cap");
                ReplaceList.AddIfNotExist("lrg Capital", "Large Cap");
                ReplaceList.AddIfNotExist("largecap", "Large Cap");
                ReplaceList.AddIfNotExist("lrgCap", "Large Cap");
                ReplaceList.AddIfNotExist("lrgcapital", "Large Cap");


                ReplaceList.AddIfNotExist("LC Value", "Large Cap Value");
                ReplaceList.AddIfNotExist("LC Growth", "Large Cap Growth");

                ReplaceList.AddIfNotExist("descipl", "disciplined");
                ReplaceList.AddIfNotExist("di sclpl", "disciplined");
                ReplaceList.AddIfNotExist("diciplnd", "disciplined");
                ReplaceList.AddIfNotExist("disc", "disciplined");
                ReplaceList.AddIfNotExist("disci pl", "disciplined");
                ReplaceList.AddIfNotExist("disci plin", "disciplined");
                ReplaceList.AddIfNotExist("discip", "disciplined");
                ReplaceList.AddIfNotExist("discipl", "disciplined");
                ReplaceList.AddIfNotExist("discipline", "disciplined");
                ReplaceList.AddIfNotExist("discpl", "disciplined");
                ReplaceList.AddIfNotExist("discplnd", "disciplined");
                ReplaceList.AddIfNotExist("disp", "disciplined");
                ReplaceList.AddIfNotExist("dispicline", "disciplined");
                ReplaceList.AddIfNotExist("dscip", "disciplined");
                ReplaceList.AddIfNotExist("dsclpnd", "disciplined");
                ReplaceList.AddIfNotExist("dscpl", "disciplined");
                ReplaceList.AddIfNotExist("duc1plincd", "disciplined");


                ReplaceList.AddIfNotExist("SC", "Small Cap");
                ReplaceList.AddIfNotExist("Idx", "Index");
                ReplaceList.AddIfNotExist("Indx", "Index");
                ReplaceList.AddIfNotExist("Mid Cp Vl", "Mid Cap Value");
                ReplaceList.AddIfNotExist("MidCap", "Mid Cap");
                ReplaceList.AddIfNotExist("MdCp", "Mid Cap");
                ReplaceList.AddIfNotExist("nw dest", "nationwide destination");
                ReplaceList.AddIfNotExist("nw", "nationwide destination");
                ReplaceList.AddIfNotExist("Amer Cent", "American Century");
                ReplaceList.AddIfNotExist("am cent", "American Century");
                ReplaceList.AddIfNotExist("amer century", "American Century");
                ReplaceList.AddIfNotExist("am century", "American Century");
                ReplaceList.AddIfNotExist("amer cent", "American Century");
                ReplaceList.AddIfNotExist("american cent", "American Century");
                ReplaceList.AddIfNotExist("amcent", "American Century");
                ReplaceList.AddIfNotExist("amrcent", "American Century");
                ReplaceList.AddIfNotExist("americent", "American Century");
                ReplaceList.AddIfNotExist("ameican century", "American Century");
                ReplaceList.AddIfNotExist("america century", "American Century");
                ReplaceList.AddIfNotExist("america n century", "American Century");
                ReplaceList.AddIfNotExist("Amercent", "American Century");


                ReplaceList.AddIfNotExist("Fndmtl", "Fundamental");
                ReplaceList.AddIfNotExist("SmallCapValue", "Small Cap Value");
                ReplaceList.AddIfNotExist("Pre Yd", "Premium Yield");
                ReplaceList.AddIfNotExist("Ord", "Ordinary");
                ReplaceList.AddIfNotExist("Ret Strat", "Retirement Strategy");
                ReplaceList.AddIfNotExist("Trsry", "Treasury");
                ReplaceList.AddIfNotExist("Shrt Dur Strat Inc", "Short Duration Strategic Income");
                ReplaceList.AddIfNotExist("Hi Yld", "High Yield");
                ReplaceList.AddIfNotExist("HiYld", "High Yield");
                ReplaceList.AddIfNotExist("Hi-Yld", "High Yield");
                ReplaceList.AddIfNotExist("Yld", "Yield");
                ReplaceList.AddIfNotExist("Aggr", "Aggressive");
                ReplaceList.AddIfNotExist("Agrsv", "Aggressive");
                ReplaceList.AddIfNotExist("Govt", "Government");
                ReplaceList.AddIfNotExist("Mgmt", "Management");
                ReplaceList.AddIfNotExist("Gr Alloc", "Growth Allocation");
                ReplaceList.AddIfNotExist("Mlt", "Multi");
                ReplaceList.AddIfNotExist("MCV", "Mid Cap Value");
                ReplaceList.AddIfNotExist("SCV", "Small Cap Value");
                ReplaceList.AddIfNotExist("Stk", "Stock");
                ReplaceList.AddIfNotExist("Opptys", "Opportunities");
                ReplaceList.AddIfNotExist("Oppty", "Opportunities");
                ReplaceList.AddIfNotExist("Oppts", "Opportunities");
                ReplaceList.AddIfNotExist("Opps", "Opportunities");
                ReplaceList.AddIfNotExist("Opptnstc", "Opportunistic");
                ReplaceList.AddIfNotExist("Rsrch", "Research");
                ReplaceList.AddIfNotExist("Lstd", "Listed");
                ReplaceList.AddIfNotExist("Infr", "Infrastructure");
                ReplaceList.AddIfNotExist("Div", "Dividend");
                ReplaceList.AddIfNotExist("Adv", "Advisory");
                ReplaceList.AddIfNotExist("Div&Inc Bldr", "Dividend and Income Builder");
                ReplaceList.AddIfNotExist("Inc Bldr", "Income Builder");
                ReplaceList.AddIfNotExist("Div Bldr", "Dividend Builder");
                ReplaceList.AddIfNotExist("Bldr", "Builder");
                ReplaceList.AddIfNotExist("GlblIncBld", "Global Income Builder");
                ReplaceList.AddIfNotExist("Ret", "Return");
                ReplaceList.AddIfNotExist("Rl Estt", "Real Estate");
                ReplaceList.AddIfNotExist("Rl Est", "Real Estate");
                ReplaceList.AddIfNotExist("Real Est", "Real Estate");
                ReplaceList.AddIfNotExist("RE Real", "Estate");
                ReplaceList.AddIfNotExist("Dyn", "Dynamic");
                ReplaceList.AddIfNotExist("L/S", "Long/Short");
                ReplaceList.AddIfNotExist("Wlth", "Wealth");
                ReplaceList.AddIfNotExist("Cntr-Trnd", "Counter-Trend");
                ReplaceList.AddIfNotExist("Nat Res", "Natural Resources");
                ReplaceList.AddIfNotExist("natrl rsrc", "Natural Resources");
                ReplaceList.AddIfNotExist("natl rsrc", "Natural Resources");
                ReplaceList.AddIfNotExist("natr rsrc", "Natural Resources");
                ReplaceList.AddIfNotExist("natural reso", "Natural Resources");
                ReplaceList.AddIfNotExist("nat resources", "Natural Resources");
                ReplaceList.AddIfNotExist("natural res", "Natural Resources");
                ReplaceList.AddIfNotExist("nannl resources", "Natural Resources");
                ReplaceList.AddIfNotExist("natural resor", "Natural Resources");
                ReplaceList.AddIfNotExist("nannl resources", "Natural Resources");
                ReplaceList.AddIfNotExist("nat resour", "Natural Resources");
                ReplaceList.AddIfNotExist("natural resor", "Natural Resources");

                ReplaceList.AddIfNotExist("Prtnrs", "Partners");
                ReplaceList.AddIfNotExist("PrtnrsIII", "Partners III");
                ReplaceList.AddIfNotExist("Svc", "Service");
                ReplaceList.AddIfNotExist("Svcs", "Services");
                ReplaceList.AddIfNotExist("Finl", "Financial");
                ReplaceList.AddIfNotExist("Corp", "Corporate");
                ReplaceList.AddIfNotExist("Bd", "Bond");
                ReplaceList.AddIfNotExist("Bnd", "Bond");
                ReplaceList.AddIfNotExist("Fnd", "Fund");
                ReplaceList.AddIfNotExist("Muni", "Municipal");
                ReplaceList.AddIfNotExist("Dbt", "Debt");
                ReplaceList.AddIfNotExist("Shrt-Trm", "Short-Term");
                ReplaceList.AddIfNotExist("Strat", "Strategy");
                ReplaceList.AddIfNotExist("Cnsrv", "Conservative");
                ReplaceList.AddIfNotExist("consrv", "Conservative");
                ReplaceList.AddIfNotExist("Tx-Mgd", "Tax-Managed");
                ReplaceList.AddIfNotExist("Cmdty", "Commodity");
                ReplaceList.AddIfNotExist("Mgd Volatility", "Managed Volatility");
                ReplaceList.AddIfNotExist("Mgd Vol", "Managed Volatility");
                ReplaceList.AddIfNotExist("Managed Vol", "Managed Volatility");
                ReplaceList.AddIfNotExist("Adm", "Admiral");
                ReplaceList.AddIfNotExist("Inc", "Income");
                ReplaceList.AddIfNotExist("Trgt", "Target");
                ReplaceList.AddIfNotExist("LifeTm", "LifeTime");
                ReplaceList.AddIfNotExist("Vangrd", "Vanguard");
                ReplaceList.AddIfNotExist("PJMCO", "PIMCO");
                ReplaceList.AddIfNotExist("JH", "John Hancock");
                ReplaceList.AddIfNotExist("JHancock", "John Hancock");
                ReplaceList.AddIfNotExist("J Hancock", "John Hancock");
                ReplaceList.AddIfNotExist("Bal", "Balanced");
                ReplaceList.AddIfNotExist("Secs", "Securities");
                ReplaceList.AddIfNotExist("Ret", "Return");
                ReplaceList.AddIfNotExist("Targ", "Target");
                ReplaceList.AddIfNotExist("SSGA", "State Street");
                ReplaceList.AddIfNotExist("JP Morgan", "JPMorgan");
                ReplaceList.AddIfNotExist("Vanguard Retirement", "Vanguard Target Retirement");
                ReplaceList.AddIfNotExist("T.Rowe", "T. Rowe");
                ReplaceList.AddIfNotExist("Tgt", "Target");
                ReplaceList.AddIfNotExist("FA", "Freedom");
                ReplaceList.AddIfNotExist("SSgA", "State Street");
                ReplaceList.AddIfNotExist("Prin", "Principal");
                ReplaceList.AddIfNotExist("JPM", "JPMorgan");
                ReplaceList.AddIfNotExist("SMARTRET", "SmartRetirement");
                ReplaceList.AddIfNotExist("Bal-Risk", "Balanced Risk");
                ReplaceList.AddIfNotExist("Advtg", "Advantage");
                ReplaceList.AddIfNotExist("RealRetirement", "Real Retirement");
                ReplaceList.AddIfNotExist("TRwPr", "T. Rowe Price");
                ReplaceList.AddIfNotExist("T.Row", "T. Rowe");
                ReplaceList.AddIfNotExist("TRP", "T. Rowe Price");

                ReplaceList.AddIfNotExist("Bal", "Balanced");
                ReplaceList.AddIfNotExist("Secs", "Securities");
                ReplaceList.AddIfNotExist("Ret", "Return");
                ReplaceList.AddIfNotExist("Targ", "Target");
                ReplaceList.AddIfNotExist("SSGA", "State Street");
                ReplaceList.AddIfNotExist("JP Morgan", "JPMorgan");
                ReplaceList.AddIfNotExist("Vanguard Retirement", "Vanguard Target Retirement");
                ReplaceList.AddIfNotExist("T.Rowe", "T. Rowe");
                ReplaceList.AddIfNotExist("Tgt", "Target");

                ReplaceList.AddIfNotExist("af", "American Funds");
                ReplaceList.AddIfNotExist("am fnds", "American Funds");
                ReplaceList.AddIfNotExist("am funds", "American Funds");
                ReplaceList.AddIfNotExist("amer funds", "American Funds");
                ReplaceList.AddIfNotExist("am fund", "American Funds");
                ReplaceList.AddIfNotExist("american fund", "American Funds");
                ReplaceList.AddIfNotExist("amfunds", "American Funds");
                ReplaceList.AddIfNotExist("amrfunds", "American Funds");
                ReplaceList.AddIfNotExist("amerifunds", "American Funds");
                ReplaceList.AddIfNotExist("ameican funds", "American Funds");
                ReplaceList.AddIfNotExist("america funds", "American Funds");
                ReplaceList.AddIfNotExist("america n funds", "American Funds");
                ReplaceList.AddIfNotExist("amfund", "American Funds");
                ReplaceList.AddIfNotExist("amrfund", "American Funds");
                ReplaceList.AddIfNotExist("amerifund", "American Funds");
                ReplaceList.AddIfNotExist("ameican fund", "American Funds");
                ReplaceList.AddIfNotExist("america fund", "American Funds");
                ReplaceList.AddIfNotExist("america n fund", "American Funds");

                ReplaceList.AddIfNotExist("FA", "Freedom");
                ReplaceList.AddIfNotExist("SSgA", "State Street");
                ReplaceList.AddIfNotExist("Prin", "Principal");
                ReplaceList.AddIfNotExist("JPM", "JPMorgan");
                ReplaceList.AddIfNotExist("SMARTRET", "SmartRetirement");
                ReplaceList.AddIfNotExist("Bal-Risk", "Balanced Risk");
                ReplaceList.AddIfNotExist("Advtg", "Advantage");
                ReplaceList.AddIfNotExist("RealRetirement", "Real Retirement");
                ReplaceList.AddIfNotExist("TRwPr ", "T. Rowe Price");
                ReplaceList.AddIfNotExist("T.Row", "T. Rowe");
                ReplaceList.AddIfNotExist("dimensional advisors", "DFA");
                ReplaceList.AddIfNotExist("Dimensional", "DFA");
                ReplaceList.AddIfNotExist("cons str", "conservative strategy");
                ReplaceList.AddIfNotExist("mod str", "moderate strategy");
                ReplaceList.AddIfNotExist("agg str", "aggressive strategy");
                ReplaceList.AddIfNotExist("Eqty", "Equity");
                ReplaceList.AddIfNotExist("Infl", "Inflation");
                ReplaceList.AddIfNotExist("Adj", "Adjusted");
                ReplaceList.AddIfNotExist("App", "Appreciation");
                ReplaceList.AddIfNotExist("Midcap", "Mid Cap");
                ReplaceList.AddIfNotExist("Val", "Value");
                ReplaceList.AddIfNotExist("Fdamental", "Fundamental");
                ReplaceList.AddIfNotExist("FD", "Fund");
                ReplaceList.AddIfNotExist("Indx", "Index");
                ReplaceList.AddIfNotExist("Smcap", "Small Cap");
                ReplaceList.AddIfNotExist("Small-Cap", "Small Cap");
                ReplaceList.AddIfNotExist("Smallcap", "Small Cap");
                ReplaceList.AddIfNotExist("SmallCap", "Small Cap");
                ReplaceList.AddIfNotExist("Sm-Mid Cp", "Small Mid Cap");
                ReplaceList.AddIfNotExist("smallmid", "Small Mid");
                ReplaceList.AddIfNotExist("Mid Cp", "Mid Cap");
                ReplaceList.AddIfNotExist("ldrs", "Leaders");


                ReplaceList.AddIfNotExist("S-M", "Small Mid");
                ReplaceList.AddIfNotExist("S/M", "Small Mid");
                ReplaceList.AddIfNotExist("Mid-Cap", "Mid Cap");
                ReplaceList.AddIfNotExist("Midcap", "Mid Cap");
                ReplaceList.AddIfNotExist("Md cp", "Mid Cap");
                ReplaceList.AddIfNotExist("midcp", "Mid Cap");
                ReplaceList.AddIfNotExist("Cap", "Capital");
                ReplaceList.AddIfNotExist("mid-capital", "Mid Capital");
                ReplaceList.AddIfNotExist("Dvsd", "diversified");
                ReplaceList.AddIfNotExist("mut global disc", "Mutual Global Discovery");
                ReplaceList.AddIfNotExist("Emg Market Op", "Emerging Markets Opportunity");
                ReplaceList.AddIfNotExist("Fndtn", "Foundation");
                ReplaceList.AddIfNotExist("Abs Rtn", "Absolute Return");

                ReplaceList.AddIfNotExist("Econ", "Economy");
                ReplaceList.AddIfNotExist("AdvOne", "Advisor One");
                ReplaceList.AddIfNotExist("Principal Global Investors", "Principal Global");
                ReplaceList.AddIfNotExist("Apprec", "Appreciation");
                ReplaceList.AddIfNotExist("fdmntl", "Fundamental");
                ReplaceList.AddIfNotExist("John Hancock DFA", "DFA");
                ReplaceList.AddIfNotExist("John Hancock T. Rowe", "T. Rowe");
                ReplaceList.AddIfNotExist("John Hancock T.Rowe", "T. Rowe");
                ReplaceList.AddIfNotExist("Invs", "Investors");
                ReplaceList.AddIfNotExist("sml", "Small");
                ReplaceList.AddIfNotExist("modt", "Moderate");
                ReplaceList.AddIfNotExist("Mdt Agg", "Moderately Aggressive");

                ReplaceList.AddIfNotExist("JHFunds2", "John Hancock II");
                ReplaceList.AddIfNotExist("Jobn", "John");
                ReplaceList.AddIfNotExist("Tech", "Technology");
                ReplaceList.AddIfNotExist("BioTech", "BioTechnology");
                ReplaceList.AddIfNotExist("amfds", "American Funds");
                ReplaceList.AddIfNotExist("amcent", "American Century");
                ReplaceList.AddIfNotExist("Extnd", "Extended");
                ReplaceList.AddIfNotExist("Qlty", "Quality");


                ReplaceList.AddIfNotExist("fds", "Funds");
                ReplaceList.AddIfNotExist("fd", "Fund");
                ReplaceList.AddIfNotExist("fid", "Fidelity");
                ReplaceList.AddIfNotExist("Advt", "Advantage");
                ReplaceList.AddIfNotExist("sptn", "Spartan");
                ReplaceList.AddIfNotExist("opphmr", "Oppenheimer");
                ReplaceList.AddIfNotExist("Tot", "Total");
                ReplaceList.AddIfNotExist("Glb", "Global");
                ReplaceList.AddIfNotExist("discovy", "Discovery");
                ReplaceList.AddIfNotExist("Tmpni", "timpani");
                ReplaceList.AddIfNotExist("NY", "New York");

                ReplaceList.AddIfNotExist("AB", "Alliance Bernstein");
                ReplaceList.AddIfNotExist("AllianceBernstein", "Alliance Bernstein");
                ReplaceList.AddIfNotExist("alliance bernst", "Alliance Bernstein");
                ReplaceList.AddIfNotExist("bernst", "Alliance Bernstein");
                ReplaceList.AddIfNotExist("alliancebernstcin", "Alliance Bernstein");
                ReplaceList.AddIfNotExist("alliancebern", "Alliance Bernstein");

                ReplaceList.AddIfNotExist("Smart Retirement", "SmartRetirement");
                ReplaceList.AddIfNotExist("mmkt", "Money Market");
                ReplaceList.AddIfNotExist("equ", "equity");
                ReplaceList.AddIfNotExist("inco", "income");
                ReplaceList.AddIfNotExist("dev", "developed");
                ReplaceList.AddIfNotExist("oppenhmr", "Oppenheimer");
                ReplaceList.AddIfNotExist("alliancebernstein", "Alliance Bernstein");
                ReplaceList.AddIfNotExist("wamu", "Washington Mutual");
                ReplaceList.AddIfNotExist("m&n", "Manning & Napier");
                ReplaceList.AddIfNotExist("pru", "Prudential");
                ReplaceList.AddIfNotExist("Prudential/j", "Prudential Jennison");
                ReplaceList.AddIfNotExist("Invesco Van Kampen", "Van Kampen");
                ReplaceList.AddIfNotExist("prspctv", "perspective");
                ReplaceList.AddIfNotExist("fun", "fundamental");
                ReplaceList.AddIfNotExist("invesco v k", "Invesco");
                ReplaceList.AddIfNotExist("am cent", "American Century");
                ReplaceList.AddIfNotExist("amerfds", "American Funds");
                ReplaceList.AddIfNotExist("Great-West Templeton", "Templeton");
                ReplaceList.AddIfNotExist("ing", "Voya");
                ReplaceList.AddIfNotExist("PRU", "Prudential");
                ReplaceList.AddIfNotExist("INFLAT", "Inflation");
                ReplaceList.AddIfNotExist("Fred Alger", "Alger"); // ТАК НАДО
                ReplaceList.AddIfNotExist("Alger", "Fred Alger"); // ТАК НАДО
                ReplaceList.AddIfNotExist("CAPAPP", "Capital Appreciation");
                ReplaceList.AddIfNotExist("CONSRV", "Conservative");
                ReplaceList.AddIfNotExist("Diversified In", "Diversified Income");
                ReplaceList.AddIfNotExist("Protect", "Protected");
                ReplaceList.AddIfNotExist("AGGR", "Aggressive");
                ReplaceList.AddIfNotExist("Goldman Sachs", "Goldman");
                ReplaceList.AddIfNotExist("MOD", "Moderate");
                ReplaceList.AddIfNotExist("&", "and");
                ReplaceList.AddIfNotExist("Prudential Jennison", "Jennison");
                ReplaceList.AddIfNotExist("lord abbet", "Lord Abbett");
                ReplaceList.AddIfNotExist("europac", "Europacific");
                ReplaceList.AddIfNotExist("Equity-Income", "Equity Income");
                ReplaceList.AddIfNotExist("Jenn", "Jennison");
                ReplaceList.AddIfNotExist("Emergin", "Emerging");

                ReplaceList.AddIfNotExist("Morg Stan", "morgan stanley");
                ReplaceList.AddIfNotExist("lordabbett", "lord abbett");
                ReplaceList.AddIfNotExist("columbian", "columbia");
                ReplaceList.AddIfNotExist("advisory", "advisor");
                ReplaceList.AddIfNotExist("fundsamerican", "funds american");
                ReplaceList.AddIfNotExist("fundscapital", "funds capital");

                ReplaceList.AddIfNotExist("Shrt-Interm", "Short Intermediate");
                ReplaceList.AddIfNotExist("Interm", "Intermediate");
                ReplaceList.AddIfNotExist("Ttl", "Total");
                ReplaceList.AddIfNotExist("dtsch", "Deutsche");
                ReplaceList.AddIfNotExist("deutsch", "Deutsche");
                ReplaceList.AddIfNotExist("deutche", "Deutsche");
                ReplaceList.AddIfNotExist("dws", "Deutsche");

                ReplaceList.AddIfNotExist("shrt", "short");
                ReplaceList.AddIfNotExist("blkrk", "BlackRock");
                ReplaceList.AddIfNotExist("black rock", "BlackRock");
                ReplaceList.AddIfNotExist("black rock", "BlackRock");
                ReplaceList.AddIfNotExist("blkrock", "BlackRock");
                ReplaceList.AddIfNotExist("blkrck", "BlackRock");
                ReplaceList.AddIfNotExist("blckrck", "BlackRock");
                ReplaceList.AddIfNotExist("blockrock", "BlackRock");
                ReplaceList.AddIfNotExist("block rock", "BlackRock");
                ReplaceList.AddIfNotExist("blackrodk", "BlackRock");
                ReplaceList.AddIfNotExist("blackrck", "BlackRock");
                ReplaceList.AddIfNotExist("blackroek", "BlackRock");
                ReplaceList.AddIfNotExist("blackrick", "BlackRock");
                ReplaceList.AddIfNotExist("blackrnck", "BlackRock");
                ReplaceList.AddIfNotExist("blackkrock", "BlackRock");
                ReplaceList.AddIfNotExist("blck rck", "BlackRock");
                ReplaceList.AddIfNotExist("blakrock", "BlackRock");

                ReplaceList.AddIfNotExist("mdcpvl", "Mid Cap Value");
                ReplaceList.AddIfNotExist("fnklntmp", "Franklin Templeton");
                ReplaceList.AddIfNotExist("leggm", "Legg Mason");
                ReplaceList.AddIfNotExist("clrbrdg", "ClearBridge");
                ReplaceList.AddIfNotExist("Clear Bridge", "ClearBridge");
                ReplaceList.AddIfNotExist("clr bridge", "ClearBridge");
                ReplaceList.AddIfNotExist("clr brdg", "ClearBridge");
                ReplaceList.AddIfNotExist("clarebridge", "ClearBridge");
                ReplaceList.AddIfNotExist("cleorbridge", "ClearBridge");
                ReplaceList.AddIfNotExist("clrbridge", "ClearBridge");
                ReplaceList.AddIfNotExist("clcarbridge", "ClearBridge");
                ReplaceList.AddIfNotExist("clearbrdg", "ClearBridge");

                ReplaceList.AddIfNotExist("cons", "Conservative");

                ReplaceList.AddIfNotExist("mdcap", "Mid Cap");
                ReplaceList.AddIfNotExist("opport", "Opportunities");

                ReplaceList.AddIfNotExist("hldgs", "holdings");
                ReplaceList.AddIfNotExist("aggress", "aggressive");
                ReplaceList.AddIfNotExist("allloc", "allocate");
                ReplaceList.AddIfNotExist("oppenhmer", "Oppenheimer");
                ReplaceList.AddIfNotExist("hlthcare", "Healthcare");

                ReplaceList.AddIfNotExist("lnfl prot", "inflation protected");
                ReplaceList.AddIfNotExist("lnflatprot", "inflation protected");
                ReplaceList.AddIfNotExist("Inflation prot", "inflation protected");
                ReplaceList.AddIfNotExist("jnfl prot", "inflation protected");
                ReplaceList.AddIfNotExist("inflate prot", "inflation protected");
                ReplaceList.AddIfNotExist("inf prot", "inflation protected");
                ReplaceList.AddIfNotExist("inflaprot", "inflation protected");
                ReplaceList.AddIfNotExist("inflationprot", "inflation protected");
                ReplaceList.AddIfNotExist("Inflationpro", "inflation protected");


                ReplaceList.AddIfNotExist("inflt", "inflation");
                ReplaceList.AddIfNotExist("j p morgan", "jpmorgan");
                ReplaceList.AddIfNotExist("joh n", "john");
                ReplaceList.AddIfNotExist("fnkln", "Franklin");
                ReplaceList.AddIfNotExist("fkln", "Franklin");
                ReplaceList.AddIfNotExist("lfcyle", "lifecycle");
                ReplaceList.AddIfNotExist("incm", "income");
                ReplaceList.AddIfNotExist("Equityrtmt", "Equity Retirement");
                ReplaceList.AddIfNotExist("Growthrtmt", "Growth Retirement");
                ReplaceList.AddIfNotExist("Securitiesrtmt", "Securities Retirement");
                ReplaceList.AddIfNotExist("indexrtmt", "index Retirement");
                ReplaceList.AddIfNotExist("ldxrtmt", "index Retirement");
                ReplaceList.AddIfNotExist("Valuertmt", "Value Retirement");
                ReplaceList.AddIfNotExist("Incomertmt", "Income Retirement");
                ReplaceList.AddIfNotExist("smartrtmt", "SmartRetirement");

                ReplaceList.AddIfNotExist("Retire", "Retirement");
                ReplaceList.AddIfNotExist("rtmt", "Retirement");
                ReplaceList.AddIfNotExist("rtimt", "Retirement");
                ReplaceList.AddIfNotExist("rtmnt", "Retirement");
                ReplaceList.AddIfNotExist("rmt", "Retirement");
                ReplaceList.AddIfNotExist("rlrmt", "Retirement");
                ReplaceList.AddIfNotExist("rtnnt", "Retirement");
                ReplaceList.AddIfNotExist("rtrm", "Retirement");
                ReplaceList.AddIfNotExist("rtrmt", "Retirement");
                ReplaceList.AddIfNotExist("rtmnt", "Retirement");
                ReplaceList.AddIfNotExist("rctiremtllt", "Retirement");
                ReplaceList.AddIfNotExist("retiren1ent", "Retirement");
                ReplaceList.AddIfNotExist("rctircmnt", "Retirement");
                ReplaceList.AddIfNotExist("rcliremcnl", "Retirement");
                ReplaceList.AddIfNotExist("re!ircmcnl", "Retirement");
                ReplaceList.AddIfNotExist("petirement", "Retirement");
                ReplaceList.AddIfNotExist("rtrmnt", "Retirement");
                ReplaceList.AddIfNotExist("irement", "Retirement");
                ReplaceList.AddIfNotExist("rtmt", "Retirement");
                ReplaceList.AddIfNotExist("retire1nent", "Retirement");
                ReplaceList.AddIfNotExist("retmt", "Retirement");
                ReplaceList.AddIfNotExist("Return irement", "Retirement");
                ReplaceList.AddIfNotExist("relirement", "Retirement");
                ReplaceList.AddIfNotExist("trmt", "Retirement");
                ReplaceList.AddIfNotExist("retlrment", "Retirement");
                ReplaceList.AddIfNotExist("retirment", "Retirement");


                ReplaceList.AddIfNotExist("Gro", "Growth");
                ReplaceList.AddIfNotExist("Gr", "Growth");
                ReplaceList.AddIfNotExist("Grth", "Growth");
                ReplaceList.AddIfNotExist("grwth", "growth");
                ReplaceList.AddIfNotExist("grow", "growth");
                ReplaceList.AddIfNotExist("growh", "growth");
                ReplaceList.AddIfNotExist("grw", "growth");
                ReplaceList.AddIfNotExist("grovvth", "Growth");
                ReplaceList.AddIfNotExist("gwth", "growth");
                ReplaceList.AddIfNotExist("grwt", "growth");
                ReplaceList.AddIfNotExist("grevth", "growth");
                ReplaceList.AddIfNotExist("grovth", "growth");
                ReplaceList.AddIfNotExist("gowth", "growth");


                ReplaceList.AddIfNotExist("CommoditiesPLUS", "Commodities Plus");
                ReplaceList.AddIfNotExist("Royce Penn", "Royce Pennsylvania");
                ReplaceList.AddIfNotExist("U.S.", "US");
                ReplaceList.AddIfNotExist("Great-West Putnam", "Putnam");
                ReplaceList.AddIfNotExist("aggressive", "aggressive");
                ReplaceList.AddIfNotExist("spa1tan", "spartan");
                ReplaceList.AddIfNotExist("Lev", "Leveraged");
                ReplaceList.AddIfNotExist("SM", "Small");
                ReplaceList.AddIfNotExist("Inv", "Investment");
                ReplaceList.AddIfNotExist("AC", "American Century");
                ReplaceList.AddIfNotExist("wf", "Wells Fargo");
                ReplaceList.AddIfNotExist("wfa", "Wells Fargo Advantage");



                ReplaceList.AddIfNotExist("SandP", "S&P");
                ReplaceList.AddIfNotExist("gandi", "growth and income");

                ReplaceList.AddIfNotExist("Fund", "");
                ReplaceList.AddIfNotExist("Class", "");
            }

            #endregion
            /*-------------------------------------------------------------------*/
            foreach (var word in ReplaceList)
            {
                //ret = ret.ReplaceWholeWord((toLower ? word.Key.ToLower() : word.Key), word.Value);
                ret = ret.ReplaceWholeWord(word.Key, word.Value, RegexOptions.IgnoreCase);

            }
            /*-------------------------------------------------------------------*/
            ret = ret.Replace("Institutional;Institutional", "Institutional");
            ret = ret.Replace("Service;Service", "Service");
            ret = ret.Replace("Advisory;Advisory", "Advisory");
            ret = ret.Replace("Admiral;Admiral", "Admiral");
            ret = ret.Replace("Select;Select", "Select");
            ret = ret.Replace("Inv;Inv", "Inv");

            return ret;
        }
    }
}
