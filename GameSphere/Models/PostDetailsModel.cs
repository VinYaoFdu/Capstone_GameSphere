namespace GameSphere.Models
{
    public class PostDetailsModel
    {
        public Post PostImage { get; set; }
        public Post Post { get; set; }
        public int LikesCount { get; set; }
        public List<Reply> Replies { get; set; }
        public IWebHostEnvironment HostingEnvironment { get; set; }
    }
}
