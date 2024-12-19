using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SiteMonitor.Web.Models;

namespace SiteMonitor.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet definitions for your actual models
    // Note: ErrorViewModel and MonitoringSummary don't need DbSets as they're view models
    // WeeklyReportData is also a model for reporting and doesn't need persistence
    public DbSet<MonitoredSite> MonitoredSites { get; set; } = null!;
    public DbSet<SiteStatusHistory> SiteStatusHistory { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity tables
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(u => u.UserName)
                .HasMaxLength(256)
                .IsRequired();
        });

        // Configure MonitoredSite entity
        builder.Entity<MonitoredSite>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationship with ApplicationUser
            entity.HasOne(e => e.User)
                .WithMany(u => u.MonitoredSites)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Url);
        });

        // Configure SiteStatusHistory entity
        builder.Entity<SiteStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CheckedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(e => e.StatusCode)
                .IsRequired();

            entity.Property(e => e.IsUp)
                .IsRequired();

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            // Relationship with MonitoredSite
            entity.HasOne(e => e.MonitoredSite)
                .WithMany(s => s.StatusHistory)
                .HasForeignKey(e => e.MonitoredSiteId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Index for faster queries
            entity.HasIndex(e => new { e.MonitoredSiteId, e.CheckedAt });
        });

        // Configure default query filter for history
        builder.Entity<SiteStatusHistory>()
            .HasQueryFilter(h => h.CheckedAt >= DateTime.UtcNow.AddMonths(-3));
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is MonitoredSite || e.Entity is SiteStatusHistory)
            .Where(e => e.State == EntityState.Added);

        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.Entity)
            {
                case MonitoredSite site:
                    site.CreatedAt = utcNow;
                    break;
                case SiteStatusHistory history:
                    history.CheckedAt = utcNow;
                    break;
            }
        }
    }
}