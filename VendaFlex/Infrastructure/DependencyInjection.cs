using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Services;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddVendaFlex(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext via configuração centralizada
            DatabaseConfiguration.ConfigureDbContext(services, configuration);

            // AutoMapper - registrar perfil explicitamente
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            // Repositórios específicos
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<Person>, PersonRepository>();
            services.AddScoped<IRepository<Product>, ProductRepository>();
            services.AddScoped<IRepository<Category>, CategoryRepository>();
            services.AddScoped<IRepository<Stock>, StockRepository>();
            services.AddScoped<IRepository<Invoice>, InvoiceRepository>();
            services.AddScoped<IRepository<Payment>, PaymentRepository>();
            services.AddScoped<IRepository<Expense>, ExpenseRepository>();
            services.AddScoped<IRepository<ExpenseType>, ExpenseTypeRepository>();
            services.AddScoped<IRepository<AuditLog>, AuditLogRepository>();
            services.AddScoped<IRepository<Privilege>, PrivilegeRepository>();
            services.AddScoped<IRepository<UserPrivilege>, UserPrivilegeRepository>();

            // Genéricos
            services.AddScoped<IRepository<InvoiceProduct>, GenericRepository<InvoiceProduct>>();
            services.AddScoped<IRepository<PaymentType>, GenericRepository<PaymentType>>();
            services.AddScoped<IRepository<StockMovement>, GenericRepository<StockMovement>>();
            services.AddScoped<IRepository<Expiration>, GenericRepository<Expiration>>();
            services.AddScoped<IRepository<CompanyConfig>, GenericRepository<CompanyConfig>>();
            services.AddScoped<IRepository<PriceHistory>, GenericRepository<PriceHistory>>();

            // Serviços de domínio
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IPrivilegeService, PrivilegeService>();
            services.AddScoped<IUserPrivilegeService, UserPrivilegeService>();
            services.AddScoped<ICompanyConfigService, CompanyConfigService>();
            services.AddScoped<IPriceHistoryService, PriceHistoryService>();
            //services.AddScoped<IReceiptPrintService, ReceiptPrintService>();
            //services.AddSingleton<IFileStorageService, Infrastructure.Services.FileStorageService>();

            return services;
        }
    }
}
