namespace Server.Models
{
    public class Token
    {
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? ExpiresIn { get; set; }
    }
}
