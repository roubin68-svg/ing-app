using IngApp.Domain.Entities.Auth;
using IngApp.Domain.Entities.Financial;
using IngApp.Domain.Entities.Kyc;
using IngApp.Domain.Entities.Menus;
using IngApp.Domain.Entities.Offers;
using IngApp.Domain.Entities.Permissions;
using IngApp.Domain.Entities.Products;
using IngApp.Domain.Entities.Roles;
using IngApp.Domain.Entities.Suppliers;
using IngApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IngApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserType> UserTypes => Set<UserType>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<SupplierProfile> SupplierProfiles => Set<SupplierProfile>();
    public DbSet<SupplierType> SupplierTypes => Set<SupplierType>();

    public DbSet<KycAttributeDefinition> KycAttributeDefinitions => Set<KycAttributeDefinition>();
    public DbSet<KycTemplate> KycTemplates => Set<KycTemplate>();
    public DbSet<UserDocument> UserDocuments => Set<UserDocument>();
    public DbSet<SupplierVerificationHistory> SupplierVerificationHistories { get; set; } = null!;
    public DbSet<SupplierActivityLog> SupplierActivityLogs { get; set; } = null!;
    public DbSet<VisitorProfile> VisitorProfiles => Set<VisitorProfile>();
    public DbSet<BuyerProfile> BuyerProfiles => Set<BuyerProfile>();


    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductAttributeDefinition> ProductAttributeDefinitions => Set<ProductAttributeDefinition>();
    public DbSet<ProductAttributeTemplate> ProductAttributeTemplates => Set<ProductAttributeTemplate>();
    public DbSet<SupplierCategoryAccess> SupplierCategoryAccesses => Set<SupplierCategoryAccess>();

    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<OfferDocument> OfferDocuments => Set<OfferDocument>();
    public DbSet<OfferClickLog> OfferClickLogs => Set<OfferClickLog>();
    public DbSet<OfferStatusHistory> OfferStatusHistories => Set<OfferStatusHistory>();
    public DbSet<OfferContactUnlock> OfferContactUnlocks => Set<OfferContactUnlock>();

    // Financial Entities
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<WalletType> WalletTypes => Set<WalletType>();
    public DbSet<TransactionDirection> TransactionDirections => Set<TransactionDirection>();
    public DbSet<FinancialOperationType> FinancialOperationTypes => Set<FinancialOperationType>();
    public DbSet<FinancialTransactionStatus> FinancialTransactionStatuses => Set<FinancialTransactionStatus>();
    public DbSet<FinancialReferenceType> FinancialReferenceTypes => Set<FinancialReferenceType>();
    public DbSet<UnlockSourceType> UnlockSourceTypes => Set<UnlockSourceType>();
    public DbSet<Pricing> Pricings => Set<Pricing>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<SubscriptionStatus> SubscriptionStatuses => Set<SubscriptionStatus>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();
    public DbSet<PaymentStatus> PaymentStatuses => Set<PaymentStatus>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
    public DbSet<CommissionTransaction> CommissionTransactions => Set<CommissionTransaction>();
    public DbSet<VisitorCommissionRule> VisitorCommissionRules => Set<VisitorCommissionRule>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // تمام configuration های داخل اسمبلی Infrastructure را اعمال می‌کند
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    }

}

