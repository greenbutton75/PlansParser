using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace PlansParser
{
    public class FlurlHelper
    {
        private static Action<object> _extraAction;

        public static void SetAdditionalOutputForLogger(Action<object> action)
        {
            _extraAction = action;
        }

        public static async Task<dynamic> RunActionOnRixtremaWsWithRetry(string actionName, object additionalData = null, int retryTimeInSec = 5, int retryCount = 5)
        {
            try
            {
                return await Retry.DoAsync(() => RunActionOnRixtremaWs(actionName, additionalData), TimeSpan.FromSeconds(retryTimeInSec), retryCount);
            }
            catch (Exception e)
            {
                var errorString = string.Format("Error process {0} action. Ex: {1}", actionName, e);
                Logger.Log(errorString);
                if (_extraAction != null) _extraAction(string.Format("Error process {0} action.", actionName));
                return null;
            }
        }

        private static async Task<dynamic> RunActionOnRixtremaWs(string actionName, object additionalData = null)
        {
            var rixtremaUrl = Properties.Settings.Default.AJAXFCTUrl;
            var data = new
            {
                Action = actionName,
                Login = Properties.Settings.Default.AJAXFCTLogin,
                Password = Properties.Settings.Default.AJAXFCTPassword,
            };

            var queryParams = rixtremaUrl.AppendPathSegment("AJAXFCT.aspx").SetQueryParams(data).WithTimeout(TimeSpan.FromMinutes(15));
            var httpResponseMessage = await queryParams.PostUrlEncodedAsync(additionalData ?? new object()).ReceiveJson();

            if (httpResponseMessage.FCT.Result != "Success")
            {
                var errorString = string.Format("Error process {0} action. Response: {1}", actionName, ObjectExtentions.ToJson(httpResponseMessage));
                Logger.Log(errorString);
                if (_extraAction != null) _extraAction(string.Format("Warning process {0} action.", actionName));
                throw new Exception(errorString);
            }

            if (_extraAction != null) _extraAction(string.Format("{0} success sent to server.", actionName));
            return httpResponseMessage;
        }
    }
}