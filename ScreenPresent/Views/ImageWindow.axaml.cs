using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;
using ScreenPresent.Classes;
using System;
using System.Linq;
using System.Threading;

namespace ScreenPresent.Views;

/// <summary>
/// Interaction logic for EasyImageWindow.xaml
/// </summary>
public partial class ImageWindow : Window
{
    private const double BannerPadding = 10.0; // Zusätzlicher Padding für den Banner-Text
    private const int REPETITION_COUNT = 2;

    public readonly ViewModels.ImageViewModel ViewModel;

    public void CreateBanner()
    {
        SpBanner.Children.Clear();
        if (string.IsNullOrWhiteSpace(ViewModel.Text))
        {
            return;
        }
        ViewModel.BannerVisible = true;
        for (int i = 0; i < REPETITION_COUNT; i++)
        {
            TextBlock bannerText = new()
            {
                Text = ViewModel.Text,
                FontSize = GlobalConfig.JsonFile.Settings.BannerTextSize,
                Foreground = Brushes.White,
                Margin = new Thickness(BannerPadding),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            SpBanner.Children.Add(bannerText);
        }

        StartBannerAnimation();
    }

    CancellationTokenSource cancellationTokenSource = new();

    private void StartBannerAnimation()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;
        DispatcherTimer.Run(() =>
        {
            if (token.IsCancellationRequested)
            {
                return false;
            }
            TextBlock textBlock = SpBanner.Children.Cast<TextBlock>().FirstOrDefault()!;
            if (textBlock == null)
            {
                return false;
            }
            double bannerSpeed = GlobalConfig.JsonFile.Settings.BannerSpeed / 10d;
            textBlock.Margin = new Thickness(textBlock.Margin.Left - bannerSpeed, BannerPadding, BannerPadding, BannerPadding);

            if (-textBlock.Margin.Left > textBlock.Bounds.Width + BannerPadding)
            {
                SpBanner.Children.Remove(textBlock);
                textBlock.Margin = new Thickness(BannerPadding);
                SpBanner.Children.Add(textBlock);
            }

            return SpBanner.IsVisible;
        }, TimeSpan.FromSeconds(1d / GlobalConfig.JsonFile.Settings.NewstickerFrames));
    }

    public ImageWindow()
    {
        ViewModel = null!;
    }

    public ImageWindow(ViewModels.SettingsViewModel settings)
    {
        InitializeComponent();
        DataContext = ViewModel = new ViewModels.ImageViewModel(settings);

        // Subcribe to position changes, this is because we can't bind the position.
        this.Position = ViewModel.Settings.Position;
        ViewModel.Settings.ObservableForProperty(x => x.Position).Subscribe(x =>
        {
            this.Position = x.Value;
        });

        // Avalonia doesn't refresh the bind.
        this.Width = ViewModel.Settings.Width;
        this.Height = ViewModel.Settings.Height;

        ViewModel.Settings.PropertyChanged += SettingsViewModel_PropertyChanged;
    }

    private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Settings.WindowState))
        {
            if (ViewModel.Settings.WindowState == WindowState.FullScreen)
            {
                var screen = ViewModel.Settings.SelectedMonitore.Value;
                this.Width = screen.Bounds.Width;
                this.Height = screen.Bounds.Height;
            }
        }
    }

    internal void StopNewsticker()
    {
        ViewModel.BannerVisible = false;
        SpBanner.Children.Clear();
    }

    public Action? CloseWindow { get; set; }
    private void Window_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            CloseWindow?.Invoke();
        }
    }
}
