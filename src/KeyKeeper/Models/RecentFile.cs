using System;

namespace KeyKeeper.Models;

public struct RecentFile
{
    public string Path { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; }

    public string DisplayPath => Path;

    public RecentFile(string path, DateTime lastOpened)
    {
        Path = path;
        LastOpened = lastOpened;
    }

    public RecentFile(string path): this(path, DateTime.Now)
    {}

    public override string ToString()
    {
        return DisplayPath;
    }
}