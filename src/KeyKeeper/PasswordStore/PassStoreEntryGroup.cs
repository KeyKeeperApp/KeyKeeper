using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore;

public class PassStoreEntryGroup : PassStoreEntry, IPassStoreDirectory
{
    public byte GroupType { get; set; }
    public Guid? CustomGroupSubtype { get; set; }
    public List<PassStoreEntry> ChildEntries { get; set; }

    public PassStoreEntryGroup(Guid id, DateTime createdAt, DateTime modifiedAt,
                               Guid iconType, string name, byte groupType,
                               List<PassStoreEntry>? children = null,
                               Guid? customGroupSubtype = null)
    {
        Id = id;
        CreationDate = createdAt;
        ModificationDate = modifiedAt;
        IconType = iconType;
        Name = name;
        GroupType = groupType;
        if (GroupType == GROUP_TYPE_CUSTOM && customGroupSubtype == null)
            throw new ArgumentNullException("custom group type");
        CustomGroupSubtype = customGroupSubtype;

        ChildEntries = children ?? new();
    }

    public static PassStoreEntry ReadFromStream(Stream str, Guid id, DateTime createdAt, DateTime modifiedAt, Guid iconType, string name)
    {
        BinaryReader rd = new(str);
        try
        {
            byte groupType = rd.ReadByte();
            byte[] guidBuffer = new byte[16];
            Guid? customGroupSubtype = null;
            if (groupType == GROUP_TYPE_CUSTOM)
            {
                if (rd.Read(guidBuffer) < 16)
                    throw PassStoreFileException.UnexpectedEndOfFile;
                customGroupSubtype = new Guid(guidBuffer);
            }

            PassStoreEntryGroup group = new(id, createdAt, modifiedAt, iconType, name, groupType, null, customGroupSubtype);

            int entryCount = rd.Read7BitEncodedInt();
            List<PassStoreEntry> children = new();
            for (int i = 0; i < entryCount; i++)
            {
                PassStoreEntry entry = PassStoreEntry.ReadFromStream(str);
                entry.Parent = group;
                children.Add(entry);
            }
            group.ChildEntries = children;
            
            return group;
        } catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
    }

    public bool DeleteEntry(Guid id)
    {
        if (ChildEntries == null)
            return false;
        for (int i = 0; i < ChildEntries.Count; i++)
        {
            if (ChildEntries[i].Id == id)
            {
                ChildEntries.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    IEnumerator<PassStoreEntry> IEnumerable<PassStoreEntry>.GetEnumerator()
    {
        return ChildEntries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ChildEntries.GetEnumerator();
    }

    public override string ToString()
    {
        return string.Format(
            "EntryGroup (id={0} name={1} chilren=[{2}])",
            Id, Name, string.Join(", ", ChildEntries));
    }

    protected override byte[] InnerSerialize()
    {
        MemoryStream str = new();
        str.WriteByte(ENTRY_GROUP_ID);

        str.WriteByte(GroupType);
        if (GroupType == GROUP_TYPE_CUSTOM)
            str.Write(CustomGroupSubtype!.Value.ToByteArray());

        BinaryWriter wr = new(str);
        wr.Write7BitEncodedInt(ChildEntries.Count);
        foreach (PassStoreEntry entry in ChildEntries)
            entry.WriteToStream(str);

        return str.ToArray();
    }
}