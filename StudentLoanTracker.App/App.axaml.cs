using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using StudentLoanTracker.App.ViewModels;
using StudentLoanTracker.App.Views;
using StudentLoanTracker.Core;
using StudentLoanTracker.Data;

namespace StudentLoanTracker.App;

/// <summary>
/// Avalonia application: loads global styles, ensures the SQLite file exists, registers DI services,
/// and assigns <see cref="MainWindowViewModel"/> to the main window. Child views resolve dependencies
/// through constructor injection on their view models (created manually when navigating).
/// </summary>
public partial class App : Application
{
    /// <summary>Built after startup; useful if you later add more windows or dialogs that need services.</summary>
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            // Create the database file and Loans table under %AppData%\StudentLoanTracker if needed.
            DatabaseInitializer.Initialize();
            var services = new ServiceCollection();
            services.AddSingleton<ILoanRepository>(_ => new SqliteLoanRepository());
            services.AddSingleton<ILoanCalculator>(_ => new LoanCalculator());
            services.AddTransient<MainWindowViewModel>();
            Services = services.BuildServiceProvider();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Removes Avalonia's data-annotations validation plugin so validation is driven by our own
    /// <see cref="StudentLoanTracker.Core.LoanValidator"/> messages instead of duplicate UI errors.
    /// </summary>
    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}