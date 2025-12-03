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

    private void WriteField(Stream str, LoginField field)
    {
        str.WriteByte(field.Type);
        if (field.Type == LOGIN_FIELD_CUSTOM_ID)
            str.Write(field.CustomFieldSubtype.ToByteArray());
        FileFormatUtil.WriteU16TaggedString(str, field.Value);
    }
}