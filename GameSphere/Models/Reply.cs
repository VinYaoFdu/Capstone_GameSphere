namespace GameSphere.Models
{
    public class Reply
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}
