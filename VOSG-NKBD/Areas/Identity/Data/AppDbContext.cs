using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Data
{
    public class AppDbContext : IdentityDbContext<VOSG_NKBDUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Location> Locations { get; set; } = default!;
        public DbSet<Place> Places { get; set; } = default!;
        public DbSet<Confirmation> Confirmations { get; set; } = default!;
        public DbSet<Payment> Payments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}