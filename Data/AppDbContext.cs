using Microsoft.EntityFrameworkCore;
using JobAlertApi.Models;

namespace JobAlertApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ------------------------
        // Tables
        // ------------------------

        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<SavedJob> SavedJobs { get; set; } = null!;
        public DbSet<JobApplication> JobApplications { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        // ------------------------
        // Model Configuration (PRO)
        // ------------------------

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                // ✅ INDEX ON HASH (NOT Token ❗)
                entity.HasIndex(x => x.TokenHash).IsUnique();

                // ✅ Relationship: User → RefreshTokens
                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable("RefreshTokens");
            });
        }
    }
}