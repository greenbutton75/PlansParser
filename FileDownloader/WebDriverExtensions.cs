using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Keys = OpenQA.Selenium.Keys;

namespace PlansParser.FileDownloader
{
    public static class WebDriverExtensions
    {
        public static void eprint(this IWebElement element, string text)
        {
            element.Clear();
            element.SendKeys(Keys.Control + "a");
            element.SendKeys(Keys.Delete);
            element.SendKeys(text);
        }
        public static void SafeClick(this IWebDriver driver, IWebElement element)
        {
            if (IsElementPresent(driver, element))
            {
                element.Click();
            }
        }
        public static void jsClick(this IWebDriver driver, IWebElement element, IJavaScriptExecutor js)
        {
            js.ExecuteScript("var element = arguments[0]; element.click()", element);
        }
        public static bool IsElementPresent(this IWebDriver driver, IWebElement element, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                try
                { 
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                    wait.Until(drv =>
                    {
                        return IsElementPresent(drv, element);
                    });
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return IsElementPresent(driver, element);
        }
        public static bool IsElementPresent(this IWebDriver driver, IWebElement element)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
            try
            {
                string tagname = element.TagName;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(20));
            }
        }
        public static bool IsElementPresent(this IWebDriver driver, By by)
        {
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));

            try
            {
                IWebElement el = driver.FindElement(by);
                return driver.IsElementPresent(el);
            }
            catch
            {
                return false;
            }
            finally
            {
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(20));
            }
        }
        public static bool IsElementVisible(this IWebDriver driver, IWebElement element)
        {
            return element.Displayed && element.Enabled;
        }
        public static IWebElement Wait_Visible(this IWebDriver driver, IWebElement element, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.Until(drv =>
                {
                    if (drv.IsElementVisible(element))
                        return element;
                    else
                        return null;
                });
            }
            return element;
        }

        public static IWebElement Wait_IsNotVisible(this IWebDriver driver, IWebElement element, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                    wait.Until(drv =>
                    {
                        if (IsElementPresent(drv, element, timeoutInSeconds))
                        {
                            if (drv.IsElementVisible(element))
                                return null;
                            else
                                return element;
                        }
                        else
                            return null;
                    });
                }
                catch
                {
                    // ignore exceptions
                }
            }
            return element;
        }

        public static string download(this IWebDriver driver, string nameOfFile, string downloadDir)
        {
            const uint WM_SETTEXT = 0x000C;
            const uint WM_LBUTTONDOWN = 0x0201;
            const uint WM_LBUTTONUP = 0x0202;

            string path = Path.Combine(downloadDir, nameOfFile);

            if (!File.Exists(path))
            {
                //IntPtr ptr = WinAPI.FindWindow("Chrome_WidgetWin_1", null);
                IntPtr ptr = IntPtr.Zero;

                int sleepIterator = 100;
                int wait = 10000;

                do
                {
                    ptr = WinAPI.FindWindow(null, "Сохранение");

                    if (ptr.ToInt32() == 0)
                        ptr = WinAPI.FindWindow(null, "Save as");

                    Thread.Sleep(sleepIterator);
                    wait = wait - sleepIterator;
                }
                while (ptr.ToInt32() == 0 && wait > 0);

                if (wait <= 0) return null;

                if (ptr.ToInt32() != 0)
                {
                    IntPtr tempWindow = WinAPI.FindWindowEx(ptr, IntPtr.Zero, "DUIViewWndClassName", null);
                    Console.WriteLine("tempWindow {0}", tempWindow.ToInt32());

                    tempWindow = WinAPI.FindWindowEx(tempWindow, IntPtr.Zero, "DirectUIHWND", null);
                    tempWindow = WinAPI.FindWindowEx(tempWindow, IntPtr.Zero, "FloatNotifySink", null);
                    tempWindow = WinAPI.FindWindowEx(tempWindow, IntPtr.Zero, "ComboBox", null);
                    tempWindow = WinAPI.FindWindowEx(tempWindow, IntPtr.Zero, "Edit", null);
                    WinAPI.SendMessage(tempWindow, Convert.ToInt32(WM_SETTEXT), IntPtr.Zero, @path);


                    tempWindow = WinAPI.FindWindowEx(ptr, IntPtr.Zero, "Button", null);
                    WinAPI.SendMessage(tempWindow, Convert.ToInt32(WM_LBUTTONDOWN), IntPtr.Zero, null);
                    WinAPI.SendMessage(tempWindow, Convert.ToInt32(WM_LBUTTONUP), IntPtr.Zero, null);
                    
                    var fileExist = false;
                    
                    sleepIterator = 100;
                    wait = 1200000;

                    do
                    {
                        Application.DoEvents();
                        fileExist = File.Exists(path);
                        Thread.Sleep(sleepIterator);

                        wait = wait - sleepIterator;
                        
                    }
                    while (!fileExist && wait > 0);

                    if (wait <= 0) return null;

                    
                }
            }

            return path;
        }

        public static void hClick(this IWebDriver driver, IWebElement webElement)
        {

            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript(
                        "arguments[0].scrollIntoView(true);", webElement);
            }
            catch (Exception e)
            {

            }
            

            webElement.Click();

        }
    }
    public static class WinAPI
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(
          IntPtr hwndParent,
          IntPtr hwndChildAfter,
          string lpszClass,
          string lpszWindow
        );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr HWnd, GetWindow_Cmd cmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6,
            WM_GETTEXT = 0x000D
        }
    }
}
