using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GameSphere.Models;

namespace GameSphere.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext() // Parameterless constructor
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Post> Post { get; set; }
        //public object Likes { get; internal set; }

        public DbSet<Likes> Likes { get; set; }
        public DbSet<Reply> Replies { get; set; }
        //public IEnumerable<object> Reply { get; internal set; }
        //public DbSet<GameSphere.Models.Post> Post { get; set; } = default!;
    }
}