using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using KeyKeeper.PasswordStore;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.ViewModels;

public partial class RepositoryWindowViewModel : ViewModelBase
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(5);

    private object currentPage;
    private IPassStore passStore;
    private DispatcherTimer? _lockTimer;
    private DateTime _timerStart;
    private string _lockTimerDisplay = string.Empty;

    public Func<string, Task> ShowErrorPopup;

    public object CurrentPage
    {
        get => currentPage;
        set { currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
    }

    public string LockTimerDisplay
    {
        get => _lockTimerDisplay;
        private set { _lockTimerDisplay = value; OnPropertyChanged(nameof(LockTimerDisplay)); }
    }

    public RepositoryWindowViewModel(IPassStore store)
    {
        passStore = store;
        UpdateLockStatus();
    }

    public void UpdateLockStatus()
    {
        if ((currentPage == null || currentPage is LockedRepositoryViewModel) && !passStore.Locked)
            SwitchToUnlocked();
        else if ((currentPage == null || currentPage is UnlockedRepositoryViewModel) && passStore.Locked)
            SwitchToLocked();
    }

    /// <summary>
    /// Сбрасывает таймер блокировки (вызывается при любой активности пользователя).
    /// </summary>
    public void ResetLockTimer()
    {
        if (_lockTimer != null && _lockTimer.IsEnabled)
            _timerStart = DateTime.UtcNow;
    }

    private void SwitchToUnlocked()
    {
        var directory = passStore.GetGroupByType(GROUP_TYPE_DEFAULT)
            ?? passStore.GetRootDirectory();
        CurrentPage = new UnlockedRepositoryViewModel(passStore, directory);
        StartLockTimer();
    }

    private void SwitchToLocked()
    {
        StopLockTimer();
        CurrentPage = new LockedRepositoryViewModel(passStore, this);
    }

    public void StartLockTimer()
    {
        StopLockTimer();
        _timerStart = DateTime.UtcNow;
        _lockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _lockTimer.Tick += OnLockTimerTick;
        _lockTimer.Start();
        UpdateTimerDisplay();
    }

    public void StopLockTimer()
    {
        if (_lockTimer != null)
        {
            _lockTimer.Tick -= OnLockTimerTick;
            _lockTimer.Stop();
            _lockTimer = null;
        }
        LockTimerDisplay = string.Empty;
    }

    private void OnLockTimerTick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.UtcNow - _timerStart;
        var remaining = LockTimeout - elapsed;

        if (remaining <= TimeSpan.Zero)
        {
            StopLockTimer();
            passStore.Lock();
            UpdateLockStatus();
            return;
        }

        UpdateTimerDisplay(remaining);
    }

    private void UpdateTimerDisplay(TimeSpan? remaining = null)
    {
        var r = remaining ?? LockTimeout;
        LockTimerDisplay = $"{r:mm\\:ss}";
    }
}
