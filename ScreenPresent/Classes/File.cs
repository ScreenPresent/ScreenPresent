using System.Collections.Generic;
using System.Linq;

namespace ScreenPresent.Classes; 
public class File(string filename)
{
    public string Filename { get; } = filename;
    public double Duration { get; set; }
    public bool ShowFullLength { get; set; }
    public bool Stretch { get; set; }
    public bool IsPrio { get; set; }

    private List<string> _videoEnds = new List<string>() {
        ".gif", ".avi", ".mp4", ".mov"
    };
    public bool IsVideo => _videoEnds.Any(x => Filename.EndsWith(x, System.StringComparison.OrdinalIgnoreCase));
}
