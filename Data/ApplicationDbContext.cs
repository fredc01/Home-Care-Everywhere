using HomeCarePH.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Household> Households => Set<Household>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Caregiver> Caregivers => Set<Caregiver>();
    public DbSet<CareVisit> CareVisits => Set<CareVisit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ContactNumber).HasMaxLength(40);
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Household)
                .WithMany(x => x.Residents)
                .HasForeignKey(x => x.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Caregiver>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(120);
            entity.Property(x => x.ContactNumber).HasMaxLength(40);
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<CareVisit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.ScheduledAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Resident).WithMany().HasForeignKey(x => x.ResidentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Caregiver).WithMany().HasForeignKey(x => x.CaregiverId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
