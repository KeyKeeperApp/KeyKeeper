using System;

namespace KeyKeeper.PasswordStore;

interface IPassStoreEntry
{
    string Name { get; set; }
    PassStoreEntryType Type { get; set; }
    DateTime CreationDate { get; }
}