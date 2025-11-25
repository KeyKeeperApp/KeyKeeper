using System.Security.Cryptography;
using KeyKeeper.PasswordStore.Crypto.KeyDerivation;

namespace KeyKeeper.PasswordStore;

static class FileFormatConstants
{
    public const int MIN_MASTER_SALT_LEN = 8;
    public const int MAX_MASTER_SALT_LEN = 40;
    public const int MIN_AESKDF_ROUNDS = AesKdf.MIN_ROUNDS;
    public const int MAX_AESKDF_ROUNDS = AesKdf.MAX_ROUNDS;
    public const byte ENCRYPT_ALGO_AES = 14;
    public const byte KDF_TYPE_AESKDF = 195;
    public const int HMAC_SIZE = HMACSHA3_512.HashSizeInBytes;
}