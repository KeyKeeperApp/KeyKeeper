using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public partial class RepositoryWindowViewModel : ViewModelBase
{
    private object currentPage;
    private IPassStore passStore;

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
        CurrentPage = new UnlockedRepositoryViewModel(passStore);
    }

    private void SwitchToLocked()
    {
        CurrentPage = new LockedRepositoryViewModel(passStore, this);
    }
}