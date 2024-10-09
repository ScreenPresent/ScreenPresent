using ScreenPresent.Enums;
using System;
using System.Collections.Generic;

namespace ScreenPresent.ViewModels;
public static class StaticPlayTypes {
    public static IEnumerable<PlayType> PlayTypes { get; } = Enum.GetValues<PlayType>();
}
