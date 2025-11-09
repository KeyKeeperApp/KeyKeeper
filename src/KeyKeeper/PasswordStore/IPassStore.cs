namespace KeyKeeper.PasswordStore;

interface IPassStore
{
    IPassStoreDirectory GetRootDirectory();
    int GetTotalEntryCount();
}