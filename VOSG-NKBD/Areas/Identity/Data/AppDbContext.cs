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

           

            builder.Entity<Confirmation>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.VOSG_NKBDId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>()
                .HasOne(p => p.Confirmation)
                .WithOne(c => c.Payment)
                .HasForeignKey<Payment>(p => p.ConfirmationID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}