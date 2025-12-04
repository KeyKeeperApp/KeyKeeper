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
    public const uint FILE_FIELD_BEGIN = 0x7853dbd5;
    public const uint FILE_FIELD_INNER_CRYPTO = 0x613e91e4;
    public const uint FILE_FIELD_CONFIG = 0xd36a53c0;
    public const uint FILE_FIELD_STORE = 0x981f9bc8;
    public const uint FILE_FIELD_END = 0x010ba81a;
    public const byte ENTRY_PASS_ID = 0x00;
    public const byte ENTRY_GROUP_ID = 0x01;
    public const byte LOGIN_FIELD_PASSWORD_ID = 0x00;
    public const byte LOGIN_FIELD_USERNAME_ID = 0x01;
    public const byte LOGIN_FIELD_EMAIL_ID = 0x02;
    public const byte LOGIN_FIELD_ACCOUNT_NUMBER_ID = 0x03;
    public const byte LOGIN_FIELD_NOTES_ID = 0x04;
    public const byte LOGIN_FIELD_CUSTOM_ID = 0xff; // пока не используется
    public const byte GROUP_TYPE_ROOT = 0x00;
    public const byte GROUP_TYPE_DEFAULT = 0x01;
    public const byte GROUP_TYPE_FAVOURITES = 0x02;
    public const byte GROUP_TYPE_SIMPLE = 0x03;
    public const byte GROUP_TYPE_CUSTOM = 0xff; // пока не используется
    public static readonly byte[] BEGIN_MARKER = [0x5f, 0x4f, 0xcf, 0x67, 0xc0, 0x90, 0xd0, 0xe5];
}