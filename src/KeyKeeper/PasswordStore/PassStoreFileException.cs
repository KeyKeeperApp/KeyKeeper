using System;

namespace KeyKeeper.PasswordStore;

public class PassStoreFileException : Exception
{
    public static readonly PassStoreFileException UnexpectedEndOfFile = new("unexpected EOF");
    public static readonly PassStoreFileException IncorrectMagicNumber = new("incorrect signature (magic number)");
    public static readonly PassStoreFileException UnsupportedVersion = new("unsupported format version");
    public static readonly PassStoreFileException InvalidCryptoHeader = new("invalid encryption header");
    public static readonly PassStoreFileException ContentHMACMismatch = new("content HMAC mismatch");
    public static readonly PassStoreFileException InvalidPassStoreEntry = new("invalid store entry");
    public static readonly PassStoreFileException InvalidBeginMarker = new("invalid marker of the beginning of data");

    public PassStoreFileException(string description): base(description)
    {
    }
}
