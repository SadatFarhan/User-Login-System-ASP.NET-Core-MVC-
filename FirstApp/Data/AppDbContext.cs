using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FirstApp.Models;

namespace FirstApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>  options) : base(options)
        { 


        }
        public DbSet<Users> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Users>()
            //    .HasIndex(u => u.Email)
            //    .IsUnique();
            //modelBuilder.Entity<Users>()
            //    .HasIndex(u => u.Name)
            //    .IsUnique();
        }
    }


}
