using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public class RepositoryWindowViewModel : ViewModelBase
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

    private void UpdateLockStatus()
    {
        if ((currentPage == null || currentPage is LockedRepositoryViewModel) && !passStore.Locked)
            SwitchToUnlocked();
        else if ((currentPage == null || currentPage is UnlockedRepositoryViewModel) && passStore.Locked)
            SwitchToLocked();
    }

    private void SwitchToUnlocked()
    {
        currentPage = new UnlockedRepositoryViewModel(passStore);
    }

    private void SwitchToLocked()
    {
        currentPage = new LockedRepositoryViewModel(passStore);
    }
}