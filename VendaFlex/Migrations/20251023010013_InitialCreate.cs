using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VendaFlex.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyConfigs",
                columns: table => new
                {
                    CompanyConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IndustryType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxRegime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrencySymbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DefaultTaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceFooterText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InvoicePrefix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NextInvoiceNumber = table.Column<int>(type: "int", nullable: false),
                    InvoiceFormat = table.Column<int>(type: "int", nullable: false),
                    IncludeCustomerData = table.Column<bool>(type: "bit", nullable: false),
                    AllowAnonymousInvoice = table.Column<bool>(type: "bit", nullable: false),
                    BusinessHours = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyConfigs", x => x.CompanyConfigId);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseTypes",
                columns: table => new
                {
                    ExpenseTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseTypes", x => x.ExpenseTypeId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequiresReference = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.PaymentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdentificationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.PersonId);
                });

            migrationBuilder.CreateTable(
                name: "Privileges",
                columns: table => new
                {
                    PrivilegeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privileges", x => x.PrivilegeId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 18, scale: 2, nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 18, scale: 2, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    AllowBackorder = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ControlsStock = table.Column<bool>(type: "bit", nullable: false),
                    MinimumStock = table.Column<int>(type: "int", nullable: true),
                    MaximumStock = table.Column<int>(type: "int", nullable: true),
                    ReorderPoint = table.Column<int>(type: "int", nullable: true),
                    HasExpirationDate = table.Column<bool>(type: "bit", nullable: false),
                    ExpirationDays = table.Column<int>(type: "int", nullable: true),
                    ExpirationWarningDays = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InternalCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ExternalCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Persons_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expirations",
                columns: table => new
                {
                    ExpirationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expirations", x => x.ExpirationId);
                    table.ForeignKey(
                        name: "FK_Expirations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    PriceHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OldSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NewSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OldCostPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NewCostPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.PriceHistoryId);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReservedQuantity = table.Column<int>(type: "int", nullable: true),
                    LastStockUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastStockUpdateByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Stocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    NewValues = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseTypeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.ExpenseId);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseTypes_ExpenseTypeId",
                        column: x => x.ExpenseTypeId,
                        principalTable: "ExpenseTypes",
                        principalColumn: "ExpenseTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    InternalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    StockMovementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PreviousQuantity = table.Column<int>(type: "int", nullable: true),
                    NewQuantity = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.StockMovementId);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPrivileges",
                columns: table => new
                {
                    UserPrivilegeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PrivilegeId = table.Column<int>(type: "int", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrivileges", x => x.UserPrivilegeId);
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Privileges_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privileges",
                        principalColumn: "PrivilegeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPrivileges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceProducts",
                columns: table => new
                {
                    InvoiceProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceProducts", x => x.InvoiceProductId);
                    table.ForeignKey(
                        name: "FK_InvoiceProducts_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "PaymentTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "Code", "CreatedAt", "CreatedByUserId", "DeletedAt", "Description", "DisplayOrder", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentCategoryId", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, "GEN", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Categoria padrão", 0, null, true, false, "Geral", null, null, null },
                    { 2, "BEV", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Bebidas e refrigerantes", 1, null, true, false, "Bebidas", null, null, null },
                    { 3, "FOOD", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Alimentos e perecíveis", 2, null, true, false, "Alimentos", null, null, null },
                    { 4, "HYG", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Higiene pessoal", 3, null, true, false, "Higiene", null, null, null },
                    { 5, "CLEAN", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Produtos de limpeza", 4, null, true, false, "Limpeza", null, null, null },
                    { 6, "ELEC", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Produtos eletrônicos", 5, null, true, false, "Eletrônicos", null, null, null },
                    { 7, "STAT", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Papelaria e escritório", 6, null, true, false, "Papelaria", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "ExpenseTypes",
                columns: new[] { "ExpenseTypeId", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Despesas com aluguel", true, "Aluguel" },
                    { 2, "Pagamento de salários", true, "Salários" },
                    { 3, "Água, luz, internet", true, "Utilities" },
                    { 4, "Despesas com marketing", true, "Marketing" },
                    { 5, "Manutenção e reparos", true, "Manutenção" },
                    { 6, "Outras despesas", true, "Outros" }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "PaymentTypeId", "Description", "IsActive", "Name", "RequiresReference" },
                values: new object[,]
                {
                    { 1, "Pagamento em dinheiro", true, "Dinheiro", false },
                    { 2, "Pagamento com cartão de débito", true, "Cartão de Débito", true },
                    { 3, "Pagamento com cartão de crédito", true, "Cartão de Crédito", true },
                    { 4, "Transferência bancária", true, "Transferência Bancária", true },
                    { 5, "Pagamento via PIX", true, "PIX", true },
                    { 6, "Pagamento com múltiplas formas", true, "Múltiplas Formas", false }
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "PersonId", "Address", "City", "ContactPerson", "Country", "CreatedAt", "CreatedByUserId", "CreditLimit", "CurrentBalance", "DeletedAt", "Email", "IdentificationNumber", "IsActive", "IsDeleted", "MobileNumber", "Name", "Notes", "PhoneNumber", "PostalCode", "ProfileImageUrl", "Rating", "State", "TaxId", "Type", "UpdatedAt", "UpdatedByUserId", "Website" },
                values: new object[,]
                {
                    { 1, "Rua A, 100", "Luanda", null, "Angola", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 100000m, 0m, null, "carlos.silva@example.com", null, true, false, null, "Duarte Gauss", null, "+244900111111", null, null, null, null, null, 2, null, null, null },
                    { 101, "Rua A, 100", "Luanda", null, "Angola", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 100000m, 0m, null, "carlos.silva@example.com", null, true, false, null, "Carlos Silva", null, "+244900111111", null, null, null, null, null, 1, null, null, null },
                    { 102, "Av. Central, 200", "Luanda", null, "Angola", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 150000m, 0m, null, "ana.pereira@example.com", null, true, false, null, "Ana Pereira", null, "+244900222222", null, null, null, null, null, 1, null, null, null },
                    { 103, "Parque Industrial, Armazém 3", "Luanda", null, "Angola", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 500000m, 0m, null, "compras@empresaabc.ao", null, true, false, null, "Empresa ABC Lda", null, "+244900333333", null, null, null, null, null, 1, null, null, null },
                    { 201, "Zona Comercial, Loja 10", "Luanda", null, "Angola", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0m, 0m, null, "contato@alimentoscia.ao", null, true, false, null, "Alimentos & Cia", null, "+244910000001", null, null, 5, null, null, 2, null, null, null },
                    { 202, "Parque das Indústrias, Lote 4", "Luanda", null, "Angola", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0m, 0m, null, "vendas@bebidassa.ao", null, true, false, null, "Bebidas SA", null, "+244910000002", null, null, 4, null, null, 2, null, null, null },
                    { 203, "Rua da Tecnologia, 55", "Luanda", null, "Angola", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0m, 0m, null, "suporte@techimport.ao", null, true, false, null, "Tech Import", null, "+244910000003", null, null, 5, null, null, 2, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Privileges",
                columns: new[] { "PrivilegeId", "Code", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "ADMIN", "Acesso total ao sistema", true, "Administrador" },
                    { 2, "MANAGER", "Gestão de vendas e relatórios", true, "Gerente" },
                    { 3, "SELLER", "Realizar vendas", true, "Vendedor" },
                    { 4, "VIEWER", "Apenas visualização", true, "Visualizador" },
                    { 5, "AUDITOR", "Auditoria e logs", true, "Auditor" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "AllowBackorder", "Barcode", "CategoryId", "ControlsStock", "CostPrice", "CreatedAt", "CreatedByUserId", "DeletedAt", "Description", "Dimensions", "DiscountPercentage", "DisplayOrder", "ExpirationDays", "ExpirationWarningDays", "ExternalCode", "HasExpirationDate", "InternalCode", "IsDeleted", "IsFeatured", "MaximumStock", "MinimumStock", "Name", "PhotoUrl", "ReorderPoint", "SKU", "SalePrice", "ShortDescription", "Status", "SupplierId", "TaxRate", "UpdatedAt", "UpdatedByUserId", "Weight" },
                values: new object[,]
                {
                    { 1, false, "5600000000011", 3, true, 3500m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Saco de arroz branco tipo 1 com 5kg", "35x25x8cm", 0m, 1, null, null, "EXT-0001", false, "P0001", false, true, 300, 20, "Arroz 5kg", "", 50, "ARZ-5KG", 4500m, "Arroz tipo 1 - 5kg", 1, 201, 14m, null, null, "5kg" },
                    { 2, false, "5600000000028", 3, true, 800m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Pacote de feijão carioca de 1kg", "20x12x5cm", 0m, 2, null, null, "EXT-0002", false, "P0002", false, false, 400, 30, "Feijão 1kg", "", 60, "FEJ-1KG", 1200m, "Feijão carioca 1kg", 1, 201, 14m, null, null, "1kg" },
                    { 3, false, "5600000000035", 3, true, 1000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Garrafa de óleo vegetal com 1 litro", "10x10x25cm", 0m, 3, 365, 30, "EXT-0003", true, "P0003", false, false, 200, 20, "Óleo 1L", "", 40, "OLEO-1L", 1500m, "Óleo vegetal 1L", 1, 201, 14m, null, null, "1L" },
                    { 4, false, "5600000000042", 2, true, 700m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Leite UHT integral 1L", "7x7x20cm", 0m, 4, 180, 20, "EXT-0004", true, "P0004", false, true, 500, 40, "Leite UHT 1L", "", 80, "LEI-1L", 900m, "Leite UHT integral", 1, 202, 14m, null, null, "1L" },
                    { 5, false, "5600000000059", 2, true, 500m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Garrafa de refrigerante cola 2 litros", "12x12x32cm", 0m, 5, 365, 30, "EXT-0005", true, "P0005", false, false, 600, 50, "Refrigerante Cola 2L", "", 100, "COLA-2L", 800m, "Refrigerante sabor cola 2L", 1, 202, 14m, null, null, "2L" },
                    { 6, true, "5600000000066", 2, true, 150m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Garrafa de água mineral 1.5L", "10x10x30cm", 0m, 6, 365, 60, "EXT-0006", true, "P0006", false, false, 1000, 80, "Água Mineral 1.5L", "", 200, "AGUA-1_5L", 300m, "Água mineral sem gás 1.5L", 1, 202, 14m, null, null, "1.5L" },
                    { 7, false, "5600000000073", 4, true, 120m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Sabonete perfumado em barra 90g", "8x5x3cm", 0m, 7, null, null, "EXT-0007", false, "P0007", false, false, 800, 50, "Sabonete 90g", "", 150, "SAB-90G", 250m, "Sabonete em barra 90g", 1, 203, 14m, null, null, "90g" },
                    { 8, false, "5600000000080", 4, true, 1200m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Frasco de shampoo 400ml", "8x5x18cm", 0m, 8, 730, 60, "EXT-0008", true, "P0008", false, true, 200, 20, "Shampoo 400ml", "", 40, "SHA-400", 1800m, "Shampoo hidratante 400ml", 1, 203, 14m, null, null, "400ml" },
                    { 9, false, "5600000000097", 5, true, 400m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Garrafa de detergente 1L", "10x10x25cm", 0m, 9, null, null, "EXT-0009", false, "P0009", false, false, 300, 30, "Detergente 1L", "", 60, "DET-1L", 700m, "Detergente multiuso 1L", 1, 203, 14m, null, null, "1L" },
                    { 10, false, "5600000000103", 7, true, 2800m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Resma de papel A4 com 500 folhas", "30x21x5cm", 0m, 10, null, null, "EXT-0010", false, "P0010", false, true, 100, 10, "Papel A4 500 folhas", "", 20, "PAP-A4-500", 3500m, "Resma Papel A4 500fls", 1, 203, 14m, null, null, "2.5kg" },
                    { 11, true, "5600000000110", 7, true, 50m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Caneta esferográfica tinta azul", "14x1x1cm", 0m, 11, null, null, "EXT-0011", false, "P0011", false, false, 2000, 100, "Caneta Azul", "", 300, "CAN-AZUL", 100m, "Caneta esferográfica azul", 1, 203, 14m, null, null, "20g" },
                    { 12, false, "5600000000127", 3, true, 250m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Tablete de chocolate ao leite 100g", "10x5x1cm", 0m, 12, 365, 30, "EXT-0012", true, "P0012", false, false, 400, 30, "Chocolate 100g", "", 60, "CHOC-100", 400m, "Tablete de chocolate 100g", 1, 201, 14m, null, null, "100g" },
                    { 13, false, "5600000000134", 3, true, 200m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Pacote de biscoito recheado 200g", "15x10x3cm", 0m, 13, 270, 30, "EXT-0013", true, "P0013", false, false, 500, 40, "Biscoito 200g", "", 80, "BIS-200", 350m, "Biscoito recheado 200g", 1, 201, 14m, null, null, "200g" },
                    { 14, false, "5600000000141", 3, true, 1500m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Pacote de café 500g", "20x10x6cm", 0m, 14, 540, 45, "EXT-0014", true, "P0014", false, true, 200, 15, "Café 500g", "", 30, "CAF-500", 2200m, "Café torrado e moído 500g", 1, 201, 14m, null, null, "500g" },
                    { 15, true, "5600000000158", 6, true, 10000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, "Fones de ouvido com microfone", "10x8x5cm", 0m, 15, null, null, "EXT-0015", false, "P0015", false, true, 100, 5, "Fones de Ouvido", "", 10, "FONES-BT", 15000m, "Fones intra-auriculares", 1, 203, 14m, null, null, "150g" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "CreatedByUserId", "DeletedAt", "FailedLoginAttempts", "IsDeleted", "LastLoginAt", "LastLoginIp", "LockedUntil", "PasswordHash", "PersonId", "Status", "UpdatedAt", "UpdatedByUserId", "Username" },
                values: new object[] { 4, new DateTime(2025, 10, 23, 1, 0, 11, 740, DateTimeKind.Utc).AddTicks(3814), null, null, 0, false, null, "", null, "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918", 1, 1, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "Expirations",
                columns: new[] { "ExpirationId", "BatchNumber", "CreatedAt", "CreatedByUserId", "DeletedAt", "ExpirationDate", "IsDeleted", "Notes", "ProductId", "Quantity", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, "OLEO-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote inicial", 3, 30, null, null },
                    { 2, "OLEO-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote reserva", 3, 30, null, null },
                    { 3, "LEI-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote A", 4, 60, null, null },
                    { 4, "LEI-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote B", 4, 60, null, null },
                    { 5, "COLA-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Produção 1", 5, 45, null, null },
                    { 6, "COLA-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Produção 2", 5, 45, null, null },
                    { 7, "AGUA-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote principal", 6, 100, null, null },
                    { 8, "AGUA-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote adicional", 6, 100, null, null },
                    { 9, "SHA-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote A", 8, 35, null, null },
                    { 10, "SHA-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote B", 8, 35, null, null },
                    { 11, "CHOC-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote doce A", 12, 25, null, null },
                    { 12, "CHOC-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote doce B", 12, 25, null, null },
                    { 13, "BIS-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote crocante A", 13, 35, null, null },
                    { 14, "BIS-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote crocante B", 13, 40, null, null },
                    { 15, "CAF-2025A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote café A", 14, 25, null, null },
                    { 16, "CAF-2025B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Lote café B", 14, 30, null, null }
                });

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "InvoiceId", "CreatedAt", "CreatedByUserId", "Date", "DeletedAt", "DiscountAmount", "DueDate", "InternalNotes", "InvoiceNumber", "IsDeleted", "Notes", "PaidAmount", "PersonId", "ShippingCost", "Status", "SubTotal", "TaxAmount", "Total", "UpdatedAt", "UpdatedByUserId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, 0m, new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), "Seed invoice", "INV-2025-0001", false, "Venda balcão", 13908m, 101, 0m, 3, 12200m, 1708m, 13908m, null, null, 4 },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), null, 400m, new DateTime(2025, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Seed invoice", "INV-2025-0002", false, "Venda com desconto", 10000m, 102, 0m, 2, 20000m, 2744m, 22344m, null, null, 4 },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), null, 0m, new DateTime(2025, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Seed invoice", "INV-2025-0003", false, "Pedido corporativo", 0m, 103, 1000m, 2, 23800m, 3332m, 28132m, null, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "PriceHistories",
                columns: new[] { "PriceHistoryId", "ChangeDate", "CreatedAt", "CreatedByUserId", "DeletedAt", "IsDeleted", "NewCostPrice", "NewSalePrice", "OldCostPrice", "OldSalePrice", "ProductId", "Reason", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 3500m, 4500m, 3300m, 4200m, 1, "Ajuste inicial de preços", null, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 800m, 1200m, 700m, 1100m, 2, "Correção de margem", null, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 1000m, 1500m, 900m, 1400m, 3, "Atualização de fornecedor", null, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 700m, 900m, 650m, 850m, 4, "Ajuste sazonal", null, null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 500m, 800m, 450m, 750m, 5, "Tabela atualizada", null, null },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 150m, 300m, 140m, 280m, 6, "Ajuste de custo", null, null },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 120m, 250m, 100m, 230m, 7, "Custo matéria-prima", null, null },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 1200m, 1800m, 1100m, 1700m, 8, "Reposicionamento", null, null },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 400m, 700m, 350m, 650m, 9, "Ajuste geral", null, null },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 2800m, 3500m, 2600m, 3300m, 10, "Câmbio", null, null },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 50m, 100m, 45m, 90m, 11, "Margem mínima", null, null },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 250m, 400m, 230m, 350m, 12, "Ajuste sazonal", null, null },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 200m, 350m, 180m, 320m, 13, "Demanda", null, null },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 1500m, 2200m, 1400m, 2100m, 14, "Custo de importação", null, null },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, false, 10000m, 15000m, 9500m, 14000m, 15, "Nova coleção", null, null }
                });

            migrationBuilder.InsertData(
                table: "StockMovements",
                columns: new[] { "StockMovementId", "CreatedAt", "CreatedByUserId", "Date", "DeletedAt", "IsDeleted", "NewQuantity", "Notes", "PreviousQuantity", "ProductId", "Quantity", "Reference", "TotalCost", "Type", "UnitCost", "UpdatedAt", "UpdatedByUserId", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 100, "Entrada inicial de estoque", 0, 1, 100, "SEED", 350000m, 1, 3500m, null, null, 4 },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 80, "Entrada inicial de estoque", 0, 2, 80, "SEED", 64000m, 1, 800m, null, null, 4 },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 60, "Entrada inicial de estoque", 0, 3, 60, "SEED", 60000m, 1, 1000m, null, null, 4 },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 120, "Entrada inicial de estoque", 0, 4, 120, "SEED", 84000m, 1, 700m, null, null, 4 },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 90, "Entrada inicial de estoque", 0, 5, 90, "SEED", 45000m, 1, 500m, null, null, 4 },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 200, "Entrada inicial de estoque", 0, 6, 200, "SEED", 30000m, 1, 150m, null, null, 4 },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 150, "Entrada inicial de estoque", 0, 7, 150, "SEED", 18000m, 1, 120m, null, null, 4 },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 70, "Entrada inicial de estoque", 0, 8, 70, "SEED", 84000m, 1, 1200m, null, null, 4 },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 110, "Entrada inicial de estoque", 0, 9, 110, "SEED", 44000m, 1, 400m, null, null, 4 },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 40, "Entrada inicial de estoque", 0, 10, 40, "SEED", 112000m, 1, 2800m, null, null, 4 },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 300, "Entrada inicial de estoque", 0, 11, 300, "SEED", 15000m, 1, 50m, null, null, 4 },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 50, "Entrada inicial de estoque", 0, 12, 50, "SEED", 12500m, 1, 250m, null, null, 4 },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 75, "Entrada inicial de estoque", 0, 13, 75, "SEED", 15000m, 1, 200m, null, null, 4 },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 55, "Entrada inicial de estoque", 0, 14, 55, "SEED", 82500m, 1, 1500m, null, null, 4 },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, 30, "Entrada inicial de estoque", 0, 15, 30, "SEED", 300000m, 1, 10000m, null, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "Stocks",
                columns: new[] { "ProductId", "LastStockUpdate", "LastStockUpdateByUserId", "Quantity", "ReservedQuantity" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 100, 5 },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 80, 0 },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 60, 2 },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 120, 10 },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 90, 0 },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 200, 0 },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 150, 0 },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 70, 3 },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 110, 4 },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 40, 0 },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 300, 10 },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 50, 0 },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 75, 0 },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 55, 0 },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 30, 2 }
                });

            migrationBuilder.InsertData(
                table: "InvoiceProducts",
                columns: new[] { "InvoiceProductId", "DiscountPercentage", "InvoiceId", "ProductId", "Quantity", "TaxRate", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 0m, 1, 1, 2, 14m, 4500m },
                    { 2, 0m, 1, 4, 3, 14m, 900m },
                    { 3, 0m, 1, 11, 5, 14m, 100m },
                    { 4, 5m, 2, 5, 10, 14m, 800m },
                    { 5, 0m, 2, 6, 20, 14m, 300m },
                    { 6, 0m, 2, 12, 15, 14m, 400m },
                    { 7, 0m, 3, 15, 1, 14m, 15000m },
                    { 8, 0m, 3, 10, 2, 14m, 3500m },
                    { 9, 0m, 3, 8, 1, 14m, 1800m }
                });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "PaymentId", "Amount", "CreatedAt", "CreatedByUserId", "DeletedAt", "InvoiceId", "IsConfirmed", "IsDeleted", "Notes", "PaymentDate", "PaymentTypeId", "Reference", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1, 13908m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, 1, true, false, "Pagamento integral", new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 1, "CASH-INV-2025-0001", null, null },
                    { 2, 5000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, 2, true, false, "Parcial 1", new DateTime(2025, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), 1, "CASH-INV-2025-0002-1", null, null },
                    { 3, 5000m, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, 2, true, false, "Parcial 2", new DateTime(2025, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), 3, "CARD-INV-2025-0002-2", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseTypeId",
                table: "Expenses",
                column: "ExpenseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expirations_ProductId",
                table: "Expirations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProducts_InvoiceId",
                table: "InvoiceProducts",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProducts_ProductId",
                table: "InvoiceProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Date",
                table: "Invoices",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PersonId",
                table: "Invoices",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId",
                table: "Invoices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Email",
                table: "Persons",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_TaxId",
                table: "Persons",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Type",
                table: "Persons",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_ProductId",
                table: "PriceHistories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_InternalCode",
                table: "Products",
                column: "InternalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_Date",
                table: "StockMovements",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_UserId",
                table: "StockMovements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivileges_PrivilegeId",
                table: "UserPrivileges",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrivileges_UserId",
                table: "UserPrivileges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PersonId",
                table: "Users",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CompanyConfigs");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Expirations");

            migrationBuilder.DropTable(
                name: "InvoiceProducts");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "UserPrivileges");

            migrationBuilder.DropTable(
                name: "ExpenseTypes");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Privileges");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
