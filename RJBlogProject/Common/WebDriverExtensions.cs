using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RJBlogProject.Common
{
    public static class WebDriverExtensions
    {
        /// <summary>
        /// 안전하게 요소를 찾아 반환합니다. 찾지 못하면 null을 반환합니다.
        /// </summary>
        public static IWebElement FindElementSafely(this IWebDriver driver, By by)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        /// <summary>
        /// 요소가 나타날 때까지 기다린 후 찾아 반환합니다.
        /// </summary>
        public static IWebElement WaitAndFindElement(this IWebDriver driver, By by, int timeoutInSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout waiting for element: {by.ToString()}. Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 요소 목록이 나타날 때까지 기다린 후 찾아 반환합니다.
        /// </summary>
        public static IList<IWebElement> WaitAndFindElements(this IWebDriver driver, By by, int timeoutInSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => {
                    var elements = drv.FindElements(by);
                    return elements.Count > 0 ? elements : null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout waiting for elements: {by.ToString()}. Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// JavaScript를 사용하여 요소를 클릭합니다.
        /// </summary>
        public static void ClickWithJavaScript(this IWebDriver driver, IWebElement element)
        {
            try
            {
                if (element == null)
                    throw new ArgumentNullException(nameof(element));
                
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clicking element with JavaScript: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// iframe으로 전환합니다.
        /// </summary>
        public static void SwitchToFrameSafely(this IWebDriver driver, string frameId, int timeoutInSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.Until(drv => drv.FindElement(By.CssSelector($"iframe#{frameId}")));
                driver.SwitchTo().Frame(frameId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error switching to frame {frameId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 명시적으로 지정된 시간 동안 대기합니다.
        /// </summary>
        public static void WaitForPageLoad(this IWebDriver driver, int milliseconds = 2000)
        {
            try
            {
                Thread.Sleep(milliseconds);
            }
            catch (ThreadInterruptedException ex)
            {
                Console.WriteLine($"Thread sleep interrupted: {ex.Message}");
            }
        }

        /// <summary>
        /// 클립보드를 통해 텍스트를 입력합니다. (네이버 로그인 우회)
        /// </summary>
        public static void InputTextViaClipboard(this IWebDriver driver, IWebElement element, string text)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            
            try
            {
                element.Click();
                System.Windows.Forms.Clipboard.SetText(text);
                var actions = new OpenQA.Selenium.Interactions.Actions(driver);
                actions.KeyDown(OpenQA.Selenium.Keys.Control)
                       .SendKeys("v")
                       .KeyUp(OpenQA.Selenium.Keys.Control)
                       .Perform();
                Thread.Sleep(500); // 입력 대기
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inputting text via clipboard: {ex.Message}");
                throw;
            }
        }
    }
}