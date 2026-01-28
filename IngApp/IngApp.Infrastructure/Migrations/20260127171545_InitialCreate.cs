using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IngApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommissionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ExchangeRateToRial = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialOperationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialOperationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReferenceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReferenceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KycAttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    DefaultRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycAttributeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequiredPermissionCode = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WizardStep = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HasTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpireAtBySupplier = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpireAtBySystem = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SearchDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Purpose = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientIdentifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentGateways",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGateways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    PriceRial = table.Column<long>(type: "bigint", nullable: false),
                    UnlimitedContactViews = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pricings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AmountRial = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataType = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionDirections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDirections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnlockSourceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnlockSourceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KycTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierTypeId = table.Column<int>(type: "int", nullable: false),
                    KycAttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KycTemplates_KycAttributeDefinitions_KycAttributeDefinitionId",
                        column: x => x.KycAttributeDefinitionId,
                        principalTable: "KycAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferClickLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClickType = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClickedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferClickLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferClickLogs_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferDocuments_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    AdminUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferStatusHistories_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierCategoryAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCategoryAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierCategoryAccesses_ProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferContactUnlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChargedTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferContactUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferContactUnlocks_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferContactUnlocks_UnlockSourceTypes_SourceTypeId",
                        column: x => x.SourceTypeId,
                        principalTable: "UnlockSourceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserTypeId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_UserTypes_UserTypeId",
                        column: x => x.UserTypeId,
                        principalTable: "UserTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributeTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAttributeTemplates_ProductAttributeDefinitions_AttributeDefinitionId",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "ProductAttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductAttributeTemplates_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommissionTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommissionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalAmountRial = table.Column<long>(type: "bigint", nullable: false),
                    CommissionAmountRial = table.Column<long>(type: "bigint", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    WalletTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionTransactions_Users_BuyerUserId",
                        column: x => x.BuyerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionTransactions_Users_VisitorUserId",
                        column: x => x.VisitorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GatewayId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    AmountRial = table.Column<long>(type: "bigint", nullable: false),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WalletTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GatewayResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentGateways_GatewayId",
                        column: x => x.GatewayId,
                        principalTable: "PaymentGateways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "PaymentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierTypeId = table.Column<int>(type: "int", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessType = table.Column<int>(type: "int", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPosition = table.Column<int>(type: "int", nullable: true),
                    ContactMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationStatus = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierProfiles_SupplierTypes_SupplierTypeId",
                        column: x => x.SupplierTypeId,
                        principalTable: "SupplierTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KycAttributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDocuments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "SubscriptionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitorProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactMobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitorProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    WalletTypeId = table.Column<int>(type: "int", nullable: false),
                    BalanceRial = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wallets_WalletTypes_WalletTypeId",
                        column: x => x.WalletTypeId,
                        principalTable: "WalletTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierActivityLogs_SupplierProfiles_SupplierProfileId",
                        column: x => x.SupplierProfileId,
                        principalTable: "SupplierProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierVerificationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    AdminUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierVerificationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierVerificationHistories_SupplierProfiles_SupplierProfileId",
                        column: x => x.SupplierProfileId,
                        principalTable: "SupplierProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactMobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferredByVisitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuyerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerProfiles_VisitorProfiles_ReferredByVisitorId",
                        column: x => x.ReferredByVisitorId,
                        principalTable: "VisitorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitorCommissionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommissionRuleCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorCommissionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitorCommissionRules_VisitorProfiles_VisitorProfileId",
                        column: x => x.VisitorProfileId,
                        principalTable: "VisitorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectionId = table.Column<int>(type: "int", nullable: false),
                    AmountRial = table.Column<long>(type: "bigint", nullable: false),
                    OperationTypeId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ReferenceTypeId = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_FinancialOperationTypes_OperationTypeId",
                        column: x => x.OperationTypeId,
                        principalTable: "FinancialOperationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_FinancialReferenceTypes_ReferenceTypeId",
                        column: x => x.ReferenceTypeId,
                        principalTable: "FinancialReferenceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_FinancialTransactionStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "FinancialTransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_TransactionDirections_DirectionId",
                        column: x => x.DirectionId,
                        principalTable: "TransactionDirections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CommissionRules",
                columns: new[] { "Id", "Code", "CommissionPercentage", "CreatedAt", "Description", "EffectiveFrom", "EffectiveTo", "IsActive", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "UnlockContactCommission", 10.00m, new DateTime(2026, 1, 27, 17, 15, 44, 973, DateTimeKind.Utc).AddTicks(2658), "پورسانت از هزینه باز کردن اطلاعات تماس آگهی", null, null, true, "پورسانت باز کردن اطلاعات تماس", null },
                    { 2, "SubscriptionCommission", 15.00m, new DateTime(2026, 1, 27, 17, 15, 44, 973, DateTimeKind.Utc).AddTicks(2660), "پورسانت از خرید اشتراک", null, null, true, "پورسانت خرید اشتراک", null }
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "Description", "ExchangeRateToRial", "IsActive", "Symbol", "Title" },
                values: new object[] { 1, "IRR", null, 1m, true, "ریال", "ریال ایران" });

            migrationBuilder.InsertData(
                table: "FinancialOperationTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "TopUp", "واریز وجه به کیف پول", true, "شارژ کیف پول" },
                    { 2, "UnlockContactFee", "هزینه نمایش اطلاعات تماس آگهی", true, "هزینه باز کردن اطلاعات تماس" },
                    { 3, "SubscriptionPurchase", "خرید پکیج/اشتراک", true, "خرید اشتراک" },
                    { 4, "OnboardingFee", "هزینه یک‌باره ثبت‌نام به عنوان تأمین‌کننده", true, "هزینه ثبت‌نام تأمین‌کننده" },
                    { 5, "CommissionEarned", "پورسانت دریافتی از بازاریابی", true, "دریافت پورسانت" }
                });

            migrationBuilder.InsertData(
                table: "FinancialReferenceTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Offer", "مرجع: آگهی", true, "آگهی" },
                    { 2, "Subscription", "مرجع: اشتراک/پکیج", true, "اشتراک" },
                    { 3, "Payment", "مرجع: پرداخت/شارژ", true, "پرداخت" },
                    { 4, "SupplierOnboarding", "مرجع: ثبت‌نام تأمین‌کننده", true, "ثبت‌نام تأمین‌کننده" },
                    { 5, "WalletTransaction", "مرجع: تراکنش دیگر (مثلاً برای پورسانت)", true, "تراکنش کیف پول" }
                });

            migrationBuilder.InsertData(
                table: "FinancialTransactionStatuses",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Pending", "تراکنش در حال پردازش", true, "در انتظار" },
                    { 2, "Committed", "تراکنش با موفقیت انجام شد", true, "تأیید شده" },
                    { 3, "Failed", "تراکنش با خطا مواجه شد", true, "ناموفق" },
                    { 4, "Reversed", "تراکنش برگشت داده شد", true, "برگشت خورده" }
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[,]
                {
                    { 1, "DashboardOutlined", true, "dashboard", 1, null, null, "/", "داشبورد" },
                    { 2, "ShoppingOutlined", true, "products", 2, null, "Product.ViewAll", "#", "مدیریت محصولات" },
                    { 5, "SettingOutlined", true, "settings", 4, null, "Settings.View", "#", "تنظیمات" },
                    { 6, "TeamOutlined", true, "user-management", 3, null, "User.Manage", "#", "مدیریت کاربران" },
                    { 11, "TeamOutlined", true, "suppliers", 5, null, "Supplier.View", "#", "مدیریت تأمین‌کنندگان" }
                });

            migrationBuilder.InsertData(
                table: "PaymentGateways",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Mock", "درگاه پرداخت Mock برای تست", true, "درگاه پرداخت آزمایشی" },
                    { 2, "Zarinpal", "درگاه پرداخت زرین‌پال", false, "زرین‌پال" }
                });

            migrationBuilder.InsertData(
                table: "PaymentStatuses",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Pending", "پرداخت در انتظار است", true, "در انتظار" },
                    { 2, "Success", "پرداخت با موفقیت انجام شد", true, "موفق" },
                    { 3, "Failed", "پرداخت ناموفق بود", true, "ناموفق" },
                    { 4, "Cancelled", "پرداخت لغو شد", true, "لغو شده" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Code", "Description", "DisplayName", "IsActive" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), "Settings.View", "", "مشاهده تنظیمات", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000002"), "User.Manage", "", "مدیریت کاربران", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000003"), "Role.Manage", "", "مدیریت نقش‌ها", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000004"), "Permission.Manage", "", "مدیریت دسترسی‌ها", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000005"), "Menu.Manage", "", "مدیریت منوها", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000006"), "Product.ViewAll", "", "مشاهده محصولات", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000007"), "ProductCategory.Manage", "", "مدیریت دسته‌بندی محصولات", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000008"), "SupplierType.Manage", "", "مدیریت نوع تأمین‌کننده", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000009"), "Supplier.Manage", "", "مدیریت تأمین‌کنندگان", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000a"), "Kyc.Review", "", "بررسی مدارک KYC", true },
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), "Offer.Manage", "", "مدیریت آگهی‌ها", true }
                });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "DisplayOrder", "DurationMonths", "IsActive", "PriceRial", "Title", "UnlimitedContactViews", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Plan1Month", new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8608), "اشتراک 1 ماهه با دسترسی نامحدود به اطلاعات تماس", 1, 1, true, 1000000L, "پلن 1 ماهه", true, null },
                    { 2, "Plan3Month", new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8610), "اشتراک 3 ماهه با دسترسی نامحدود به اطلاعات تماس", 2, 3, true, 2700000L, "پلن 3 ماهه", true, null },
                    { 3, "Plan6Month", new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8613), "اشتراک 6 ماهه با دسترسی نامحدود به اطلاعات تماس", 3, 6, true, 5100000L, "پلن 6 ماهه", true, null },
                    { 4, "Plan12Month", new DateTime(2026, 1, 27, 17, 15, 44, 975, DateTimeKind.Utc).AddTicks(8615), "اشتراک 12 ماهه با دسترسی نامحدود به اطلاعات تماس", 4, 12, true, 9600000L, "پلن 12 ماهه", true, null }
                });

            migrationBuilder.InsertData(
                table: "Pricings",
                columns: new[] { "Id", "AmountRial", "Code", "CreatedAt", "Description", "EffectiveFrom", "EffectiveTo", "IsActive", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 10000L, "UnlockContactFee", new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(940), "هزینه یک‌باره برای نمایش اطلاعات تماس یک آگهی", new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(936), null, true, "هزینه باز کردن اطلاعات تماس", null },
                    { 2, 50000L, "OnboardingFee", new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(942), "هزینه یک‌باره ثبت‌نام به عنوان تأمین‌کننده", new DateTime(2026, 1, 27, 17, 15, 44, 976, DateTimeKind.Utc).AddTicks(942), null, true, "هزینه ثبت‌نام تأمین‌کننده", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "DisplayName", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), "دسترسی‌های پایه کاربر", "خریدار", true, "Buyer" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "دسترسی‌های پنل تأمین‌کننده", "تأمین‌کننده", true, "Supplier" },
                    { new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), "دسترسی کامل به سیستم", "ادمین", true, "Admin" }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionStatuses",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Active", "اشتراک فعال است", true, "فعال" },
                    { 2, "Expired", "اشتراک منقضی شده است", true, "منقضی شده" },
                    { 3, "Cancelled", "اشتراک لغو شده است", true, "لغو شده" },
                    { 4, "Pending", "اشتراک در انتظار فعال‌سازی", true, "در انتظار" }
                });

            migrationBuilder.InsertData(
                table: "TransactionDirections",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Credit", "افزایش موجودی کیف پول", true, "واریز" },
                    { 2, "Debit", "کاهش موجودی کیف پول", true, "برداشت" }
                });

            migrationBuilder.InsertData(
                table: "UnlockSourceTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Paid", "از طریق پرداخت از کیف پول", true, "پرداخت شده" },
                    { 2, "Subscription", "از طریق اشتراک فعال", true, "اشتراک" }
                });

            migrationBuilder.InsertData(
                table: "UserTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { 1, "Buyer", null, true, "خریدار" },
                    { 2, "Supplier", null, true, "تأمین‌کننده" },
                    { 3, "Admin", null, true, "مدیر سیستم" },
                    { 4, "Visitor", null, true, "بازاریاب" }
                });

            migrationBuilder.InsertData(
                table: "WalletTypes",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[] { 1, "Main", null, true, "کیف پول اصلی" });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Icon", "IsActive", "Key", "Order", "ParentId", "RequiredPermissionCode", "Route", "Title" },
                values: new object[,]
                {
                    { 3, null, true, "products-list", 1, 2, "Product.ViewAll", "/products", "لیست محصولات" },
                    { 4, null, true, "category-management", 2, 2, "ProductCategory.Manage", "/product-categories", "مدیریت دسته‌بندی‌ها" },
                    { 7, null, true, "users", 1, 6, "User.Manage", "/users", "کاربران" },
                    { 8, null, true, "roles", 2, 6, "Role.Manage", "/roles", "نقش‌ها" },
                    { 9, null, true, "permissions", 3, 6, "Permission.Manage", "/permissions", "دسترسی‌ها" },
                    { 10, null, true, "menu-settings", 2, 5, "Menu.Manage", "/menu-settings", "تنظیمات منو" },
                    { 12, null, true, "supplier-types", 2, 11, "SupplierType.Manage", "/supplier-types", "مدیریت نوع تأمین‌کننده" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000002"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000003"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000004"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000005"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000006"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000007"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000008"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000009"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000a"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") },
                    { new Guid("aaaaaaaa-0000-0000-0000-00000000000b"), new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1") }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DisplayName", "IsActive", "PasswordHash", "PhoneNumber", "SubscriptionLevel", "UpdatedAt", "UserTypeId", "VerificationStatus" },
                values: new object[] { new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0"), new DateTime(2026, 1, 27, 17, 15, 44, 988, DateTimeKind.Utc).AddTicks(1020), "علی هور", true, null, "09123823632", 0, null, 3, 0 });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("a3f0b7e8-3a42-4b27-a9c4-64e0a91b9fd1"), new Guid("64fa4b00-95cf-4a58-6f40-08de38f0e8e0") });

            migrationBuilder.CreateIndex(
                name: "IX_BuyerProfiles_ReferredByVisitorId",
                table: "BuyerProfiles",
                column: "ReferredByVisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerProfiles_UserId",
                table: "BuyerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_Code",
                table: "CommissionRules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_BuyerUserId",
                table: "CommissionTransactions",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_ReferenceId",
                table: "CommissionTransactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_VisitorUserId",
                table: "CommissionTransactions",
                column: "VisitorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionTransactions_VisitorUserId_CommissionType",
                table: "CommissionTransactions",
                columns: new[] { "VisitorUserId", "CommissionType" });

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperationTypes_Code",
                table: "FinancialOperationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReferenceTypes_Code",
                table: "FinancialReferenceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionStatuses_Code",
                table: "FinancialTransactionStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KycTemplates_KycAttributeDefinitionId",
                table: "KycTemplates",
                column: "KycAttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_Key",
                table: "MenuItems",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentId",
                table: "MenuItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferClickLogs_ClickedAt",
                table: "OfferClickLogs",
                column: "ClickedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OfferClickLogs_OfferId_ClickType",
                table: "OfferClickLogs",
                columns: new[] { "OfferId", "ClickType" });

            migrationBuilder.CreateIndex(
                name: "IX_OfferContactUnlocks_OfferId",
                table: "OfferContactUnlocks",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferContactUnlocks_OfferId_UserId",
                table: "OfferContactUnlocks",
                columns: new[] { "OfferId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfferContactUnlocks_SourceTypeId",
                table: "OfferContactUnlocks",
                column: "SourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferContactUnlocks_UserId",
                table: "OfferContactUnlocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferDocuments_AttributeDefinitionId",
                table: "OfferDocuments",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferDocuments_OfferId",
                table: "OfferDocuments",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferDocuments_OfferId_IsDeleted",
                table: "OfferDocuments",
                columns: new[] { "OfferId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_ProductId",
                table: "Offers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SearchDateTime",
                table: "Offers",
                column: "SearchDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_Status",
                table: "Offers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SupplierUserId",
                table: "Offers",
                column: "SupplierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferStatusHistories_CreatedAt",
                table: "OfferStatusHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OfferStatusHistories_OfferId",
                table: "OfferStatusHistories",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_Code",
                table: "PaymentGateways",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayId",
                table: "Payments",
                column: "GatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayTransactionId",
                table: "Payments",
                column: "GatewayTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StatusId",
                table: "Payments",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId_StatusId",
                table: "Payments",
                columns: new[] { "UserId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_Code",
                table: "PaymentStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Code",
                table: "Plans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_Code",
                table: "Pricings",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeTemplates_AttributeDefinitionId",
                table: "ProductAttributeTemplates",
                column: "AttributeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeTemplates_ProductId_AttributeDefinitionId",
                table: "ProductAttributeTemplates",
                columns: new[] { "ProductId", "AttributeDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentId",
                table: "ProductCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Name",
                table: "Products",
                columns: new[] { "CategoryId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatuses_Code",
                table: "SubscriptionStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierActivityLogs_SupplierProfileId",
                table: "SupplierActivityLogs",
                column: "SupplierProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategoryAccesses_ProductCategoryId",
                table: "SupplierCategoryAccesses",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategoryAccesses_UserId_ProductCategoryId",
                table: "SupplierCategoryAccesses",
                columns: new[] { "UserId", "ProductCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProfiles_SupplierTypeId",
                table: "SupplierProfiles",
                column: "SupplierTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierProfiles_UserId",
                table: "SupplierProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierVerificationHistories_SupplierProfileId",
                table: "SupplierVerificationHistories",
                column: "SupplierProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDirections_Code",
                table: "TransactionDirections",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnlockSourceTypes_Code",
                table: "UnlockSourceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_UserId_IsDeleted",
                table: "UserDocuments",
                columns: new[] { "UserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDocuments_UserId_KycAttributeDefinitionId_IsDeleted",
                table: "UserDocuments",
                columns: new[] { "UserId", "KycAttributeDefinitionId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserTypeId",
                table: "Users",
                column: "UserTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_EndDate",
                table: "UserSubscriptions",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "UserSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_StatusId",
                table: "UserSubscriptions",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_StatusId",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "StatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTypes_Code",
                table: "UserTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitorCommissionRule_Visitor_Code_Active",
                table: "VisitorCommissionRules",
                columns: new[] { "VisitorProfileId", "CommissionRuleCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_VisitorCommissionRule_Visitor_Code_Unique",
                table: "VisitorCommissionRules",
                columns: new[] { "VisitorProfileId", "CommissionRuleCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitorProfiles_ReferralCode",
                table: "VisitorProfiles",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitorProfiles_UserId",
                table: "VisitorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CurrencyId",
                table: "Wallets",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId_WalletTypeId",
                table: "Wallets",
                columns: new[] { "UserId", "WalletTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_WalletTypeId",
                table: "Wallets",
                column: "WalletTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_DirectionId",
                table: "WalletTransactions",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_IdempotencyKey",
                table: "WalletTransactions",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_OperationTypeId",
                table: "WalletTransactions",
                column: "OperationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ReferenceTypeId_ReferenceId",
                table: "WalletTransactions",
                columns: new[] { "ReferenceTypeId", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_StatusId",
                table: "WalletTransactions",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTypes_Code",
                table: "WalletTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyerProfiles");

            migrationBuilder.DropTable(
                name: "CommissionRules");

            migrationBuilder.DropTable(
                name: "CommissionTransactions");

            migrationBuilder.DropTable(
                name: "KycTemplates");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "OfferClickLogs");

            migrationBuilder.DropTable(
                name: "OfferContactUnlocks");

            migrationBuilder.DropTable(
                name: "OfferDocuments");

            migrationBuilder.DropTable(
                name: "OfferStatusHistories");

            migrationBuilder.DropTable(
                name: "OtpCodes");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Pricings");

            migrationBuilder.DropTable(
                name: "ProductAttributeTemplates");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SupplierActivityLogs");

            migrationBuilder.DropTable(
                name: "SupplierCategoryAccesses");

            migrationBuilder.DropTable(
                name: "SupplierVerificationHistories");

            migrationBuilder.DropTable(
                name: "UserDocuments");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "VisitorCommissionRules");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "KycAttributeDefinitions");

            migrationBuilder.DropTable(
                name: "UnlockSourceTypes");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "PaymentGateways");

            migrationBuilder.DropTable(
                name: "PaymentStatuses");

            migrationBuilder.DropTable(
                name: "ProductAttributeDefinitions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "SupplierProfiles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "SubscriptionStatuses");

            migrationBuilder.DropTable(
                name: "VisitorProfiles");

            migrationBuilder.DropTable(
                name: "FinancialOperationTypes");

            migrationBuilder.DropTable(
                name: "FinancialReferenceTypes");

            migrationBuilder.DropTable(
                name: "FinancialTransactionStatuses");

            migrationBuilder.DropTable(
                name: "TransactionDirections");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "SupplierTypes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WalletTypes");

            migrationBuilder.DropTable(
                name: "UserTypes");
        }
    }
}
