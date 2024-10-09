using System;

namespace ScreenPresent.Exceptions;
[Serializable]
internal class LocationNotFoundException : Exception
{
    public LocationNotFoundException() { }
    public LocationNotFoundException(string message) : base(message) { }
    public LocationNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}