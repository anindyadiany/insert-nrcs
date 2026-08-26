using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Insert.Domain.Entities;
using Insert.Infrastructure.Identity;

namespace Insert.Infrastructure;

public class InsertDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public InsertDbContext(DbContextOptions<InsertDbContext> options) : base(options) { }

    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Script> Scripts => Set<Script>();
    public DbSet<ScriptVersion> ScriptVersions => Set<ScriptVersion>();

    public DbSet<IngestJob> IngestJobs => Set<IngestJob>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<StoryMedia> StoryMedias => Set<StoryMedia>();

    public DbSet<Approval> Approvals => Set<Approval>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("user");
        builder.Entity<ApplicationRole>().ToTable("role");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_role");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claim");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_login");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claim");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_token");

        builder.Entity<Story>().Property(s => s.Status).HasConversion<string>();
        builder.Entity<Story>().Property(s => s.Priority).HasConversion<string>();

        builder.Entity<Assignment>().Property(a => a.Status).HasConversion<string>();

        builder.Entity<IngestJob>().Property(j => j.Status).HasConversion<string>();
        builder.Entity<MediaAsset>().Property(m => m.IngestStatus).HasConversion<string>();
        builder.Entity<MediaAsset>().Property(m => m.RetentionStatus).HasConversion<string>();

        builder.Entity<Approval>().Property(a => a.Decision).HasConversion<string>();
    }
}