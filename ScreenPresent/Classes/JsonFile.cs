using ReactiveUI;
using ScreenPresent.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScreenPresent.Classes;

public static class GlobalConfig
{
    private static JsonFile? _jsonFile;
    public static JsonFile JsonFile
    {
        get
        {
            if (_jsonFile == null)
            {
                if (System.IO.File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.json")))
                {
                    _jsonFile = JsonSerializer.Deserialize(System.IO.File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.json")), typeof(JsonFile), JsonContext.Default) as JsonFile;
                    if (_jsonFile == null)
                    {
                        throw new InvalidOperationException($"The jsonfile could not be read.");
                    }
                }
                else
                {
                    _jsonFile = new JsonFile();
                }
            }
            return _jsonFile;
        }
    }

    public static void SaveChanges()
    {
        System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration.json"), JsonSerializer.Serialize(JsonFile, JsonContext.Default.JsonFile));
    }

    public static int GetBetween(int value, int start, int end)
    {
        if (value <= start)
        {
            return start;
        }
        if (value >= end)
        {
            return end;
        }
        return value;
    }
}

public class JsonFile
{
    public List<DisplayPath> Paths { get; set; } = new List<DisplayPath>();
    public Settings Settings { get; set; } = new Settings();
}

public class DisplayPath : ReactiveObject
{
    [JsonIgnore]
    public Func<Task>? OnSelectPathCommand_Invoked { get; set; }
    [JsonIgnore]
    public Func<Task>? OnSelectTimeSpanCommand_Invoked { get; set; }

    public string? Name { get; set; }
    private string? _directory;
    public string? Directory
    {
        get => _directory;
        set => this.RaiseAndSetIfChanged(ref _directory, value);
    }

    private int _duration = 1;
    public int Duration
    {
        get => _duration;
        set
        {
            if (_duration != value && value > 0)
            {
                _duration = value;
            }
        }
    }

    public bool HasPrio { get; set; }

    public PlayType PlayType { get; set; }

    private TimeType _timeType;
    public TimeType TimeType
    {
        get => _timeType;
        set => this.RaiseAndSetIfChanged(ref _timeType, value, nameof(TimeSpan));
    }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public List<Weekly> Days { get; set; } = new();
    [JsonIgnore]
    public string TimeSpan
    {
        get
        {
            switch (TimeType)
            {
                case TimeType.None:
                    return TimeType.None.GetDescription();
                case TimeType.Weekly:
                    return string.Join(" ", Days.Select(x => x.GetDescription().Substring(0, 2) + "."));
                case TimeType.TimeSpan:
                    if (!DateStart.HasValue || !DateEnd.HasValue)
                    {
                        return TimeType.None.GetDescription();
                    }
                    return $"Vom {DateStart.Value.ToShortDateString()} {DateStart.Value.ToShortTimeString()} Uhr bis {DateEnd.Value.ToShortDateString()} {DateEnd.Value.ToShortTimeString()} Uhr";
            }
            return TimeType.None.GetDescription();
        }
    }

    public bool EveryInterval { get; set; }

    public bool CheckAndDelete(DateTime dt)
    {
        if (DeleteAfterInterval && !EveryInterval && DateEnd < dt)
        {
            if (Directory != null)
            {
                System.IO.Directory.CreateDirectory(Path.Combine(Directory, "Papierkorb"));
                MoveRecursive(Directory, Path.Combine(Directory, "Papierkorb"));
            }
            return true;
        }
        return false;
    }

    private void MoveRecursive(string sourcePath, string destPath)
    {
        foreach (string file in System.IO.Directory.GetFiles(sourcePath).Where(f => !new FileInfo(f).Attributes.HasFlag(FileAttributes.Hidden)))
        {
            System.IO.File.Move(file, Path.Combine(destPath, Path.GetFileName(file)));
        }
        foreach (string dir in System.IO.Directory.GetDirectories(sourcePath).Where(x => x != Path.Combine(sourcePath, "Papierkorb")))
        {
            System.IO.Directory.Move(dir, Path.Combine(destPath, Path.GetFileName(dir)));
        }
    }

    public bool VideosFullLength { get; set; }

    public bool IsInTime(DateTime dt)
    {
        switch (TimeType)
        {
            case TimeType.None:
                return true;
            case TimeType.Weekly:
                switch (dt.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        return Days.Contains(Weekly.Sunday);
                    case DayOfWeek.Monday:
                        return Days.Contains(Weekly.Monday);
                    case DayOfWeek.Tuesday:
                        return Days.Contains(Weekly.Tuesday);
                    case DayOfWeek.Wednesday:
                        return Days.Contains(Weekly.Wednesday);
                    case DayOfWeek.Thursday:
                        return Days.Contains(Weekly.Thursday);
                    case DayOfWeek.Friday:
                        return Days.Contains(Weekly.Friday);
                    case DayOfWeek.Saturday:
                        return Days.Contains(Weekly.Saturday);
                }
                return false;
            case TimeType.TimeSpan:
                if (!DateStart.HasValue || !DateEnd.HasValue)
                {
                    return true;
                }
                if (!EveryInterval)
                {
                    return DateStart <= dt && DateEnd > dt;
                }
                DateTime start = new DateTime(dt.Year, DateStart.Value.Month, DateStart.Value.Day, DateStart.Value.Hour, DateStart.Value.Minute, DateStart.Value.Second);
                DateTime end = new DateTime(dt.Year, DateEnd.Value.Month, DateEnd.Value.Day, DateEnd.Value.Hour, DateEnd.Value.Minute, DateEnd.Value.Second);
                if (end < start)
                {
                    end = end.AddYears(1);
                    if (dt < start)
                    {
                        dt.AddYears(1);
                    }
                }
                return dt >= start && dt <= end;
        }
        return true;
    }

    public bool DeleteAfterInterval { get; set; }

    public IEnumerable<File> GetFiles()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Directory) || !System.IO.Directory.Exists(Directory))
            {
                return Enumerable.Empty<File>();
            }
            var files = System.IO.Directory.GetFiles(Directory, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.StartsWith(Path.Combine(Directory, "Papierkorb")))
                .Where(f => !new FileInfo(f).Attributes.HasFlag(FileAttributes.Hidden));
            return
                files
                .Select(x => new File(x)
                {
                    Duration = PlayType == PlayType.Folder ? Duration / (double)files.Count() : Duration,
                    ShowFullLength = VideosFullLength,
                    Stretch = Stretch,
                    IsPrio = HasPrio,
                });
        }
        catch (Exception)
        {
            return new List<File>();
        }
    }

    private ICommand? _selectPathCommand;
    [JsonIgnore]
    public ICommand SelectPathCommand => _selectPathCommand ??= ReactiveCommand.Create(() => OnSelectPathCommand_Invoked?.Invoke());

    private ICommand? _selectTimeSpanCommand;
    [JsonIgnore]
    public ICommand SelectTimeSpanCommand => _selectTimeSpanCommand ??= ReactiveCommand.Create(() => OnSelectTimeSpanCommand_Invoked?.Invoke());

    public bool Stretch { get; set; }
}

public class Settings
{
    public Theme Theme { get; set; }
    public int Height { get; set; } = 500;
    public int Width { get; set; } = 500;
    public bool TopMost { get; set; }
    public string? BannerPath { get; set; }
    private int _bannerSpeed = 20;
    public int BannerSpeed { get => GlobalConfig.GetBetween(_bannerSpeed, 1, 100); set => _bannerSpeed = value; }
    private int _bannerTextSize = 20;
    public int BannerTextSize { get => GlobalConfig.GetBetween(_bannerTextSize, 1, 100); set => _bannerTextSize = value; }
    public WeatherSettings WeatherApi { get; set; } = new();
    private int _visibleDays = 5;
    public int VisibleDays { get => GlobalConfig.GetBetween(_visibleDays, 1, 5); set => _visibleDays = value; }
    private int _weatherDuration = 10;
    public int WeatherDuration { get => GlobalConfig.GetBetween(_weatherDuration, 10, 60); set => _weatherDuration = value; }
    private int _placeLetterSize = 12;
    public int PlaceLetterSize { get => GlobalConfig.GetBetween(_placeLetterSize, 10, 30); set => _placeLetterSize = value; }
    public bool FullScreen { get; set; }
    public bool StartDiashowAtProgramStart { get; set; }

    public bool ShowWeather { get; set; }
    public bool ShowNewsticker { get; set; }
    public bool OrderFoldersRandom { get; set; }
    private int _newstickerFrames = 200;
    public int NewstickerFrames { get => GlobalConfig.GetBetween(_newstickerFrames, 100, 400); set => _newstickerFrames = value; }
    public string? SelectedMonitorName { get; set; }
}

public class WeatherSettings
{
    public string? ApiKey { get; set; }
    public string? Location { get; set; }
}
