using ScreenPresent.Enums;
using System;
using System.Collections.Generic;

namespace ScreenPresent.Classes; 
public class SelectTimeSpanConstructor(List<Weekly> days)
{
    public TimeType TimeTyp { get; set; }
    public List<Weekly> Days { get; set; } = days;

    public DateTime? _dateStart;
    public DateTime? DateStart {
        get => _dateStart ?? DateTime.Now;
        set => _dateStart = value;
    }
    public DateTime? _dateEnd;
    public DateTime? DateEnd {
        get => _dateEnd ?? DateTime.Now;
        set => _dateEnd = value;
    }
    public bool EveryInterval { get; set; }
    public bool DeleteAfterInterval { get; set; }
}
