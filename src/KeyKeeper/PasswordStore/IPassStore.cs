using KeyKeeper.PasswordStore.Crypto;

namespace KeyKeeper.PasswordStore;

public interface IPassStore
{
    bool Locked { get; }

    IPassStoreDirectory GetRootDirectory();
    int GetTotalEntryCount();
    void Unlock(CompositeKey key);
    void Lock();
}
