using RJBlogProject.Common;
using RJBlogProject.Config;
using RJBlogProject.Service;
using System;
using System.Configuration;
using System.Threading;

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

                // 블로그 자동화 실행
                ExecuteBlogAutomation(settings);
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