using System;

namespace KeyKeeper.PasswordStore;

public interface IPassStoreEntry
{
    string Name { get; set; }
    PassStoreEntryType Type { get; set; }
    DateTime CreationDate { get; }
}
