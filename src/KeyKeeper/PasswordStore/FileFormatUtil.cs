using System;
using System.IO;
using System.Text;

namespace KeyKeeper.PasswordStore;

static class FileFormatUtil
{
    public static int WriteVarUint16(Stream str, ulong number)
    {
        int size = 0;
        do {
            ushort portion = (ushort)(number & 0x7fff);
            number >>= 15;
            if (number != 0)
                portion |= 1 << 15;
            size += 2;
            str.WriteByte((byte)(portion & 0xff));
            str.WriteByte((byte)(portion >> 8));
        }
        while (number != 0);
        return size;
    }

    public static int WriteU8TaggedString(Stream str, string s)
    {
        byte[] b = Encoding.UTF8.GetBytes(s);
        if (b.Length > 255)
            throw new ArgumentException("string too long");
        str.WriteByte((byte)b.Length);
        str.Write(b);
        return b.Length + 1;
    }

    public static int WriteU16TaggedString(Stream str, string s)
    {
        byte[] b = Encoding.UTF8.GetBytes(s);
        if (b.Length > 65535)
            throw new ArgumentException("string too long");
        str.WriteByte((byte)(b.Length & 0xff));
        str.WriteByte((byte)(b.Length >> 8));
        str.Write(b);
        return b.Length + 2;
    }
}