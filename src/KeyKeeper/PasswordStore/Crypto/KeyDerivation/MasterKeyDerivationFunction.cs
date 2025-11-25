namespace KeyKeeper.PasswordStore.Crypto;

public abstract class MasterKeyDerivationFunction
{
    public abstract byte[] Derive(CompositeKey source, int keySizeBytes);
}
