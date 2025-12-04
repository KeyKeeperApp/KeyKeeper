using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using KeyKeeper.PasswordStore;
using KeyKeeper.PasswordStore.Crypto;

namespace KeyKeeper.ViewModels;

public partial class LockedRepositoryViewModel : ViewModelBase
{
    RepositoryWindowViewModel parent;
    private IPassStore passStore;
    private string password;

    public LockedRepositoryViewModel(IPassStore store, RepositoryWindowViewModel parent)
    {
        passStore = store;
        this.parent = parent;
    }

    public string UnlockPassword
    {
        get => password;
        set { password = value; OnPropertyChanged(nameof(UnlockPassword)); }
    }

    [RelayCommand]
    public void TryUnlock()
    {
        try
        {
            passStore.Unlock(new CompositeKey(UnlockPassword, null));
            parent.UpdateLockStatus();
        } catch (PassStoreFileException e)
        {
            // TODO
            Console.WriteLine("pass store file exception: " + e.Message);
        } catch (Exception e)
        {
            // TODO
            Console.WriteLine(e);
        }
    }
}