using MagicVilla_CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_CouponAPI.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            // Ensure the database is created and apply any pending migrations
            Database.EnsureCreated();
        }

        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<LocalUser> LocalUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                Id = 1,
                Name = "10OFF",
                Percent = 10,                
                IsActive = true
            });
            modelBuilder.Entity<Coupon>().HasData(new Coupon
            {
                Id = 2,
                Name = "20OFF",
                Percent = 20,
                IsActive = true,
            });
        }
    }
}
