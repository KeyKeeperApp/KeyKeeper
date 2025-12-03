using System;
using System.IO;
using System.Security.Cryptography;

namespace KeyKeeper.PasswordStore.Crypto;

public class OuterEncryptionWriter : Stream
{
    public override bool CanRead => false;
    public override bool CanWrite => true;
    public override bool CanSeek => false;
    public override bool CanTimeout => false;
    public override int ReadTimeout
    {
        get { throw new InvalidOperationException(); }
        set { throw new InvalidOperationException(); }
    }
    public override int WriteTimeout
    {
        get { throw new InvalidOperationException(); }
        set { throw new InvalidOperationException(); }
    }
    public override long Position
    {
        get => position;
        set { throw new NotSupportedException(); }
    }
    public override long Length => throw new NotSupportedException();

    private FileStream file;
    private byte[] key;
    private Aes aes;
    private ICryptoTransform encryptor;

    private byte[] currentChunk;
    private int currentChunkOrdinal = 0;
    private int chunkPosition = 0;
    private long position = 0;

    public OuterEncryptionWriter(FileStream file, byte[] key, byte[] iv)
    {
        if (!file.CanWrite)
            throw new ArgumentException("file must be writeable");
        this.file = file;
        this.key = key;

        currentChunk = new byte[524288];

        aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CFB;
        aes.Padding = PaddingMode.None;
        encryptor = aes.CreateEncryptor();
    }

    public override void SetLength(long value)
        => throw new NotSupportedException();
    
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count,
                                           AsyncCallback? callback, object? state)
        => throw new NotSupportedException();
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count,
                                            AsyncCallback? callback, object? state)
        => throw new NotSupportedException();

    public override int EndRead(IAsyncResult asyncResult)
        => throw new NotSupportedException();
    public override void EndWrite(IAsyncResult asyncResult)
        => throw new NotSupportedException();


    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override int Read(Span<byte> buffer)
        => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count)
        => Write(new ReadOnlySpan<byte>(buffer, offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        int written = 0;
        while (written < buffer.Length)
        {
            if (chunkPosition == currentChunk.Length)
                EncryptAndStoreCurrentFullChunk();
            int n = Math.Min(buffer.Length, currentChunk.Length - chunkPosition);
            buffer.Slice(written, n).CopyTo(new Span<byte>(currentChunk, chunkPosition, n));
            written += n;
            chunkPosition += n;
            position += n;
        }
    }

    /// <summary>
    /// Шифрует оставшиеся данные и добавляет к файлу чанк с пометкой о том
    /// что он последний. Вызов после завершения записи полезных данных
    /// обязателен, в противном случае потеря данных неизбежна.
    /// </summary>
    public override void Flush()
    {
        byte[] encryptedData = encryptor.TransformFinalBlock(currentChunk, 0, chunkPosition);
        PassStoreContentChunk chunk = PassStoreContentChunk.FromEncryptedContent(encryptedData, key, currentChunkOrdinal, true);
        file.Write(chunk.Chunk);
        EraseCurrentChunk();
        chunkPosition = 0;
    }

    private void EncryptAndStoreCurrentFullChunk()
    {
        byte[] encryptedData = new byte[currentChunk.Length];
        encryptor.TransformBlock(currentChunk, 0, currentChunk.Length, encryptedData, 0);
        PassStoreContentChunk chunk = PassStoreContentChunk.FromEncryptedContent(encryptedData, key, currentChunkOrdinal, false);
        currentChunkOrdinal += 1;
        file.Write(chunk.Chunk);
        EraseCurrentChunk();
        chunkPosition = 0;
    }

    private void EraseCurrentChunk()
    {
        if (currentChunk == null) return;
        Array.Fill<byte>(currentChunk, 0);
    }
}