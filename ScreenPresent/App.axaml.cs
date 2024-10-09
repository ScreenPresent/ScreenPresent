using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ScreenPresent.ViewModels;
using ScreenPresent.Views;
using System.IO;
using System.Reflection;

namespace ScreenPresent;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        SettingsViewModel settings = new();
        this.DataContext = settings;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(settings);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
