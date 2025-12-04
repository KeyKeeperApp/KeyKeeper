using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public class LockedRepositoryViewModel : ViewModelBase
{
    private IPassStore passStore;

    public LockedRepositoryViewModel(IPassStore store)
    {
        passStore = store;
    }
}