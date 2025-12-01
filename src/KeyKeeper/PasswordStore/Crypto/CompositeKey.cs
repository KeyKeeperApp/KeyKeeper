using System;
using System.Security.Cryptography;
using System.Text;

namespace KeyKeeper.PasswordStore.Crypto;

public class CompositeKey
{
    public string Password { get; }
    public byte[] Salt { get; }

    public CompositeKey(string password, byte[] salt)
    {
        if (password == null)
            throw new ArgumentNullException("password");
        Password = password;

        if (salt == null || salt.Length < FileFormatConstants.MIN_MASTER_SALT_LEN ||
            salt.Length > FileFormatConstants.MAX_MASTER_SALT_LEN)
            throw new ArgumentException("salt");
        Salt = salt;
    }

    public byte[] Hash()
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(Password);
        byte[] hashedString = new byte[passwordBytes.Length + Salt.Length * 2];
        Salt.CopyTo(hashedString, 0);
        passwordBytes.CopyTo(hashedString, Salt.Length);
        Salt.CopyTo(hashedString, Salt.Length + passwordBytes.Length);

        return SHA256.HashData(hashedString);
    }
}
