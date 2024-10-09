using ReactiveUI;
using ScreenPresent.Classes;
using ScreenPresent.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ScreenPresent.ViewModels;

public class SelectTimeSpanViewModel : ReactiveObject
{
    public SelectTimeSpanViewModel(Classes.SelectTimeSpanConstructor constructor)
    {
        Days = Enum.GetValues<Weekly>().Select(x => new Day()
        {
            IsChecked = constructor.Days.Contains(x),
            Week = x
        }).ToList();
        SelectedTimeType = constructor.TimeTyp;
        DateStart = constructor.DateStart;
        DateEnd = constructor.DateEnd;
        ShowEveryInterval = constructor.EveryInterval;
        DeleteAfterInterval = constructor.DeleteAfterInterval;
    }

    public List<TimeType> TimeTypes { get; } = Enum.GetValues<TimeType>().ToList();

    private TimeType _selectedTimeType;
    public TimeType SelectedTimeType
    {
        get => _selectedTimeType;
        set
        {
            if (_selectedTimeType != value)
            {
                _selectedTimeType = value;
                this.RaisePropertyChanged(nameof(IsWeekly));
                this.RaisePropertyChanged(nameof(IsYearly));
                this.RaisePropertyChanged(nameof(ShowEveryIntervalText));
                this.RaisePropertyChanged(nameof(ShowCheckbox));
                this.RaisePropertyChanged(nameof(ErrorText));
            }
        }
    }

    public bool IsWeekly => SelectedTimeType == TimeType.Weekly;
    public bool IsYearly => SelectedTimeType == TimeType.TimeSpan;

    public List<Day> Days { get; }

    private DateTimeOffset? _dateEnd;
    public DateTimeOffset? DateEnd
    {
        get => _dateEnd;
        set
        {
            this.RaiseAndSetIfChanged(ref _dateEnd, value);
            this.RaisePropertyChanged(nameof(ErrorText));
        }
    }
    private DateTimeOffset? _dateStart;
    public DateTimeOffset? DateStart
    {
        get => _dateStart;
        set
        {
            this.RaiseAndSetIfChanged(ref _dateStart, value);
            this.RaisePropertyChanged(nameof(ErrorText));
        }
    }

    private bool _showEveryInterval;
    public bool ShowEveryInterval
    {
        get => _showEveryInterval;
        set
        {
            if (_showEveryInterval != value)
            {
                _showEveryInterval = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(CanDeleteAfterInterval));
                if (value)
                {
                    DeleteAfterInterval = false;
                }
            }
        }
    }
    public bool CanDeleteAfterInterval => !ShowEveryInterval;
    private bool _deleteAfterInterval;
    public bool DeleteAfterInterval
    {
        get => _deleteAfterInterval;
        set => this.RaiseAndSetIfChanged(ref _deleteAfterInterval, value);
    }
    public bool ShowCheckbox => SelectedTimeType == TimeType.TimeSpan;
    public string? ErrorText => SelectedTimeType == TimeType.TimeSpan && DateEnd < DateStart 
        ? "Das ausgewählte Ende Datum ist kleiner als das Start Datum." 
        : null;

    public event Action<SelectTimeSpanConstructor>? OnAccept;
    #region accept command
    private ICommand? _acceptCommand;
    public ICommand AcceptCommand => _acceptCommand ??= ReactiveCommand.Create(Accept);
    public void Accept()
    {
        if (!string.IsNullOrEmpty(ErrorText))
        {
            return;
        }
        OnAccept?.Invoke(new SelectTimeSpanConstructor(Days.Where(x => x.IsChecked).Select(x => x.Week).ToList())
        {
            DateEnd = DateEnd?.Date,
            DateStart = DateStart?.Date,
            TimeTyp = SelectedTimeType,
            EveryInterval = ShowEveryInterval,
            DeleteAfterInterval = DeleteAfterInterval,
        });
    }
    #endregion

    public string ShowEveryIntervalText
    {
        get
        {
            return SelectedTimeType switch
            {
                TimeType.TimeSpan => "Jedes Jahr wiederholen",
                _ => string.Empty,
            };
        }
    }
}
