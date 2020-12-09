using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace PlansParser
{
    enum QualityActionsAliases
    {
        [Description("UPDFUND_LOADFROMGOV")]
        UPDFUND_LOADFROMGOV,

        [Description("UPDFUND_LOAD")]
        UPDFUND_LOAD,

        [Description("UPDFUND_CONVERTPDFTOXLS")]
        UPDFUND_CONVERTPDFTOXLS,

        [Description("UPDFUND_LOADBIGASSDATA")]
        UPDFUND_LOADBIGASSDATA,

        [Description("UPDFUND_ConvertXsToCsv")]
        UPDFUND_ConvertXsToCsv,

        [Description("UPDFUND_RunMappingSql")]
        UPDFUND_RunMappingSql,

        [Description("UPDFUND_LoadNewMappingSearch")]
        UPDFUND_LoadNewMappingSearch
    }

    class QualityControl
    {
        public async Task<int> CreateEvent(QualityActionsAliases alias, string qaData, TimeSpan duration)
        {
            Logger.Log(string.Format("{0}. {1}", alias.Description(), qaData));

            try
            {
                var rixtremaUrl = Properties.Settings.Default.AJAXFCTUrl;
                var httpResponseMessage = await rixtremaUrl.AppendPathSegment("AJAXFCT.aspx").PostUrlEncodedAsync(new
                {
                    Action = "CREATEEVENT",
                    Login = Properties.Settings.Default.AJAXFCTLogin,
                    Password = Properties.Settings.Default.AJAXFCTPassword,
                    Actor = "PlansParser",
                    Milestone = alias.Description(),
                    QAData = qaData
                }).ReceiveJson();
                if (httpResponseMessage.FCT.Result == "Success")
                {
                    return int.Parse(httpResponseMessage.FCT.Event.ID);
                }

                Logger.Log(string.Format("Error logging QAEvent. Response: {0}", httpResponseMessage));
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("Error logging QAEvent. Ex: {0}", e));
            }

            return 0;
        }
    }
}
