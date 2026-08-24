namespace PortfolioApp.Api.Model
{
    public class VisitorLog
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
    }
}
