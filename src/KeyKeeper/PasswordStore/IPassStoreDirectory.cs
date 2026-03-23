using System;
using System.Collections.Generic;

namespace KeyKeeper.PasswordStore;

public interface IPassStoreDirectory : IEnumerable<PassStoreEntry>
{
    bool DeleteEntry(Guid id);
    void AddEntry(PassStoreEntry entry);
}
