using System;
using System.IO;
using System.Threading;

namespace PlansParser
{
    public class Logger
    {
        private static readonly ReaderWriterLockSlim FileLock = new ReaderWriterLockSlim();

        private static Action<object> _extraAction;

        public static void SetAdditionalOutputForLogger(Action<object> action)
        {
            _extraAction = action;
        }

        public static void Log(object format)
        {

            FileLock.EnterWriteLock();
            try
            {
                var contents = string.Format("{0}{1}  Thread: {2}. {3}. Memory usage {4}kb", Environment.NewLine,
                    DateTime.Now,
                    Thread.CurrentThread.ManagedThreadId,
                    format,
                    GC.GetTotalMemory(false) / 1000);
                var logsFolder = Path.Combine(Constants.BaseDir, @"logs");
                if (!Directory.Exists(logsFolder)) Directory.CreateDirectory(logsFolder);
                File.AppendAllText(Path.Combine(logsFolder, "rc_log_" + DateTime.Today.ToString("yyyyMMdd") + ".txt"), contents);

                if (_extraAction != null)
                {
                    try
                    {
                        _extraAction(format);
                    }
                    catch (Exception e)
                    {
                    }

                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                FileLock.ExitWriteLock();
            }

        }
    }
}