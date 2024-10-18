using Avalonia.Threading;
using ReactiveUI;
using ScreenPresent.Classes;
using ScreenPresent.Services.OpenWeatherService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace ScreenPresent.ViewModels;

public class MainViewModel : ReactiveObject
{
    public List<DisplayPath> Paths { get; set; }
    public List<DisplayPath> DisplayPaths
    {
        get => Paths.FindAll(path => path.Name?.Contains(SearchText ?? string.Empty, StringComparison.OrdinalIgnoreCase) ?? false);
        set => Paths = value;
    }

    private string? _updateText;
    public string? UpdateText
    {
        get => _updateText;
        set
        {
            if (_updateText != value)
            {
                _updateText = value;
                this.RaisePropertyChanged(nameof(UpdateText));
                this.RaisePropertyChanged(nameof(MayUpdate));
            }
        }
    }
    public bool MayUpdate => !string.IsNullOrEmpty(UpdateText);

    public Views.ImageWindow ImageWindow { get; set; }
    public SettingsViewModel Settings { get; set; }

    private DisplayPath? _selectedPath;
    public DisplayPath? SelectedPath
    {
        get => _selectedPath;
        set
        {
            if (_selectedPath != value)
            {
                _selectedPath = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(CanPathUpExecute));
                this.RaisePropertyChanged(nameof(CanPathDownExecute));
            }
        }
    }

    private string _searchText = null!;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(DisplayPaths));
            }
        }
    }

    private DateTime? _loadedDate;
    public async void LoadWeather()
    {
        try
        {
            if (string.IsNullOrEmpty(Settings.WeatherApiKey) || string.IsNullOrEmpty(Settings.WeatherLocation))
            {
                ImageWindow.ViewModel.Weather = null;
                return;
            }
            if (ImageWindow.ViewModel.Weather == null || !_loadedDate.HasValue || _loadedDate.Value <= DateTime.Now.AddHours(-1))
            {
                OpenWeatherMapService openWeatherMapService = new();
                ImageWindow.ViewModel.Weather = await openWeatherMapService.GetForecastAsync(Settings.WeatherApiKey, Settings.WeatherLocation);
                _loadedDate = DateTime.Now;
            }
        }
        catch
        {
            ImageWindow.ViewModel.Weather = null;
        }
    }

    private readonly System.Timers.Timer NextImageTimer = new(1000);
    private readonly string? _updaterPath;
    public MainViewModel(SettingsViewModel settings)
    {
        NextImageTimer.Elapsed += StartTimer;
        LoadFilesTimer.Elapsed += LoadAndDisplayFiles;

        Paths = GlobalConfig.JsonFile.Paths;
        Settings = settings;

        this.RaisePropertyChanged(nameof(DisplayPaths));

        Settings.RecreateNewsticker += Settings_RecreateNewsticker;
        Settings.StopNewsticker += Settings_StopNewsticker;
        ImageWindow = new Views.ImageWindow(Settings);
        ImageWindow.CloseWindow = () => StartPresentation(false);
        // This event is first fired before the image window exists and before the event is listening
        ImageWindow.ViewModel.MediaEnded += ImageWindow_MediaEnded;
        // This is for initiliezing the sizes of the texts
        ImageWindow_RefreshText();

        if (Settings.StartDiashowAtProgramStart)
        {
            StartPresentation(true);
        }

        CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        string? newestVersion = await Classes.Version.GetNewestVersionStringAsync();

        string? currentVersion = Classes.Version.GetCurrentVersion();

        if (!string.IsNullOrWhiteSpace(newestVersion) && !string.IsNullOrWhiteSpace(currentVersion) && newestVersion != currentVersion)
        {
            UpdateText = string.IsNullOrEmpty(newestVersion) ? null : $"Es ist ein Update auf Version {newestVersion} verfügbar.";
        }
    }

    private void Settings_StopNewsticker()
    {
        ImageWindow.StopNewsticker();
    }

    private void Settings_RecreateNewsticker()
    {
        if (Settings.ShowNewsticker && DiashowIsRunning)
        {
            ImageWindow.CreateBanner();
        }
    }

    private int _weatherDuration;
    public int WeatherDuration
    {
        get => _weatherDuration;
        set => this.RaiseAndSetIfChanged(ref _weatherDuration, value);
    }

    // TODO Refresh sometimes
    private void ImageWindow_RefreshText()
    {
        if (string.IsNullOrWhiteSpace(Settings.BannerPath) || !System.IO.Directory.Exists(Settings.BannerPath))
        {
            return;
        }
        List<string> banner = new();
        try
        {
            foreach (string file in System.IO.Directory.GetFiles(Settings.BannerPath)
                .Where(f => !new System.IO.FileInfo(f).Attributes.HasFlag(System.IO.FileAttributes.Hidden)))
            {
                banner.Add(string.Join(" +++ ", System.IO.File.ReadAllLines(file)));
            }
        }
        catch
        {
            return;
        }
        string text = $"{string.Join(" +++ ", banner)} +++".ToString();
        if (text != ImageWindow.ViewModel.GetText())
        {
            ImageWindow.ViewModel.Text = text;
            if (DiashowIsRunning)
            {
                ImageWindow.CreateBanner();
            }
        }
    }

    private bool LoadFiles()
    {
        List<DisplayPath> clearedPaths = Paths.Where(path => !path.CheckAndDelete(DateTime.Now)).ToList();
        if (!Paths.SequenceEqual(clearedPaths))
        {
            Paths = clearedPaths;
            this.RaisePropertyChanged(nameof(DisplayPaths));
        }

        if (Settings.ShowWeather && Settings.ShowOnlyWeather)
        {
            _files = new List<File>() {
                 new File(Constant.ConstantWeatherId.ToString()) {
                     Duration  = Settings.WeatherDuration
                  }
            };
            return true;
        }

        // load all paths, that should be displayd now and that exists
        IEnumerable<DisplayPath> paths = Paths.Where(x => x.IsInTime(DateTime.Now) && System.IO.Directory.Exists(x.Directory));

        if (Settings.OrderFoldersRandom)
        {
            paths = paths.OrderBy(x => Guid.NewGuid()).ToList();
        }

        // initilize files with only prio files
        List<File> files = paths.Where(x => x.HasPrio).SelectMany(x => x.GetFiles()).ToList();
        if (files.Any())
        {
            _files = files;
            return true;
        }
        else
        {
            // if there are no prio files, load all files
            _files = paths.SelectMany(x => x.GetFiles()).ToList();
            if (Settings.ShowWeather)
            {
                _files.Add(new File(Constant.ConstantWeatherId.ToString())
                {
                    Duration = Settings.WeatherDuration
                });
            }
        }
        return _files.Any() && DiashowIsRunning;
    }

    private List<File> _files = [];
    private int _currentFile = 0;
    public int CurrentFile
    {
        get => _currentFile;
        set
        {
            _currentFile = value;
            Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(CanDiashowNext));
                this.RaisePropertyChanged(nameof(CanDiashowBack));
            });
        }
    }

    private bool DisplayFile()
    {
        if (!DiashowIsRunning)
        {
            return true;// should be false, but the next file should not be displayed
        }
        File file = _files[CurrentFile];
        if (file.Filename == Constant.ConstantWeatherId.ToString())
        {
            return ShowWeather();
        }
        ImageWindow.ViewModel.ShowWeather = false;
        if (!System.IO.File.Exists(file.Filename))
        {
            _files.Remove(file);
            CurrentFile--;
            return false;
        }
        ImageWindow.ViewModel.SetImage(file);
        return true;
    }

    private bool ShowWeather()
    {
        LoadWeather();
        if (ImageWindow.ViewModel.Weather != null)
        {
            ImageWindow.ViewModel.ShowWeather = true;
            return true;
        }
        return false;
    }

    #region commands

    #region paths

    #region add path
    private ICommand? _addPathCommand;
    public ICommand AddPathCommand => _addPathCommand ??= ReactiveCommand.Create(AddPath);
    public void AddPath()
    {
        Paths.Add(new DisplayPath()
        {
            Name = "Neu"
        });
        this.RaisePropertyChanged(nameof(DisplayPaths));
    }
    #endregion

    #region remove path
    private ICommand? _removePathCommand;
    public ICommand RemovePathCommand => _removePathCommand ??= ReactiveCommand.Create(RemovePath, this.WhenAnyValue(x => x.SelectedPath, selector: (selectedPath) => selectedPath != null));
    public void RemovePath()
    {
        ArgumentNullException.ThrowIfNull(SelectedPath);

        Paths.Remove(SelectedPath);
        this.RaisePropertyChanged(nameof(DisplayPaths));
    }
    #endregion

    #region path up
    private ICommand? _pathUpCommand;
    public ICommand PathUpCommand => _pathUpCommand ??= ReactiveCommand.Create(PathUp, this.WhenAnyValue(x => x.CanPathUpExecute, selector: (canPathUpExecute) => canPathUpExecute));
    public bool CanPathUpExecute => SelectedPath != null && SelectedPath != Paths.FirstOrDefault();
    public void PathUp()
    {
        MovePath(-1);
    }
    #endregion

    #region path down
    private ICommand? _pathDownCommand;
    public ICommand PathDownCommand => _pathDownCommand ??= ReactiveCommand.Create(PathDown, this.WhenAnyValue(x => x.CanPathDownExecute, selector: (canPathDownExecute) => canPathDownExecute));
    public bool CanPathDownExecute => SelectedPath != null && SelectedPath != Paths.LastOrDefault();

    public void PathDown()
    {
        MovePath(1);
    }
    #endregion

    private void MovePath(int indexChange)
    {
        ArgumentNullException.ThrowIfNull(SelectedPath);

        List<DisplayPath> paths = Paths.ToList();
        paths.Remove(SelectedPath);
        int newIndex = Paths.IndexOf(SelectedPath) + indexChange;
        paths.Insert(newIndex, SelectedPath);
        DisplayPaths = paths.ToList();
        this.RaisePropertyChanged(nameof(DisplayPaths));
        SelectedPath = Paths[newIndex];
    }

    #endregion

    #region Diashow
    #region Start
    private ICommand? _DiashowStartCommand;
    public ICommand DiashowStartCommand => _DiashowStartCommand ??= ReactiveCommand.Create(new Action<string>((x) => StartPresentation(bool.Parse(x))));

    public bool CanDiashowStart => !DiashowIsRunning;
    public string DiashowStartText => DiashowIsRunning ? "Stop" : "Start";
    private bool _diashowIsRunning;
    public bool DiashowIsRunning
    {
        get => _diashowIsRunning;
        set
        {
            this.RaiseAndSetIfChanged(ref _diashowIsRunning, value);
            DiashowPaused = false;
            this.RaisePropertyChanged(nameof(CanDiashowPause));
            this.RaisePropertyChanged(nameof(CanDiashowStart));
            this.RaisePropertyChanged(nameof(DiashowStartText));
            this.RaisePropertyChanged(nameof(CanDiashowNext));
            this.RaisePropertyChanged(nameof(CanDiashowBack));
        }
    }
    #endregion

    #region Next
    private ICommand? _diashowNextCommand;
    public ICommand DiashowNextCommand => _diashowNextCommand ??= ReactiveCommand.Create(DiashowNext, this.WhenAnyValue(x => x.CanDiashowNext, selector: canDiashowNext => canDiashowNext));
    public bool CanDiashowNext => DiashowIsRunning && CurrentFile < _files?.Count;

    public void DiashowNext()
    {
        NextImageTimer.Stop();
        StartTimer(this, null);
    }
    #endregion

    #region Back
    private ICommand? _diashowBackCommand;
    public ICommand DiashowBackCommand => _diashowBackCommand ??= ReactiveCommand.Create(DiashowBack, this.WhenAnyValue(x => x.CanDiashowBack, selector: canDiashowBack => canDiashowBack));
    public bool CanDiashowBack => DiashowIsRunning && CurrentFile > 1;

    public void DiashowBack()
    {
        NextImageTimer.Stop();
        CurrentFile -= 2;
        StartTimer(this, null);
    }
    #endregion

    #region Pause
    private ICommand? _diashowPauseCommand;
    public ICommand DiashowPauseCommand => _diashowPauseCommand ??= ReactiveCommand.Create(() => DiashowPause(), this.WhenAnyValue(x => x.CanDiashowPause, selector: canDiashowPause => canDiashowPause));
    public bool CanDiashowPause => DiashowIsRunning;

    public string DiashowPauseText => DiashowPaused ? "Fortsetzen" : "Pause";
    private bool _diashowPaused = false;
    public bool DiashowPaused
    {
        get => _diashowPaused;
        set
        {
            this.RaiseAndSetIfChanged(ref _diashowPaused, value);
            this.RaisePropertyChanged(nameof(DiashowPauseText));
        }
    }

    public void DiashowPause()
    {
        if (DiashowPaused)
        {
            StartTimer(this, null);
        }
        else
        {
            NextImageTimer.Stop();
        }
        DiashowPaused = !DiashowPaused;
    }
    #endregion

    #region Update
    private ICommand? _updateCommand;
    public ICommand UpdateCommand => _updateCommand ??= ReactiveCommand.Create(Update, this.WhenAnyValue(x => x.UpdateIsRunning, selector: x => !x));
    public bool UpdateIsRunning { get; set; }

    public void Update()
    {
        if (string.IsNullOrEmpty(_updaterPath) || !System.IO.File.Exists(_updaterPath))
        {
            return;
        }
        UpdateIsRunning = true;
        this.RaisePropertyChanged(nameof(UpdateIsRunning));
        try
        {
            Process.Start(_updaterPath);
            Environment.Exit(0);
        }
        finally
        {
            UpdateIsRunning = false;
            this.RaisePropertyChanged(nameof(UpdateIsRunning));
        }
    }
    #endregion

    #endregion

    #endregion
    private bool _keepRunning;
    /// <summary>
    /// If the newsticker should be active, this will start the newsticker. Starts the diashow.
    /// </summary>
    /// <param name="keepRunning">If the diashow should be run in a loop or only once</param>
    private void StartPresentation(bool keepRunning)
    {
        if (!DiashowIsRunning)
        {
            _keepRunning = keepRunning;
            DiashowIsRunning = true;
            ImageWindow.Show();
            LoadAndDisplayFiles();
            ImageWindow.CreateBanner();
            // fix fullscreen
            Settings.AllowFullScreen = true;
        }
        else
        {
            Settings_StopNewsticker();
            ImageWindow.Hide();
            NextImageTimer.Stop();
            ImageWindow.ViewModel.ImageSource = null;
            ImageWindow.ViewModel.StopMediaIfIsPlaying();
            CurrentFile = 0;
            DiashowIsRunning = false;
        }
    }

    private readonly Timer LoadFilesTimer = new(2000);
    private bool _loadFiles = false;
    private void LoadAndDisplayFiles(object? sender, ElapsedEventArgs e)
    {
        if (_loadFiles)
        {
            return;
        }
        _loadFiles = true;
        LoadAndDisplayFiles();
        _loadFiles = false;
    }

    private void LoadAndDisplayFiles()
    {
        if (LoadFiles())
        {
            LoadFilesTimer.Stop();
            CurrentFile = 0;
            StartTimer(null, null);
        }
        else
        {
            LoadFilesTimer.Start();
        }
    }

    private bool isRunning;
    private void StartTimer(object? sender, ElapsedEventArgs? e)
    {
        if (isRunning)
        {
            return;
        }
        isRunning = true;
        if (_files.Count <= CurrentFile)
        {
            // Alle Dateien wurden einmal angezeigt.
            // Dateien neu laden.
            if (!_keepRunning)
            {
                ImageWindow.ViewModel.ImageSource = null;
                ImageWindow.ViewModel.StopMediaIfIsPlaying();
                return;
            }
            NextImageTimer.Stop();
            isRunning = false;
            LoadFilesTimer.Start();
            return;
        }
        NextImageTimer.Stop();
        // Naechste Datei auswaehlen.
        File file = _files[CurrentFile];
        if (file.Filename == Constant.ConstantWeatherId.ToString() && Settings.ShowWeather && Settings.ShowOnlyWeather && file.Duration == 0)
        {
            // Falls das Weter angezeigt wird und dort keine Zeit hinterlegt ist, 10 Sekunden nehmen.
            file.Duration = 10;
        }
        else if (Settings.ShowWeather && Settings.ShowOnlyWeather)
        {
            // Falls nur das Weter angezeigt werden soll.
            ShowWeather();
            isRunning = false;
            // refresh all 10 seconds
            NextImageTimer.Interval = 10000;
            NextImageTimer.Start();
            return;
        }
        if (file.Duration <= 0)
        {
            // Falls keine Zeit eingetragen ist, direkt zur naechsten Datei gehen.
            CurrentFile++;
            isRunning = false;
            StartTimer(sender, e);
            return;
        }

        // if the current file is not a prio file, check if there is any prio file.
        // if yes, stop showing current run.
        bool anyPrioFile = !file.IsPrio
            && Paths.Where(x => x.HasPrio
                && x.IsInTime(DateTime.Now)
                && System.IO.Directory.Exists(x.Directory))
            .SelectMany(x => x.GetFiles())
            .Any();
        if (anyPrioFile)
        {
            CurrentFile = _files.Count;
            isRunning = false;
            StartTimer(sender, e);
            return;
        }

        // Datei anzeigen
        if (!DisplayFile())
        {
            // Falls die Datei entfernt wurde.
            CurrentFile++;
            isRunning = false;
            StartTimer(sender, e);
            return;
        }
        if (!file.IsVideo || !file.ShowFullLength)
        {
            // Falls kein Video in voller Laenge angezeigt werden soll.
            NextImageTimer.Interval = file.Duration * 1000;
            NextImageTimer.Start();
        }
        else
        {
            // Falls ein Video in voller Laenge angezeigt werden soll, Wait For End setzen.
            _waitForEnd = true;
        }
        CurrentFile++;
        isRunning = false;
    }

    private bool _waitForEnd;
    private void ImageWindow_MediaEnded()
    {
        if (_waitForEnd)
        {
            _waitForEnd = false;
            StartTimer(this, null);
        }
    }
}
