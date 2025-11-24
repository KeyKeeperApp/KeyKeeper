using System;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore.Crypto;

/// <summary>
/// Класс, представляющий собой обертку над content chunkом, считанным из файла
/// хранилища. Не расшифровывает содержимое, но проверяет целостность и
/// подлинность при создании объекта.
/// </summary>
public class PassStoreContentChunk
{
    private byte[] chunk;
    private int chunkLen;

    /// <summary>
    /// Создаёт объект content chunk, считывая массив байт. Бросает исключение
    /// в случае, если массив не содержит корректный content chunk или не
    /// совпадает HMAC
    /// </summary>
    /// <param name="chunk">Массив байт, содержащий весь content chunk, включая
    /// длину и HMAC</param>
    /// <param name="key">Ключ от хранилища. Используется только для
    /// проверки HMAC и не хранится в объекте</param>
    /// <param name="chunkOrdinal">Порядковый номер content chunk'а, начиная
    /// с 0</param>
    public PassStoreContentChunk(byte[] chunk, byte[] key, int chunkOrdinal)
    {
        this.chunk = chunk;

        MemoryStream str = new(chunk);
        BinaryReader rd = new(str);

        try
        {
            chunkLen = rd.ReadUInt16();
            chunkLen = (chunkLen << 8) | rd.ReadByte();
        }
        catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }

        if (chunk.Length != chunkLen + 3 + HMAC_SIZE)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }

        byte[] storedHmac = new byte[HMAC_SIZE];
        str.Read(storedHmac, 0, HMAC_SIZE);

        HMACSHA3_512 hmac = new(key);
        hmac.TransformBlock(chunk, (int)str.Position, Math.Min(chunkLen, chunk.Length - (int)str.Position), null, 0);

        byte[] encodedOrdinal = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(new Span<byte>(encodedOrdinal), chunkOrdinal);
        hmac.TransformBlock(encodedOrdinal, 0, encodedOrdinal.Length, null, 0);
        
        byte[] actualHmac = hmac.Hash!;

        if (!storedHmac.Equals(actualHmac))
        {
            throw PassStoreFileException.ContentHMACMismatch;
        }
    }

    /// <summary>
    /// Создаёт объект content chunk, считывая байты из потока. Бросает
    /// исключение в случае, если массив не содержит корректный content chunk
    /// или не совпадает HMAC
    /// </summary>
    /// <param name="chunk">Массив байт, содержащий весь content chunk, включая
    /// длину и HMAC</param>
    /// <param name="key">Ключ от хранилища. Используется только для
    /// проверки HMAC и не хранится в объекте</param>
    /// <param name="chunkOrdinal">Порядковый номер content chunk'а, начиная
    /// с 0</param>
    public static PassStoreContentChunk GetFromStream(Stream s, byte[] key, int chunkOrdinal)
    {
        BinaryReader rd = new(s);
        int chunkLen;
        try
        {
            chunkLen = rd.ReadUInt16();
            chunkLen = (chunkLen << 8) | rd.ReadByte();
        }
        catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
        byte[] chunk = new byte[3 + HMAC_SIZE + chunkLen];
        if (s.Read(chunk) < chunk.Length)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
        return new PassStoreContentChunk(chunk, key, chunkOrdinal);
    }

    public ReadOnlySpan<byte> GetContent()
    {
        return new ReadOnlySpan<byte>(chunk, 3 + HMAC_SIZE, chunkLen);
    }
}
