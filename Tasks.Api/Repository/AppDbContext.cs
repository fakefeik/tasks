using Microsoft.EntityFrameworkCore;
using Tasks.Api.DataTypes;

namespace Tasks.Api.Repository
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskObject> Tasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=localhost;Database=my_db;Username=postgres;Password=postgres");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity(typeof(TaskObject)).HasKey("Id");
        }
    }
}