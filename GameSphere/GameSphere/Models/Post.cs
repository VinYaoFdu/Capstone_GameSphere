namespace GameSphere.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Topic { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string PostedBy { get; set; } = null!;
        public DateTime MessaAt { get; set; } = DateTime.UtcNow;
    }
}
