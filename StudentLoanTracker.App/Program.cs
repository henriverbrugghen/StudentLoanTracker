using Avalonia;
using System;

namespace StudentLoanTracker.App;

/// <summary>Desktop entry point: starts Avalonia with the classic Win/macOS/Linux window lifetime.</summary>
sealed class Program
{
    // Avoid Avalonia APIs, other UI toolkits, or anything that needs a SynchronizationContext before
    // the app builder runs — the framework is not ready yet and you can hit subtle startup bugs.

    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>Central Avalonia setup (theme, fonts, logging). Kept in a static method for the XAML designer.</summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
