using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DemoPulse.Interop;
using DemoPulse.Models;
using DemoPulse.Services;
using DemoPulse.Services.Providers;

namespace DemoPulse;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (sender, args) =>
        {
            System.Diagnostics.Debug.WriteLine($"[WPF Global Unhandled Exception] {args.Exception}");
            args.Handled = true; // Prevents crash
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            System.Diagnostics.Debug.WriteLine($"[AppDomain Fatal Exception] {args.ExceptionObject}");
        };

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AppSettings>(sp => AppSettings.Load());
        services.AddSingleton<WpfUiMessenger>();
        services.AddSingleton<IUiMessenger>(sp => sp.GetRequiredService<WpfUiMessenger>());
        services.AddSingleton<IDialogService, WpfDialogService>();
        services.AddSingleton<IFileSystemService, PhysicalFileSystemService>();
        services.AddSingleton<IDemoService, DemoService>();

        // Auto-register all non-abstract ICommandHandler implementations via reflection
        var commandHandlerTypes = typeof(ICommandHandler).Assembly.GetTypes()
            .Where(t => typeof(ICommandHandler).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in commandHandlerTypes)
        {
            services.AddSingleton(typeof(ICommandHandler), handlerType);
        }

        services.AddSingleton<CommandDispatcher>();
        services.AddSingleton<WebViewMessageRouter>();
        services.AddTransient<MainWindow>();
    }
}

