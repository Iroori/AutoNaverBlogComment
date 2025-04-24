using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using RJBlogProject.Common;
using RJBlogProject.Config;
using System;
using System.Collections.Generic;

namespace RJBlogProject.Service
{
    public class NaverBlogCommentService
    {
        private readonly IWebDriver _driver;
        private readonly AppSettings _settings;

        public NaverBlogCommentService(IWebDriver driver, AppSettings settings = null)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _settings = settings ?? AppSettings.Load();
        }

        /// <summary>
        /// 블로그의 최신 글로 이동합니다.
        /// </summary>
        public bool GoToLatestPost(string blogId = null)
        {
            try
            {
                blogId = blogId ?? _settings.DefaultBlogId;
                Logger.Info($"Navigating to latest post of blog: {blogId}");

                // 블로그 메인 페이지로 이동
                _driver.Navigate().GoToUrl($"https://blog.naver.com/{blogId}");
                _driver.WaitForPageLoad(3000);

                // iframe으로 전환
                _driver.SwitchToFrameSafely("mainFrame");

                // 최신 글로 이동 시도
                try
                {
                    var latestPostLink = _driver.WaitAndFindElement(By.XPath("//*[@id='prologue']/dl/dd[1]/ul/li[1]/a"));
                    _driver.ClickWithJavaScript(latestPostLink);
                    _driver.WaitForPageLoad(2000);
                    Logger.Info("Navigated to latest post successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to navigate to latest post", ex);
                    
                    // 다른 선택자로 재시도
                    try
                    {
                        var alternatePosts = _driver.WaitAndFindElements(By.CssSelector(".blog2_series_list .series_item a"));
                        if (alternatePosts.Count > 0)
                        {
                            _driver.ClickWithJavaScript(alternatePosts[0]);
                            _driver.WaitForPageLoad(2000);
                            Logger.Info("Navigated to latest post using alternate selector");
                            return true;
                        }
                    }
                    catch (Exception altEx)
                    {
                        Logger.Error("Failed with alternate selector too", altEx);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GoToLatestPost", ex);
                return false;
            }
        }

        /// <summary>
        /// 최신 댓글의 작성자 블로그를 방문하여 댓글을 작성합니다.
        /// </summary>
        public void ReplyToLatestComment(string commentText = null)
        {
            string originalWindow = _driver.CurrentWindowHandle;

            try
            {
                commentText = commentText ?? _settings.DefaultCommentText;
                Logger.Info("Looking for comments section...");

                // 댓글 섹션으로 이동
                var commentButton = _driver.WaitAndFindElement(By.CssSelector("a.btn_comment"));
                _driver.ClickWithJavaScript(commentButton);
                _driver.WaitForPageLoad(2000);

                // 댓글 목록 가져오기
                Logger.Info("Finding comments...");
                var commentsList = _driver.WaitAndFindElement(By.CssSelector("ul.u_cbox_list"));
                var comments = commentsList.FindElements(By.CssSelector("li"));
                
                if (comments.Count == 0)
                {
                    Logger.Warning("No comments found");
                    return;
                }

                Logger.Info($"Found {comments.Count} comments");
                ProcessComments(comments, commentText);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in ReplyToLatestComment", ex);
            }
            finally
            {
                try
                {
                    // 원래 창으로 돌아가기
                    if (_driver.WindowHandles.Count > 1)
                    {
                        _driver.SwitchTo().Window(originalWindow);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error switching back to original window", ex);
                }
            }
        }

        private void ProcessComments(IList<IWebElement> comments, string commentText)
        {
            int processedCount = 0;
            string originalWindow = _driver.CurrentWindowHandle;

            foreach (var comment in comments)
            {
                if (processedCount >= 5) // 최대 5개 댓글만 처리
                {
                    Logger.Info("Reached maximum comment processing limit");
                    break;
                }

                try
                {
                    Logger.Info($"Processing comment {processedCount + 1}");
                    
                    // 댓글 작성자 링크 찾기
                    var aTag = comment.FindElement(By.ClassName("u_cbox_name"));
                    var blogUrl = aTag.GetAttribute("href");
                    
                    if (string.IsNullOrEmpty(blogUrl))
                    {
                        Logger.Warning("Blog URL is empty, skipping");
                        continue;
                    }

                    Logger.Info($"Opening blog: {blogUrl}");
                    
                    // 새 탭에서 블로그 열기
                    ((IJavaScriptExecutor)_driver).ExecuteScript("window.open();");
                    var tabs = _driver.WindowHandles;
                    _driver.SwitchTo().Window(tabs[tabs.Count - 1]);
                    _driver.Navigate().GoToUrl(blogUrl);
                    _driver.WaitForPageLoad(2000);

                    // 블로그의 최신 글에 댓글 작성
                    if (PostCommentOnBlog(commentText))
                    {
                        processedCount++;
                    }

                    // 탭 닫기 및 원래 탭으로 전환
                    _driver.Close();
                    _driver.SwitchTo().Window(originalWindow);
                    _driver.WaitForPageLoad(1000);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error processing comment", ex);
                    
                    // 오류 발생 시 원래 창으로 복구 시도
                    try
                    {
                        if (_driver.WindowHandles.Count > 1)
                        {
                            _driver.Close();
                            _driver.SwitchTo().Window(originalWindow);
                        }
                    }
                    catch (Exception)
                    {
                        // 복구 실패 시 무시하고 계속 진행
                    }
                }
            }

            Logger.Info($"Successfully processed {processedCount} comments");
        }

        private bool PostCommentOnBlog(string commentText)
        {
            try
            {
                // iframe으로 전환
                _driver.SwitchToFrameSafely("mainFrame");
                
                // 최신 글의 댓글 버튼 찾기
                var postElement = _driver.FindElementSafely(By.XPath("//*[@id='post_1']"));
                if (postElement == null)
                {
                    Logger.Warning("Post element not found");
                    return false;
                }

                // 댓글 버튼 찾기 (동적 ID)
                var dynamicElement = postElement.FindElementSafely(By.XPath(".//a[contains(@id, 'Comi') and contains(@class, 'btn_comment')]"));
                if (dynamicElement == null)
                {
                    Logger.Warning("Comment button not found");
                    return false;
                }

                _driver.ClickWithJavaScript(dynamicElement);
                _driver.WaitForPageLoad(1500);

                // 댓글 입력 영역 찾기
                var divElement = _driver.WaitAndFindElement(By.XPath("//*[contains(@id,'write_textarea')]"));
                
                // 댓글 입력
                var actions = new Actions(_driver);
                actions.MoveToElement(divElement).Click()
                       .SendKeys(commentText)
                       .Perform();
                
                _driver.WaitForPageLoad(1000);

                // 댓글 등록 버튼 클릭
                var button = _driver.WaitAndFindElement(By.XPath("//*[@class='u_cbox_btn_upload' and @type='button']"));
                _driver.ClickWithJavaScript(button);
                
                _driver.WaitForPageLoad(1500);
                Logger.Info("Comment posted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error posting comment", ex);
                return false;
            }
        }
    }
}