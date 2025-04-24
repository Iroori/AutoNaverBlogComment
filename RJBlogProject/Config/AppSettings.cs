using System;
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
        
        private static readonly string ConfigFileName = "AppSettings.xml";
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        
        public static AppSettings Load()
        {
            if (!File.Exists(ConfigFilePath))
            {
                var defaultSettings = new AppSettings
                {
                    NaverId = "YOUR_ID_HERE",
                    NaverPassword = "YOUR_PASSWORD_HERE",
                    DefaultBlogId = "rcolfree",
                    DefaultWaitTimeSeconds = 10,
                    DefaultCommentText = "좋은하루되세요 포스팅잘보고갑니다~!"
                };
                defaultSettings.Save();
                return defaultSettings;
            }

            try
            {
                using (var reader = new StreamReader(ConfigFilePath))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    return (AppSettings)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return new AppSettings();
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