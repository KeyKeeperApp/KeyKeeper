using KeyKeeper.PasswordStore;

namespace KeyKeeper.ViewModels;

public class UnlockedRepositoryViewModel : ViewModelBase
{
    private IPassStore passStore;

    public UnlockedRepositoryViewModel(IPassStore store)
    {
        passStore = store;
    }
}