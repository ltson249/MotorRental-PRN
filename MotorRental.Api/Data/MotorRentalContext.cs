using Microsoft.EntityFrameworkCore;
using MotorRental.Api.Models;

namespace MotorRental.Api.Data;

public class MotorRentalContext : DbContext
{
    public MotorRentalContext(DbContextOptions<MotorRentalContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Motorbike> Motorbikes => Set<Motorbike>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.Username).IsUnique();
        });
        mb.Entity<Customer>(e =>
        {
            e.ToTable("Customers");
            e.HasKey(x => x.CustomerId);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
        });
        mb.Entity<Motorbike>(e =>
        {
            e.ToTable("Motorbikes");
            e.HasKey(x => x.MotorbikeId);
            e.Property(x => x.PricePerDay).HasPrecision(18, 0);
            e.Property(x => x.Category).HasMaxLength(30).HasDefaultValue("Xe số");
            e.Property(x => x.Color).HasMaxLength(30);
            e.Property(x => x.ImageUrl).HasMaxLength(300);
            e.Property(x => x.CurrentOdometer).HasDefaultValue(0);
        });
        mb.Entity<Rental>(e =>
        {
            e.ToTable("Rentals");
            e.HasKey(x => x.RentalId);
            e.Property(x => x.PricePerDay).HasPrecision(18, 0);
            e.Property(x => x.Deposit).HasPrecision(18, 0);
            e.Property(x => x.TotalPrice).HasPrecision(18, 0);
            e.Property(x => x.LateFee).HasPrecision(18, 0);
            e.Property(x => x.DamageFee).HasPrecision(18, 0);
            e.Property(x => x.OtherFee).HasPrecision(18, 0);
            e.Property(x => x.ActualPayment).HasPrecision(18, 0);
            e.Property(x => x.ConditionNote).HasMaxLength(300);

            e.HasOne(x => x.Customer).WithMany(c => c.Rentals).HasForeignKey(x => x.CustomerId);
            e.HasOne(x => x.Motorbike).WithMany(m => m.Rentals).HasForeignKey(x => x.MotorbikeId);
            e.HasOne(x => x.Staff).WithMany(u => u.Rentals).HasForeignKey(x => x.UserId);
        });
        mb.Entity<Maintenance>(e =>
        {
            e.ToTable("Maintenances");
            e.HasKey(x => x.MaintenanceId);
            e.Property(x => x.Cost).HasPrecision(18, 0);
            e.Property(x => x.Description).HasMaxLength(300);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Completed");
            e.HasOne(x => x.Motorbike).WithMany(m => m.Maintenances).HasForeignKey(x => x.MotorbikeId);
        });
    }
}
