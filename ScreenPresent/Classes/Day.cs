using ScreenPresent.Enums;

namespace ScreenPresent.Classes;
public class Day
{
    public Weekly Week { get; set; }
    public string Value => Week.GetDescription();
    public bool IsChecked { get; set; }
}

