using System;
using System.Security.Cryptography;

namespace KeyKeeper.PasswordStore.Crypto.KeyDerivation;

public class AesKdf : MasterKeyDerivationFunction
{
    public const int MIN_ROUNDS = 10;
    public const int MAX_ROUNDS = 25_000_000;
    public const int SEED_LENGTH = 32;

    private int rounds;
    private byte[] seed;

    public AesKdf(int rounds, byte[] seed)
    {
        if (rounds < MIN_ROUNDS || rounds > MAX_ROUNDS)
            throw new ArgumentOutOfRangeException(nameof(rounds));
        if (seed.Length != SEED_LENGTH)
            throw new ArgumentException("seed length must be " + SEED_LENGTH);
        this.rounds = rounds;
        this.seed = seed;
    }

    public override byte[] Derive(CompositeKey source, int keySizeBytes)
    {
        if (keySizeBytes > SEED_LENGTH)
            throw new ArgumentOutOfRangeException(nameof(keySizeBytes));

        byte[] key = source.Hash()[..SEED_LENGTH];
        byte[] nextKey = new byte[SEED_LENGTH];
        Aes cipher = Aes.Create();
        cipher.KeySize = SEED_LENGTH * 8;
        for (int i = 0; i < rounds; ++i)
        {
            cipher.Key = key;
            cipher.EncryptEcb(seed, nextKey, PaddingMode.None);
            (nextKey, key) = (key, nextKey);
        }
        return key;
    }
}
