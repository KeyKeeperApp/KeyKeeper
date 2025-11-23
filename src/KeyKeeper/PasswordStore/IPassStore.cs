namespace KeyKeeper.PasswordStore;

public interface IPassStore
{
    IPassStoreDirectory GetRootDirectory();
    int GetTotalEntryCount();
}
