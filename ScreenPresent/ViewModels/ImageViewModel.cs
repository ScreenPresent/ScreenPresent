using Avalonia.Media.Imaging;
using LibVLCSharp.Shared;
using ReactiveUI;
using ScreenPresent.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using ScreenPresent.Enums;
using Directory = MetadataExtractor.Directory;

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

    public bool BannerVisible
    {
        get => _bannerVisible;
        set => this.RaiseAndSetIfChanged(ref _bannerVisible, value);
    }

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
            ImageSource = LoadOrientation(new Bitmap(file.Filename), file.Filename);
        }
        catch (FileNotFoundException)
        {
            //The file has been deleted
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Unable to load the image: '" + file.Filename + "'.");
        }
    }

    public Bitmap LoadOrientation(Bitmap bitmap, string filePath)
    {
        var directories = ImageMetadataReader.ReadMetadata(filePath);
        var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

        if (ifd0 == null)
        {
            return bitmap.ApplyOrientation(ImageOrientation.Unknown);
        }

        // EXIF Tag ID 274 = Orientation
        if (ifd0.TryGetInt32(ExifDirectoryBase.TagOrientation, out int orientationValue))
        {
            ImageOrientation imageOrientation = orientationValue switch
            {
                1 => ImageOrientation.Normal,
                2 => ImageOrientation.MirrorHorizontal,
                3 => ImageOrientation.Rotate180,
                4 => ImageOrientation.MirrorVertical,
                5 => ImageOrientation.MirrorHorizontalAndRotate270CW,
                6 => ImageOrientation.Rotate90CW,
                7 => ImageOrientation.MirrorHorizontalAndRotate90CW,
                8 => ImageOrientation.Rotate270CW,
                _ => ImageOrientation.Unknown
            };
            return bitmap.ApplyOrientation(imageOrientation);
        }


        return bitmap.ApplyOrientation(ImageOrientation.Unknown);
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
        get => _weather == null
            ? null
            : new WeatherResult($"Wetter in {_weather.CityName}",
                _weather.DailyWeather.Take(Settings.VisibleDays).ToList());
        set => this.RaiseAndSetIfChanged(ref _weather, value);
    }

    public SettingsViewModel Settings { get; set; }
}