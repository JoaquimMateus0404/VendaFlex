using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Data.Entities;

namespace VendaFlex.Infrastructure
{
    public class AutoMapperProfile : Profile
    {
       public AutoMapperProfile()
       {
            // Person
            CreateMap<Person, PersonDto>();
            CreateMap<PersonDto, Person>()
                .ForMember(d => d.SuppliedProducts, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.Invoices, o => o.Ignore());

            // User
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>()
                .ForMember(d => d.Person, o => o.Ignore())
                .ForMember(d => d.UserPrivileges, o => o.Ignore())
                .ForMember(d => d.Invoices, o => o.Ignore())
                .ForMember(d => d.StockMovements, o => o.Ignore())
                .ForMember(d => d.Expenses, o => o.Ignore());

            // Privilege
            CreateMap<Privilege, PrivilegeDto>();
            CreateMap<PrivilegeDto, Privilege>()
                .ForMember(d => d.UserPrivileges, o => o.Ignore());

            // UserPrivilege
            CreateMap<UserPrivilege, UserPrivilegeDto>();
            CreateMap<UserPrivilegeDto, UserPrivilege>()
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.Privilege, o => o.Ignore());

            // Category
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products != null ? s.Products.Count : 0));
            CreateMap<CategoryDto, Category>()
                .ForMember(d => d.Products, o => o.Ignore());

            // Product
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.InternalCode))
                .ForMember(d => d.InternalCode, o => o.MapFrom(s => s.InternalCode))
                .ForMember(d => d.ExternalCode, o => o.MapFrom(s => s.ExternalCode))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.CurrentStock, o => o.MapFrom(s => s.Stock != null ? s.Stock.Quantity : 0));

            // ProductDto -> Product (mapeamento reverso explícito)
            CreateMap<ProductDto, Product>()
                .ForMember(d => d.InternalCode, o => o.MapFrom(s => s.Code))
                .ForMember(d => d.ExternalCode, o => o.MapFrom(s => s.ExternalCode))
                // IGNORAR propriedades de navegação - usar apenas FKs
                .ForMember(d => d.Category, o => o.Ignore())
                .ForMember(d => d.Supplier, o => o.Ignore())
                .ForMember(d => d.Stock, o => o.Ignore())
                .ForMember(d => d.StockMovements, o => o.Ignore())
                .ForMember(d => d.Expirations, o => o.Ignore())
                .ForMember(d => d.InvoiceProducts, o => o.Ignore())
                .ForMember(d => d.PriceHistories, o => o.Ignore());

            // Stock
            CreateMap<Stock, StockDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.MinimumStock, o => o.MapFrom(s => s.Product != null ? s.Product.MinimumStock : null))
                .ForMember(d => d.ReorderPoint, o => o.MapFrom(s => s.Product != null ? s.Product.ReorderPoint : null));

            // StockDto -> Stock (mapeamento reverso explícito)
            CreateMap<StockDto, Stock>()
                // IGNORAR propriedades de navegação - usar apenas FK ProductId
                .ForMember(d => d.Product, o => o.Ignore());

            // StockMovement
            CreateMap<StockMovement, StockMovementDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null && s.User.Person != null ? s.User.Person.Name : string.Empty));

            // StockMovementDto -> StockMovement (mapeamento reverso explícito)
            CreateMap<StockMovementDto, StockMovement>()
                // IGNORAR propriedades de navegação - usar apenas FKs
                .ForMember(d => d.Product, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore());

            // Expiration
            CreateMap<Expiration, ExpirationDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.ExpirationWarningDays, o => o.MapFrom(s => s.Product != null ? s.Product.ExpirationWarningDays : null))
                .ForMember(d => d.IsNearExpiration, o => o.MapFrom(s => 
                    s.ExpirationDate.Date >= DateTime.Now.Date && 
                    (s.ExpirationDate.Date - DateTime.Now.Date).Days <= (s.Product != null && s.Product.ExpirationWarningDays.HasValue ? s.Product.ExpirationWarningDays.Value : 30)));

            // ExpirationDto -> Expiration (mapeamento reverso explícito)
            CreateMap<ExpirationDto, Expiration>()
                // IGNORAR propriedades de navegação - usar apenas FK ProductId
                .ForMember(d => d.Product, o => o.Ignore());

            // Invoice
            CreateMap<Invoice, InvoiceDto>();
            CreateMap<InvoiceDto, Invoice>()
                // IGNORAR propriedades de navegação - usar apenas FKs PersonId e UserId
                .ForMember(d => d.Person, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.InvoiceProducts, o => o.Ignore())
                .ForMember(d => d.Payments, o => o.Ignore());

            // InvoiceProduct
            CreateMap<InvoiceProduct, InvoiceProductDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product != null ? s.Product.InternalCode : string.Empty));
            
            CreateMap<InvoiceProductDto, InvoiceProduct>()
                // IGNORAR propriedades de navegação - usar apenas FKs InvoiceId e ProductId
                .ForMember(d => d.Invoice, o => o.Ignore())
                .ForMember(d => d.Product, o => o.Ignore());

            // Payment
            CreateMap<Payment, PaymentDto>();
            CreateMap<PaymentDto, Payment>()
                // IGNORAR propriedades de navegação - usar apenas FKs InvoiceId e PaymentTypeId
                .ForMember(d => d.Invoice, o => o.Ignore())
                .ForMember(d => d.PaymentType, o => o.Ignore());

            // PaymentType
            CreateMap<PaymentType, PaymentTypeDto>();
            CreateMap<PaymentTypeDto, PaymentType>()
                .ForMember(d => d.Payments, o => o.Ignore());

            // Expense
            CreateMap<Expense, ExpenseDto>();
            CreateMap<ExpenseDto, Expense>()
                // IGNORAR propriedades de navegação - usar apenas FKs ExpenseTypeId e UserId
                .ForMember(d => d.ExpenseType, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore());

            // ExpenseType
            CreateMap<ExpenseType, ExpenseTypeDto>();
            CreateMap<ExpenseTypeDto, ExpenseType>()
                .ForMember(d => d.Expenses, o => o.Ignore());

            // AuditLog
            CreateMap<AuditLog, AuditLogDto>();
            CreateMap<AuditLogDto, AuditLog>();

            // CompanyConfig (enum -> int)
            CreateMap<CompanyConfig, CompanyConfigDto>()
                    .ForMember(d => d.InvoiceFormat, o => o.MapFrom(s => (int)s.InvoiceFormat));
            CreateMap<CompanyConfigDto, CompanyConfig>()
                .ForMember(d => d.InvoiceFormat, o => o.MapFrom(s => (CompanyConfig.InvoiceFormatType)s.InvoiceFormat));

            // PriceHistory
            CreateMap<PriceHistory, PriceHistoryDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty));
            CreateMap<PriceHistoryDto, PriceHistory>()
                // IGNORAR propriedades de navegação - usar apenas FK ProductId
                .ForMember(d => d.Product, o => o.Ignore());
        }
    }
}
