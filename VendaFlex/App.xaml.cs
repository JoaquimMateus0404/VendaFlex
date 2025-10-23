using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using VendaFlex.UI.Views.Main;
using VendaFlex.ViewModels.Main;
using VendaFlex.Infrastructure.Navigation;

namespace VendaFlex
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (ServiceProvider != null)
            {
                // Criar a SplashView como janela principal
                var splashView = new SplashView();
                
                // Resolver o ViewModel do container de DI
                var splashViewModel = ServiceProvider.GetRequiredService<SplashViewModel>();
                splashView.DataContext = splashViewModel;

                // Registrar a janela no NavigationService
                var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
                navigationService.SetCurrentWindow(splashView);

                // Exibir a janela
                splashView.Show();
            }
        }
    }
}
