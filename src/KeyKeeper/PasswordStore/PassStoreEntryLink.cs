using System;
using System.IO;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore;

public class PassStoreEntryLink : PassStoreEntry
{
    public PassStoreEntry? LinkTarget;
    public Guid LinkTargetId;

    public override string DisplayName => LinkTarget?.DisplayName ?? Name;

    public PassStoreEntryLink(Guid id, DateTime createdAt, DateTime modifiedAt,
                              Guid targetId, PassStoreEntry? target = null)
    {
        Id = id;
        CreationDate = createdAt;
        ModificationDate = modifiedAt;
        IconType = BuiltinEntryIconType.DEFAULT;
        Name = "";
        LinkTargetId = targetId;
        LinkTarget = target;
    }

    public static PassStoreEntry ReadFromStream(Stream str, Guid id, DateTime createdAt, DateTime modifiedAt, Guid iconType, string name)
    {
        BinaryReader rd = new(str);
        try
        {
            byte[] guidBuffer = new byte[16];
            if (rd.Read(guidBuffer) < 16)
                throw PassStoreFileException.UnexpectedEndOfFile;
            Guid linkTargetId = new(guidBuffer);

            return new PassStoreEntryLink(id, createdAt, modifiedAt, linkTargetId);
        } catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
    }

    public override string ToString()
    {
        return string.Format(
            "EntryLink (id={0} target_id={1} target={2})",
            Id, LinkTargetId, LinkTarget);
    }

    protected override byte[] InnerSerialize()
    {
        MemoryStream str = new();
        str.WriteByte(ENTRY_LINK_ID);
        str.Write(LinkTargetId.ToByteArray());

        return str.ToArray();
    }
}
