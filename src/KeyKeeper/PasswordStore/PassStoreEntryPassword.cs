using System;
using System.Collections.Generic;
using System.IO;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore;

public class PassStoreEntryPassword : PassStoreEntry
{
    public LoginField Username { get; set; }
    public LoginField Password { get; set; }
    public List<LoginField> ExtraFields { get; set; }

    public PassStoreEntryPassword(Guid id, DateTime createdAt, DateTime modifiedAt, Guid iconType, string name, LoginField username, LoginField password, List<LoginField>? extras = null)
    {
        Id = id;
        CreationDate = createdAt;
        ModificationDate = modifiedAt;
        IconType = iconType;
        Name = name;
        Username = username;
        Password = password;
        ExtraFields = extras ?? new();
    }

    public static PassStoreEntry ReadFromStream(Stream str, Guid id, DateTime createdAt, DateTime modifiedAt, Guid iconType, string name)
    {
        BinaryReader rd = new(str);
        try
        {
            LoginField username = ReadField(str);
            LoginField password = ReadField(str);
            PassStoreEntryPassword entry = new(id, createdAt, modifiedAt, iconType, name, username, password);
            int extraFieldCount = rd.Read7BitEncodedInt();
            for (; extraFieldCount > 0; extraFieldCount--)
                entry.ExtraFields.Add(ReadField(str));
            return entry;
        } catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
    }

    public override string ToString()
    {
        return string.Format(
            "EntryPassword(id={0} name={1} fields=[first={2} second={3} extra={4}])",
            Id, Name, Username, Password, string.Join(", ", ExtraFields));
    }

    protected override byte[] InnerSerialize()
    {
        MemoryStream str = new();
        str.WriteByte(ENTRY_PASS_ID);
        WriteField(str, Username);
        WriteField(str, Password);
        BinaryWriter wr = new(str);
        wr.Write7BitEncodedInt(ExtraFields.Count);
        foreach (LoginField field in ExtraFields)
            WriteField(str, field);
        return str.ToArray();
    }

    private static void WriteField(Stream str, LoginField field)
    {
        str.WriteByte(field.Type);
        if (field.Type == LOGIN_FIELD_CUSTOM_ID)
            str.Write(field.CustomFieldSubtype.ToByteArray());
        FileFormatUtil.WriteU16TaggedString(str, field.Value);
    }

    private static LoginField ReadField(Stream str)
    {
        int t = str.ReadByte();
        if (t == -1)
            throw PassStoreFileException.UnexpectedEndOfFile;
        LoginField field = new() { Value = "" };
        field.Type = (byte)t;
        if (t == LOGIN_FIELD_CUSTOM_ID)
        {
            byte[] uuidBuffer = new byte[16];
            if (str.Read(uuidBuffer) < 16)
                throw PassStoreFileException.UnexpectedEndOfFile;
            field.CustomFieldSubtype = new Guid(uuidBuffer);
        }
        field.Value = FileFormatUtil.ReadU16TaggedString(str);
        return field;
    }
}