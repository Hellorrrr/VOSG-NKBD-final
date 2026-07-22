using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Data
{
    // the database context - EF Core's "door" into SQL Server.
    // inherits IdentityDbContext so the login/user tables come for free
    public class AppDbContext : IdentityDbContext<VOSG_NKBDUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // each DbSet = one table in the database
        public DbSet<Location> Locations { get; set; } = default!;
        public DbSet<Place> Places { get; set; } = default!;
        public DbSet<Confirmation> Confirmations { get; set; } = default!;
        public DbSet<Payment> Payments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // FIX for SQL Server: both Confirmation and Payment link to the User,
            // and Payment also links to Confirmation. that creates two cascade
            // delete paths to Payments which SQL Server doesnt allow
            // (SQLite was fine with it, thats why it only broke after migrating).
            // Restrict = deleting a confirmation wont auto-delete its payment.
            builder.Entity<Payment>()
                .HasOne(p => p.Confirmation)
                .WithMany()
                .HasForeignKey(p => p.ConfirmationID)
                .OnDelete(DeleteBehavior.Restrict);

            // same reason - stop the User -> Payment cascade path too
            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.VOSG_NKBDId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}