namespace WILK.Services.Configuration
{
    public class ConnectionStrings
    {
        public const string SectionName = "ConnectionStrings";
        
        public string DefaultConnection { get; set; } = string.Empty;
    }

    public class AppSettings
    {
        public const string SectionName = "AppSettings";
        
        public string Version { get; set; } = string.Empty;
    }
}