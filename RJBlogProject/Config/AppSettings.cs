using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;

namespace RJBlogProject.Config
{
    [Serializable]
    public class AppSettings
    {
        public string NaverId { get; set; }
        public string NaverPassword { get; set; }
        public string DefaultBlogId { get; set; }
        public int DefaultWaitTimeSeconds { get; set; } = 10;
        public string DefaultCommentText { get; set; } = "좋은하루되세요 포스팅잘보고갑니다~!";
        
        private static readonly string ConfigFileName = "UserSettings.xml";
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        
        public static AppSettings Load()
        {
            // App.config에서 설정 읽기 시도
            var settings = LoadFromAppConfig();
            
            // 로컬 XML 파일에서 설정 읽기 시도 (사용자 오버라이드 용)
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    using (var reader = new StreamReader(ConfigFilePath))
                    {
                        var serializer = new XmlSerializer(typeof(AppSettings));
                        var userSettings = (AppSettings)serializer.Deserialize(reader);
                        
                        // 사용자 설정으로 기본 설정 오버라이드
                        if (!string.IsNullOrEmpty(userSettings.NaverId))
                            settings.NaverId = userSettings.NaverId;
                        if (!string.IsNullOrEmpty(userSettings.NaverPassword))
                            settings.NaverPassword = userSettings.NaverPassword;
                        if (!string.IsNullOrEmpty(userSettings.DefaultBlogId))
                            settings.DefaultBlogId = userSettings.DefaultBlogId;
                        if (userSettings.DefaultWaitTimeSeconds > 0)
                            settings.DefaultWaitTimeSeconds = userSettings.DefaultWaitTimeSeconds;
                        if (!string.IsNullOrEmpty(userSettings.DefaultCommentText))
                            settings.DefaultCommentText = userSettings.DefaultCommentText;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading user settings: {ex.Message}");
                }
            }
            
            return settings;
        }
        
        private static AppSettings LoadFromAppConfig()
        {
            try
            {
                return new AppSettings
                {
                    NaverId = ConfigurationManager.AppSettings["NaverId"] ?? "YOUR_ID_HERE",
                    NaverPassword = ConfigurationManager.AppSettings["NaverPassword"] ?? "YOUR_PASSWORD_HERE",
                    DefaultBlogId = ConfigurationManager.AppSettings["DefaultBlogId"] ?? "rcolfree",
                    DefaultWaitTimeSeconds = int.TryParse(ConfigurationManager.AppSettings["DefaultWaitTimeSeconds"], out int waitTime) ? waitTime : 10,
                    DefaultCommentText = ConfigurationManager.AppSettings["DefaultCommentText"] ?? "좋은하루되세요 포스팅잘보고갑니다~!"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings from App.config: {ex.Message}");
                return new AppSettings
                {
                    NaverId = "YOUR_ID_HERE",
                    NaverPassword = "YOUR_PASSWORD_HERE",
                    DefaultBlogId = "rcolfree"
                };
            }
        }

        public void Save()
        {
            try
            {
                using (var writer = new StreamWriter(ConfigFilePath))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}