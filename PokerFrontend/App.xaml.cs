using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AdonisUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using PokerFrontend.Business;
using PokerFrontend.ViewModel.HandHistories;
using PokerFrontend.ViewModel.Ranges.Add;

namespace PokerFrontend
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
            var mainWindow = Services.GetService<MainWindow>();
            mainWindow.DataContext = Services.GetService<MainViewModel>();
            mainWindow.Show();
        }



        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;


        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<RangesBusinessHandler>(); 
            services.AddTransient<ReadSaveSettingsBusinessHandler>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<CreateRangesViewModel>();
            services.AddTransient<CreateViewModel>();
            services.AddSingleton<MainWindow>();



            return services.BuildServiceProvider();
        }


    }
}
