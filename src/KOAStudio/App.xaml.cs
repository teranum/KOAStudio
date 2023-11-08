using CommunityToolkit.Mvvm.DependencyInjection;
using KOAStudio.Business;
using KOAStudio.Core.Helpers;
using KOAStudio.Core.Services;
using KOAStudio.Core.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace KOAStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Ioc.Default.ConfigureServices(
                new ServiceCollection()

                //Services
                .AddSingleton<IAppRegistry>(new AppRegistry("teranum"))
                .AddSingleton<BusinessLogic>()

                .AddTransient<IUIRequest>((p) => p.GetRequiredService<BusinessLogic>())

                .BuildServiceProvider()
                );
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new KOAWindow();
            window.Show();
        }
    }
}
