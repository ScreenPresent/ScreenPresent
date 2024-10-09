using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using ReactiveUI;
using ScreenPresent.Classes;
using ScreenPresent.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace ScreenPresent.ViewModels;

public class SettingsViewModel : ReactiveObject
{
    public void SetMonitors(IDictionary<string, Screen> monitors)
    {
        Monitors = monitors;
    }

    #region Events
    public event Action? RecreateNewsticker;
    public event Action? StopNewsticker;
    public event Action? RefreshWeather;
    #endregion

    #region Monitors
    public IDictionary<string, Screen> Monitors { get; internal set; } = new Dictionary<string, Screen>();
    // TODO: https://github.com/AvaloniaUI/Avalonia/issues/11512
    public KeyValuePair<string, Screen> SelectedMonitore
    {
        get => GlobalConfig.JsonFile.Settings.SelectedMonitorName != null && Monitors.ContainsKey(GlobalConfig.JsonFile.Settings.SelectedMonitorName) ? Monitors.Single(x => x.Key == GlobalConfig.JsonFile.Settings.SelectedMonitorName) : GetFirstMonitor();
        set
        {
            if (GlobalConfig.JsonFile.Settings.SelectedMonitorName != value.Key)
            {
                if (value.Key == null)
                {
                    value = GetFirstMonitor();
                }

                GlobalConfig.JsonFile.Settings.SelectedMonitorName = value.Key;
                this.RaisePropertyChanged();

                bool fullScreen = FullScreen;
                FullScreen = false;
                this.RaisePropertyChanged(nameof(Position));
                FullScreen = fullScreen;
            }
        }
    }

    private KeyValuePair<string, Screen> GetFirstMonitor()
    {
        KeyValuePair<string, Screen> screen = Monitors.FirstOrDefault(x => !x.Value.IsPrimary);
        if (screen.Key == null)
        {
            screen = Monitors.FirstOrDefault();
        }
        return screen;
    }

    public PixelPoint Position
    {
        get => SelectedMonitore.Value.Bounds.Position;
    }

    public string BannerFontSizeText => BannerFontSize == 0 || !ShowNewsticker ? "Banner nicht aktiv" : $"Schriftgröße: {BannerFontSize}";
    #endregion

    public List<Theme> Themes { get; } = Enum.GetValues<Theme>().ToList();
    public Theme Theme
    {
        get => GlobalConfig.JsonFile.Settings.Theme;
        set
        {
            if (GlobalConfig.JsonFile.Settings.Theme != value)
            {
                GlobalConfig.JsonFile.Settings.Theme = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(ThemeName));
            }
        }
    }
    public string ThemeName => Theme == Theme.Light ? "Light" : "Dark";

    public int Height
    {
        get => GlobalConfig.JsonFile.Settings.Height;
        set
        {
            GlobalConfig.JsonFile.Settings.Height = value;
            this.RaisePropertyChanged();
        }
    }

    public int Width
    {
        get => GlobalConfig.JsonFile.Settings.Width;
        set
        {
            GlobalConfig.JsonFile.Settings.Width = value;
            this.RaisePropertyChanged();
            RecreateNewsticker?.Invoke();
        }
    }
    public bool TopMost
    {
        get => GlobalConfig.JsonFile.Settings.TopMost;
        set
        {
            GlobalConfig.JsonFile.Settings.TopMost = value;
            this.RaisePropertyChanged();
        }
    }
    public string? BannerPath
    {
        get => GlobalConfig.JsonFile.Settings.BannerPath;
        set
        {
            GlobalConfig.JsonFile.Settings.BannerPath = value;
            this.RaisePropertyChanged();
        }
    }
    public int BannerSpeed
    {
        get => GlobalConfig.JsonFile.Settings.BannerSpeed;
        set
        {
            GlobalConfig.JsonFile.Settings.BannerSpeed = value;
            RecreateNewsticker?.Invoke();
            this.RaisePropertyChanged();
        }
    }
    public int BannerFontSize
    {
        get => GlobalConfig.JsonFile.Settings.BannerTextSize;
        set
        {
            GlobalConfig.JsonFile.Settings.BannerTextSize = value;
            RecreateNewsticker?.Invoke();
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(BannerFontSizeText));
        }
    }

    public string? WeatherApiKey
    {
        get => GlobalConfig.JsonFile.Settings.WeatherApi.ApiKey;
        set
        {
            GlobalConfig.JsonFile.Settings.WeatherApi.ApiKey = value;
            this.RaisePropertyChanged();
        }
    }

    public string? WeatherLocation
    {
        get => GlobalConfig.JsonFile.Settings.WeatherApi.Location;
        set
        {
            GlobalConfig.JsonFile.Settings.WeatherApi.Location = value;
            this.RaisePropertyChanged();
        }
    }
    public int VisibleDays
    {
        get => GlobalConfig.JsonFile.Settings.VisibleDays;
        set
        {
            GlobalConfig.JsonFile.Settings.VisibleDays = value;
            RefreshWeather?.Invoke();
        }
    }
    public int WeatherDuration
    {
        get => GlobalConfig.JsonFile.Settings.WeatherDuration;
        set
        {
            if (value < 10)
            {
                value = 10;
            }

            GlobalConfig.JsonFile.Settings.WeatherDuration = value;
        }
    }
    public int PlaceLetterSize
    {
        get => GlobalConfig.JsonFile.Settings.PlaceLetterSize;
        set
        {
            GlobalConfig.JsonFile.Settings.PlaceLetterSize = value;
            this.RaisePropertyChanged();
        }
    }

    public WindowState WindowState
    {
        get => GlobalConfig.JsonFile.Settings.FullScreen && AllowFullScreen ? WindowState.FullScreen : WindowState.Normal;
        set => GlobalConfig.JsonFile.Settings.FullScreen = value == WindowState.FullScreen;
    }

    private bool _allowFullScreen;
    public bool AllowFullScreen
    {
        get => _allowFullScreen;
        set
        {
            _allowFullScreen = value;
            this.RaisePropertyChanged(nameof(WindowState));
        }
    }

    public bool FullScreen
    {
        get => GlobalConfig.JsonFile.Settings.FullScreen;
        set
        {
            GlobalConfig.JsonFile.Settings.FullScreen = value;
            this.RaisePropertyChanged(nameof(WindowState));
            if (!value)
            {
                this.RaisePropertyChanged(nameof(Width));
                this.RaisePropertyChanged(nameof(Height));
            }
            RecreateNewsticker?.Invoke();
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(Position));
        }
    }

    public bool StartDiashowAtProgramStart
    {
        get => GlobalConfig.JsonFile.Settings.StartDiashowAtProgramStart;
        set => GlobalConfig.JsonFile.Settings.StartDiashowAtProgramStart = value;
    }

    public bool ShowWeather
    {
        get => GlobalConfig.JsonFile.Settings.ShowWeather;
        set => GlobalConfig.JsonFile.Settings.ShowWeather = value;
    }

    public bool ShowNewsticker
    {
        get => GlobalConfig.JsonFile.Settings.ShowNewsticker;
        set
        {
            GlobalConfig.JsonFile.Settings.ShowNewsticker = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(BannerFontSizeText));
            if (value)
            {
                RecreateNewsticker?.Invoke();
            }
            else
            {
                StopNewsticker?.Invoke();
            }
        }
    }

    public bool OrderFoldersRandom
    {
        get => GlobalConfig.JsonFile.Settings.OrderFoldersRandom;
        set => GlobalConfig.JsonFile.Settings.OrderFoldersRandom = value;
    }

    public int NewstickerFrames
    {
        get => GlobalConfig.JsonFile.Settings.NewstickerFrames;
        set
        {
            GlobalConfig.JsonFile.Settings.NewstickerFrames = value;
            this.RaisePropertyChanged();
            RecreateNewsticker?.Invoke();
        }
    }

    public bool ShowOnlyWeather { get; set; }

    #region Commands

    #region newsticker

    #region slower
    private ICommand? _newstickerSlowerCommand;
    [JsonIgnore]
    public ICommand NewstickerSlowerCommand => _newstickerSlowerCommand ??= ReactiveCommand.Create(() => BannerSpeed--, this.WhenAnyValue(x => x.BannerSpeed, bannerSpeed => bannerSpeed > 1));
    #endregion

    #region faster
    private ICommand? _newstickerFasterCommand;
    [JsonIgnore]
    public ICommand NewstickerFasterCommand => _newstickerFasterCommand ??= ReactiveCommand.Create(() => BannerSpeed++, this.WhenAnyValue(x => x.BannerSpeed, bannerSpeed => bannerSpeed < 100));
    #endregion

    #region smaller
    private ICommand? _newstickerSmallerCommand;
    [JsonIgnore]
    public ICommand NewstickerSmallerCommand => _newstickerSmallerCommand ??= ReactiveCommand.Create(() => BannerFontSize--, this.WhenAnyValue(x => x.BannerFontSize, bannerFontSize => bannerFontSize > 1));
    #endregion

    #region larger
    private ICommand? _newstickerLargerCommand;
    [JsonIgnore]
    public ICommand NewstickerLargerCommand => _newstickerLargerCommand ??= ReactiveCommand.Create(() => BannerFontSize++, this.WhenAnyValue(x => x.BannerFontSize, bannerFontSize => bannerFontSize < 100));
    #endregion

    #endregion

    #region weather

    #region remove day
    private ICommand? _removeDayCommand;
    [JsonIgnore]
    public ICommand RemoveDayCommand => _removeDayCommand ??= ReactiveCommand.Create(() => VisibleDays--, this.WhenAnyValue(x => x.VisibleDays, visibleDays => visibleDays > 1));
    #endregion

    #region add day
    private ICommand? _addDayCommand;
    [JsonIgnore]
    public ICommand AddDayCommand => _addDayCommand ??= ReactiveCommand.Create(() => VisibleDays++, this.WhenAnyValue(x => x.VisibleDays, visibleDays => visibleDays < 5));

    [JsonIgnore]
    internal bool HasLibVlcError { get; set; }

    #endregion

    #endregion


    #endregion
}
