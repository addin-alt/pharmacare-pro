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

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<Purchase> Purchases => Set<Purchase>();

    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

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

        builder.Entity<Supplier>(entity =>
        {
            entity.ToTable(
                "Suppliers",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_Suppliers_OpeningBalance",
                        "\"OpeningBalance\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Suppliers_CurrentBalance",
                        "\"CurrentBalance\" >= 0");
                });

            entity.HasKey(supplier => supplier.Id);

            entity.HasIndex(supplier => supplier.SupplierCode)
                .IsUnique();

            entity.HasIndex(supplier => supplier.Name);
            entity.HasIndex(supplier => supplier.Phone);
            entity.HasIndex(supplier => supplier.IsActive);

            entity.Property(supplier => supplier.OpeningBalance)
                .HasPrecision(18, 2);

            entity.Property(supplier => supplier.CurrentBalance)
                .HasPrecision(18, 2);
        });

        builder.Entity<Purchase>(entity =>
        {
            entity.ToTable(
                "Purchases",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_Purchases_Subtotal",
                        "\"Subtotal\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Purchases_GrandTotal",
                        "\"GrandTotal\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Purchases_PaidAmount",
                        "\"PaidAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Purchases_DueAmount",
                        "\"DueAmount\" >= 0");
                });

            entity.HasKey(purchase => purchase.Id);

            entity.HasIndex(purchase => purchase.PurchaseNumber)
                .IsUnique();

            entity.HasIndex(purchase => purchase.SupplierId);
            entity.HasIndex(purchase => purchase.PurchaseDateUtc);
            entity.HasIndex(purchase => purchase.Status);

            entity.Property(purchase => purchase.Subtotal)
                .HasPrecision(18, 2);

            entity.Property(purchase => purchase.DiscountAmount)
                .HasPrecision(18, 2);

            entity.Property(purchase => purchase.TaxAmount)
                .HasPrecision(18, 2);

            entity.Property(purchase => purchase.GrandTotal)
                .HasPrecision(18, 2);

            entity.Property(purchase => purchase.PaidAmount)
                .HasPrecision(18, 2);

            entity.Property(purchase => purchase.DueAmount)
                .HasPrecision(18, 2);

            entity.Property(purchase => purchase.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.Property(purchase => purchase.Status)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(purchase => purchase.Supplier)
                .WithMany()
                .HasForeignKey(purchase => purchase.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchaseItem>(entity =>
        {
            entity.ToTable(
                "PurchaseItems",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_PurchaseItems_Quantity",
                        "\"Quantity\" > 0");

                    table.HasCheckConstraint(
                        "CK_PurchaseItems_FreeQuantity",
                        "\"FreeQuantity\" >= 0");

                    table.HasCheckConstraint(
                        "CK_PurchaseItems_LineTotal",
                        "\"LineTotal\" >= 0");
                });

            entity.HasKey(item => item.Id);

            entity.HasIndex(item => item.PurchaseId);
            entity.HasIndex(item => item.MedicineId);
            entity.HasIndex(item => item.BatchNumber);
            entity.HasIndex(item => item.ExpiryDate);

            entity.Property(item => item.ManufacturingDate)
                .HasColumnType("date");

            entity.Property(item => item.ExpiryDate)
                .HasColumnType("date");

            entity.Property(item => item.PurchasePrice)
                .HasPrecision(18, 2);

            entity.Property(item => item.SellingPrice)
                .HasPrecision(18, 2);

            entity.Property(item => item.LineTotal)
                .HasPrecision(18, 2);

            entity.HasOne(item => item.Purchase)
                .WithMany(purchase => purchase.Items)
                .HasForeignKey(item => item.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Medicine)
                .WithMany()
                .HasForeignKey(item => item.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
