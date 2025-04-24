using OpenQA.Selenium;
using RJBlogProject.Common;
using RJBlogProject.Config;
using System;

namespace RJBlogProject.Service
{
    public class NaverLoginService : IDisposable
    {
        private readonly AppSettings _settings;
        public IWebDriver Driver { get; private set; }

        public NaverLoginService(AppSettings settings = null)
        {
            _settings = settings ?? AppSettings.Load();
        }

        public void OpenWebMode(bool headless = false)
        {
            try
            {
                Logger.Info("Initializing web driver...");
                Driver = WebDriverFactory.CreateChromeDriver(headless);
                Logger.Info("Web driver initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize web driver", ex);
                throw;
            }
        }

        public void CloseBrowser()
        {
            WebDriverFactory.QuitDriver(Driver);
            Driver = null;
            Logger.Info("Browser closed");
        }

        public bool Login(string userId = null, string password = null)
        {
            try
            {
                // 설정에서 ID와 비밀번호를 사용하거나 제공된 값 사용
                userId = userId ?? _settings.NaverId;
                password = password ?? _settings.NaverPassword;

                Logger.Info($"Navigating to Naver login page...");
                Driver.Navigate().GoToUrl("https://nid.naver.com/nidlogin.login");
                Driver.WaitForPageLoad(2000);

                // 아이디 입력
                var idInput = Driver.WaitAndFindElement(By.Id("id"));
                Driver.InputTextViaClipboard(idInput, userId);

                // 패스워드 입력
                var pwInput = Driver.WaitAndFindElement(By.Id("pw"));
                Driver.InputTextViaClipboard(pwInput, password);

                // 로그인 버튼 클릭
                var loginButton = Driver.WaitAndFindElement(By.Id("log.login"));
                Driver.ClickWithJavaScript(loginButton);

                Driver.WaitForPageLoad(2000);

                // 로그인 확인
                bool isLoggedIn = IsLoggedIn();
                if (isLoggedIn)
                {
                    Logger.Info("Login successful");
                    HandleLoginPopups();
                }
                else
                {
                    Logger.Warning("Login may have failed");
                }

                return isLoggedIn;
            }
            catch (Exception ex)
            {
                Logger.Error("Error during login", ex);
                return false;
            }
        }

        public void GoToBlog(string blogId = null)
        {
            try
            {
                blogId = blogId ?? _settings.DefaultBlogId;
                Logger.Info($"Navigating to blog: {blogId}");
                
                Driver.Navigate().GoToUrl($"https://blog.naver.com/{blogId}");
                Driver.WaitForPageLoad(2000);

                Logger.Info("Blog navigation successful");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error navigating to blog", ex);
                throw;
            }
        }

        private bool IsLoggedIn()
        {
            // 로그인 실패 페이지 여부 확인
            var errorElement = Driver.FindElementSafely(By.ClassName("error"));
            if (errorElement != null && errorElement.Displayed)
            {
                return false;
            }

            // 로그인 성공 여부 확인 (URL 또는 특정 요소 존재 확인)
            return true;
        }

        private void HandleLoginPopups()
        {
            try
            {
                // '등록하지 않음' 버튼이나 다른 팝업 처리
                var cancelBtn = Driver.FindElementSafely(By.CssSelector("span.btn_cancel"));
                if (cancelBtn != null && cancelBtn.Displayed)
                {
                    Driver.ClickWithJavaScript(cancelBtn);
                    Logger.Info("Popup handled");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"No popup found or error handling popup: {ex.Message}");
            }
        }

        public void Dispose()
        {
            CloseBrowser();
        }
    }
}