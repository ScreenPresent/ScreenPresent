using System;
using System.Diagnostics;
using System.IO;

namespace ScreenPresent.Classes;
internal static class Version
{
    public static string? GetVersion()
    {
        return FileVersionInfo.GetVersionInfo(Path.Combine(AppContext.BaseDirectory, "ScreenPresent.exe")).FileVersion;
    }
}
