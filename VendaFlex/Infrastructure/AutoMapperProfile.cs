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
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Product
            CreateMap<Product, ProductDto>().ReverseMap();

            // Stock
            CreateMap<Stock, StockDto>().ReverseMap();

            // StockMovement
            CreateMap<StockMovement, StockMovementDto>().ReverseMap();

            // Expiration
            CreateMap<Expiration, ExpirationDto>().ReverseMap();

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
            CreateMap<PriceHistory, PriceHistoryDto>().ReverseMap();
        }
    }
}
