using Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class DataContext : IdentityDbContext<ApplicationUser>
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public DbSet<AddressEntity> Addresses { get; set; } = null!;
    public DbSet<MemberEntity> Members { get; set; } = null!;
    public DbSet<RoleEntity> MemberRoles { get; set; } = null!;
    public DbSet<ProjectEntity> Projects { get; set; } = null!;
    public DbSet<ClientEntity> Clients { get; set; } = null!;
    public DbSet<StatusEntity> Statuses { get; set; } = null!;
    public DbSet<NotificationEntity> Notifications { get; set; } = null!;
    public DbSet<UserNotificationEntity> UserNotifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // MemberEntity relations
        modelBuilder.Entity<MemberEntity>()
            .HasOne(m => m.Role)
            .WithMany(r => r.Members)
            .HasForeignKey(m => m.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MemberEntity>()
            .HasOne(m => m.Address)
            .WithMany(a => a.Members)
            .HasForeignKey(m => m.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        // Project <-> Member many-to-many via ProjectMembers join table
        modelBuilder.Entity<ProjectEntity>()
            .HasMany(p => p.Members)
            .WithMany(m => m.Projects)
            .UsingEntity<Dictionary<string, object>>(
                "ProjectMembers",
                // Configure Member side
                j => j
                    .HasOne<MemberEntity>()
                    .WithMany()
                    .HasForeignKey("MemberId")
                    .HasConstraintName("FK_ProjectMembers_Member")
                    .OnDelete(DeleteBehavior.Cascade),
                // Configure Project side
                j => j
                    .HasOne<ProjectEntity>()
                    .WithMany()
                    .HasForeignKey("ProjectId")
                    .HasConstraintName("FK_ProjectMembers_Project")
                    .OnDelete(DeleteBehavior.Cascade),
                // Configure join table
                j =>
                {
                    j.HasKey("ProjectId", "MemberId");
                    j.ToTable("ProjectMembers");
                });

        // Project -> Client (1:n)
        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Client)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project -> Status (1:n)
        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Status)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserNotification relations
        modelBuilder.Entity<UserNotificationEntity>()
            .HasOne(un => un.Notification)
            .WithMany(n => n.UserNotifications)
            .HasForeignKey(un => un.NotificationId);

        modelBuilder.Entity<UserNotificationEntity>()
            .HasOne(un => un.User)
            .WithMany()
            .HasForeignKey(un => un.UserId);

        base.OnModelCreating(modelBuilder);
    }
}
