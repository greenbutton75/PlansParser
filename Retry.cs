using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlansParser
{
    public static class Retry
    {
        public static void Do(
            Action action,
            TimeSpan retryInterval,
            int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                        Task.Delay(retryInterval.Milliseconds);
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            Logger.Log(string.Format("Error process {0} action. Retry again. Ex: {1}", action.Method.Name, new AggregateException(exceptions)));
            throw new AggregateException(exceptions);
        }

        public static async Task<T> DoAsync<T>(
            Func<Task<T>> action,
            TimeSpan retryInterval,
            int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                        await Task.Delay(retryInterval.Milliseconds);
                    return await action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            Logger.Log(string.Format("Error process {0} action. Retry again. Ex: {1}", action.Method.Name, new AggregateException(exceptions)));
            throw new AggregateException(exceptions);
        }
    }
}