using HomeCarePH.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Household> Households => Set<Household>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Caregiver> Caregivers => Set<Caregiver>();
    public DbSet<CareVisit> CareVisits => Set<CareVisit>();
    public DbSet<HouseholdMembership> HouseholdMemberships => Set<HouseholdMembership>();
    public DbSet<HouseholdInvitation> HouseholdInvitations => Set<HouseholdInvitation>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<RoomSpace> RoomSpaces => Set<RoomSpace>();
    public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();
    public DbSet<PropertyRecord> PropertyRecords => Set<PropertyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
        });

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
            entity.HasOne(x => x.Household).WithMany(x => x.Residents).HasForeignKey(x => x.HouseholdId).OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<HouseholdMembership>(entity =>
        {
            entity.HasKey(x => new { x.HouseholdId, x.UserId });
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.JoinedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Household).WithMany(x => x.Memberships).HasForeignKey(x => x.HouseholdId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User).WithMany(x => x.HouseholdMemberships).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HouseholdInvitation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasIndex(x => new { x.HouseholdId, x.Email, x.AcceptedAtUtc });
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Token).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.ExpiresAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AcceptedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Household).WithMany(x => x.Invitations).HasForeignKey(x => x.HouseholdId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.InvitedByUser).WithMany().HasForeignKey(x => x.InvitedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PropertyType).HasMaxLength(100);
            entity.Property(x => x.AddressLine1).HasMaxLength(200);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Province).HasMaxLength(100);
            entity.Property(x => x.PostalCode).HasMaxLength(20);
            entity.Property(x => x.Description).HasMaxLength(1200);
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Household).WithMany().HasForeignKey(x => x.HouseholdId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomSpace>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SpaceType).HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Property).WithMany(x => x.Rooms).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyPhoto>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredPath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Caption).HasMaxLength(180);
            entity.Property(x => x.ContentType).HasMaxLength(120);
            entity.Property(x => x.UploadedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Property).WithMany(x => x.Photos).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropertyRecord>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.RecordType).HasMaxLength(80);
            entity.Property(x => x.RecordedAtUtc).HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Property).WithMany(x => x.Records).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
