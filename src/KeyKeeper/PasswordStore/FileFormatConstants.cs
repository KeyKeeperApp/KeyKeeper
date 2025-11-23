using System.Security.Cryptography;

namespace KeyKeeper.PasswordStore;

static class FileFormatConstants
{
    public const int MIN_MASTER_SALT_LEN = 8;
    public const int MAX_MASTER_SALT_LEN = 40;
    public const int MIN_AESKDF_ROUNDS = 10;
    public const int MAX_AESKDF_ROUNDS = 65536;
    public const byte ENCRYPT_ALGO_AES = 14;
    public const byte KDF_TYPE_AESKDF = 195;
    public const int HMAC_SIZE = HMACSHA3_512.HashSizeInBytes;
}