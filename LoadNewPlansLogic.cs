using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Flurl;
using Flurl.Http;
using LumenWorks.Framework.IO.Csv;

namespace PlansParser
{
    class LoadNewPlansLogic
    {
        readonly SemaphoreSlim _throttler = new SemaphoreSlim(10, 10);
        private int _totalPlansSendedToCheck;
        private int _totalSuccessPlansSendedToCheck;
        private int _totalNewPlans;
        private readonly string _baseDir;
        private readonly QualityControl _qualityControl;

        private readonly ToolStripProgressBar _prgLine;
        private readonly ToolStripStatusLabel _prgLabel;

        public LoadNewPlansLogic(string baseDir, ToolStripProgressBar prgLine, ToolStripStatusLabel prgLabel, QualityControl qualityControl)
        {
            _baseDir = baseDir;
            _prgLine = prgLine;
            _prgLabel = prgLabel;
            _qualityControl = qualityControl;
        }

        readonly Stopwatch _addPlansLoadStopwatch = new Stopwatch();

        public async Task AddPlanLoad()
        {
            _addPlansLoadStopwatch.Restart();

            _prgLabel.Text = "Begin AddPlanLoad...";


            var curYear = DateTime.Now.Year - 1;

            var file5500PrivYear = Path.Combine(_baseDir, string.Format("f_5500_{0}_all.csv", curYear - 1));
            var fileSCH_HPrivYear = Path.Combine(_baseDir, string.Format("F_SCH_H_{0}_all.csv", curYear - 1));

            var privYearPlans = GetPlansFromFiles(fileSCH_HPrivYear, file5500PrivYear);
            var plansCombined = privYearPlans;

            if (DateTime.Now.Month != 1) // for the Jan there is no data for current year!
            {
                var file5500 = Path.Combine(_baseDir, string.Format("f_5500_{0}_all.csv", curYear));
                var fileSCH_H = Path.Combine(_baseDir, string.Format("F_SCH_H_{0}_all.csv", curYear));

                var plans = GetPlansFromFiles(fileSCH_H, file5500);
                plansCombined = plans.Union(privYearPlans);
            }




            var tasks = new List<Task>();
            bool isFirstStart = true;
            foreach (var newPlansBatch in plansCombined.Union(privYearPlans).Batch(40))
            {
                if (isFirstStart)
                {
                    await LoadPlansToRextremaWS(newPlansBatch, true);
                    isFirstStart = false;
                }

                tasks.Add(LoadPlansToRextremaWS(newPlansBatch, false));
            }

            await Task.WhenAll(tasks);
            var httpResponseMessage = await FlurlHelper.RunActionOnRixtremaWsWithRetry("FINISHADDPLANLOAD");
            List<dynamic> newPlans = httpResponseMessage.FCT.NewPlans;
            _totalNewPlans = newPlans.Count;
            var len = newPlans.Select(x => x.IdToKeySponsorNamePair).Where(x => ((string)x).Length > 100).ToList();
            Logger.Log(len.ToJson());
            File.WriteAllText(_baseDir + @"\_DownloadPDF.csv", string.Join("\r\n", newPlans.Select(x => x.IdToKeySponsorNamePair)) + "\r\n");

            _prgLabel.Text = "Ready";
            _prgLine.Value = 0;
            _prgLine.Maximum = 0;
            var qaData = new { Total = _totalPlansSendedToCheck, Success = _totalSuccessPlansSendedToCheck, New = _totalNewPlans }.ToJson();
            _addPlansLoadStopwatch.Stop();
            _qualityControl.CreateEvent(QualityActionsAliases.UPDFUND_LOAD, qaData, _addPlansLoadStopwatch.Elapsed);
        }

        private IEnumerable<NewPlan> GetPlansFromFiles(string fileSCH_H, string file5500)
        {

            DataTable dt_SCH_H = CSVParser.CSVToDataTable(fileSCH_H, true, 0, 0);

            Dictionary<string, DataRow> idToDataRowMap = new Dictionary<string, DataRow>();
            foreach (DataRow row_SCH_H in dt_SCH_H.Rows)
            {
                string _ACK_ID = row_SCH_H["ACK_ID"].ToString().Trim();
                idToDataRowMap[_ACK_ID] = row_SCH_H;
            }

            _prgLine.Value = 0;
            _prgLine.Maximum = dt_SCH_H.Rows.Count;

            CsvReader csv = new CsvReader(new StreamReader(file5500), true, ',');
            while (csv.ReadNextRecord())
            {
                Application.DoEvents();

                var row_ = csv;
                var SPONS_DFE_MAIL_US_ADDRESS = row_["SPONS_DFE_MAIL_US_ADDRESS1"].Trim();
                SPONS_DFE_MAIL_US_ADDRESS += (" " + row_["SPONS_DFE_MAIL_US_ADDRESS2"]).Trim().TakeFirstNLetters(80);
                string ACK_ID = row_[0].Trim().TakeFirstNLetters(30);
                if (ACK_ID == "ACK_ID") continue; // Skip header
                if (ACK_ID == "") continue; // Skip empty line
                string PLAN_NAME = row_[15].Trim().ToUpper().TakeFirstNLetters(150);
                if (PLAN_NAME.Length > 140)
                {
                    _prgLabel.Text = "Alert!!";
                }
                string SPONSOR_DFE_NAME = row_[18].Trim().TakeFirstNLetters(100);
                string SPONS_DFE_MAIL_US_CITY = row_[23].Trim().TakeFirstNLetters(50);
                string SPONS_DFE_MAIL_US_STATE = row_[24].Trim().TakeFirstNLetters(10);

                string SPONS_DFE_MAIL_US_ZIP = row_[25].Trim().TakeFirstNLetters(5);
                if (SPONS_DFE_MAIL_US_ZIP.Length < 5)
                    SPONS_DFE_MAIL_US_ZIP = string.Format("{0}{1}", new string('0', 5 - SPONS_DFE_MAIL_US_ZIP.Length), SPONS_DFE_MAIL_US_ZIP);

                string TOT_PARTCP_BOY_CNT = row_[70];
                string FORM_TAX_PRD = row_[2];
                string SPONS_DFE_PHONE_NUM = row_[44].TakeFirstNLetters(15);
                string ADMIN_SIGNED_NAME = row_[65].TakeFirstNLetters(50);
                string SPONS_SIGNED_NAME = row_[67].TakeFirstNLetters(50);
                string SPONS_DFE_PN = row_[16].TakeFirstNLetters(3);
                string SPONS_DFE_EIN = row_[43].TakeFirstNLetters(9);
                string DATE_RECEIVED = row_[101].TakeFirstNLetters(10);

                string TOT_ASSETS_BOY_AMT = "0";
                string TOT_ASSETS_EOY_AMT = "0";
                if (TOT_PARTCP_BOY_CNT == "") TOT_PARTCP_BOY_CNT = "0";

                if (Convert.ToDouble(TOT_PARTCP_BOY_CNT) == 0) continue; // do not write plans with 0 participants

                if (idToDataRowMap.ContainsKey(ACK_ID))
                {
                    TOT_ASSETS_BOY_AMT = idToDataRowMap[ACK_ID]["TOT_ASSETS_BOY_AMT"].ToString();
                    TOT_ASSETS_EOY_AMT = idToDataRowMap[ACK_ID]["TOT_ASSETS_EOY_AMT"].ToString();

                    if (TOT_ASSETS_BOY_AMT == "") TOT_ASSETS_BOY_AMT = "0";
                    if (TOT_ASSETS_EOY_AMT == "") TOT_ASSETS_EOY_AMT = "0";


                    yield return new NewPlan
                    {
                        PLAN_NAME = PLAN_NAME,
                        SPONSOR_DFE_NAME = SPONSOR_DFE_NAME,
                        SPONS_DFE_MAIL_US_CITY = SPONS_DFE_MAIL_US_CITY,
                        SPONS_DFE_MAIL_US_STATE = SPONS_DFE_MAIL_US_STATE,
                        TOT_PARTCP_BOY_CNT = TOT_PARTCP_BOY_CNT,
                        FORM_TAX_PRD = FORM_TAX_PRD,
                        SPONS_DFE_MAIL_US_ZIP = SPONS_DFE_MAIL_US_ZIP,
                        SPONS_DFE_PHONE_NUM = SPONS_DFE_PHONE_NUM,
                        ADMIN_SIGNED_NAME = ADMIN_SIGNED_NAME,
                        SPONS_SIGNED_NAME = SPONS_SIGNED_NAME,
                        TOT_ASSETS_BOY_AMT = TOT_ASSETS_BOY_AMT,
                        TOT_ASSETS_EOY_AMT = TOT_ASSETS_EOY_AMT,
                        SPONS_DFE_PN = SPONS_DFE_PN,
                        SPONS_DFE_EIN = SPONS_DFE_EIN,
                        DATE_RECEIVED = DATE_RECEIVED,
                        ACK_ID = ACK_ID,
                        SPONS_DFE_MAIL_US_ADDRESS = SPONS_DFE_MAIL_US_ADDRESS
                    };
                }
            }
        }

        private class NewPlan
        {
            public string PLAN_NAME { get; set; }
            public string SPONSOR_DFE_NAME { get; set; }
            public string SPONS_DFE_MAIL_US_CITY { get; set; }
            public string SPONS_DFE_MAIL_US_STATE { get; set; }
            public string TOT_PARTCP_BOY_CNT { get; set; }
            public string FORM_TAX_PRD { get; set; }
            public string SPONS_DFE_MAIL_US_ZIP { get; set; }
            public string SPONS_DFE_PHONE_NUM { get; set; }
            public string ADMIN_SIGNED_NAME { get; set; }
            public string SPONS_SIGNED_NAME { get; set; }
            public string TOT_ASSETS_BOY_AMT { get; set; }
            public string TOT_ASSETS_EOY_AMT { get; set; }
            public string SPONS_DFE_PN { get; set; }
            public string SPONS_DFE_EIN { get; set; }
            public string DATE_RECEIVED { get; set; }
            public string ACK_ID { get; set; }
            public string SPONS_DFE_MAIL_US_ADDRESS { get; set; }
        }

        private async Task LoadPlansToRextremaWS(IList<NewPlan> plans, bool isFirstStart)
        {
            try
            {
                await _throttler.WaitAsync();
                var stopWath = new Stopwatch();
                stopWath.Start();
                var data = new
                {
                    isFirstStart = isFirstStart ? "1" : "0",
                    PLAN_NAME = string.Join("||", plans.Select(x => x.PLAN_NAME)),
                    SPONSOR_DFE_NAME = string.Join("||", plans.Select(x => x.SPONSOR_DFE_NAME)),
                    SPONS_DFE_MAIL_US_CITY = string.Join("||", plans.Select(x => x.SPONS_DFE_MAIL_US_CITY)),
                    SPONS_DFE_MAIL_US_STATE = string.Join("||", plans.Select(x => x.SPONS_DFE_MAIL_US_STATE)),
                    TOT_PARTCP_BOY_CNT = string.Join("||", plans.Select(x => x.TOT_PARTCP_BOY_CNT)),
                    FORM_TAX_PRD = string.Join("||", plans.Select(x => x.FORM_TAX_PRD)),
                    SPONS_DFE_MAIL_US_ZIP = string.Join("||", plans.Select(x => x.SPONS_DFE_MAIL_US_ZIP)),
                    SPONS_DFE_PHONE_NUM = string.Join("||", plans.Select(x => x.SPONS_DFE_PHONE_NUM)),
                    ADMIN_SIGNED_NAME = string.Join("||", plans.Select(x => x.ADMIN_SIGNED_NAME)),
                    SPONS_SIGNED_NAME = string.Join("||", plans.Select(x => x.SPONS_SIGNED_NAME)),
                    TOT_ASSETS_BOY_AMT = string.Join("||", plans.Select(x => x.TOT_ASSETS_BOY_AMT)),
                    TOT_ASSETS_EOY_AMT = string.Join("||", plans.Select(x => x.TOT_ASSETS_EOY_AMT)),
                    SPONS_DFE_PN = string.Join("||", plans.Select(x => x.SPONS_DFE_PN)),
                    SPONS_DFE_EIN = string.Join("||", plans.Select(x => x.SPONS_DFE_EIN)),
                    DATE_RECEIVED = string.Join("||", plans.Select(x => x.DATE_RECEIVED)),
                    ACK_ID = string.Join("||", plans.Select(x => x.ACK_ID)),
                    SPONS_DFE_MAIL_US_ADDRESS = string.Join("||", plans.Select(x => x.SPONS_DFE_MAIL_US_ADDRESS))
                };

                var httpResponseMessage = await FlurlHelper.RunActionOnRixtremaWsWithRetry("AddPlanLoad", data);

                if (httpResponseMessage == null || httpResponseMessage.FCT.Result != "Success")
                {
                    var errorString = string.Format("Error process AddPlanLoad action. Response: {0}",
                        ObjectExtentions.ToJson(httpResponseMessage));
                    Logger.Log(errorString);
                    _prgLabel.Text = "Error process AddPlanLoad action";
                    throw new Exception("Error process AddPlanLoad action");
                }

                _totalSuccessPlansSendedToCheck += plans.Count;
                stopWath.Stop();
                _prgLabel.Text = string.Format("AddPlanLoad for part Success. {0} of {1}. Duration {2}", _prgLine.Value,
                    _prgLine.Maximum, stopWath.Elapsed);

                _prgLine.Value += plans.Count;
                _totalPlansSendedToCheck += plans.Count;
            }
            catch (Exception ex)
            {
                Logger.Log("Error process AddPlanLoad action! Ex " + ex);
            }
            finally
            {
                _throttler.Release();
            }
        }
    }
}
