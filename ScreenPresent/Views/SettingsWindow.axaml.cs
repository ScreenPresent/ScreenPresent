using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ScreenPresent.Views;
/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window {

    private readonly ViewModels.SettingsViewModel _viewModel;
    public SettingsWindow() {
        // Avalonia build constructor
        _viewModel = null!;
    }

    public SettingsWindow(ViewModels.SettingsViewModel viewModel) {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private async void OnBtnSelectBannerPathClick(object sender, RoutedEventArgs e)
    {
        IReadOnlyList<IStorageFolder> result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        if (result.Count == 1)
        {
            _viewModel.BannerPath = result[0].Path.LocalPath;
        }
    }

    private void BtnOpenLink_Click(object? sender, RoutedEventArgs e)
    {
        string? url = ((Button)sender).Tag?.ToString();
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    private void OnBtnCloseClick(object sender, RoutedEventArgs e) {
        this.Close();
    }
}
