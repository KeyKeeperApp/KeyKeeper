using System;
using KeyKeeper.PasswordStore.Crypto;

namespace KeyKeeper.PasswordStore;

public interface IPassStore
{
    bool Locked { get; }

    PassStoreEntryGroup GetRootDirectory();
    PassStoreEntryGroup? GetGroupByType(byte groupType);
    public PassStoreEntry GetEntryById(Guid id);
    int GetTotalEntryCount();
    void Unlock(CompositeKey key);
    void Lock();
    void Save();

    bool DeleteEntry(PassStoreEntryGroup? group, Guid id);
    void AddEntry(PassStoreEntryGroup group, PassStoreEntry entry);
    void UpdateEntry(PassStoreEntryGroup? group, Guid id, PassStoreEntry entry);
}
