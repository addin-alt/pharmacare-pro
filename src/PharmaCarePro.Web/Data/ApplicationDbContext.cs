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

    public DbSet<Sale> Sales => Set<Sale>();

    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

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
        builder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sales");

            entity.HasKey(sale => sale.Id);

            entity.HasIndex(sale => sale.InvoiceNumber)
                .IsUnique();

            entity.HasIndex(sale => sale.SoldAtUtc);
            entity.HasIndex(sale => sale.Status);
            entity.HasIndex(sale => sale.CustomerPhone);

            entity.Property(sale => sale.Subtotal).HasPrecision(18, 2);
            entity.Property(sale => sale.DiscountAmount).HasPrecision(18, 2);
            entity.Property(sale => sale.TaxAmount).HasPrecision(18, 2);
            entity.Property(sale => sale.GrandTotal).HasPrecision(18, 2);
            entity.Property(sale => sale.PaidAmount).HasPrecision(18, 2);
            entity.Property(sale => sale.DueAmount).HasPrecision(18, 2);

            entity.Property(sale => sale.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.Property(sale => sale.Status)
                .HasConversion<string>()
                .HasMaxLength(40);
        });

        builder.Entity<SaleItem>(entity =>
        {
            entity.ToTable("SaleItems");

            entity.HasKey(item => item.Id);

            entity.HasIndex(item => item.SaleId);
            entity.HasIndex(item => item.MedicineId);
            entity.HasIndex(item => item.MedicineBatchId);

            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
            entity.Property(item => item.DiscountAmount).HasPrecision(18, 2);
            entity.Property(item => item.LineTotal).HasPrecision(18, 2);

            entity.HasOne(item => item.Sale)
                .WithMany(sale => sale.Items)
                .HasForeignKey(item => item.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Medicine)
                .WithMany()
                .HasForeignKey(item => item.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(item => item.MedicineBatch)
                .WithMany()
                .HasForeignKey(item => item.MedicineBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
