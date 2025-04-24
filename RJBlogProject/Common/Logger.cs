using System;
using System.IO;

namespace RJBlogProject.Common
{
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string LogFilePath = Path.Combine(LogDirectory, $"Log_{DateTime.Now:yyyyMMdd}.txt");
        
        static Logger()
        {
            // 로그 디렉토리가 없으면 생성
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        public static void Info(string message)
        {
            Log("INFO", message);
        }

        public static void Warning(string message)
        {
            Log("WARNING", message);
        }

        public static void Error(string message)
        {
            Log("ERROR", message);
        }

        public static void Error(string message, Exception ex)
        {
            Log("ERROR", $"{message} - Exception: {ex.Message}");
            Log("ERROR", $"StackTrace: {ex.StackTrace}");
        }

        private static void Log(string level, string message)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            
            // 콘솔에 출력
            Console.WriteLine(logMessage);
            
            try
            {
                // 파일에 로그 추가
                using (StreamWriter writer = File.AppendText(LogFilePath))
                {
                    writer.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}