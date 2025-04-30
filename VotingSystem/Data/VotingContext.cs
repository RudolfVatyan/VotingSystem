using Microsoft.EntityFrameworkCore;
using VotingSystem.Models;

namespace VotingSystem.Data
{
    public class VotingContext : DbContext
    {
        public VotingContext(DbContextOptions<VotingContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
