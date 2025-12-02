using KeyKeeper.PasswordStore.Crypto;

namespace KeyKeeper.PasswordStore;

public record StoreCreationOptions
{
    public int LockTimeoutSeconds { get; init; }
    public CompositeKey Key { get; init; }
}