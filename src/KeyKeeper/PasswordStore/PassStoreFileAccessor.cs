using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using KeyKeeper.PasswordStore.Crypto;
using KeyKeeper.PasswordStore.Crypto.KeyDerivation;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore;

/// <summary>
/// Класс, содержащий реализацию доступа к хранилищу паролей
/// </summary>
public class PassStoreFileAccessor : IPassStore
{
    private const ushort FORMAT_VERSION_MAJOR = 0;
    private const ushort FORMAT_VERSION_MINOR = 0;
    private static readonly byte[] FORMAT_MAGIC = [0xf5, 0x3a, 0xa4, 0xb7, 0xeb, 0xd9, 0xc2, 0x12];

    private string filename;
    private byte[]? key;
    private IPassStoreDirectory? root;

    public PassStoreFileAccessor(string filename, bool create, StoreCreationOptions? createOptions)
    {
        this.filename = filename;
        this.key = null;
        if (!create)
        {
            CheckStoreFile();
        } else if (createOptions != null)
        {
            CreateNewAndUnlock(createOptions);
        } else throw new ArgumentException("createOptions must not be null when creating a new store");
    }

    public bool Locked
    {
        get { return key != null; }
    }

    public IPassStoreDirectory GetRootDirectory()
    {
        return root!;
    }

    public int GetTotalEntryCount()
    {
        throw new NotImplementedException();
    }

    public void Unlock(CompositeKey key)
    {
        if (!Locked) return;
    }

    public void Lock()
    {
        if (Locked) return;
    }

    /// <summary>
    /// Проверяет внешнюю целостность файла хранилища, то есть:
    /// 1. совпадает сигнатура (magic number)
    /// 2. совпадает версия формата
    /// 3. поля криптозаголовка корректны
    /// </summary>
    void CheckStoreFile()
    {
        using FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        BinaryReader f = new(file);
        byte[] magic = new byte[8];
        if (f.Read(magic, 0, 8) < 8)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
        if (magic != FORMAT_MAGIC)
        {
            throw PassStoreFileException.IncorrectMagicNumber;
        }

        ushort version;
        try
        {
            version = f.ReadUInt16();
        }
        catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
        if (version != FORMAT_VERSION_MAJOR)
        {
            throw PassStoreFileException.UnsupportedVersion;
        }

        try
        {
            version = f.ReadUInt16();
        }
        catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
        if (version != FORMAT_VERSION_MINOR)
        {
            throw PassStoreFileException.UnsupportedVersion;
        }

        OuterEncryptionUtil.CheckOuterEncryptionHeader(file);
    }

    private void CreateNewAndUnlock(StoreCreationOptions options)
    {
        using FileStream file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None);
        FileHeader newHeader = FileHeader.Default();
        newHeader.WriteTo(file);

        options.Key.Salt = newHeader.PreSalt;

        int randomPaddingLen = 4096 - (int)(file.Position % 4096);
        if (randomPaddingLen > 0)
        {
            byte[] randomPadding = new byte[randomPaddingLen];
            RandomNumberGenerator.Fill(randomPadding);
        }

        byte[] masterKey = newHeader.KdfInfo.GetKdf().Derive(options.Key, 32);
        // пока предполагаем что везде используется AES
        OuterEncryptionWriter cryptoWriter = new(file, masterKey, ((OuterAesHeader)newHeader.OuterCryptoHeader).InitVector);

        BinaryWriter wr = new(cryptoWriter);

        wr.Write(FILE_FIELD_BEGIN);
        cryptoWriter.Write(BEGIN_MARKER);

        byte[] innerKey = new byte[32];
        RandomNumberGenerator.Fill(innerKey);
        byte[] innerIv = new byte[16];
        RandomNumberGenerator.Fill(innerIv);

        wr.Write(FILE_FIELD_INNER_CRYPTO);
        cryptoWriter.Write(innerKey);
        cryptoWriter.Write(innerIv);

        wr.Write(FILE_FIELD_CONFIG);

        wr.Write(FILE_FIELD_STORE);
        root = (IPassStoreDirectory) WriteInitialStoreTree(cryptoWriter);
        cryptoWriter.Flush();
        cryptoWriter.Dispose();
    }

    private PassStoreEntry WriteInitialStoreTree(OuterEncryptionWriter w)
    {
        PassStoreEntry root =
            new PassStoreEntryGroup(
                Guid.NewGuid(),
                DateTime.UtcNow,
                DateTime.UtcNow,
                Guid.Empty,
                "",
                GROUP_TYPE_ROOT
            );
        root.WriteToStream(w);
        return root;
    }

    record FileHeader (
        ushort FileVersionMajor,
        ushort FileVersionMinor,
        byte[] PreSalt,
        OuterEncryptionHeader OuterCryptoHeader,
        KdfHeader KdfInfo
    )
    {
        public static FileHeader Default()
        {
            int saltLen = (MIN_MASTER_SALT_LEN + MAX_MASTER_SALT_LEN) / 2;
            byte[] preSalt = new byte[saltLen];
            RandomNumberGenerator.Fill(preSalt);

            byte[] iv = new byte[16];
            RandomNumberGenerator.Fill(iv);

            byte[] aesKdfSeed = new byte[32];
            RandomNumberGenerator.Fill(aesKdfSeed);

            return new FileHeader
            (
                FORMAT_VERSION_MAJOR,
                FORMAT_VERSION_MINOR,
                preSalt,
                new OuterAesHeader
                (
                    iv
                ),
                new AesKdfHeader
                (
                    MAX_AESKDF_ROUNDS,
                    aesKdfSeed
                )
            );
        }
        public int WriteTo(Stream s)
        {
            int written = 0;

            s.Write(FORMAT_MAGIC);
            written += FORMAT_MAGIC.Length;

            BinaryWriter wr = new(s);
            wr.Write(FORMAT_VERSION_MAJOR);
            wr.Write(FORMAT_VERSION_MINOR);
            written += 4;

            wr.Write((byte)PreSalt.Length);
            s.Write(PreSalt);
            written += 1 + PreSalt.Length;

            if (OuterCryptoHeader is OuterAesHeader aes)
            {
                wr.Write(ENCRYPT_ALGO_AES);
                s.Write(aes.InitVector);
                written += 1 + aes.InitVector.Length;
            }
            if (KdfInfo is AesKdfHeader aesKdf)
            {
                long pos = s.Position;
                wr.Write(KDF_TYPE_AESKDF);
                wr.Write7BitEncodedInt(aesKdf.Rounds);
                wr.Write(aesKdf.Seed);
                written += (int)(s.Position - pos);
            }
            return written;
        }
    };

    record OuterEncryptionHeader {}

    record OuterAesHeader(
        byte[] InitVector
    ) : OuterEncryptionHeader;

    abstract record KdfHeader
    {
        public abstract MasterKeyDerivationFunction GetKdf();
    }

    record AesKdfHeader(
        int Rounds,
        byte[] Seed
    ) : KdfHeader
    {
        public override MasterKeyDerivationFunction GetKdf()
        {
            return new AesKdf(Rounds, Seed);
        }
    }
}
