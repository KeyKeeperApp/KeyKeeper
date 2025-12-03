using System.Collections.Generic;

namespace KeyKeeper.PasswordStore;

public interface IPassStoreDirectory : IEnumerable<PassStoreEntry>
{
}
