using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ScreenPresent.Classes;
using ScreenPresent.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScreenPresent.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        _viewModel = null!;
    }

    public MainWindow(SettingsViewModel settings)
    {
        InitializeComponent();
        Title = $"{Title} {ScreenPresent.Classes.Version.GetVersion()}";

        List<Screen> screens = Screens.All.ToList();

        settings.SetMonitors(GetMonitors());

        DataContext = _viewModel = new MainViewModel(settings);

        ResetRowsCommandHandling();
        _viewModel.ObservableForProperty(x => x.DisplayPaths).Subscribe((_) => ResetRowsCommandHandling());
    }

    private Dictionary<string, Screen> GetMonitors()
    {
        List<Screen> screens = [.. Screens.All];

        return screens.ToDictionary(x => x.IsPrimary
            ? $"Hauptbildschirm{(string.IsNullOrWhiteSpace(x.DisplayName) ? "" : $" ({x.DisplayName})")}"
            : $"{screens.Where(y => !y.IsPrimary).ToList().IndexOf(x) + 2}. Bildschirm{(string.IsNullOrWhiteSpace(x.DisplayName) ? "" : $" ({x.DisplayName})")}", x => x); ;
    }

    private void ResetRowsCommandHandling()
    {
        _viewModel.Paths.ForEach(x => x.OnSelectPathCommand_Invoked = async () => await HandleSelectPath(x));
        _viewModel.Paths.ForEach(x => x.OnSelectTimeSpanCommand_Invoked = async () => await HandleSelectTimeSpan(x));
    }

    private void DataGrid_LoadingRow(object sender, Avalonia.Controls.DataGridRowEventArgs e)
    {
        e.Row.Header = (e.Row.GetIndex() + 1).ToString();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        GlobalConfig.SaveChanges();
        if (!_viewModel.ImageWindow.IsVisible)
        {
            _viewModel.ImageWindow.Close();
            Environment.Exit(0);
        }
    }

    private void OnBtnSettingsClick(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new(_viewModel.Settings);
        settingsWindow.Show(this);
    }

    private async Task HandleSelectPath(DisplayPath path)
    {
        IReadOnlyList<IStorageFolder> result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
        if (result.Count == 1)
        {
            path.Directory = result[0].Path.LocalPath;
        }
    }

    private async Task HandleSelectTimeSpan(DisplayPath path)
    {
        SelectTimeSpanWindow selectTimeSpanWindow = new SelectTimeSpanWindow(new SelectTimeSpanConstructor(path.Days ?? new List<Enums.Weekly>())
        {
            DateStart = path.DateStart,
            DateEnd = path.DateEnd,
            TimeTyp = path.TimeType,
            EveryInterval = path.EveryInterval,
            DeleteAfterInterval = path.DeleteAfterInterval,
        });


        void ViewModel_OnAccept(SelectTimeSpanConstructor obj)
        {
            path.DateStart = obj.DateStart;
            path.DateEnd = obj.DateEnd;
            path.Days = obj.Days;
            path.TimeType = obj.TimeTyp;
            path.EveryInterval = obj.EveryInterval;
            path.DeleteAfterInterval = obj.DeleteAfterInterval;
        }

        selectTimeSpanWindow.ViewModel.OnAccept += ViewModel_OnAccept;
        await selectTimeSpanWindow.ShowDialog(this);
        selectTimeSpanWindow.ViewModel.OnAccept -= ViewModel_OnAccept;
    }
}