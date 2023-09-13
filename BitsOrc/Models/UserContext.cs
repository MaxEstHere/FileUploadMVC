using Microsoft.EntityFrameworkCore;

namespace BitsOrc.Models
{
    public class UserContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; } = null!;

        public UserContext(DbContextOptions<UserContext> options ) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
