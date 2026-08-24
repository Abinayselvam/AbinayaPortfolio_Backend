namespace PortfolioApp.Api.Model
{

    public class ProjectEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty; // Stored as comma-separated: "Angular,.NET,SQL"
        public string? LiveUrl { get; set; }
        public string? GithubUrl { get; set; }
        public bool Featured { get; set; }
    }

    
}
