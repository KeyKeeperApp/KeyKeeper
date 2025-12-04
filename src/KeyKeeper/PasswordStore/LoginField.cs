using System;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore;

public struct LoginField
{
    public byte Type;
    public Guid CustomFieldSubtype;
    public required string Value;

    public override string ToString()
    {
        return string.Format("LoginField(type={0} {1} value={2})", Type, Type == LOGIN_FIELD_CUSTOM_ID ? "customtype=" + CustomFieldSubtype : "", Value);
    }
}