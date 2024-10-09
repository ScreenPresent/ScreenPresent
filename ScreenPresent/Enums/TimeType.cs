using System.ComponentModel;

namespace ScreenPresent.Enums; 
public enum TimeType {
    [Description("Immer")]
    None = 0,
    /// <summary>
    /// User can select multiple <see cref="Weekly"/> values.
    /// </summary>
    [Description("Wochentage")]
    Weekly = 1,
    /// <summary>
    /// User can select a date from and a date to.
    /// </summary>
    [Description("Zeitraum")]
    TimeSpan = 2,
}

public enum Weekly {
    [Description("Montag")]
    Monday = 1,
    [Description("Dienstag")]
    Tuesday = 2,
    [Description("Mittwoch")]
    Wednesday = 3,
    [Description("Donnerstag")]
    Thursday = 4,
    [Description("Freitag")]
    Friday = 5,
    [Description("Samstag")]
    Saturday = 6,
    [Description("Sonntag")]
    Sunday = 7,
}
