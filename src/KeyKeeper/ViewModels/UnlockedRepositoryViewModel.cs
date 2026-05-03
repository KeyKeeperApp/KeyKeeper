using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Avalonia.Threading;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public class UnlockedRepositoryViewModel : ViewModelBase
{
    private IPassStore passStore;
    private IPassStoreDirectory currentDirectory;
    private PassStoreEntryGroup? rootDirectory;
    private bool hasUnsavedChanges;
    private DispatcherTimer? _totpRefreshTimer;
    private Dictionary<Guid, string> _totpCodes = new();

    public IEnumerable<PassStoreEntryPassword> Passwords
    {
        get
        {
            return currentDirectory
                .Where(entry => entry is PassStoreEntryPassword)
                .Select(entry => (entry as PassStoreEntryPassword)!);
        }
    }

    public IEnumerable<PassStoreEntryGroup> PasswordGroups
    {
        get
        {
            if (rootDirectory == null) return [];
            return rootDirectory
                .Where(entry => entry is PassStoreEntryGroup)
                .Select(entry => (entry as PassStoreEntryGroup)!);
        }
    }
    public PassStoreEntryGroup SelectedPasswordGroup
    {
        get
        {
            return PasswordGroups.First(group => group == currentDirectory);
        }
        set
        {
            if (PasswordGroups.Any(group => group == value))
            {
                ChangeDirectory(value);
            }
        }
    }

    public bool HasUnsavedChanges
    {
        get => hasUnsavedChanges;
        private set
        {
            hasUnsavedChanges = value;
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }

    public UnlockedRepositoryViewModel(IPassStore store, IPassStoreDirectory directory)
    {
        passStore = store;
        currentDirectory = directory;
        rootDirectory = (directory as PassStoreEntryGroup)?.Parent;
        HasUnsavedChanges = false;
        InitializeTotpCodes();
        StartTotpRefreshTimer();
    }

    /// <summary>
    /// Gets the current TOTP code for an entry, or empty string if TOTP not configured.
    /// </summary>
    public string GetTotpCode(PassStoreEntryPassword entry)
    {
        if (entry.Totp == null)
            return string.Empty;

        if (_totpCodes.TryGetValue(entry.Id, out var code))
            return code;

        return TotpCodeGenerator.GenerateCode(entry.Totp);
    }

    public void AddEntry(PassStoreEntry entry)
    {
        if (entry is PassStoreEntryPassword)
        {
            currentDirectory.AddEntry(entry);
            HasUnsavedChanges = true;
            OnPropertyChanged(nameof(Passwords));
        }
    }

    public void DeleteEntry(Guid id)
    {
        currentDirectory.DeleteEntry(id);
        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(Passwords));
    }

    public void UpdateEntry(PassStoreEntryPassword updatedEntry)
    {
        currentDirectory.UpdateEntry(updatedEntry.Id, updatedEntry);
        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(Passwords));
    }

    public void Save()
    {
        passStore.Save();
        HasUnsavedChanges = false;
    }

    private void ChangeDirectory(PassStoreEntryGroup newDir)
    {
        if (newDir == currentDirectory)
            return;

        currentDirectory = newDir;
        InitializeTotpCodes();
        StartTotpRefreshTimer();

        OnPropertyChanged(nameof(SelectedPasswordGroup));
        OnPropertyChanged(nameof(Passwords));
    }

    private void InitializeTotpCodes()
    {
        _totpCodes.Clear();
        foreach (var entry in Passwords.Where(e => e.Totp != null))
        {
            _totpCodes[entry.Id] = TotpCodeGenerator.GenerateCode(entry.Totp!);
        }
    }

    private void StartTotpRefreshTimer()
    {
        // Calculate time until next TOTP period boundary
        int secondsUntilNextCode = CalculateSecondsUntilNextTotpRefresh();

        if (_totpRefreshTimer != null)
        {
            _totpRefreshTimer.Stop();
        }
        _totpRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(secondsUntilNextCode)
        };
        _totpRefreshTimer.Tick += OnTotpRefreshTimerTick;
        _totpRefreshTimer.Start();
    }

    private void OnTotpRefreshTimerTick(object? sender, EventArgs e)
    {
        // Refresh all TOTP codes
        InitializeTotpCodes();
        OnPropertyChanged(nameof(Passwords)); // Trigger UI update

        // Calculate next refresh time and reschedule
        if (_totpRefreshTimer != null)
        {
            _totpRefreshTimer.Stop();
            int secondsUntilNextCode = CalculateSecondsUntilNextTotpRefresh();
            _totpRefreshTimer.Interval = TimeSpan.FromSeconds(secondsUntilNextCode);
            _totpRefreshTimer.Start();
        }
    }

    private int CalculateSecondsUntilNextTotpRefresh()
    {
        // Find the minimum seconds until next code change across all TOTP entries
        var totpEntries = Passwords.Where(e => e.Totp != null).ToList();
        if (totpEntries.Count == 0)
            return 60; // Default to 60 seconds if no TOTP entries

        // All periods should be the same, but use the minimum to be safe
        int minSeconds = totpEntries
            .Select(e => TotpCodeGenerator.GetSecondsUntilNextCode(e.Totp!))
            .Min();

        return Math.Max(1, minSeconds); // At least 1 second
    }

    public void StopTotpRefreshTimer()
    {
        if (_totpRefreshTimer != null)
        {
            _totpRefreshTimer.Stop();
            _totpRefreshTimer = null;
        }
    }
}
