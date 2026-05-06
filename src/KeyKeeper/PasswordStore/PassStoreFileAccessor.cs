using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private const ushort FORMAT_VERSION_MAJOR = 1;
    private const ushort FORMAT_VERSION_MINOR = 1;
    private static readonly byte[] FORMAT_MAGIC = [0xf5, 0x3a, 0xa4, 0xb7, 0xeb, 0xd9, 0xc2, 0x12];

    private string filename;
    private byte[]? key;
    private InnerEncryptionInfo? innerCrypto;
    private OuterEncryptionHeader? outerCryptoHdr;
    private PassStoreEntryGroup? root;
    private Dictionary<Guid, PassStoreEntry> allEntries;

    public PassStoreFileAccessor(string filename, bool create, StoreCreationOptions? createOptions)
    {
        this.filename = filename;
        this.key = null;
        this.allEntries = new();
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
        get { return key == null; }
    }

    public PassStoreEntryGroup GetRootDirectory()
    {
        if (Locked)
            throw new InvalidOperationException();
        return root!;
    }

    public PassStoreEntryGroup? GetGroupByType(byte groupType)
    {
        if (Locked)
            throw new InvalidOperationException();
        return (root as PassStoreEntryGroup)?.ChildEntries
            .OfType<PassStoreEntryGroup>()
            .FirstOrDefault(g => g.GroupType == groupType);
    }

    public int GetTotalEntryCount()
    {
        throw new NotImplementedException();
    }

    public void Unlock(CompositeKey key)
    {
        if (!Locked) return;

        using FileStream file = new(filename, FileMode.Open, FileAccess.Read, FileShare.None);
        FileHeader hdr = FileHeader.ReadFrom(file);
        outerCryptoHdr = hdr.OuterCryptoHeader;

        file.Seek((file.Position + 4096 - 1) / 4096 * 4096, SeekOrigin.Begin);

        key.Salt = hdr.PreSalt;
        byte[] masterKey = hdr.KdfInfo.GetKdf().Derive(key, 32);
        using OuterEncryptionReader cryptoReader = new(file, masterKey, ((OuterAesHeader)hdr.OuterCryptoHeader).InitVector);
        using BinaryReader rd = new(cryptoReader);

        {
            if (rd.ReadUInt32() != FILE_FIELD_BEGIN)
                throw PassStoreFileException.InvalidBeginMarker;
            Span<byte> marker = stackalloc byte[8];
            if (rd.Read(marker) < 8)
                throw PassStoreFileException.UnexpectedEndOfFile;
            if (!marker.SequenceEqual(BEGIN_MARKER))
                throw PassStoreFileException.InvalidBeginMarker;
        }

        while (true)
        {
            try
            {
                uint fileField = rd.ReadUInt32();
                bool end = false;
                switch (fileField)
                {
                    case FILE_FIELD_INNER_CRYPTO:
                        ReadInnerCryptoInfo(cryptoReader);
                        break;
                    case FILE_FIELD_CONFIG:
                        break;
                    case FILE_FIELD_STORE:
                        root = PassStoreEntry.ReadFromStream(cryptoReader) as PassStoreEntryGroup;
                        AddEntriesToDict(root!);
                        ResolveLinks();
                        break;
                    case FILE_FIELD_END:
                        end = true;
                        break;
                }
                if (end) break;
            } catch (EndOfStreamException)
            {
                throw PassStoreFileException.UnexpectedEndOfFile;
            }
        }
        this.key = masterKey;
    }

    public void Lock()
    {
        if (Locked) return;
        Save();
        Array.Fill<byte>(key!, 0);
        key = null;
        root = null;
        allEntries = new();
    }

    public void Save()
    {
        if (Locked) return;

        // skip file header
        using FileStream file = new(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        FileHeader.ReadFrom(file);
        file.Seek((file.Position + 4096 - 1) / 4096 * 4096, SeekOrigin.Begin);

        // write the new contents
        using OuterEncryptionWriter cryptoWriter = new(file, key!, ((OuterAesHeader)outerCryptoHdr!).InitVector);

        using (BinaryWriter wr = new(cryptoWriter))
        {
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
            root!.WriteToStream(cryptoWriter);

            wr.Write(FILE_FIELD_END);
        }
        cryptoWriter.Flush();

        file.SetLength(file.Position);
    }

    public void AddEntry(PassStoreEntryGroup group, PassStoreEntry entry)
    {
        if (Locked) throw new InvalidOperationException("store locked");

        entry.Parent = group;
        group.ChildEntries.Add(entry);
        if (entry is PassStoreEntryLink link)
        {
            link.LinkTarget ??= allEntries[link.LinkTargetId];
            if (link.LinkTarget == null)
                throw new ArgumentException("invalid link target");
            PassStoreEntry t = link.LinkTarget;
            if (!t.Backlinks.Contains(entry))
                t.Backlinks.Add(entry);
        }
        allEntries[entry.Id] = entry;
    }

    public bool DeleteEntry(PassStoreEntryGroup? group, Guid id)
    {
        if (Locked) throw new InvalidOperationException("store locked");

        if (group == null)
            group = allEntries[id]?.Parent;

        if (group == null || group.ChildEntries == null)
            return false;

        var ch = group.ChildEntries;
        for (int i = 0; i < ch.Count; i++)
        {
            if (ch[i].Id == id)
            {
                for (int j = 0; j < ch[i].Backlinks.Count; j++)
                {
                    PassStoreEntry bl = ch[i].Backlinks[j];
                    if (bl is PassStoreEntryLink)
                        DeleteEntry(bl.Parent, bl.Id);
                }
                if (ch[i] is PassStoreEntryLink lnk && lnk.LinkTarget != null)
                {
                    lnk.LinkTarget.Backlinks.Remove(lnk);
                }
                allEntries.Remove(ch[i].Id);
                ch.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public void UpdateEntry(PassStoreEntryGroup? group, Guid id, PassStoreEntry entry)
    {
        if (Locked) throw new InvalidOperationException("store locked");

        if (group == null)
            group = allEntries[id]?.Parent;

        if (group == null || group.ChildEntries == null)
            return;

        entry.Parent = group;
        entry.Backlinks = allEntries[id].Backlinks;
        foreach (PassStoreEntry bl in entry.Backlinks)
        {
            if (bl is PassStoreEntryLink lnk)
            {
                lnk.LinkTarget = entry;
            }
        }
        
        var ch = group.ChildEntries;
        for (int i = 0; i < ch.Count; i++)
        {
            if (ch[i].Id == id)
            {
                allEntries.Remove(ch[i].Id); // убрать
                allEntries[entry.Id] = entry;
                ch[i] = entry;
                return;
            }
        }
    }

    public PassStoreEntry GetEntryById(Guid id)
    {
        if (Locked) throw new InvalidOperationException("store locked");
        return allEntries[id];
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
        if (!magic.SequenceEqual(FORMAT_MAGIC))
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

    private void AddEntriesToDict(PassStoreEntryGroup root)
    {
        allEntries.Add(root.Id, root);
        foreach (PassStoreEntry entry in root.ChildEntries)
        {
            if (entry is PassStoreEntryGroup group)
                AddEntriesToDict(group);
            else
                allEntries.Add(entry.Id, entry);
        }
    }

    private void ResolveLinks()
    {
        foreach (var kv in allEntries)
        {
            if (kv.Value is PassStoreEntryLink lnk)
            {
                lnk.LinkTarget = allEntries[lnk.LinkTargetId];
                lnk.LinkTarget.Backlinks.Add(lnk);
            }
        }
    }

    private void CreateNewAndUnlock(StoreCreationOptions options)
    {
        using FileStream file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None);
        FileHeader newHeader = FileHeader.Default();
        newHeader.WriteTo(file);
        outerCryptoHdr = newHeader.OuterCryptoHeader;

        options.Key.Salt = newHeader.PreSalt;

        int randomPaddingLen = 4096 - (int)(file.Position % 4096);
        if (randomPaddingLen > 0)
        {
            byte[] randomPadding = new byte[randomPaddingLen];
            RandomNumberGenerator.Fill(randomPadding);
            file.Write(randomPadding);
        }

        key = newHeader.KdfInfo.GetKdf().Derive(options.Key, 32);
        // пока предполагаем что везде используется AES
        OuterEncryptionWriter cryptoWriter = new(file, key, ((OuterAesHeader)newHeader.OuterCryptoHeader).InitVector);

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
        root = WriteInitialStoreTree(cryptoWriter);

        wr.Write(FILE_FIELD_END);

        cryptoWriter.Flush();
        cryptoWriter.Dispose();

        AddEntriesToDict(root);
        ResolveLinks();
    }

    private PassStoreEntryGroup WriteInitialStoreTree(OuterEncryptionWriter w)
    {
        PassStoreEntryGroup defaultGroup = new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            Guid.Empty,
            "",
            GROUP_TYPE_DEFAULT
        );
        PassStoreEntryGroup favourites = new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            Guid.Empty,
            "",
            GROUP_TYPE_FAVOURITES
        );

        PassStoreEntryGroup root = new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            Guid.Empty,
            "",
            GROUP_TYPE_ROOT,
            [defaultGroup, favourites]
        );
        defaultGroup.Parent = root;
        favourites.Parent = root;
        root.WriteToStream(w);
        return root;
    }

    private InnerEncryptionInfo ReadInnerCryptoInfo(Stream str)
    {
        byte[] key = new byte[32];
        byte[] iv = new byte[16];
        if (str.Read(key) < 32)
            throw PassStoreFileException.UnexpectedEndOfFile;
        if (str.Read(iv) < 16)
            throw PassStoreFileException.UnexpectedEndOfFile;
        return new(key, iv);
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
                    200000,
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

        public static FileHeader ReadFrom(Stream s)
        {
            BinaryReader rd = new(s);
            {
                byte[] magic = new byte[8];
                if (rd.Read(magic, 0, 8) < 8)
                    throw PassStoreFileException.UnexpectedEndOfFile;
                if (!magic.SequenceEqual(FORMAT_MAGIC))
                    throw PassStoreFileException.IncorrectMagicNumber;
            }
            try
            {
                ushort major, minor;
                major = rd.ReadUInt16();
                minor = rd.ReadUInt16();
                if (major != FORMAT_VERSION_MAJOR || minor != FORMAT_VERSION_MINOR)
                    throw PassStoreFileException.UnsupportedVersion;

                byte saltLen = rd.ReadByte();
                if (saltLen < MIN_MASTER_SALT_LEN || saltLen > MAX_MASTER_SALT_LEN)
                    throw PassStoreFileException.InvalidCryptoHeader;

                byte[] salt = new byte[saltLen];
                if (rd.Read(salt) < saltLen)
                    throw PassStoreFileException.UnexpectedEndOfFile;

                byte typeDiscrim = rd.ReadByte();
                OuterEncryptionHeader outerEncrHdr;
                if (typeDiscrim == ENCRYPT_ALGO_AES)
                {
                    byte[] iv = new byte[16];
                    if (rd.Read(iv) < 16)
                        throw PassStoreFileException.UnexpectedEndOfFile;
                    outerEncrHdr = new OuterAesHeader(iv);
                } else
                {
                    throw PassStoreFileException.InvalidCryptoHeader;
                }

                typeDiscrim = rd.ReadByte();
                KdfHeader kdfHdr;
                if (typeDiscrim == KDF_TYPE_AESKDF)
                {
                    int rounds = rd.Read7BitEncodedInt();
                    byte[] seed = new byte[32];
                    if (rd.Read(seed) < 32)
                        throw PassStoreFileException.UnexpectedEndOfFile;
                    kdfHdr = new AesKdfHeader(rounds, seed);
                } else
                {
                    throw PassStoreFileException.InvalidCryptoHeader;
                }

                return new FileHeader(
                    major, minor, salt,
                    outerEncrHdr, kdfHdr
                );
            }
            catch (EndOfStreamException)
            {
                throw PassStoreFileException.UnexpectedEndOfFile;
            }
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

    record struct InnerEncryptionInfo(
        byte[] Key,
        byte[] Iv
    )
    {}
}
