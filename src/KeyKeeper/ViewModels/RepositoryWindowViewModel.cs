using System;
using System.Threading.Tasks;
using KeyKeeper.PasswordStore;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.ViewModels;

public partial class RepositoryWindowViewModel : ViewModelBase
{
    private object currentPage;
    private IPassStore passStore;

    public Func<string, Task> ShowErrorPopup;

    public object CurrentPage
    {
        get => currentPage;
        set { currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
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

    private void SwitchToUnlocked()
    {
        var directory = passStore.GetGroupByType(GROUP_TYPE_DEFAULT)
            ?? passStore.GetRootDirectory();
        CurrentPage = new UnlockedRepositoryViewModel(passStore, directory);
    }

    private void SwitchToLocked()
    {
        CurrentPage = new LockedRepositoryViewModel(passStore, this);
    }
}