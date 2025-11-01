using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.DTOs.Validators;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Services;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;
using VendaFlex.Infrastructure.Database;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.Infrastructure.Navigation;
using VendaFlex.Infrastructure.Services;
using VendaFlex.UI.Views.Authentication;
using VendaFlex.UI.Views.Dashboard;
using VendaFlex.UI.Views.Sales;
using VendaFlex.UI.Views.Setup;
using VendaFlex.ViewModels.Authentication;
using VendaFlex.ViewModels.Dashboard;
using VendaFlex.ViewModels.Sales;
using VendaFlex.ViewModels.Setup;

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

            // Serviços de infraestrutura
            services.AddSingleton<INavigationService, NavigationService>();
            // Contexto de usuário atual (por escopo)
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();
            // Sessão também escopo para alinhar com UserContext e evitar resolução antes do DbContext em escopos globais
            services.AddScoped<ISessionService, SessionService>();
            services.AddSingleton<ICredentialManager, WindowsCredentialManager>();
            services.AddScoped<IDatabaseStatusService, DatabaseStatusService>();
            services.AddScoped<IDatabaseSyncService, DatabaseSyncService>();
            services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            services.AddSingleton<IFileStorageService, FileStorageService>();
            services.AddScoped<IReceiptPrintService, ReceiptPrintService>();

            // Repositórios
            services.AddScoped<UserRepository>();
            services.AddScoped<PersonRepository>();
            services.AddScoped<CompanyConfigRepository>();
            services.AddScoped<PrivilegeRepository>();
            services.AddScoped<UserPrivilegeRepository>();
            services.AddScoped<ProductRepository>();
            services.AddScoped<CategoryRepository>();
            services.AddScoped<StockMovementRepository>();
            services.AddScoped<StockRepository>();
            services.AddScoped<PriceHistoryRepository>();
            services.AddScoped<ExpirationRepository>();
            services.AddScoped<InvoiceRepository>();
            services.AddScoped<InvoiceProductRepository>();
            services.AddScoped<PaymentRepository>();
            services.AddScoped<PaymentTypeRepository>();

            // Serviços principais
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<ICompanyConfigService, CompanyConfigService>();
            services.AddScoped<IPrivilegeService, PrivilegeService>();
            services.AddScoped<IUserPrivilegeService, UserPrivilegeService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IExpirationService, ExpirationService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IInvoiceProductService, InvoiceProductService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentTypeService, PaymentTypeService>();
            services.AddScoped<ICategoryService, CategoryService>();

            // Validadores
            services.AddScoped<IValidator<UserDto>, UserDtoValidator>();
            services.AddScoped<IValidator<string>, PasswordValidator>();
            services.AddScoped<IValidator<PersonDto>, PersonDtoValidator>();
            services.AddScoped<IValidator<CompanyConfigDto>, CompanyConfigDtoValidator>();
            services.AddScoped<PersonBusinessValidator>();
            services.AddScoped<IValidator<PrivilegeDto>, PrivilegeDtoValidator>();
            services.AddScoped<IValidator<UserPrivilegeDto>, UserPrivilegeDtoValidator>();
            services.AddScoped<IValidator<ProductDto>, ProductDtoValidator>();
            services.AddScoped<IValidator<CategoryDto>, CategoryDtoValidator>();
            services.AddScoped<IValidator<ExpirationDto>, ExpirationDtoValidator>();
            services.AddScoped<IValidator<StockDto>, StockDtoValidator>();
            services.AddScoped<IValidator<PriceHistoryDto>, PriceHistoryDtoValidator>();
            services.AddScoped<IValidator<StockMovementDto>, StockMovementDtoValidator>();
            services.AddScoped<IValidator<InvoiceDto>, InvoiceDtoValidator>();
            services.AddScoped<IValidator<InvoiceProductDto>, InvoiceProductDtoValidator>();
            services.AddScoped<IValidator<PaymentDto>, PaymentDtoValidator>();
            services.AddScoped<IValidator<PaymentTypeDto>, PaymentTypeDtoValidator>();


            // Registrar Views e ViewModels usados pela navegação
            services.AddTransient<InitialSetupView>();
            services.AddTransient<InitialSetupViewModel>();         

            services.AddTransient<LoginView>();
            services.AddTransient<LoginViewModel>();

            services.AddTransient<DashboardView>();
            services.AddTransient<DashboardViewModel>();

            services.AddTransient<PdvView>();
            services.AddTransient<PdvViewModel>();



            //
            return services;
        }
    }
}
