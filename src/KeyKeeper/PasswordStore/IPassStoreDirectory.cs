using System.Collections.Generic;

namespace KeyKeeper.PasswordStore;

interface IPassStoreDirectory : IEnumerable<IPassStoreEntry>
{
}