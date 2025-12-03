using System;
using System.IO;
using KeyKeeper.PasswordStore.Crypto;

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

    protected abstract byte[] InnerSerialize();
}
