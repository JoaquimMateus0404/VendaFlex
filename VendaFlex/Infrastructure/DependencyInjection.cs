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
using VendaFlex.UI.Views.Setup;
using VendaFlex.ViewModels.Authentication;
using VendaFlex.ViewModels.Dashboard;
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
            services.AddSingleton<ISessionService, SessionService>();
            services.AddSingleton<ICredentialManager, WindowsCredentialManager>();
            services.AddScoped<IDatabaseStatusService, DatabaseStatusService>();
            services.AddScoped<IDatabaseSyncService, DatabaseSyncService>();
            services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            //services.AddScoped<IReceiptPrintService, ReceiptPrintService>();
            services.AddSingleton<IFileStorageService, FileStorageService>();

            // Repositórios
            services.AddScoped<UserRepository>();
            services.AddScoped<PersonRepository>();
            services.AddScoped<CompanyConfigRepository>();
            services.AddScoped<PrivilegeRepository>();
            services.AddScoped<UserPrivilegeRepository>();
            
            // Serviços principais
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<ICompanyConfigService, CompanyConfigService>();
            services.AddScoped<IPrivilegeService, PrivilegeService>();
            services.AddScoped<IUserPrivilegeService, UserPrivilegeService>();

            // Validadores
            services.AddScoped<IValidator<UserDto>, UserDtoValidator>();
            services.AddScoped<IValidator<string>, PasswordValidator>();
            services.AddScoped<IValidator<PersonDto>, PersonDtoValidator>();
            services.AddScoped<IValidator<CompanyConfigDto>, CompanyConfigDtoValidator>();
            services.AddScoped<PersonBusinessValidator>();
            services.AddScoped<IValidator<PrivilegeDto>, PrivilegeDtoValidator>();
            services.AddScoped<IValidator<UserPrivilegeDto>, UserPrivilegeDtoValidator>();





            // Genéricos
            services.AddScoped<IRepository<InvoiceProduct>, GenericRepository<InvoiceProduct>>();
            services.AddScoped<IRepository<PaymentType>, GenericRepository<PaymentType>>();
            services.AddScoped<IRepository<StockMovement>, GenericRepository<StockMovement>>();
            services.AddScoped<IRepository<Expiration>, GenericRepository<Expiration>>();
            services.AddScoped<IRepository<PriceHistory>, GenericRepository<PriceHistory>>();

            // Registrar Views e ViewModels usados pela navegação
            services.AddTransient<InitialSetupView>();
            services.AddTransient<InitialSetupViewModel>();         

            services.AddTransient<LoginView>();
            services.AddTransient<LoginViewModel>();

            services.AddTransient<DashboardView>();
            services.AddTransient<DashboardViewModel>();



            //
            return services;
        }
    }
}
