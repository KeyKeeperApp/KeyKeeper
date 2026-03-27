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
    public TotpParameters? Totp { get; set; }

    public PassStoreEntryPassword(Guid id, DateTime createdAt, DateTime modifiedAt, Guid iconType, string name,
                                  LoginField username, LoginField password,
                                  List<LoginField>? extras = null, TotpParameters? totp = null)
    {
        Id = id;
        CreationDate = createdAt;
        ModificationDate = modifiedAt;
        IconType = iconType;
        Name = name;
        Username = username;
        Password = password;
        ExtraFields = extras ?? new();
        Totp = totp;
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

            int totpPresence = str.ReadByte();
            if (totpPresence == TOTP_PRESENT)
            {
                TotpAlgorithm algo = rd.ReadByte() switch
                {
                    TOTP_ALGO_SHA256 => TotpAlgorithm.SHA256,
                    TOTP_ALGO_SHA512 => TotpAlgorithm.SHA512,
                    _ => TotpAlgorithm.SHA1,
                };
                byte digits = rd.ReadByte();
                int period = rd.Read7BitEncodedInt();
                string secret = FileFormatUtil.ReadU16TaggedString(str);
                string issuer = FileFormatUtil.ReadU16TaggedString(str);
                string accountName = FileFormatUtil.ReadU16TaggedString(str);
                entry.Totp = new TotpParameters(
                    secret, algo, digits, period,
                    issuer.Length > 0 ? issuer : null,
                    accountName.Length > 0 ? accountName : null);
            }

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

        if (Totp is TotpParameters totp)
        {
            str.WriteByte(TOTP_PRESENT);
            byte algoByte = totp.Algorithm switch
            {
                TotpAlgorithm.SHA256 => TOTP_ALGO_SHA256,
                TotpAlgorithm.SHA512 => TOTP_ALGO_SHA512,
                _ => TOTP_ALGO_SHA1,
            };
            str.WriteByte(algoByte);
            str.WriteByte((byte)totp.Digits);
            wr.Write7BitEncodedInt(totp.Period);
            FileFormatUtil.WriteU16TaggedString(str, totp.Secret);
            FileFormatUtil.WriteU16TaggedString(str, totp.Issuer ?? "");
            FileFormatUtil.WriteU16TaggedString(str, totp.AccountName ?? "");
        }
        else
        {
            str.WriteByte(TOTP_ABSENT);
        }

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
