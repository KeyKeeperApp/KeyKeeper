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
    private string password = "";

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
    public async Task TryUnlock()
    {
        try
        {
            passStore.Unlock(new CompositeKey(UnlockPassword, null));
            parent.UpdateLockStatus();
        } catch (PassStoreFileException e)
        {
            Console.WriteLine("pass store file exception: " + e.Message);
            if (e.Message == PassStoreFileException.ContentHMACMismatch.Message ||
                e.Message == PassStoreFileException.InvalidBeginMarker.Message)
            {
                await parent.ShowErrorPopup("Incorrect password or corrupted file");
            } else if (e.Message == PassStoreFileException.UnexpectedEndOfFile.Message ||
                       e.Message == PassStoreFileException.IncorrectMagicNumber.Message ||
                       e.Message == PassStoreFileException.InvalidCryptoHeader.Message ||
                       e.Message == PassStoreFileException.InvalidPassStoreEntry.Message)
            {
                await parent.ShowErrorPopup("Corrupted file");
            } else if (e.Message == PassStoreFileException.UnsupportedVersion.Message)
            {
                await parent.ShowErrorPopup("Unsupported store file version");
            } else
            {
                await parent.ShowErrorPopup("Unknown password store unlock error");
            }
        } catch (Exception e)
        {
            Console.WriteLine(e);
            await parent.ShowErrorPopup("Cannot open the password store file");
        }
    }
}