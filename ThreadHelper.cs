using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlansParser
{
    public class ThreadHelper
    {
        public static List<Thread> RunAsyncMultiThreadAction<T>(ParameterizedThreadStart action, IEnumerable<T> param,
            int threadCount)
        {
            IList<T> list = param as IList<T> ?? param.ToList();

            var batchCount = list.Count / threadCount;

            if (batchCount <= 1)
            {
                batchCount = 1;
            }

            var threads = new List<Thread>();
            foreach (var paramsBatch in list.Batch(batchCount))
            {
                var thread = new Thread(action)
                {
                    CurrentCulture = new CultureInfo(1033) { NumberFormat = { NumberGroupSeparator = "" } }
                };

                thread.Start(paramsBatch);

                threads.Add(thread);
            }

            return threads;
        }


        public static Thread RunAsyncAction<T>(ParameterizedThreadStart action, T param)
        {
            var thread = new Thread(action)
            {
                CurrentCulture = new CultureInfo(1033) { NumberFormat = { NumberGroupSeparator = "" } }
            };

            thread.Start(param);

            return thread;
        }

        public static async Task WaitAllThreadAsync(List<Thread> threads)
        {
            await Task.Run(() =>
          {
              foreach (var thread in threads)
              {
                  thread.Join();
              }
          });
        }

        public static void RunAsyncOnEndAction(List<Thread> threads, Action action)
        {
            var thread = new Thread(OnEndAction)
            {
                CurrentCulture = new CultureInfo(1033) { NumberFormat = { NumberGroupSeparator = "" } }
            };

            var args = new object[2];
            args[0] = threads;
            args[1] = action;
            thread.Start(args);
        }

        private static void OnEndAction(object argsObj)
        {
            var args = (object[])argsObj;
            var threads = (List<Thread>)args[0];
            var action = (Action)args[1];


            foreach (var thread in threads)
            {
                thread.Join();
            }

            action();
        }
    }
}