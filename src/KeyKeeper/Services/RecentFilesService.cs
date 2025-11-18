using System;
using System.Collections.ObjectModel;
using System.Linq;
using KeyKeeper.Models;

namespace KeyKeeper.Services;

internal class RecentFilesService : IRecentFilesService
{
    // files are stored in reverse chronological order
    public ObservableCollection<RecentFile> RecentFiles { get; }
    private readonly int maxEntries = 8;

    public RecentFilesService()
    {
        RecentFiles = new ObservableCollection<RecentFile>();
    }

    public void Remember(string filename)
    {
        RemoveIfExists(filename);
        RecentFiles.Insert(0, new RecentFile(filename));
        if (RecentFiles.Count > maxEntries)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }

    public void Forget(string filename)
    {
        RemoveIfExists(filename);
    }

    public void ForgetAll()
    {
        RecentFiles.Clear();
    }

    public void RemoveIfExists(string filename)
    {
        for (int i = 0; i < RecentFiles.Count; i++)
        {
            if (RecentFiles[i].Path == filename)
            {
                RecentFiles.RemoveAt(i);
                break;
            }
        }
    }
}