using Microsoft.EntityFrameworkCore;

namespace Project_MVC_.Models.DB
{
    public class CrmContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Right> Rights { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public CrmContext (DbContextOptions<CrmContext> options) : base (options) { Database.EnsureCreated(); }
    }
}
