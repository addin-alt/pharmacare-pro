using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.Web.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Medicine> Medicines => Set<Medicine>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Medicine>(entity =>
        {
            entity.ToTable("Medicines");

            entity.HasKey(medicine => medicine.Id);

            entity.HasIndex(medicine => medicine.Sku)
                .IsUnique();

            entity.HasIndex(medicine => medicine.Barcode)
                .IsUnique();

            entity.HasIndex(medicine => medicine.BrandName);
            entity.HasIndex(medicine => medicine.GenericName);
            entity.HasIndex(medicine => medicine.IsActive);

            entity.Property(medicine => medicine.PurchasePrice)
                .HasPrecision(18, 2);

            entity.Property(medicine => medicine.SellingPrice)
                .HasPrecision(18, 2);

            entity.Property(medicine => medicine.MaximumRetailPrice)
                .HasPrecision(18, 2);
        });
    }
}
