using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using RJBlogProject.Common;
using RJBlogProject.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace RJBlogProject.Service
{
    public class NaverBlogCommentService
    {
        private readonly IWebDriver _driver;
        private readonly AppSettings _settings;
        private HashSet<string> _processedAuthors = new HashSet<string>(); // 이미 처리한 작성자 추적
        private List<string> _allCollectedAuthors = new List<string>(); // 전체 수집된 작성자 (임시 저장)
        private int _currentAuthorIndex = 0; // 현재 처리 중인 작성자 인덱스 (임시)
        private List<string> _failedCommentUrls = new List<string>(); // 댓글 작성 실패한 URL 목록
        private readonly string _failedUrlsFilePath; // 실패한 URL을 저장할 파일 경로

        public NaverBlogCommentService(IWebDriver driver, AppSettings settings = null)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _settings = settings ?? AppSettings.Load();
            
            // 실패한 URL을 저장할 파일 경로 초기화 (로그 디렉토리와 동일한 위치에 저장)
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            _failedUrlsFilePath = Path.Combine(logDir, $"FailedCommentUrls_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        }

        /// <summary>
        /// 특정 URL의 블로그 글로 이동합니다.
        /// </summary>
        /// <param name="postUrl">이동할 블로그 글의 URL</param>
        /// <returns>이동 성공 여부</returns>
        public bool GoToSpecificPost(string postUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(postUrl))
                {
                    Logger.Error("Post URL is empty or null");
                    return false;
                }
                
                Logger.Info($"Navigating to specific post: {postUrl}");
                
                // URL로 이동
                _driver.Navigate().GoToUrl(postUrl);
                _driver.WaitForPageLoad(3000);
                
                // 프레임 전환 시도
                try 
                {
                    _driver.SwitchToFrameSafely("mainFrame");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Could not switch to main frame: {ex.Message}");
                    // 프레임 전환 실패해도 계속 진행 시도
                }
                
                // 현재 URL 확인하여 올바른 글로 이동했는지 확인
                string currentUrl = _driver.Url.ToLower();
                string targetUrl = postUrl.ToLower();
                
                // URL 비교 시 쿼리 파라미터나 프로토콜 차이로 인한 불일치 방지를 위해
                // 두 URL이 동일한 포스트를 가리키는지 기본 경로 확인
                if (currentUrl.Equals(targetUrl) || 
                    (currentUrl.Contains(targetUrl) || targetUrl.Contains(currentUrl)))
                {
                    Logger.Info("Successfully navigated to the specific post");
                    return true;
                }
                else
                {
                    Logger.Warning($"Navigation might have failed, current URL: {currentUrl}, target URL: {targetUrl}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in GoToSpecificPost", ex);
                return false;
            }
        }

        /// <summary>
        /// 블로그의 최신 글로 이동합니다. 이미 최신글에 있는 경우 추가 작업을 하지 않습니다.
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
                
                // 현재 URL 확인하여 이미 최신글에 있는지 확인
                string currentUrl = _driver.Url.ToLower();
                Logger.Info($"Current URL: {currentUrl}");
                
                // 이미 PostView.naver 또는 PostList.naver에 있는 경우 추가 클릭이 필요 없음
                if (currentUrl.Contains("postview.naver") || 
                    (currentUrl.Contains("postlist.naver") && currentUrl.Contains("directaccess=true")))
                {
                    Logger.Info("Already on a post page, no need to navigate to latest post");
                    return true;
                }
                
                // iframe 소스 확인
                bool isAlreadyInPostView = false;
                try 
                {
                    var iframeSrc = _driver.FindElement(By.Id("mainFrame")).GetAttribute("src");
                    Logger.Info($"iframe src: {iframeSrc}");
                    
                    isAlreadyInPostView = iframeSrc.Contains("PostView.naver") || 
                        (iframeSrc.Contains("PostList.naver") && iframeSrc.Contains("directAccess=true"));
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to check iframe src: {ex.Message}");
                }
                
                if (isAlreadyInPostView)
                {
                    Logger.Info("Already on a post page (detected via iframe), no need to navigate to latest post");
                    return true;
                }

                // 최신 글로 이동 시도
                try
                {
                    // 수정된 XPath - 첫 번째 글 선택
                    var latestPostLink = _driver.WaitAndFindElement(By.XPath("//*[@id='prologue']/dl/dd[2]/p/a"));
                    _driver.ClickWithJavaScript(latestPostLink);
                    _driver.WaitForPageLoad(2000);
                    Logger.Info("Navigated to latest post successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to navigate to latest post with primary XPath", ex);
                    
                    // 다른 XPath 패턴으로 재시도
                    try {
                        var alternateLink = _driver.FindElementSafely(By.XPath("//*[@id='prologue']/dl/dd[1]/ul/li[1]/a"));
                        if (alternateLink != null)
                        {
                            _driver.ClickWithJavaScript(alternateLink);
                            _driver.WaitForPageLoad(2000);
                            Logger.Info("Navigated to latest post using alternate XPath");
                            return true;
                        }
                    }
                    catch (Exception) {
                        Logger.Warning("Failed with first alternate XPath too");
                    }
                    
                    // CSS 선택자로 재시도
                    try
                    {
                        var alternatePosts = _driver.WaitAndFindElements(By.CssSelector(".blog2_series_list .series_item a"));
                        if (alternatePosts.Count > 0)
                        {
                            _driver.ClickWithJavaScript(alternatePosts[0]);
                            _driver.WaitForPageLoad(2000);
                            Logger.Info("Navigated to latest post using alternate CSS selector");
                            return true;
                        }
                    }
                    catch (Exception altEx)
                    {
                        Logger.Error("Failed with all selectors", altEx);
                    }
                    
                    // 포스트 내에 있는지 확인 (댓글 버튼 존재 여부로 확인)
                    try
                    {
                        var commentButton = _driver.FindElementSafely(By.CssSelector("a.btn_comment"));
                        if (commentButton != null)
                        {
                            Logger.Info("Comment button found - already on a post page");
                            return true;
                        }
                    }
                    catch (Exception) { }
                    
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
        /// 특정 게시글의 모든 댓글 페이지를 탐색하여 각 댓글 작성자의 블로그에 방문하여 댓글을 답니다.
        /// </summary>
        /// <param name="postUrl">처리할 특정 블로그 글의 URL</param>
        /// <param name="commentText">작성할 댓글 내용</param>
        /// <returns>성공 여부</returns>
        public bool ProcessSpecificPost(string postUrl, string commentText = null)
        {
            string originalWindow = _driver.CurrentWindowHandle;

            try
            {
                commentText = commentText ?? _settings.DefaultCommentText;
                Logger.Info($"Processing specific post: {postUrl}");
                
                // 특정 글로 이동
                if (!GoToSpecificPost(postUrl))
                {
                    Logger.Error($"Failed to navigate to specific post: {postUrl}");
                    return false;
                }
                
                // 댓글 섹션으로 이동
                Logger.Info("Looking for comments section...");
                var commentButton = _driver.FindElementSafely(By.CssSelector("a.btn_comment"));
                
                if (commentButton == null)
                {
                    Logger.Warning("Comment button not found on this post");
                    RecordFailedUrl(postUrl, "Comment button not found");
                    return false;
                }
                
                _driver.ClickWithJavaScript(commentButton);
                _driver.WaitForPageLoad(2000);

                // 모든 페이지의 댓글 수집
                var allCommentAuthors = CollectAllCommentAuthors();
                
                if (allCommentAuthors.Count == 0)
                {
                    Logger.Warning("No comment authors found on this post");
                    return false;
                }

                Logger.Info($"Found {allCommentAuthors.Count} total comment authors across all pages");
                
                // 전체 수집된 작성자 목록 저장
                _allCollectedAuthors = allCommentAuthors;
                
                // 모든 작성자 처리 (인덱스 0부터 시작)
                ProcessCommentAuthors(allCommentAuthors, commentText, 0);
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in ProcessSpecificPost: {ex.Message}", ex);
                return false;
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

        /// <summary>
        /// 게시글의 모든 댓글 페이지를 탐색하여 각 댓글 작성자의 블로그에 방문하여 댓글을 답니다.
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

                // 모든 페이지의 댓글 수집
                var allCommentAuthors = CollectAllCommentAuthors();
                
                if (allCommentAuthors.Count == 0)
                {
                    Logger.Warning("No comment authors found");
                    return;
                }

                Logger.Info($"Found {allCommentAuthors.Count} total comment authors across all pages");
                
                // 전체 수집된 작성자 목록 저장 (임시)
                _allCollectedAuthors = allCommentAuthors;
                
                // 작성자 처리 (첫 50명은 건너뛰기)
                ProcessCommentAuthors(allCommentAuthors, commentText, 50);
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
        
        /// <summary>
        /// 특정 인덱스부터 계속 진행하는 임시 메서드
        /// </summary>
        public void ContinueProcessingFromIndex(int startIndex, string commentText = null)
        {
            if (_allCollectedAuthors.Count == 0)
            {
                Logger.Warning("No authors collected yet. Run ReplyToLatestComment first.");
                return;
            }
            
            commentText = commentText ?? _settings.DefaultCommentText;
            Logger.Info($"Continuing to process authors from index {startIndex}");
            
            // 특정 인덱스부터 처리 계속
            ProcessCommentAuthors(_allCollectedAuthors, commentText, startIndex);
        }

        /// <summary>
        /// 모든 댓글 페이지를 탐색하여 전체 댓글 작성자 목록을 수집합니다.
        /// 마지막 페이지부터 시작하여 이전 버튼을 클릭하며 모든 페이지를 순회합니다.
        /// </summary>
        private List<string> CollectAllCommentAuthors()
        {
            var allAuthors = new List<string>();
            bool hasMorePages = true;
            int pageCount = 1;
            
            try
            {
                // 현재 페이지의 댓글 작성자 수집
                int newAuthorsCount = CollectAuthorsFromCurrentPage(allAuthors);
                
                // 이전 페이지 버튼이 있는지 확인하고 있다면 클릭
                while (hasMorePages && newAuthorsCount > 0)
                {
                    Logger.Info($"Looking for previous page button (page {pageCount})");
                    
                    // 이전 버튼 찾기 (제공해주신 XPath 사용)
                    var prevButton = _driver.FindElementSafely(By.XPath("//*[contains(@id, 'naverComment_')]/div[1]/div/div[2]/a[1]"));
                    
                    // 이전 버튼이 없거나 비활성화되었다면 종료
                    if (prevButton == null || 
                        prevButton.GetAttribute("class").Contains("disabled") || 
                        "true".Equals(prevButton.GetAttribute("aria-disabled"), StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Info("No more previous pages found");
                        hasMorePages = false;
                        break;
                    }
                    
                    // 이전 버튼 클릭
                    _driver.ClickWithJavaScript(prevButton);
                    _driver.WaitForPageLoad(2000);
                    Logger.Info($"Navigated to previous comment page");
                    
                    // 현재 페이지의 댓글 작성자 수집
                    newAuthorsCount = CollectAuthorsFromCurrentPage(allAuthors);
                    
                    // 새로 수집된 작성자가 없으면 페이지 탐색 중단
                    if (newAuthorsCount == 0)
                    {
                        Logger.Info("No new authors found on this page, stopping collection");
                        break;
                    }
                    
                    pageCount++;
                }
                
                Logger.Info($"Collected {allAuthors.Count} unique comment authors from {pageCount} pages");
                return allAuthors;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error collecting all comment authors: {ex.Message}");
                return allAuthors; // 수집된 댓글이라도 반환
            }
        }

        /// <summary>
        /// 현재 페이지의 댓글 작성자를 수집합니다.
        /// </summary>
        /// <returns>새로 수집된 작성자 수</returns>
        private int CollectAuthorsFromCurrentPage(List<string> authors)
        {
            try
            {
                // 현재 페이지 번호 확인 시도
                string currentPageNum = "Unknown";
                try
                {
                    var activePage = _driver.FindElementSafely(By.CssSelector(".u_cbox_page .u_cbox_num.u_cbox_active"));
                    if (activePage != null)
                    {
                        currentPageNum = activePage.Text;
                    }
                }
                catch { /* 페이지 번호를 찾지 못해도 계속 진행 */ }
                
                Logger.Info($"Collecting authors from page {currentPageNum}");
                
                // 댓글 목록 가져오기
                var commentsList = _driver.WaitAndFindElement(By.CssSelector("ul.u_cbox_list"));
                var comments = commentsList.FindElements(By.CssSelector("li"));
                
                Logger.Info($"Found {comments.Count} comments on this page");
                
                int newAuthorsCount = 0;
                
                // 각 댓글에서 작성자 블로그 URL 추출
                foreach (var comment in comments)
                {
                    try
                    {
                        var authorElement = comment.FindElement(By.ClassName("u_cbox_name"));
                        var authorName = authorElement.Text;
                        var blogUrl = authorElement.GetAttribute("href");
                        
                        if (!string.IsNullOrEmpty(blogUrl) && !_processedAuthors.Contains(blogUrl))
                        {
                            authors.Add(blogUrl);
                            _processedAuthors.Add(blogUrl); // 처리한 작성자 추적
                            newAuthorsCount++;
                            Logger.Info($"Added author: {authorName} with URL: {blogUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Failed to extract author from comment: {ex.Message}");
                    }
                }
                
                Logger.Info($"Added {newAuthorsCount} new authors from this page");
                return newAuthorsCount;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error collecting authors from current page: {ex.Message}");
                return 0; // 오류 발생 시 0 반환
            }
        }

        /// <summary>
        /// 수집된 댓글 작성자의 블로그를 순차적으로 방문하여 댓글을 답니다.
        /// 시작 인덱스를 지정하여 특정 위치부터 처리할 수 있습니다.
        /// </summary>
        private void ProcessCommentAuthors(List<string> authorUrls, string commentText, int startIndex = 0)
        {
            int processedCount = 0;
            int maxAuthors = authorUrls.Count; // 최대 제한 없음 (임시)
            string originalWindow = _driver.CurrentWindowHandle;

            // 시작 인덱스가 유효한지 확인
            if (startIndex >= authorUrls.Count)
            {
                Logger.Warning($"Start index {startIndex} is greater than or equal to the number of authors ({authorUrls.Count})");
                return;
            }

            Logger.Info($"Starting to process authors from index {startIndex} (total authors: {authorUrls.Count})");

            for (int i = startIndex; i < authorUrls.Count; i++)
            {
                var blogUrl = authorUrls[i];
                _currentAuthorIndex = i; // 현재 처리 중인 인덱스 저장 (임시)
                
                try
                {
                    Logger.Info($"Processing author {i+1}/{authorUrls.Count}: {blogUrl}");
                    
                    // 새 탭에서 블로그 열기
                    ((IJavaScriptExecutor)_driver).ExecuteScript("window.open();");
                    var tabs = _driver.WindowHandles;
                    _driver.SwitchTo().Window(tabs[tabs.Count - 1]);
                    _driver.Navigate().GoToUrl(blogUrl);
                    _driver.WaitForPageLoad(2000);

                    // 블로그의 최신 글에 댓글 작성
                    if (FindLatestPostAndComment(commentText))
                    {
                        processedCount++;
                        Logger.Info($"Successfully commented on blog: {blogUrl}");
                    }
                    else
                    {
                        Logger.Warning($"Failed to comment on blog: {blogUrl}");
                        RecordFailedUrl(blogUrl, "Failed to post comment");
                    }

                    // 탭 닫기 및 원래 탭으로 전환
                    _driver.Close();
                    _driver.SwitchTo().Window(originalWindow);
                    _driver.WaitForPageLoad(1000);
                    
                    // 일정 시간 대기 (봇 탐지 방지) - 주석 처리 (임시)
                    // int waitTime = new Random().Next(3000, 7000);
                    // Thread.Sleep(waitTime);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error processing author: {ex.Message}");
                    
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

            Logger.Info($"Successfully processed {processedCount} out of {authorUrls.Count - startIndex} authors (starting from index {startIndex})");
        }

        /// <summary>
        /// 댓글 작성에 실패한 URL을 기록합니다.
        /// </summary>
        private void RecordFailedUrl(string url, string reason = null)
        {
            if (string.IsNullOrEmpty(url))
                return;
                
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{url}|{reason ?? "Unknown reason"}";
            
            // 목록에 추가
            _failedCommentUrls.Add(logEntry);
            
            // 파일에 기록
            try
            {
                File.AppendAllText(_failedUrlsFilePath, logEntry + Environment.NewLine);
                Logger.Info($"Recorded failed URL: {url}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error recording failed URL to file: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 실패한 URL 목록을 CSV 파일로 내보냅니다.
        /// </summary>
        public void ExportFailedUrlsToCsv(string filePath = null)
        {
            if (_failedCommentUrls.Count == 0)
            {
                Logger.Info("No failed URLs to export");
                return;
            }
            
            string exportPath = filePath ?? Path.Combine(
                Path.GetDirectoryName(_failedUrlsFilePath),
                $"FailedUrls_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            );
            
            try
            {
                // CSV 헤더 추가
                List<string> lines = new List<string> { "Timestamp,URL,FailureReason" };
                
                // 각 실패 항목을 CSV 형식으로 변환
                foreach (string entry in _failedCommentUrls)
                {
                    string[] parts = entry.Split('|');
                    if (parts.Length >= 3)
                    {
                        string timestamp = parts[0];
                        string url = parts[1];
                        string reason = parts[2];
                        
                        // CSV 이스케이프 처리
                        url = $"\"{url.Replace("\"", "\"\"")}\"";
                        reason = $"\"{reason.Replace("\"", "\"\"")}\"";
                        
                        lines.Add($"{timestamp},{url},{reason}");
                    }
                }
                
                // 파일에 저장
                File.WriteAllLines(exportPath, lines);
                Logger.Info($"Exported {_failedCommentUrls.Count} failed URLs to: {exportPath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error exporting failed URLs to CSV: {ex.Message}");
            }
        }

        /// <summary>
        /// 현재 처리 상태를 출력합니다 (임시 디버깅 용도)
        /// </summary>
        public void PrintStatus()
        {
            Logger.Info("=== CURRENT STATUS ===");
            Logger.Info($"Total collected authors: {_allCollectedAuthors.Count}");
            Logger.Info($"Current author index: {_currentAuthorIndex}");
            Logger.Info($"Processed authors count: {_processedAuthors.Count}");
            Logger.Info($"Failed comment URLs count: {_failedCommentUrls.Count}");
            Logger.Info($"Failed URLs file: {_failedUrlsFilePath}");
            
            if (_allCollectedAuthors.Count > 0)
            {
                Logger.Info("First few author URLs:");
                for (int i = 0; i < Math.Min(5, _allCollectedAuthors.Count); i++)
                {
                    Logger.Info($"  {i}: {_allCollectedAuthors[i]}");
                }
                
                Logger.Info("Last few author URLs:");
                for (int i = Math.Max(0, _allCollectedAuthors.Count - 5); i < _allCollectedAuthors.Count; i++)
                {
                    Logger.Info($"  {i}: {_allCollectedAuthors[i]}");
                }
            }
            
            if (_failedCommentUrls.Count > 0)
            {
                Logger.Info($"Recent failed comment URLs (showing last {Math.Min(3, _failedCommentUrls.Count)}):");
                for (int i = Math.Max(0, _failedCommentUrls.Count - 3); i < _failedCommentUrls.Count; i++)
                {
                    Logger.Info($"  {i}: {_failedCommentUrls[i]}");
                }
            }
            
            Logger.Info("=====================");
        }

        private bool FindLatestPostAndComment(string commentText)
        {
            try
            {
                // iframe으로 전환
                _driver.SwitchToFrameSafely("mainFrame");
                
                // 현재 URL 확인하여 이미 최신글에 있는지 확인
                string currentUrl = _driver.Url.ToLower();
                
                // 이미 PostView.naver 또는 PostList.naver에 있는 경우 추가 클릭이 필요 없음
                if (currentUrl.Contains("postview.naver") || 
                    (currentUrl.Contains("postlist.naver") && currentUrl.Contains("directaccess=true")))
                {
                    Logger.Info("Already on a post page, proceeding to comment");
                    return PostCommentOnCurrentPost(commentText);
                }
                
                // 최신 글 찾기 (다양한 XPath 패턴 시도)
                IWebElement latestPost = null;
                
                // 1. 첫 번째 패턴 시도 (dd[1]/p/a)
                latestPost = _driver.FindElementSafely(By.XPath("//*[@id='prologue']/dl/dd[1]/p/a"));
                
                // 2. 두 번째 패턴 시도 (dd[1]/ul/li[1]/a)
                if (latestPost == null)
                {
                    latestPost = _driver.FindElementSafely(By.XPath("//*[@id='prologue']/dl/dd[1]/ul/li[1]/a"));
                }
                
                // 3. 세 번째 패턴 시도 (CSS 선택자)
                if (latestPost == null)
                {
                    var posts = _driver.FindElementSafely(By.CssSelector(".blog2_series_list .series_item:first-child a"));
                    if (posts != null)
                    {
                        latestPost = posts;
                    }
                }
                
                // 포스트 내에 있는지 확인 (댓글 버튼 존재 여부로 확인)
                if (latestPost == null)
                {
                    var commentButton = _driver.FindElementSafely(By.CssSelector("a.btn_comment"));
                    if (commentButton != null)
                    {
                        Logger.Info("Comment button found - already on a post page");
                        return PostCommentOnCurrentPost(commentText);
                    }
                    
                    Logger.Warning("Latest post not found on this blog");
                    RecordFailedUrl(_driver.Url, "Latest post not found");
                    return false;
                }
                
                // 최신 글로 이동
                _driver.ClickWithJavaScript(latestPost);
                _driver.WaitForPageLoad(2000);
                
                // 댓글 작성
                return PostCommentOnCurrentPost(commentText);
            }
            catch (Exception ex)
            {
                Logger.Error("Error finding latest post", ex);
                return false;
            }
        }

        private bool PostCommentOnCurrentPost(string commentText)
        {
            try
            {
                Logger.Info("Attempting to post comment on current post");
                
                // 댓글 버튼 찾기 (다양한 패턴 시도)
                IWebElement commentButton = null;
                
                // 1. 일반적인 댓글 버튼 패턴
                commentButton = _driver.FindElementSafely(By.CssSelector("a.btn_comment"));
                
                // 2. 동적 ID를 가진 댓글 버튼 패턴
                if (commentButton == null)
                {
                    commentButton = _driver.FindElementSafely(By.XPath("//a[contains(@id, 'Comi') and contains(@class, 'btn_comment')]"));
                }
                
                // 댓글 버튼을 찾지 못한 경우
                if (commentButton == null)
                {
                    Logger.Warning("Comment button not found");
                    RecordFailedUrl(_driver.Url, "Comment button not found");
                    return false;
                }

                _driver.ClickWithJavaScript(commentButton);
                _driver.WaitForPageLoad(2000);
                Logger.Info("Clicked comment button");

                // 댓글 작성을 위한 여러 패턴 시도
                try
                {
                    // 1. 제공된 네이버 블로그 댓글 XPath 패턴
                    // 댓글 ID가 동적으로 변경될 수 있으므로 패턴 사용
                    var commentForm = _driver.FindElementSafely(By.XPath("//*[contains(@id, 'naverComment_')]/div/div[5]/div[1]/form/fieldset"));
                    
                    if (commentForm != null)
                    {
                        Logger.Info("Found comment form with specific XPath pattern");
                        
                        // 댓글 입력 영역 찾기
                        var textArea = commentForm.FindElementSafely(By.XPath(".//div/div/div[contains(@class, 'u_cbox_text')]"));
                        if (textArea != null)
                        {
                            // 댓글 입력
                            var actions = new Actions(_driver);
                            actions.MoveToElement(textArea).Click()
                                   .SendKeys(commentText)
                                   .Perform();
                            
                            _driver.WaitForPageLoad(1000);
                            Logger.Info("Entered comment text");
                            
                            // 등록 버튼 클릭
                            var submitButton = commentForm.FindElementSafely(By.XPath(".//div/div/div[6]/button"));
                            if (submitButton != null)
                            {
                                _driver.ClickWithJavaScript(submitButton);
                                _driver.WaitForPageLoad(2000);
                                Logger.Info("Comment posted with specific XPath pattern");
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Couldn't post comment with specific XPath pattern: {ex.Message}");
                }

                // 2. 기존 방식으로 시도
                try
                {
                    var textArea = _driver.FindElementSafely(By.XPath("//*[contains(@id,'write_textarea')]"));
                    if (textArea != null)
                    {
                        var actions = new Actions(_driver);
                        actions.MoveToElement(textArea).Click()
                               .SendKeys(commentText)
                               .Perform();
                        
                        _driver.WaitForPageLoad(1000);
                        Logger.Info("Entered comment text with alternate method");
                        
                        var uploadButton = _driver.FindElementSafely(By.XPath("//*[@class='u_cbox_btn_upload' and @type='button']"));
                        if (uploadButton != null)
                        {
                            _driver.ClickWithJavaScript(uploadButton);
                            _driver.WaitForPageLoad(2000);
                            Logger.Info("Comment posted with alternate method");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Couldn't post comment with alternate method: {ex.Message}");
                }

                // 3. 일반적인 CSS 선택자로 시도
                try
                {
                    var textArea = _driver.FindElementSafely(By.CssSelector(".u_cbox_text"));
                    if (textArea != null)
                    {
                        var actions = new Actions(_driver);
                        actions.MoveToElement(textArea).Click()
                               .SendKeys(commentText)
                               .Perform();
                        
                        _driver.WaitForPageLoad(1000);
                        
                        var uploadButton = _driver.FindElementSafely(By.CssSelector(".u_cbox_btn_upload"));
                        if (uploadButton != null)
                        {
                            _driver.ClickWithJavaScript(uploadButton);
                            _driver.WaitForPageLoad(2000);
                            Logger.Info("Comment posted with CSS selector method");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"All comment posting methods failed: {ex.Message}");
                }
                
                Logger.Warning("Failed to post comment after trying multiple patterns");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error in PostCommentOnCurrentPost", ex);
                return false;
            }
        }
    }
}