using Avalonia.Media.Imaging;
using LibVLCSharp.Shared;
using ReactiveUI;
using ScreenPresent.Classes;
using System;
using System.IO;
using System.Linq;

namespace ScreenPresent.ViewModels;

public class ImageViewModel : ReactiveObject
{
    private LibVLC? _libVlc;
    public ImageViewModel(SettingsViewModel settings)
    {
        try
        {
            _libVlc = new();
            MediaPlayer = new(_libVlc);
            MediaPlayer.EndReached += (_, _) => MediaEnded?.Invoke();
        }
        catch
        {
            settings.HasLibVlcError = true;
        }

        Settings = settings;
        Settings.RefreshWeather += Settings_RefreshWeather;
    }

    public Action? MediaEnded;

    private void Settings_RefreshWeather()
    {
        this.RaisePropertyChanged(nameof(Weather));
    }

    public bool ShowImage => !ShowWeather && !ShowVideo;

    private bool _stretchImage;
    public bool StretchFiles
    {
        get => _stretchImage;
        set => this.RaiseAndSetIfChanged(ref _stretchImage, value);
    }

    private Bitmap? _imageSource;
    public Bitmap? ImageSource
    {
        get => _imageSource;
        set => this.RaiseAndSetIfChanged(ref _imageSource, value);
    }

    public MediaPlayer? MediaPlayer { get; }

    private bool _showVideo;
    public bool ShowVideo
    {
        get => !ShowWeather && _showVideo;
        set
        {
            this.RaiseAndSetIfChanged(ref _showVideo, value);
            this.RaisePropertyChanged(nameof(ShowImage));
        }
    }

    private bool _bannerVisible;
    public bool BannerVisible { get => _bannerVisible; set => this.RaiseAndSetIfChanged(ref _bannerVisible, value); }

    public static string Repeat(string s, int n)
    {
        return new System.Text.StringBuilder(s.Length * n).Insert(0, s, n).ToString();
    }

    private string _text = null!;
    public string Text
    {
        get => Repeat($"{_text} ", 20).Trim();
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    public string GetText()
    {
        return _text;
    }

    private int _textActualHeight;
    public int TextActualHeight
    {
        get => _textActualHeight;
        set => this.RaiseAndSetIfChanged(ref _textActualHeight, value);
    }

    private bool _showWeather = false;
    public bool ShowWeather
    {
        get => _showWeather;
        set
        {
            this.RaiseAndSetIfChanged(ref _showWeather, value);
            this.RaisePropertyChanged(nameof(ShowVideo));
            this.RaisePropertyChanged(nameof(ShowImage));
        }
    }

    internal void SetImage(Classes.File file)
    {
        StretchFiles = file.Stretch;
        StopMediaIfIsPlaying();
        if (file.IsVideo)
        {
            if (_libVlc is not null)
            {
                using Media media = new(_libVlc, file.Filename);
                MediaPlayer!.Play(media);
            }
            ShowVideo = true;
            return;
        }
        try
        {
            ShowVideo = false;
            Bitmap image = new(file.Filename);
            ImageSource = image;
        }
        catch (FileNotFoundException)
        {
            //The file has been deleted
        }
    }

    public void StopMediaIfIsPlaying()
    {
        if (MediaPlayer?.IsPlaying ?? false)
        {
            MediaPlayer.Stop();
        }
    }

    private WeatherResult? _weather;
    public WeatherResult? Weather
    {
        get => _weather == null ? null : new WeatherResult($"Wetter in {_weather.CityName}", _weather.DailyWeather.Take(Settings.VisibleDays).ToList());
        set => this.RaiseAndSetIfChanged(ref _weather, value);
    }

    public SettingsViewModel Settings { get; set; }
}
