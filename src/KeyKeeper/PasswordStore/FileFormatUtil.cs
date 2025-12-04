using System;
using System.Buffers.Binary;
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

    public static ulong ReadVarUint16(Stream str)
    {
        ulong number = 0;
        int i = 0;
        while (true)
        {
            int b = str.ReadByte();
            if (b == -1)
                throw PassStoreFileException.UnexpectedEndOfFile;
            number |= (ulong)b << i;
            i += 8;
            b = str.ReadByte();
            if (b == -1)
                throw PassStoreFileException.UnexpectedEndOfFile;
            number |= (ulong)(b & 0x7f) << i;
            if ((b & 0x80) == 0)
                break;
            i += 7;
        }
        return number;
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

    public static string ReadU16TaggedString(Stream str)
    {
        byte[] lenBytes = new byte[2];
        if (str.Read(lenBytes) < 2)
            throw PassStoreFileException.UnexpectedEndOfFile;
        int len = BinaryPrimitives.ReadUInt16LittleEndian(lenBytes);
        byte[] s = new byte[len];
        if (str.Read(s) < len)
            throw PassStoreFileException.UnexpectedEndOfFile;
        return Encoding.UTF8.GetString(s);
    }
}