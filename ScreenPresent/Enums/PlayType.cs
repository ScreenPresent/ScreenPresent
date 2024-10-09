using System.ComponentModel;

namespace ScreenPresent.Enums; 
public enum PlayType {
    [Description("Pro Datei")]
    PerFile = 0,
    [Description("Pro Ordner")]
    Folder = 1
}
