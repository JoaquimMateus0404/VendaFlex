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
            CreateMap<Person, PersonDto>().ReverseMap();

            // User
            CreateMap<User, UserDto>().ReverseMap();

            // Privilege
            CreateMap<Privilege, PrivilegeDto>().ReverseMap();

            // UserPrivilege
            CreateMap<UserPrivilege, UserPrivilegeDto>().ReverseMap();

            // Category
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products != null ? s.Products.Count : 0))
                .ReverseMap();

            // Product
            CreateMap<Product, ProductDto>()
                .ForMember(d => d.Code, o => o.MapFrom(s => s.Barcode))
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.CurrentStock, o => o.MapFrom(s => s.Stock != null ? s.Stock.Quantity : 0))
                .ReverseMap();

            // Stock
            CreateMap<Stock, StockDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.MinimumStock, o => o.MapFrom(s => s.Product != null ? s.Product.MinimumStock : null))
                .ForMember(d => d.ReorderPoint, o => o.MapFrom(s => s.Product != null ? s.Product.ReorderPoint : null))
                .ReverseMap();

            // StockMovement
            CreateMap<StockMovement, StockMovementDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null && s.User.Person != null ? s.User.Person.Name : string.Empty))
                .ReverseMap();

            // Expiration
            CreateMap<Expiration, ExpirationDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ReverseMap();

            // Invoice
            CreateMap<Invoice, InvoiceDto>().ReverseMap();

            // InvoiceProduct
            CreateMap<InvoiceProduct, InvoiceProductDto>().ReverseMap();

            // Payment
            CreateMap<Payment, PaymentDto>().ReverseMap();

            // PaymentType
            CreateMap<PaymentType, PaymentTypeDto>().ReverseMap();

            // Expense
            CreateMap<Expense, ExpenseDto>().ReverseMap();

            // ExpenseType
            CreateMap<ExpenseType, ExpenseTypeDto>().ReverseMap();

            // AuditLog
            CreateMap<AuditLog, AuditLogDto>().ReverseMap();

            // CompanyConfig (enum -> int)
            CreateMap<CompanyConfig, CompanyConfigDto>()
                    .ForMember(d => d.InvoiceFormat, o => o.MapFrom(s => (int)s.InvoiceFormat));
            CreateMap<CompanyConfigDto, CompanyConfig>()
                .ForMember(d => d.InvoiceFormat, o => o.MapFrom(s => (CompanyConfig.InvoiceFormatType)s.InvoiceFormat));

            // PriceHistory
            CreateMap<PriceHistory, PriceHistoryDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ReverseMap();
        }
    }
}
