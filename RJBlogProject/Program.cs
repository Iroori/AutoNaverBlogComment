using RJBlogProject.Common;
using RJBlogProject.Config;
using RJBlogProject.Service;
using RJBlogProject.UI;
using System;
using System.Threading;
using System.Windows.Forms;

namespace RJBlogProject
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Logger.Info("=== RJBlogProject Started ===");
                
                // UI 모드로 실행할지 콘솔 모드로 실행할지 선택
                bool useUI = true; // 기본값은 UI 모드

                // 명령행 인수가 있는 경우 처리
                if (args.Length > 0)
                {
                    if (args[0].ToLower() == "-console" || args[0].ToLower() == "/console")
                    {
                        useUI = false;
                    }
                }
                
                if (useUI)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                    return;
                }
                
                // 설정 불러오기
                var settings = AppSettings.Load();
                
                // 계정 정보 확인
                if (string.IsNullOrEmpty(settings.NaverId) || settings.NaverId == "YOUR_ID_HERE" || 
                    string.IsNullOrEmpty(settings.NaverPassword) || settings.NaverPassword == "YOUR_PASSWORD_HERE")
                {
                    Console.WriteLine("계정 정보가 설정되지 않았습니다. App.config 파일을 확인하세요.");
                    Console.WriteLine("현재 설정:");
                    Console.WriteLine($"NaverId: {settings.NaverId}");
                    Console.WriteLine($"NaverPassword: {(string.IsNullOrEmpty(settings.NaverPassword) ? "설정되지 않음" : "********")}");
                    Console.WriteLine($"DefaultBlogId: {settings.DefaultBlogId}");
                    
                    Console.WriteLine("\n계정 정보를 지금 입력하시겠습니까? (Y/N)");
                    var answer = Console.ReadLine();
                    
                    if (answer?.ToUpper() == "Y")
                    {
                        Console.Write("네이버 아이디: ");
                        settings.NaverId = Console.ReadLine();
                        
                        Console.Write("네이버 비밀번호: ");
                        settings.NaverPassword = Console.ReadLine();
                        
                        Console.Write("블로그 ID (기본값: rcolfree): ");
                        var inputBlogId = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(inputBlogId))
                        {
                            settings.DefaultBlogId = inputBlogId;
                        }
                        
                        // 사용자 설정 저장
                        settings.Save();
                        Logger.Info("사용자 설정이 저장되었습니다.");
                        
                        Console.WriteLine("설정이 저장되었습니다. 프로그램을 계속 실행합니다.");
                    }
                    else
                    {
                        Console.WriteLine("계정 정보 없이는 프로그램을 실행할 수 없습니다. 프로그램을 종료합니다.");
                        return;
                    }
                }

                // 작업 선택 메뉴
                Console.WriteLine("\n작업 선택:");
                Console.WriteLine("1. 최신글 처음부터 시작 (최대 50개 댓글 작성자 처리)");
                Console.WriteLine("2. 최신글 51번째부터 계속 진행");
                Console.WriteLine("3. 특정 글 URL 처리");
                Console.Write("선택 (1, 2 또는 3): ");
                var choice = Console.ReadLine();

                if (choice == "1")
                {
                    // 블로그 자동화 실행 (처음부터 시작)
                    ExecuteBlogAutomation(settings);
                }
                else if (choice == "2")
                {
                    // 51번째부터 계속 진행
                    ContinueFromIndex51(settings);
                }
                else if (choice == "3")
                {
                    // 특정 글 처리
                    Console.Write("\n처리할 블로그 글 URL: ");
                    string postUrl = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(postUrl))
                    {
                        Console.WriteLine("URL이 입력되지 않았습니다. 프로그램을 종료합니다.");
                        return;
                    }
                    
                    // 설정에 URL 저장
                    settings.SpecificPostUrl = postUrl;
                    settings.UseSpecificPost = true;
                    settings.Save();
                    
                    // 특정 글 처리 실행
                    ProcessSpecificPost(settings, postUrl);
                }
                else
                {
                    Console.WriteLine("잘못된 선택입니다. 프로그램을 종료합니다.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("프로그램 실행 중 오류가 발생했습니다", ex);
                Console.WriteLine($"오류가 발생했습니다: {ex.Message}");
                Console.WriteLine("자세한 내용은 로그 파일을 확인하세요.");
            }
            finally
            {
                Logger.Info("=== RJBlogProject Finished ===");
                Console.WriteLine("\n프로그램이 완료되었습니다. 종료하려면 아무 키나 누르세요...");
                Console.ReadKey();
            }
        }

        static void ExecuteBlogAutomation(AppSettings settings)
        {
            using (var naverService = new NaverLoginService(settings))
            {
                try
                {
                    // 웹 드라이버 초기화 및 로그인
                    naverService.OpenWebMode();
                    
                    if (!naverService.Login())
                    {
                        Logger.Error("로그인에 실패했습니다. 프로그램을 종료합니다.");
                        return;
                    }
                    
                    // 블로그로 이동
                    naverService.GoToBlog();
                    
                    // 블로그 댓글 서비스 생성 및 실행
                    var blogService = new NaverBlogCommentService(naverService.Driver, settings);
                    
                    // 최신 글로 이동
                    if (blogService.GoToLatestPost())
                    {
                        // 최신 댓글에 답글 달기
                        blogService.ReplyToLatestComment();

                        // 현재 상태 출력 및 실패한 URL 내보내기
                        blogService.PrintStatus();
                        blogService.ExportFailedUrlsToCsv();
                    }
                    else
                    {
                        Logger.Error("최신 글로 이동하지 못했습니다.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("블로그 자동화 실행 중 오류", ex);
                }
                finally
                {
                    // 브라우저 종료 (IDisposable 구현으로 자동 호출)
                    Thread.Sleep(1000); // 마지막 작업 완료 대기
                }
            }
        }

        static void ProcessSpecificPost(AppSettings settings, string postUrl)
        {
            using (var naverService = new NaverLoginService(settings))
            {
                try
                {
                    // 웹 드라이버 초기화 및 로그인
                    naverService.OpenWebMode();
                    
                    if (!naverService.Login())
                    {
                        Logger.Error("로그인에 실패했습니다. 프로그램을 종료합니다.");
                        return;
                    }
                    
                    // 블로그 댓글 서비스 생성
                    var blogService = new NaverBlogCommentService(naverService.Driver, settings);
                    
                    // 특정 글 처리
                    Console.WriteLine($"특정 글을 처리합니다: {postUrl}");
                    if (blogService.ProcessSpecificPost(postUrl))
                    {
                        Console.WriteLine("특정 글 처리가 완료되었습니다.");
                        
                        // 현재 상태 출력 및 실패한 URL 내보내기
                        blogService.PrintStatus();
                        blogService.ExportFailedUrlsToCsv();
                    }
                    else
                    {
                        Logger.Error("특정 글 처리에 실패했습니다.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("특정 글 처리 중 오류", ex);
                }
                finally
                {
                    // 브라우저 종료 (IDisposable 구현으로 자동 호출)
                    Thread.Sleep(1000); // 마지막 작업 완료 대기
                }
            }
        }

        static void ContinueFromIndex51(AppSettings settings)
        {
            using (var naverService = new NaverLoginService(settings))
            {
                try
                {
                    // 웹 드라이버 초기화 및 로그인
                    naverService.OpenWebMode();
                    
                    if (!naverService.Login())
                    {
                        Logger.Error("로그인에 실패했습니다. 프로그램을 종료합니다.");
                        return;
                    }
                    
                    // 블로그로 이동
                    naverService.GoToBlog();
                    
                    // 블로그 댓글 서비스 생성 및 실행
                    var blogService = new NaverBlogCommentService(naverService.Driver, settings);
                    
                    // 최신 글로 이동
                    if (blogService.GoToLatestPost())
                    {
                        // 먼저 댓글 작성자를 수집 (실제 댓글 작성은 50명까지만)
                        Console.WriteLine("댓글을 수집 중입니다...");
                        blogService.ReplyToLatestComment();
                        
                        // 이제 51번째부터 계속 진행
                        Console.WriteLine("51번째부터 계속 진행합니다...");
                        blogService.ContinueProcessingFromIndex(50, settings.DefaultCommentText);

                        // 현재 상태 출력 및 실패한 URL 내보내기
                        blogService.PrintStatus();
                        blogService.ExportFailedUrlsToCsv();
                    }
                    else
                    {
                        Logger.Error("최신 글로 이동하지 못했습니다.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("블로그 자동화 실행 중 오류", ex);
                }
                finally
                {
                    // 브라우저 종료 (IDisposable 구현으로 자동 호출)
                    Thread.Sleep(1000); // 마지막 작업 완료 대기
                }
            }
        }
    }
}