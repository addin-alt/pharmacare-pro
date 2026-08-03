using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaCarePro.Domain.Entities;

namespace PharmaCarePro.Web.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Medicine> Medicines => Set<Medicine>();

    public DbSet<MedicineBatch> MedicineBatches =>
        Set<MedicineBatch>();

    public DbSet<StockMovement> StockMovements =>
        Set<StockMovement>();

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

        builder.Entity<MedicineBatch>(entity =>
        {
            entity.ToTable(
                "MedicineBatches",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_MedicineBatches_ReceivedQuantity",
                        "\"ReceivedQuantity\" > 0");

                    table.HasCheckConstraint(
                        "CK_MedicineBatches_FreeQuantity",
                        "\"FreeQuantity\" >= 0");

                    table.HasCheckConstraint(
                        "CK_MedicineBatches_AvailableQuantity",
                        "\"AvailableQuantity\" >= 0");
                });

            entity.HasKey(batch => batch.Id);

            entity.HasIndex(batch => new
                {
                    batch.MedicineId,
                    batch.BatchNumber,
                })
                .IsUnique();

            entity.HasIndex(batch => batch.ExpiryDate);
            entity.HasIndex(batch => batch.AvailableQuantity);
            entity.HasIndex(batch => batch.IsQuarantined);

            entity.Property(batch => batch.ManufacturingDate)
                .HasColumnType("date");

            entity.Property(batch => batch.ExpiryDate)
                .HasColumnType("date");

            entity.Property(batch => batch.PurchasePrice)
                .HasPrecision(18, 2);

            entity.Property(batch => batch.SellingPrice)
                .HasPrecision(18, 2);

            entity.HasOne(batch => batch.Medicine)
                .WithMany(medicine => medicine.Batches)
                .HasForeignKey(batch => batch.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockMovement>(entity =>
        {
            entity.ToTable(
                "StockMovements",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_StockMovements_QuantityChange",
                        "\"QuantityChange\" <> 0");

                    table.HasCheckConstraint(
                        "CK_StockMovements_BalanceAfter",
                        "\"BalanceAfter\" >= 0");
                });

            entity.HasKey(movement => movement.Id);

            entity.HasIndex(movement => movement.MedicineBatchId);
            entity.HasIndex(movement => movement.CreatedAtUtc);
            entity.HasIndex(movement => movement.MovementType);

            entity.Property(movement => movement.MovementType)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(movement => movement.MedicineBatch)
                .WithMany(batch => batch.StockMovements)
                .HasForeignKey(movement => movement.MedicineBatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
