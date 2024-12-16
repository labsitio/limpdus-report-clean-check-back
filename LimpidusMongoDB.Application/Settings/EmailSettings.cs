namespace LimpidusMongoDB.Application.Settings
{
    public class EmailSettings
    {
        public string SMTP { get; set; }
        public int Port { get; set; }
        public bool DefaultCredentials { get; set; }
        public string FromDisplay { get; set; }
        public string From { get; set; }
        public string Password { get; set; }
    }
}
