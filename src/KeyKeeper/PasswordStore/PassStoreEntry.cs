using System;
using System.IO;
using KeyKeeper.PasswordStore.Crypto;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore;

public abstract class PassStoreEntry
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; protected set; }
    public DateTime ModificationDate { get; set; }
    public Guid IconType { get; set; }
    public string Name { get; set; }
    public PassStoreEntryType Type { get; set; }

    public void WriteToStream(Stream str)
    {
        MemoryStream tmp = new();
        BinaryWriter wr = new(tmp);
        wr.Write(Id.ToByteArray());
        ulong timestamp = (ulong) new DateTimeOffset(CreationDate.ToUniversalTime()).ToUnixTimeSeconds();
        FileFormatUtil.WriteVarUint16(tmp, timestamp);
        timestamp = (ulong) new DateTimeOffset(ModificationDate.ToUniversalTime()).ToUnixTimeSeconds();
        FileFormatUtil.WriteVarUint16(tmp, timestamp);
        wr.Write(IconType.ToByteArray());
        FileFormatUtil.WriteU16TaggedString(tmp, Name);
        wr.Write(InnerSerialize());
        byte[] serializedEntry = tmp.ToArray();
        tmp.Dispose();

        wr = new(str);
        wr.Write7BitEncodedInt(serializedEntry.Length);
        wr.Write(serializedEntry);
    }

    public static PassStoreEntry ReadFromStream(Stream str)
    {
        BinaryReader rd = new(str);
        try
        {
            rd.Read7BitEncodedInt();

            byte[] uuidBuffer = new byte[16];
            if (rd.Read(uuidBuffer) < 16)
                throw PassStoreFileException.UnexpectedEndOfFile;
            Guid id = new Guid(uuidBuffer);

            ulong timestamp = FileFormatUtil.ReadVarUint16(str);
            DateTime createdAt = DateTimeOffset.FromUnixTimeSeconds((long)timestamp).UtcDateTime;
            timestamp = FileFormatUtil.ReadVarUint16(str);
            DateTime modifiedAt = DateTimeOffset.FromUnixTimeSeconds((long)timestamp).UtcDateTime;

            if (rd.Read(uuidBuffer) < 16)
                throw PassStoreFileException.UnexpectedEndOfFile;
            Guid iconType = new Guid(uuidBuffer);

            string name = FileFormatUtil.ReadU16TaggedString(str);

            byte entryType = rd.ReadByte();
            if (entryType == ENTRY_GROUP_ID)
            {
                return PassStoreEntryGroup.ReadFromStream(str, id, createdAt, modifiedAt, iconType, name);
            } else if (entryType == ENTRY_PASS_ID)
            {
                return PassStoreEntryPassword.ReadFromStream(str, id, createdAt, modifiedAt, iconType, name);
            } else
            {
                throw PassStoreFileException.InvalidPassStoreEntry;
            }
        } catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
    }

    protected abstract byte[] InnerSerialize();
}
