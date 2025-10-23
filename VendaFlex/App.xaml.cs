using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

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

            // Criar e mostrar a janela principal
            if (ServiceProvider != null)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}
