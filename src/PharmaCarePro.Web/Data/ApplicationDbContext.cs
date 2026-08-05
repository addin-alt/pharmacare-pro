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

    public DbSet<SaleReturn> SaleReturns =>
        Set<SaleReturn>();

    public DbSet<SaleReturnItem> SaleReturnItems =>
        Set<SaleReturnItem>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<Purchase> Purchases => Set<Purchase>();

    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

    public DbSet<SupplierReturn> SupplierReturns =>
        Set<SupplierReturn>();

    public DbSet<SupplierReturnItem> SupplierReturnItems =>
        Set<SupplierReturnItem>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerPayment> CustomerPayments =>
        Set<CustomerPayment>();

    public DbSet<CustomerPaymentAllocation>
        CustomerPaymentAllocations =>
        Set<CustomerPaymentAllocation>();

    public DbSet<SupplierPayment> SupplierPayments =>
        Set<SupplierPayment>();

    public DbSet<SupplierPaymentAllocation>
        SupplierPaymentAllocations =>
        Set<SupplierPaymentAllocation>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    public DbSet<PharmacyProfile> PharmacyProfiles => Set<PharmacyProfile>();

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
            entity.HasIndex(sale => sale.CustomerId);

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

            entity.HasOne(sale => sale.Customer)
                .WithMany(customer => customer.Sales)
                .HasForeignKey(sale => sale.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SaleItem>(entity =>
        {
            entity.ToTable("SaleItems");

            entity.HasKey(item => item.Id);

            entity.HasIndex(item => item.SaleId);
            entity.HasIndex(item => item.MedicineId);
            entity.HasIndex(item => item.MedicineBatchId);

            entity.Property(item => item.UnitCost).HasPrecision(18, 2);
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


        builder.Entity<SaleReturn>(entity =>
        {
            entity.ToTable(
                "SaleReturns",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_SaleReturns_GrossReturnAmount",
                        "\"GrossReturnAmount\" > 0");

                    table.HasCheckConstraint(
                        "CK_SaleReturns_DueReductionAmount",
                        "\"DueReductionAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SaleReturns_RefundedAmount",
                        "\"RefundedAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SaleReturns_SettlementTotal",
                        "\"DueReductionAmount\" + " +
                        "\"RefundedAmount\" = " +
                        "\"GrossReturnAmount\"");

                    table.HasCheckConstraint(
                        "CK_SaleReturns_RefundMethod",
                        "(\"RefundedAmount\" = 0 AND " +
                        "\"RefundMethod\" IS NULL) OR " +
                        "(\"RefundedAmount\" > 0 AND " +
                        "\"RefundMethod\" IS NOT NULL AND " +
                        "\"RefundMethod\" <> 'Due')");
                });

            entity.HasKey(saleReturn => saleReturn.Id);

            entity.HasIndex(saleReturn => saleReturn.ReturnNumber)
                .IsUnique();

            entity.HasIndex(saleReturn => saleReturn.SaleId);
            entity.HasIndex(saleReturn => saleReturn.CustomerId);
            entity.HasIndex(saleReturn => saleReturn.ReturnedAtUtc);

            entity.Property(saleReturn =>
                    saleReturn.GrossReturnAmount)
                .HasPrecision(18, 2);

            entity.Property(saleReturn =>
                    saleReturn.DueReductionAmount)
                .HasPrecision(18, 2);

            entity.Property(saleReturn =>
                    saleReturn.RefundedAmount)
                .HasPrecision(18, 2);

            entity.Property(saleReturn =>
                    saleReturn.RefundMethod)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(saleReturn => saleReturn.Sale)
                .WithMany(sale => sale.Returns)
                .HasForeignKey(saleReturn => saleReturn.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(saleReturn => saleReturn.Customer)
                .WithMany()
                .HasForeignKey(saleReturn => saleReturn.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleReturnItem>(entity =>
        {
            entity.ToTable(
                "SaleReturnItems",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_SaleReturnItems_Quantity",
                        "\"Quantity\" > 0");

                    table.HasCheckConstraint(
                        "CK_SaleReturnItems_UnitRefundAmount",
                        "\"UnitRefundAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SaleReturnItems_LineRefundAmount",
                        "\"LineRefundAmount\" >= 0");
                });

            entity.HasKey(item => item.Id);

            entity.HasIndex(
                    item => new
                    {
                        item.SaleReturnId,
                        item.SaleItemId
                    })
                .IsUnique();

            entity.HasIndex(item => item.SaleItemId);
            entity.HasIndex(item => item.MedicineBatchId);
            entity.HasIndex(item => item.StockAction);

            entity.Property(item => item.UnitRefundAmount)
                .HasPrecision(18, 2);

            entity.Property(item => item.LineRefundAmount)
                .HasPrecision(18, 2);

            entity.Property(item => item.StockAction)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(item => item.SaleReturn)
                .WithMany(saleReturn => saleReturn.Items)
                .HasForeignKey(item => item.SaleReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.SaleItem)
                .WithMany(saleItem => saleItem.ReturnItems)
                .HasForeignKey(item => item.SaleItemId)
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


        builder.Entity<SupplierReturn>(entity =>
        {
            entity.ToTable(
                "SupplierReturns",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_SupplierReturns_GrossReturnAmount",
                        "\"GrossReturnAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierReturns_PayableReductionAmount",
                        "\"PayableReductionAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierReturns_SupplierRefundAmount",
                        "\"SupplierRefundAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierReturns_SettlementTotal",
                        "\"PayableReductionAmount\" + " +
                        "\"SupplierRefundAmount\" = " +
                        "\"GrossReturnAmount\"");

                    table.HasCheckConstraint(
                        "CK_SupplierReturns_RefundMethod",
                        "(\"SupplierRefundAmount\" = 0 AND " +
                        "\"RefundMethod\" IS NULL) OR " +
                        "(\"SupplierRefundAmount\" > 0 AND " +
                        "\"RefundMethod\" IS NOT NULL AND " +
                        "\"RefundMethod\" <> 'Due')");
                });

            entity.HasKey(supplierReturn =>
                supplierReturn.Id);

            entity.HasIndex(supplierReturn =>
                    supplierReturn.ReturnNumber)
                .IsUnique();

            entity.HasIndex(supplierReturn =>
                supplierReturn.PurchaseId);

            entity.HasIndex(supplierReturn =>
                supplierReturn.SupplierId);

            entity.HasIndex(supplierReturn =>
                supplierReturn.ReturnedAtUtc);

            entity.Property(supplierReturn =>
                    supplierReturn.GrossReturnAmount)
                .HasPrecision(18, 2);

            entity.Property(supplierReturn =>
                    supplierReturn.PayableReductionAmount)
                .HasPrecision(18, 2);

            entity.Property(supplierReturn =>
                    supplierReturn.SupplierRefundAmount)
                .HasPrecision(18, 2);

            entity.Property(supplierReturn =>
                    supplierReturn.RefundMethod)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(supplierReturn =>
                    supplierReturn.Purchase)
                .WithMany(purchase =>
                    purchase.Returns)
                .HasForeignKey(supplierReturn =>
                    supplierReturn.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(supplierReturn =>
                    supplierReturn.Supplier)
                .WithMany(supplier =>
                    supplier.Returns)
                .HasForeignKey(supplierReturn =>
                    supplierReturn.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SupplierReturnItem>(entity =>
        {
            entity.ToTable(
                "SupplierReturnItems",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_SupplierReturnItems_Quantity",
                        "\"Quantity\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierReturnItems_FreeQuantity",
                        "\"FreeQuantity\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierReturnItems_TotalQuantity",
                        "\"Quantity\" + \"FreeQuantity\" > 0");

                    table.HasCheckConstraint(
                        "CK_SupplierReturnItems_UnitReturnAmount",
                        "\"UnitReturnAmount\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierReturnItems_LineReturnAmount",
                        "\"LineReturnAmount\" >= 0");
                });

            entity.HasKey(item => item.Id);

            entity.HasIndex(
                    item => new
                    {
                        item.SupplierReturnId,
                        item.PurchaseItemId
                    })
                .IsUnique();

            entity.HasIndex(item => item.PurchaseItemId);
            entity.HasIndex(item => item.MedicineBatchId);

            entity.Property(item =>
                    item.UnitReturnAmount)
                .HasPrecision(18, 2);

            entity.Property(item =>
                    item.LineReturnAmount)
                .HasPrecision(18, 2);

            entity.HasOne(item =>
                    item.SupplierReturn)
                .WithMany(supplierReturn =>
                    supplierReturn.Items)
                .HasForeignKey(item =>
                    item.SupplierReturnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item =>
                    item.PurchaseItem)
                .WithMany(purchaseItem =>
                    purchaseItem.ReturnItems)
                .HasForeignKey(item =>
                    item.PurchaseItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(item =>
                    item.MedicineBatch)
                .WithMany()
                .HasForeignKey(item =>
                    item.MedicineBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.ToTable(
                "Customers",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_Customers_CurrentBalance",
                        "\"CurrentBalance\" >= 0");

                    table.HasCheckConstraint(
                        "CK_Customers_LoyaltyPoints",
                        "\"LoyaltyPoints\" >= 0");
                });

            entity.HasKey(customer => customer.Id);

            entity.HasIndex(customer => customer.CustomerCode)
                .IsUnique();

            entity.HasIndex(customer => customer.Name);
            entity.HasIndex(customer => customer.Phone);
            entity.HasIndex(customer => customer.IsActive);

            entity.Property(customer => customer.DateOfBirth)
                .HasColumnType("date");

            entity.Property(customer => customer.CurrentBalance)
                .HasPrecision(18, 2);
        });


        builder.Entity<CustomerPayment>(entity =>
        {
            entity.ToTable(
                "CustomerPayments",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_CustomerPayments_Amount",
                        "\"Amount\" > 0");

                    table.HasCheckConstraint(
                        "CK_CustomerPayments_BalanceBefore",
                        "\"BalanceBefore\" >= 0");

                    table.HasCheckConstraint(
                        "CK_CustomerPayments_BalanceAfter",
                        "\"BalanceAfter\" >= 0");

                    table.HasCheckConstraint(
                        "CK_CustomerPayments_BalanceFlow",
                        "\"BalanceBefore\" >= \"BalanceAfter\"");

                    table.HasCheckConstraint(
                        "CK_CustomerPayments_ApplicationTotal",
                        "\"AppliedToSalesAmount\" + " +
                        "\"AppliedToAccountBalanceAmount\" = " +
                        "\"Amount\"");

                    table.HasCheckConstraint(
                        "CK_CustomerPayments_PaymentMethod",
                        "\"PaymentMethod\" <> 'Due'");
                });

            entity.HasKey(payment => payment.Id);

            entity.HasIndex(payment => payment.ReceiptNumber)
                .IsUnique();

            entity.HasIndex(payment => payment.CustomerId);
            entity.HasIndex(payment => payment.ReceivedAtUtc);
            entity.HasIndex(payment => payment.PaymentMethod);

            entity.Property(payment => payment.Amount)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.BalanceBefore)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.BalanceAfter)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.AppliedToSalesAmount)
                .HasPrecision(18, 2);

            entity.Property(
                    payment =>
                        payment.AppliedToAccountBalanceAmount)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(payment => payment.Customer)
                .WithMany(customer => customer.Payments)
                .HasForeignKey(payment => payment.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CustomerPaymentAllocation>(entity =>
        {
            entity.ToTable(
                "CustomerPaymentAllocations",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_CustomerPaymentAllocations_Amount",
                        "\"Amount\" > 0");
                });

            entity.HasKey(allocation => allocation.Id);

            entity.HasIndex(
                    allocation => new
                    {
                        allocation.CustomerPaymentId,
                        allocation.SaleId
                    })
                .IsUnique();

            entity.HasIndex(allocation => allocation.SaleId);

            entity.Property(allocation => allocation.Amount)
                .HasPrecision(18, 2);

            entity.HasOne(
                    allocation =>
                        allocation.CustomerPayment)
                .WithMany(payment => payment.Allocations)
                .HasForeignKey(
                    allocation =>
                        allocation.CustomerPaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(allocation => allocation.Sale)
                .WithMany()
                .HasForeignKey(allocation => allocation.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SupplierPayment>(entity =>
        {
            entity.ToTable(
                "SupplierPayments",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_SupplierPayments_Amount",
                        "\"Amount\" > 0");

                    table.HasCheckConstraint(
                        "CK_SupplierPayments_BalanceBefore",
                        "\"BalanceBefore\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierPayments_BalanceAfter",
                        "\"BalanceAfter\" >= 0");

                    table.HasCheckConstraint(
                        "CK_SupplierPayments_BalanceFlow",
                        "\"BalanceBefore\" >= \"BalanceAfter\"");

                    table.HasCheckConstraint(
                        "CK_SupplierPayments_ApplicationTotal",
                        "\"AppliedToPurchasesAmount\" + " +
                        "\"AppliedToAccountBalanceAmount\" = " +
                        "\"Amount\"");

                    table.HasCheckConstraint(
                        "CK_SupplierPayments_PaymentMethod",
                        "\"PaymentMethod\" <> 'Due'");
                });

            entity.HasKey(payment => payment.Id);

            entity.HasIndex(payment => payment.PaymentNumber)
                .IsUnique();

            entity.HasIndex(payment => payment.SupplierId);
            entity.HasIndex(payment => payment.PaidAtUtc);
            entity.HasIndex(payment => payment.PaymentMethod);

            entity.Property(payment => payment.Amount)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.BalanceBefore)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.BalanceAfter)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.AppliedToPurchasesAmount)
                .HasPrecision(18, 2);

            entity.Property(
                    payment =>
                        payment.AppliedToAccountBalanceAmount)
                .HasPrecision(18, 2);

            entity.Property(payment => payment.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(payment => payment.Supplier)
                .WithMany(supplier => supplier.Payments)
                .HasForeignKey(payment => payment.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SupplierPaymentAllocation>(entity =>
        {
            entity.ToTable(
                "SupplierPaymentAllocations",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_SupplierPaymentAllocations_Amount",
                        "\"Amount\" > 0");
                });

            entity.HasKey(allocation => allocation.Id);

            entity.HasIndex(
                    allocation => new
                    {
                        allocation.SupplierPaymentId,
                        allocation.PurchaseId
                    })
                .IsUnique();

            entity.HasIndex(allocation => allocation.PurchaseId);

            entity.Property(allocation => allocation.Amount)
                .HasPrecision(18, 2);

            entity.HasOne(
                    allocation =>
                        allocation.SupplierPayment)
                .WithMany(payment => payment.Allocations)
                .HasForeignKey(
                    allocation =>
                        allocation.SupplierPaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(allocation => allocation.Purchase)
                .WithMany()
                .HasForeignKey(allocation => allocation.PurchaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Prescription>(entity =>
        {
            entity.ToTable("Prescriptions");

            entity.HasKey(prescription => prescription.Id);

            entity.HasIndex(
                    prescription =>
                        prescription.PrescriptionNumber)
                .IsUnique();

            entity.HasIndex(
                prescription => prescription.CustomerId);

            entity.HasIndex(
                prescription => prescription.IssuedDate);

            entity.HasIndex(
                prescription => prescription.Status);

            entity.HasIndex(
                prescription => prescription.PrescriberName);

            entity.Property(prescription => prescription.IssuedDate)
                .HasColumnType("date");

            entity.Property(prescription => prescription.ValidUntil)
                .HasColumnType("date");

            entity.Property(prescription => prescription.Status)
                .HasConversion<string>()
                .HasMaxLength(40);

            entity.HasOne(prescription => prescription.Customer)
                .WithMany()
                .HasForeignKey(prescription => prescription.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PrescriptionItem>(entity =>
        {
            entity.ToTable(
                "PrescriptionItems",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_PrescriptionItems_QuantityPrescribed",
                        "\"QuantityPrescribed\" > 0");

                    table.HasCheckConstraint(
                        "CK_PrescriptionItems_QuantityDispensed",
                        "\"QuantityDispensed\" >= 0");

                    table.HasCheckConstraint(
                        "CK_PrescriptionItems_DispensedNotOverPrescribed",
                        "\"QuantityDispensed\" <= " +
                        "\"QuantityPrescribed\"");
                });

            entity.HasKey(item => item.Id);

            entity.HasIndex(item => item.PrescriptionId);
            entity.HasIndex(item => item.MedicineId);

            entity.HasOne(item => item.Prescription)
                .WithMany(prescription => prescription.Items)
                .HasForeignKey(item => item.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Medicine)
                .WithMany()
                .HasForeignKey(item => item.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PharmacyProfile>(entity =>
        {
            entity.ToTable(
                "PharmacyProfiles",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_PharmacyProfiles_ExpiryAlertDays",
                        "\"ExpiryAlertDays\" >= 1");
                });

            entity.HasKey(profile => profile.Id);

            entity.HasIndex(profile => new
                {
                    profile.PharmacyName,
                    profile.BranchName
                })
                .IsUnique();

            entity.Property(profile => profile.CurrencyCode)
                .HasMaxLength(3);

            entity.Property(profile => profile.CurrencySymbol)
                .HasMaxLength(8);

            entity.Property(profile => profile.TimeZoneId)
                .HasMaxLength(100);
        });

    }
}
