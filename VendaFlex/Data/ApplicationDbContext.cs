using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace VendaFlex.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly int? _currentUserId;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, int? currentUserId = null)
            : base(options)
        {
            _currentUserId = currentUserId;
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<UserPrivilege> UserPrivileges { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Expiration> Expirations { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceProduct> InvoiceProducts { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CompanyConfig> CompanyConfigs { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar precisão decimal para todas as propriedades decimal
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // Aplicar todas as configurações de entidades
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Usar valores determinísticos para evitar PendingModelChangesWarning
            var seedTimestamp = new DateTime(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

            // Seed Privileges
            modelBuilder.Entity<Privilege>().HasData(
                new Privilege { PrivilegeId = 1, Name = "Administrador", Code = "ADMIN", Description = "Acesso total ao sistema" },
                new Privilege { PrivilegeId = 2, Name = "Gerente", Code = "MANAGER", Description = "Gestão de vendas e relatórios" },
                new Privilege { PrivilegeId = 3, Name = "Vendedor", Code = "SELLER", Description = "Realizar vendas" },
                new Privilege { PrivilegeId = 4, Name = "Visualizador", Code = "VIEWER", Description = "Apenas visualização" },
                new Privilege { PrivilegeId = 5, Name = "Auditor", Code = "AUDITOR", Description = "Auditoria e logs" }
            );

            // Seed Payment Types
            modelBuilder.Entity<PaymentType>().HasData(
                new PaymentType { PaymentTypeId = 1, Name = "Dinheiro", Description = "Pagamento em dinheiro" },
                new PaymentType { PaymentTypeId = 2, Name = "Cartão de Débito", Description = "Pagamento com cartão de débito", RequiresReference = true },
                new PaymentType { PaymentTypeId = 3, Name = "Cartão de Crédito", Description = "Pagamento com cartão de crédito", RequiresReference = true },
                new PaymentType { PaymentTypeId = 4, Name = "Transferência Bancária", Description = "Transferência bancária", RequiresReference = true },
                new PaymentType { PaymentTypeId = 5, Name = "PIX", Description = "Pagamento via PIX", RequiresReference = true },
                new PaymentType { PaymentTypeId = 6, Name = "Múltiplas Formas", Description = "Pagamento com múltiplas formas" }
            );

            // Seed Expense Types
            modelBuilder.Entity<ExpenseType>().HasData(
                new ExpenseType { ExpenseTypeId = 1, Name = "Aluguel", Description = "Despesas com aluguel" },
                new ExpenseType { ExpenseTypeId = 2, Name = "Salários", Description = "Pagamento de salários" },
                new ExpenseType { ExpenseTypeId = 3, Name = "Utilities", Description = "Água, luz, internet" },
                new ExpenseType { ExpenseTypeId = 4, Name = "Marketing", Description = "Despesas com marketing" },
                new ExpenseType { ExpenseTypeId = 5, Name = "Manutenção", Description = "Manutenção e reparos" },
                new ExpenseType { ExpenseTypeId = 6, Name = "Outros", Description = "Outras despesas" }
            );

            // Seed Company Config (Padrão)
            /* modelBuilder.Entity<CompanyConfig>().HasData(
                new CompanyConfig
                {
                    CompanyConfigId = 1,
                    CompanyName = "Minha Empresa",
                    TaxId = "000000000",
                    Address = "Rua Principal, 123",
                    City = "Luanda",
                    Country = "Angola",
                    PhoneNumber = "+244 900 000 000",
                    Email = "contato@minhaempresa.ao",
                    Currency = "AOA",
                    CurrencySymbol = "Kz",
                    DefaultTaxRate = 14,
                    InvoicePrefix = "INV",
                    NextInvoiceNumber = 1,
                    CreatedAt = seedTimestamp
                }
            ); */

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    CategoryId = 1,
                    Name = "Geral",
                    Code = "GEN",
                    Description = "Categoria padrão",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4,
                    IsActive = true,
                    DisplayOrder = 0
                },
                new Category
                {
                    CategoryId = 2,
                    Name = "Bebidas",
                    Code = "BEV",
                    Description = "Bebidas e refrigerantes",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4,
                    IsActive = true,
                    DisplayOrder = 1
                },
                new Category
                {
                    CategoryId = 3,
                    Name = "Alimentos",
                    Code = "FOOD",
                    Description = "Alimentos e perecíveis",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4,
                    IsActive = true,
                    DisplayOrder = 2
                },
                new Category
                {
                    CategoryId = 4,
                    Name = "Higiene",
                    Code = "HYG",
                    Description = "Higiene pessoal",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4,
                    IsActive = true,
                    DisplayOrder = 3
                },
                new Category
                {
                    CategoryId = 5,
                    Name = "Limpeza",
                    Code = "CLEAN",
                    Description = "Produtos de limpeza",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4,
                    IsActive = true,
                    DisplayOrder = 4
                },
                new Category
                {
                    CategoryId = 6,
                    Name = "Eletrônicos",
                    Code = "ELEC",
                    Description = "Produtos eletrônicos",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4,
                    IsActive = true,
                    DisplayOrder = 5
                },
                new Category
                {
                    CategoryId = 7,
                    Name = "Papelaria",
                    Code = "STAT",
                    Description = "Papelaria e escritório",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4,
                    IsActive = true,
                    DisplayOrder = 6
                }
            );

            // Seed Persons (Clientes e Fornecedores)
            modelBuilder.Entity<Person>().HasData(

                 new Person
                 {
                     PersonId = 1,
                     Name = "Duarte Gauss",
                     Type = PersonType.Supplier,
                     Email = "carlos.silva@example.com",
                     PhoneNumber = "+244900111111",
                     Address = "Rua A, 100",
                     City = "Luanda",
                     Country = "Angola",
                     CreditLimit = 100000m,
                     CurrentBalance = 0m,
                     IsActive = true,
                     CreatedAt = seedTimestamp,
                     CreatedByUserId = 4
                 },

                // Clientes
                new Person
                {
                    PersonId = 101,
                    Name = "Carlos Silva",
                    Type = PersonType.Customer,
                    Email = "carlos.silva@example.com",
                    PhoneNumber = "+244900111111",
                    Address = "Rua A, 100",
                    City = "Luanda",
                    Country = "Angola",
                    CreditLimit = 100000m,
                    CurrentBalance = 0m,
                    IsActive = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                new Person
                {
                    PersonId = 102,
                    Name = "Ana Pereira",
                    Type = PersonType.Customer,
                    Email = "ana.pereira@example.com",
                    PhoneNumber = "+244900222222",
                    Address = "Av. Central, 200",
                    City = "Luanda",
                    Country = "Angola",
                    CreditLimit = 150000m,
                    CurrentBalance = 0m,
                    IsActive = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                new Person
                {
                    PersonId = 103,
                    Name = "Empresa ABC Lda",
                    Type = PersonType.Customer,
                    Email = "compras@empresaabc.ao",
                    PhoneNumber = "+244900333333",
                    Address = "Parque Industrial, Armazém 3",
                    City = "Luanda",
                    Country = "Angola",
                    CreditLimit = 500000m,
                    CurrentBalance = 0m,
                    IsActive = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },



                // Fornecedores
                new Person
                {
                    PersonId = 201,
                    Name = "Alimentos & Cia",
                    Type = PersonType.Supplier,
                    Email = "contato@alimentoscia.ao",
                    PhoneNumber = "+244910000001",
                    Address = "Zona Comercial, Loja 10",
                    City = "Luanda",
                    Country = "Angola",
                    Rating = 5,
                    IsActive = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                new Person
                {
                    PersonId = 202,
                    Name = "Bebidas SA",
                    Type = PersonType.Supplier,
                    Email = "vendas@bebidassa.ao",
                    PhoneNumber = "+244910000002",
                    Address = "Parque das Indústrias, Lote 4",
                    City = "Luanda",
                    Country = "Angola",
                    Rating = 4,
                    IsActive = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                new Person
                {
                    PersonId = 203,
                    Name = "Tech Import",
                    Type = PersonType.Supplier,
                    Email = "suporte@techimport.ao",
                    PhoneNumber = "+244910000003",
                    Address = "Rua da Tecnologia, 55",
                    City = "Luanda",
                    Country = "Angola",
                    Rating = 5,
                    IsActive = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                }
            );

            //User 
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 4,
                    PersonId = 1,
                    Username = "admin",
                    PasswordHash = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918", // Admin@123 em SHA256
                    Status = VendaFlex.Data.Entities.LoginStatus.Active,
                    LastLoginIp = string.Empty,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Products (15)
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, InternalCode = "P0001", ExternalCode = "EXT-0001", Barcode = "5600000000011", Name = "Arroz 5kg", ShortDescription = "Arroz tipo 1 - 5kg", Description = "Saco de arroz branco tipo 1 com 5kg", SKU = "ARZ-5KG", Weight = "5kg", Dimensions = "35x25x8cm", CategoryId = 3, SupplierId = 201, SalePrice = 4500m, CostPrice = 3500m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = true, AllowBackorder = false, DisplayOrder = 1, ControlsStock = true, MinimumStock = 20, MaximumStock = 300, ReorderPoint = 50, HasExpirationDate = false, ExpirationDays = null, ExpirationWarningDays = null, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 2, InternalCode = "P0002", ExternalCode = "EXT-0002", Barcode = "5600000000028", Name = "Feijão 1kg", ShortDescription = "Feijão carioca 1kg", Description = "Pacote de feijão carioca de 1kg", SKU = "FEJ-1KG", Weight = "1kg", Dimensions = "20x12x5cm", CategoryId = 3, SupplierId = 201, SalePrice = 1200m, CostPrice = 800m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = false, DisplayOrder = 2, ControlsStock = true, MinimumStock = 30, MaximumStock = 400, ReorderPoint = 60, HasExpirationDate = false, ExpirationDays = null, ExpirationWarningDays = null, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 3, InternalCode = "P0003", ExternalCode = "EXT-0003", Barcode = "5600000000035", Name = "Óleo 1L", ShortDescription = "Óleo vegetal 1L", Description = "Garrafa de óleo vegetal com 1 litro", SKU = "OLEO-1L", Weight = "1L", Dimensions = "10x10x25cm", CategoryId = 3, SupplierId = 201, SalePrice = 1500m, CostPrice = 1000m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = false, DisplayOrder = 3, ControlsStock = true, MinimumStock = 20, MaximumStock = 200, ReorderPoint = 40, HasExpirationDate = true, ExpirationDays = 365, ExpirationWarningDays = 30, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 4, InternalCode = "P0004", ExternalCode = "EXT-0004", Barcode = "5600000000042", Name = "Leite UHT 1L", ShortDescription = "Leite UHT integral", Description = "Leite UHT integral 1L", SKU = "LEI-1L", Weight = "1L", Dimensions = "7x7x20cm", CategoryId = 2, SupplierId = 202, SalePrice = 900m, CostPrice = 700m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = true, AllowBackorder = false, DisplayOrder = 4, ControlsStock = true, MinimumStock = 40, MaximumStock = 500, ReorderPoint = 80, HasExpirationDate = true, ExpirationDays = 180, ExpirationWarningDays = 20, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 5, InternalCode = "P0005", ExternalCode = "EXT-0005", Barcode = "5600000000059", Name = "Refrigerante Cola 2L", ShortDescription = "Refrigerante sabor cola 2L", Description = "Garrafa de refrigerante cola 2 litros", SKU = "COLA-2L", Weight = "2L", Dimensions = "12x12x32cm", CategoryId = 2, SupplierId = 202, SalePrice = 800m, CostPrice = 500m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = false, DisplayOrder = 5, ControlsStock = true, MinimumStock = 50, MaximumStock = 600, ReorderPoint = 100, HasExpirationDate = true, ExpirationDays = 365, ExpirationWarningDays = 30, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 6, InternalCode = "P0006", ExternalCode = "EXT-0006", Barcode = "5600000000066", Name = "Água Mineral 1.5L", ShortDescription = "Água mineral sem gás 1.5L", Description = "Garrafa de água mineral 1.5L", SKU = "AGUA-1_5L", Weight = "1.5L", Dimensions = "10x10x30cm", CategoryId = 2, SupplierId = 202, SalePrice = 300m, CostPrice = 150m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = true, DisplayOrder = 6, ControlsStock = true, MinimumStock = 80, MaximumStock = 1000, ReorderPoint = 200, HasExpirationDate = true, ExpirationDays = 365, ExpirationWarningDays = 60, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 7, InternalCode = "P0007", ExternalCode = "EXT-0007", Barcode = "5600000000073", Name = "Sabonete 90g", ShortDescription = "Sabonete em barra 90g", Description = "Sabonete perfumado em barra 90g", SKU = "SAB-90G", Weight = "90g", Dimensions = "8x5x3cm", CategoryId = 4, SupplierId = 203, SalePrice = 250m, CostPrice = 120m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = false, DisplayOrder = 7, ControlsStock = true, MinimumStock = 50, MaximumStock = 800, ReorderPoint = 150, HasExpirationDate = false, ExpirationDays = null, ExpirationWarningDays = null, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 8, InternalCode = "P0008", ExternalCode = "EXT-0008", Barcode = "5600000000080", Name = "Shampoo 400ml", ShortDescription = "Shampoo hidratante 400ml", Description = "Frasco de shampoo 400ml", SKU = "SHA-400", Weight = "400ml", Dimensions = "8x5x18cm", CategoryId = 4, SupplierId = 203, SalePrice = 1800m, CostPrice = 1200m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = true, AllowBackorder = false, DisplayOrder = 8, ControlsStock = true, MinimumStock = 20, MaximumStock = 200, ReorderPoint = 40, HasExpirationDate = true, ExpirationDays = 730, ExpirationWarningDays = 60, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 9, InternalCode = "P0009", ExternalCode = "EXT-0009", Barcode = "5600000000097", Name = "Detergente 1L", ShortDescription = "Detergente multiuso 1L", Description = "Garrafa de detergente 1L", SKU = "DET-1L", Weight = "1L", Dimensions = "10x10x25cm", CategoryId = 5, SupplierId = 203, SalePrice = 700m, CostPrice = 400m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = false, DisplayOrder = 9, ControlsStock = true, MinimumStock = 30, MaximumStock = 300, ReorderPoint = 60, HasExpirationDate = false, ExpirationDays = null, ExpirationWarningDays = null, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 10, InternalCode = "P0010", ExternalCode = "EXT-0010", Barcode = "5600000000103", Name = "Papel A4 500 folhas", ShortDescription = "Resma Papel A4 500fls", Description = "Resma de papel A4 com 500 folhas", SKU = "PAP-A4-500", Weight = "2.5kg", Dimensions = "30x21x5cm", CategoryId = 7, SupplierId = 203, SalePrice = 3500m, CostPrice = 2800m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = true, AllowBackorder = false, DisplayOrder = 10, ControlsStock = true, MinimumStock = 10, MaximumStock = 100, ReorderPoint = 20, HasExpirationDate = false, ExpirationDays = null, ExpirationWarningDays = null, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 11, InternalCode = "P0011", ExternalCode = "EXT-0011", Barcode = "5600000000110", Name = "Caneta Azul", ShortDescription = "Caneta esferográfica azul", Description = "Caneta esferográfica tinta azul", SKU = "CAN-AZUL", Weight = "20g", Dimensions = "14x1x1cm", CategoryId = 7, SupplierId = 203, SalePrice = 100m, CostPrice = 50m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = true, DisplayOrder = 11, ControlsStock = true, MinimumStock = 100, MaximumStock = 2000, ReorderPoint = 300, HasExpirationDate = false, ExpirationDays = null, ExpirationWarningDays = null, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 12, InternalCode = "P0012", ExternalCode = "EXT-0012", Barcode = "5600000000127", Name = "Chocolate 100g", ShortDescription = "Tablete de chocolate 100g", Description = "Tablete de chocolate ao leite 100g", SKU = "CHOC-100", Weight = "100g", Dimensions = "10x5x1cm", CategoryId = 3, SupplierId = 201, SalePrice = 400m, CostPrice = 250m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = false, DisplayOrder = 12, ControlsStock = true, MinimumStock = 30, MaximumStock = 400, ReorderPoint = 60, HasExpirationDate = true, ExpirationDays = 365, ExpirationWarningDays = 30, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 13, InternalCode = "P0013", ExternalCode = "EXT-0013", Barcode = "5600000000134", Name = "Biscoito 200g", ShortDescription = "Biscoito recheado 200g", Description = "Pacote de biscoito recheado 200g", SKU = "BIS-200", Weight = "200g", Dimensions = "15x10x3cm", CategoryId = 3, SupplierId = 201, SalePrice = 350m, CostPrice = 200m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = false, AllowBackorder = false, DisplayOrder = 13, ControlsStock = true, MinimumStock = 40, MaximumStock = 500, ReorderPoint = 80, HasExpirationDate = true, ExpirationDays = 270, ExpirationWarningDays = 30, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 14, InternalCode = "P0014", ExternalCode = "EXT-0014", Barcode = "5600000000141", Name = "Café 500g", ShortDescription = "Café torrado e moído 500g", Description = "Pacote de café 500g", SKU = "CAF-500", Weight = "500g", Dimensions = "20x10x6cm", CategoryId = 3, SupplierId = 201, SalePrice = 2200m, CostPrice = 1500m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = true, AllowBackorder = false, DisplayOrder = 14, ControlsStock = true, MinimumStock = 15, MaximumStock = 200, ReorderPoint = 30, HasExpirationDate = true, ExpirationDays = 540, ExpirationWarningDays = 45, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Product { ProductId = 15, InternalCode = "P0015", ExternalCode = "EXT-0015", Barcode = "5600000000158", Name = "Fones de Ouvido", ShortDescription = "Fones intra-auriculares", Description = "Fones de ouvido com microfone", SKU = "FONES-BT", Weight = "150g", Dimensions = "10x8x5cm", CategoryId = 6, SupplierId = 203, SalePrice = 15000m, CostPrice = 10000m, DiscountPercentage = 0m, TaxRate = 14m, PhotoUrl = "", Status = ProductStatus.Active, IsFeatured = true, AllowBackorder = true, DisplayOrder = 15, ControlsStock = true, MinimumStock = 5, MaximumStock = 100, ReorderPoint = 10, HasExpirationDate = false, ExpirationDays = null, ExpirationWarningDays = null, CreatedAt = seedTimestamp, CreatedByUserId = 4 }
            );

            // Seed Stock (um registro por produto)
            modelBuilder.Entity<Stock>().HasData(
                new Stock { ProductId = 1, Quantity = 100, ReservedQuantity = 5, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 2, Quantity = 80, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 3, Quantity = 60, ReservedQuantity = 2, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 4, Quantity = 120, ReservedQuantity = 10, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 5, Quantity = 90, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 6, Quantity = 200, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 7, Quantity = 150, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 8, Quantity = 70, ReservedQuantity = 3, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 9, Quantity = 110, ReservedQuantity = 4, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 10, Quantity = 40, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 11, Quantity = 300, ReservedQuantity = 10, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 12, Quantity = 50, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 13, Quantity = 75, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 14, Quantity = 55, ReservedQuantity = 0, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 },
                new Stock { ProductId = 15, Quantity = 30, ReservedQuantity = 2, LastStockUpdate = seedTimestamp, LastStockUpdateByUserId = 4 }
            );

            // Seed StockMovements (entrada inicial por produto)
            modelBuilder.Entity<StockMovement>().HasData(
                new StockMovement { StockMovementId = 1, ProductId = 1, UserId = 4, Quantity = 100, PreviousQuantity = 0, NewQuantity = 100, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 3500m, TotalCost = 350000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 2, ProductId = 2, UserId = 4, Quantity = 80, PreviousQuantity = 0, NewQuantity = 80, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 800m, TotalCost = 64000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 3, ProductId = 3, UserId = 4, Quantity = 60, PreviousQuantity = 0, NewQuantity = 60, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 1000m, TotalCost = 60000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 4, ProductId = 4, UserId = 4, Quantity = 120, PreviousQuantity = 0, NewQuantity = 120, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 700m, TotalCost = 84000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 5, ProductId = 5, UserId = 4, Quantity = 90, PreviousQuantity = 0, NewQuantity = 90, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 500m, TotalCost = 45000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 6, ProductId = 6, UserId = 4, Quantity = 200, PreviousQuantity = 0, NewQuantity = 200, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 150m, TotalCost = 30000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 7, ProductId = 7, UserId = 4, Quantity = 150, PreviousQuantity = 0, NewQuantity = 150, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 120m, TotalCost = 18000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 8, ProductId = 8, UserId = 4, Quantity = 70, PreviousQuantity = 0, NewQuantity = 70, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 1200m, TotalCost = 84000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 9, ProductId = 9, UserId = 4, Quantity = 110, PreviousQuantity = 0, NewQuantity = 110, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 400m, TotalCost = 44000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 10, ProductId = 10, UserId = 4, Quantity = 40, PreviousQuantity = 0, NewQuantity = 40, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 2800m, TotalCost = 112000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 11, ProductId = 11, UserId = 4, Quantity = 300, PreviousQuantity = 0, NewQuantity = 300, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 50m, TotalCost = 15000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 12, ProductId = 12, UserId = 4, Quantity = 50, PreviousQuantity = 0, NewQuantity = 50, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 250m, TotalCost = 12500m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 13, ProductId = 13, UserId = 4, Quantity = 75, PreviousQuantity = 0, NewQuantity = 75, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 200m, TotalCost = 15000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 14, ProductId = 14, UserId = 4, Quantity = 55, PreviousQuantity = 0, NewQuantity = 55, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 1500m, TotalCost = 82500m, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new StockMovement { StockMovementId = 15, ProductId = 15, UserId = 4, Quantity = 30, PreviousQuantity = 0, NewQuantity = 30, Date = seedTimestamp, Type = StockMovementType.Entry, Notes = "Entrada inicial de estoque", Reference = "SEED", UnitCost = 10000m, TotalCost = 300000m, CreatedAt = seedTimestamp, CreatedByUserId = 4 }
            );

            // Seed Expirations (para produtos com validade)
            modelBuilder.Entity<Expiration>().HasData(
                // Produto 3 - Óleo 1L
                new Expiration { ExpirationId = 1, ProductId = 3, ExpirationDate = seedTimestamp.AddMonths(6), Quantity = 30, BatchNumber = "OLEO-2025A", Notes = "Lote inicial", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 2, ProductId = 3, ExpirationDate = seedTimestamp.AddMonths(12), Quantity = 30, BatchNumber = "OLEO-2025B", Notes = "Lote reserva", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                // Produto 4 - Leite UHT 1L
                new Expiration { ExpirationId = 3, ProductId = 4, ExpirationDate = seedTimestamp.AddMonths(3), Quantity = 60, BatchNumber = "LEI-2025A", Notes = "Lote A", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 4, ProductId = 4, ExpirationDate = seedTimestamp.AddMonths(5), Quantity = 60, BatchNumber = "LEI-2025B", Notes = "Lote B", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                // Produto 5 - Refrigerante Cola 2L
                new Expiration { ExpirationId = 5, ProductId = 5, ExpirationDate = seedTimestamp.AddMonths(10), Quantity = 45, BatchNumber = "COLA-2025A", Notes = "Produção 1", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 6, ProductId = 5, ExpirationDate = seedTimestamp.AddMonths(14), Quantity = 45, BatchNumber = "COLA-2025B", Notes = "Produção 2", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                // Produto 6 - Água Mineral 1.5L
                new Expiration { ExpirationId = 7, ProductId = 6, ExpirationDate = seedTimestamp.AddMonths(12), Quantity = 100, BatchNumber = "AGUA-2025A", Notes = "Lote principal", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 8, ProductId = 6, ExpirationDate = seedTimestamp.AddMonths(18), Quantity = 100, BatchNumber = "AGUA-2025B", Notes = "Lote adicional", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                // Produto 8 - Shampoo 400ml
                new Expiration { ExpirationId = 9, ProductId = 8, ExpirationDate = seedTimestamp.AddMonths(18), Quantity = 35, BatchNumber = "SHA-2025A", Notes = "Lote A", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 10, ProductId = 8, ExpirationDate = seedTimestamp.AddMonths(24), Quantity = 35, BatchNumber = "SHA-2025B", Notes = "Lote B", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                // Produto 12 - Chocolate 100g
                new Expiration { ExpirationId = 11, ProductId = 12, ExpirationDate = seedTimestamp.AddMonths(8), Quantity = 25, BatchNumber = "CHOC-2025A", Notes = "Lote doce A", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 12, ProductId = 12, ExpirationDate = seedTimestamp.AddMonths(12), Quantity = 25, BatchNumber = "CHOC-2025B", Notes = "Lote doce B", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                // Produto 13 - Biscoito 200g
                new Expiration { ExpirationId = 13, ProductId = 13, ExpirationDate = seedTimestamp.AddMonths(6), Quantity = 35, BatchNumber = "BIS-2025A", Notes = "Lote crocante A", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 14, ProductId = 13, ExpirationDate = seedTimestamp.AddMonths(9), Quantity = 40, BatchNumber = "BIS-2025B", Notes = "Lote crocante B", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                // Produto 14 - Café 500g
                new Expiration { ExpirationId = 15, ProductId = 14, ExpirationDate = seedTimestamp.AddMonths(12), Quantity = 25, BatchNumber = "CAF-2025A", Notes = "Lote café A", CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new Expiration { ExpirationId = 16, ProductId = 14, ExpirationDate = seedTimestamp.AddMonths(18), Quantity = 30, BatchNumber = "CAF-2025B", Notes = "Lote café B", CreatedAt = seedTimestamp, CreatedByUserId = 4 }
            );

            // Seed PriceHistory (um registro por produto)
            modelBuilder.Entity<PriceHistory>().HasData(
                new PriceHistory { PriceHistoryId = 1, ProductId = 1, OldSalePrice = 4200m, NewSalePrice = 4500m, OldCostPrice = 3300m, NewCostPrice = 3500m, Reason = "Ajuste inicial de preços", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 2, ProductId = 2, OldSalePrice = 1100m, NewSalePrice = 1200m, OldCostPrice = 700m, NewCostPrice = 800m, Reason = "Correção de margem", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 3, ProductId = 3, OldSalePrice = 1400m, NewSalePrice = 1500m, OldCostPrice = 900m, NewCostPrice = 1000m, Reason = "Atualização de fornecedor", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 4, ProductId = 4, OldSalePrice = 850m, NewSalePrice = 900m, OldCostPrice = 650m, NewCostPrice = 700m, Reason = "Ajuste sazonal", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 5, ProductId = 5, OldSalePrice = 750m, NewSalePrice = 800m, OldCostPrice = 450m, NewCostPrice = 500m, Reason = "Tabela atualizada", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 6, ProductId = 6, OldSalePrice = 280m, NewSalePrice = 300m, OldCostPrice = 140m, NewCostPrice = 150m, Reason = "Ajuste de custo", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 7, ProductId = 7, OldSalePrice = 230m, NewSalePrice = 250m, OldCostPrice = 100m, NewCostPrice = 120m, Reason = "Custo matéria-prima", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 8, ProductId = 8, OldSalePrice = 1700m, NewSalePrice = 1800m, OldCostPrice = 1100m, NewCostPrice = 1200m, Reason = "Reposicionamento", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 9, ProductId = 9, OldSalePrice = 650m, NewSalePrice = 700m, OldCostPrice = 350m, NewCostPrice = 400m, Reason = "Ajuste geral", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 10, ProductId = 10, OldSalePrice = 3300m, NewSalePrice = 3500m, OldCostPrice = 2600m, NewCostPrice = 2800m, Reason = "Câmbio", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 11, ProductId = 11, OldSalePrice = 90m, NewSalePrice = 100m, OldCostPrice = 45m, NewCostPrice = 50m, Reason = "Margem mínima", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 12, ProductId = 12, OldSalePrice = 350m, NewSalePrice = 400m, OldCostPrice = 230m, NewCostPrice = 250m, Reason = "Ajuste sazonal", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 13, ProductId = 13, OldSalePrice = 320m, NewSalePrice = 350m, OldCostPrice = 180m, NewCostPrice = 200m, Reason = "Demanda", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 14, ProductId = 14, OldSalePrice = 2100m, NewSalePrice = 2200m, OldCostPrice = 1400m, NewCostPrice = 1500m, Reason = "Custo de importação", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 },
                new PriceHistory { PriceHistoryId = 15, ProductId = 15, OldSalePrice = 14000m, NewSalePrice = 15000m, OldCostPrice = 9500m, NewCostPrice = 10000m, Reason = "Nova coleção", ChangeDate = seedTimestamp, CreatedAt = seedTimestamp, CreatedByUserId = 4 }
            );

            // Seed Invoices
            modelBuilder.Entity<Invoice>().HasData(
                new Invoice
                {
                    InvoiceId = 1,
                    InvoiceNumber = "INV-2025-0001",
                    Date = seedTimestamp.AddDays(2),
                    DueDate = seedTimestamp.AddDays(15),
                    PersonId = 101, // Carlos Silva
                    UserId = 4,
                    Status = InvoiceStatus.Paid,
                    SubTotal = 12200m,
                    TaxAmount = 1708m,
                    DiscountAmount = 0m,
                    ShippingCost = 0m,
                    Total = 13908m,
                    PaidAmount = 13908m,
                    Notes = "Venda balcão",
                    InternalNotes = "Seed invoice",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                new Invoice
                {
                    InvoiceId = 2,
                    InvoiceNumber = "INV-2025-0002",
                    Date = seedTimestamp.AddDays(3),
                    DueDate = seedTimestamp.AddDays(20),
                    PersonId = 102, // Ana Pereira
                    UserId = 4,
                    Status = InvoiceStatus.Confirmed,
                    SubTotal = 20000m,
                    TaxAmount = 2744m,
                    DiscountAmount = 400m,
                    ShippingCost = 0m,
                    Total = 22344m,
                    PaidAmount = 10000m,
                    Notes = "Venda com desconto",
                    InternalNotes = "Seed invoice",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                new Invoice
                {
                    InvoiceId = 3,
                    InvoiceNumber = "INV-2025-0003",
                    Date = seedTimestamp.AddDays(4),
                    DueDate = seedTimestamp.AddDays(30),
                    PersonId = 103, // Empresa ABC Lda
                    UserId = 4,
                    Status = InvoiceStatus.Confirmed,
                    SubTotal = 23800m,
                    TaxAmount = 3332m,
                    DiscountAmount = 0m,
                    ShippingCost = 1000m,
                    Total = 28132m,
                    PaidAmount = 0m,
                    Notes = "Pedido corporativo",
                    InternalNotes = "Seed invoice",
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                }
            );

            // Seed InvoiceProducts
            modelBuilder.Entity<InvoiceProduct>().HasData(
                // Invoice 1
                new InvoiceProduct { InvoiceProductId = 1, InvoiceId = 1, ProductId = 1, Quantity = 2, UnitPrice = 4500m, DiscountPercentage = 0m, TaxRate = 14m },
                new InvoiceProduct { InvoiceProductId = 2, InvoiceId = 1, ProductId = 4, Quantity = 3, UnitPrice = 900m, DiscountPercentage = 0m, TaxRate = 14m },
                new InvoiceProduct { InvoiceProductId = 3, InvoiceId = 1, ProductId = 11, Quantity = 5, UnitPrice = 100m, DiscountPercentage = 0m, TaxRate = 14m },
                // Invoice 2
                new InvoiceProduct { InvoiceProductId = 4, InvoiceId = 2, ProductId = 5, Quantity = 10, UnitPrice = 800m, DiscountPercentage = 5m, TaxRate = 14m },
                new InvoiceProduct { InvoiceProductId = 5, InvoiceId = 2, ProductId = 6, Quantity = 20, UnitPrice = 300m, DiscountPercentage = 0m, TaxRate = 14m },
                new InvoiceProduct { InvoiceProductId = 6, InvoiceId = 2, ProductId = 12, Quantity = 15, UnitPrice = 400m, DiscountPercentage = 0m, TaxRate = 14m },
                // Invoice 3
                new InvoiceProduct { InvoiceProductId = 7, InvoiceId = 3, ProductId = 15, Quantity = 1, UnitPrice = 15000m, DiscountPercentage = 0m, TaxRate = 14m },
                new InvoiceProduct { InvoiceProductId = 8, InvoiceId = 3, ProductId = 10, Quantity = 2, UnitPrice = 3500m, DiscountPercentage = 0m, TaxRate = 14m },
                new InvoiceProduct { InvoiceProductId = 9, InvoiceId = 3, ProductId = 8, Quantity = 1, UnitPrice = 1800m, DiscountPercentage = 0m, TaxRate = 14m }
            );

            // Seed Payments (apenas para faturas 1 e 2)
            modelBuilder.Entity<Payment>().HasData(
                // Invoice 1 - pagamento total em dinheiro
                new Payment
                {
                    PaymentId = 1,
                    InvoiceId = 1,
                    PaymentTypeId = 1, // Dinheiro
                    Amount = 13908m,
                    PaymentDate = seedTimestamp.AddDays(2),
                    Reference = "CASH-INV-2025-0001",
                    Notes = "Pagamento integral",
                    IsConfirmed = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                // Invoice 2 - pagamentos parciais (dinheiro + cartão)
                new Payment
                {
                    PaymentId = 2,
                    InvoiceId = 2,
                    PaymentTypeId = 1, // Dinheiro
                    Amount = 5000m,
                    PaymentDate = seedTimestamp.AddDays(3),
                    Reference = "CASH-INV-2025-0002-1",
                    Notes = "Parcial 1",
                    IsConfirmed = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                },
                new Payment
                {
                    PaymentId = 3,
                    InvoiceId = 2,
                    PaymentTypeId = 3, // Cartão de Crédito
                    Amount = 5000m,
                    PaymentDate = seedTimestamp.AddDays(3),
                    Reference = "CARD-INV-2025-0002-2",
                    Notes = "Parcial 2",
                    IsConfirmed = true,
                    CreatedAt = seedTimestamp,
                    CreatedByUserId = 4
                }
            );
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is AuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (AuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedByUserId = _currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UpdatedByUserId = _currentUserId;
                }
            }
        }

        /// <summary>
        /// Ignora o filtro de soft delete para queries específicas
        /// </summary>
        public IQueryable<T> GetAllIncludingDeleted<T>() where T : class
        {
            return Set<T>().IgnoreQueryFilters();
        }
    }
}
