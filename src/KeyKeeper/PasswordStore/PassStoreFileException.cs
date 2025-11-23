using System;

namespace KeyKeeper.PasswordStore;

public class PassStoreFileException : Exception
{
    public static readonly PassStoreFileException UnexpectedEndOfFile = new("unexpected EOF");
    public static readonly PassStoreFileException IncorrectMagicNumber = new("incorrect signature (magic number)");
    public static readonly PassStoreFileException UnsupportedVersion = new("unsupported format version");
    public static readonly PassStoreFileException InvalidCryptoHeader = new("invalid encryption header");
    public string Description { get; }

    public PassStoreFileException(string description)
    {
        Description = description;
    }
}
