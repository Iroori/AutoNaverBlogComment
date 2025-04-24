using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace RJBlogProject.Common
{
    public static class WebDriverFactory
    {
        /// <summary>
        /// Chrome 웹 드라이버를 생성하고 초기화합니다.
        /// </summary>
        public static IWebDriver CreateChromeDriver(bool headless = false)
        {
            try
            {
                var options = new ChromeOptions();
                
                if (headless)
                {
                    options.AddArgument("--headless");
                }
                
                // 기타 옵션 설정
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-notifications");
                
                var driver = new ChromeDriver(options);
                
                // 페이지 로드 타임아웃 설정
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                
                return driver;
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating Chrome driver", ex);
                throw;
            }
        }
        
        /// <summary>
        /// 웹 드라이버를 안전하게 종료합니다.
        /// </summary>
        public static void QuitDriver(IWebDriver driver)
        {
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error quitting driver", ex);
                }
            }
        }
    }
}