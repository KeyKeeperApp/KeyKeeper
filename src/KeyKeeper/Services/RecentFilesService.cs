using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using KeyKeeper.Models;

namespace KeyKeeper.Services;

internal class RecentFilesService : IRecentFilesService
{
    private const string RecentFilesFilename = "recent-files.json";
    private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

    // files are stored in reverse chronological order
    public ObservableCollection<RecentFile> RecentFiles { get; }
    private readonly int maxEntries = 8;
    private readonly string recentFilesPath;

    public RecentFilesService()
    {
        RecentFiles = new ObservableCollection<RecentFile>();
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KeyKeeper");
        recentFilesPath = Path.Combine(appDataDirectory, RecentFilesFilename);
    }

    public void Load()
    {
        RecentFiles.Clear();

        if (!File.Exists(recentFilesPath))
        {
            return;
        }

        try
        {
            var content = File.ReadAllText(recentFilesPath);
            var loadedFiles = JsonSerializer.Deserialize<List<RecentFile>>(content) ?? new List<RecentFile>();

            foreach (var recentFile in loadedFiles
                         .OrderByDescending(file => file.LastOpened)
                         .Take(maxEntries))
            {
                RecentFiles.Add(recentFile);
            }
        }
        catch
        {
            // ignore broken data and continue with empty recent files
        }
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(recentFilesPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var payload = JsonSerializer.Serialize(RecentFiles, jsonOptions);
        File.WriteAllText(recentFilesPath, payload);
    }

    public void Remember(string filename)
    {
        RemoveIfExists(filename);
        RecentFiles.Insert(0, new RecentFile(filename));
        if (RecentFiles.Count > maxEntries)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
        Save();
    }

    public void Forget(string filename)
    {
        RemoveIfExists(filename);
        Save();
    }

    public void ForgetAll()
    {
        RecentFiles.Clear();
        Save();
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