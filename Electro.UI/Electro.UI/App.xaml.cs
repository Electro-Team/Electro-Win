using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Electro.UI.ViewModels.DNS;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Windows.Threading;
using DotRas;
using Electro.UI.Interfaces;
using Electro.UI.Services;
using Electro.UI.Tools;
using Electro.UI.ViewModels;
using Electro.UI.Views.Authenticate;

namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider serviceProvider;

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Startup += App_Startup;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) 
        {
            MyLogger.GetInstance().Logger.Error(e.Exception);
            //If we have a main window continue working unless let the exception go 
            //that will cause application close
            if (Current.MainWindow != null &&
                Current.MainWindow.IsInitialized &&
                Current.MainWindow.IsLoaded)
            {
                e.Handled = true;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MyLogger.GetInstance().Logger.Error(e.ExceptionObject);
            MyLogger.GetInstance().Logger.Error("Shutting down due to an unhandled exception.");
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;

            var runningInstances = System.Diagnostics.Process.GetProcessesByName(
                    System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (runningInstances.Length > 1)
            {
                Environment.Exit(0);
            }

            var serviceProvider = configureServices();
            this.serviceProvider = serviceProvider;
            initServices(serviceProvider);
        }

        private void initServices(IServiceProvider serviceProvider)
        {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            Current.MainWindow = mainWindow;

            mainWindow.DataContext = serviceProvider.GetRequiredService<MainViewModel>();

            mainWindow.Show();
        }


        private IServiceProvider configureServices()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IDNSService, DNSService>();
            serviceCollection.AddSingleton<DNSViewModel>();
            serviceCollection.AddSingleton<IConnectionObserver>(s => s.GetRequiredService<DNSViewModel>());
            serviceCollection.AddSingleton<PPTP>();
            serviceCollection.AddSingleton<OpenVPN>();
            serviceCollection.AddSingleton<Socks5>();

            serviceCollection.AddScoped<MainViewModel>();
            serviceCollection.AddScoped<MainWindow>();

            return serviceCollection.BuildServiceProvider();

        }
        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            // Creating thread
            // Using thread class
            //Thread thr = new Thread(new ThreadStart(DNSViewModel.UnsetDnsEvent));
            //thr.Start();
            // DNSViewModel.UnsetDnsEvent();

            var pptpService = serviceProvider.GetRequiredService<PPTP>();
            var dnsService = serviceProvider.GetRequiredService<IDNSService>();
            await dnsService.UnsetDNS();
            var openVpnProcess = Process.GetProcesses().
                Where(pr => pr.ProcessName == "openvpn");

            foreach (var process in openVpnProcess)
                process.Kill();

            pptpService.Dispose();
        }
    }
}
