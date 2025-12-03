using System;

namespace KeyKeeper.PasswordStore;

public struct LoginField
{
    public byte Type;
    public Guid CustomFieldSubtype;
    public required string Value;
}