using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<AddressEntity> Addresses { get; set; } = null!;
    public DbSet<MemberEntity> Members { get; set; } = null!;
    public DbSet<RoleEntity> Roles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        base.OnModelCreating(modelBuilder);
    }
}
