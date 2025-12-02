using System;
using System.IO;
using System.Security.Cryptography;

namespace KeyKeeper.PasswordStore.Crypto;

public class OuterEncryptionReader : Stream
{
    public override bool CanRead => true;
    public override bool CanWrite => false;
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
    private ICryptoTransform decryptor;

    /// <summary>
    /// Последний считанный из файла расшифрованный чанк. Первые
    /// <see cref="chunkPosition"/> байт - уже отданные при вызовах Read,
    /// остальные - еще не отданные.    
    /// </summary>
    private byte[]? currentChunk;
    private int chunkPosition = 0;
    /// <summary>
    /// Порядковый номер чанка, лежащего в <see cref="currentChunk"/>. 
    /// </summary>
    private int nextChunkOrdinal = 0;
    private bool isCurrentChunkLast;
    private long position = 0;

    /// <summary>
    /// Ещё не расшифрованные байты, которые были считаны из файла, но их
    /// оказалось меньше, чем вмещает блок AES (менее 16 байт).
    /// </summary>
    private byte[] encryptedRemainder;
    /// <summary>
    /// Количество полезных байт в <see cref="encryptedRemainder"/>. 
    /// </summary>
    private int encryptedRemainderLength;

    /// <summary>
    /// Создаёт экземпляр reader, использующий файловый поток для чтения.
    /// </summary>
    /// <param name="file">Файловый поток, указатель которого должен стоять на
    /// первом content chunk.</param>
    /// <param name="key">Ключ, который будет использован для проверки HMAC
    /// и расшифровки содержимого.</param>
    public OuterEncryptionReader(FileStream file, byte[] key, byte[] iv)
    {
        aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CFB;
        aes.Padding = PaddingMode.None;
        decryptor = aes.CreateDecryptor();

        this.file = file;
        this.key = key;
        currentChunk = null;
        encryptedRemainder = new byte[16];
        encryptedRemainderLength = 0;
        LoadAndDecryptNextChunk();
    }

    public override void SetLength(long value)
        => throw new NotSupportedException();

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count,
                                           AsyncCallback? callback, object? state)
        => throw new NotSupportedException();
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count,
                                            AsyncCallback? callback, object? state)
        => throw new NotSupportedException();

    public override void Flush()
    {}

    public override int EndRead(IAsyncResult asyncResult)
        => throw new NotSupportedException();
    public override void EndWrite(IAsyncResult asyncResult)
        => throw new NotSupportedException();


    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
        => Read(new Span<byte>(buffer, offset, count));

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
    public override void Write(ReadOnlySpan<byte> buffer)
        => throw new NotSupportedException();

    public override int Read(Span<byte> buffer)
    {
        Console.WriteLine("OE read " + buffer.Length);
        int toRead = buffer.Length;
        int read = 0;
        while (toRead > 0)
        {
            if (currentChunk == null || currentChunk.Length - chunkPosition == 0)
            {
                if (!isCurrentChunkLast)
                {
                    Console.WriteLine("OER: reading next chunk");
                    LoadAndDecryptNextChunk();
                }
                else
                {
                    Console.WriteLine("OER: read " + read + " bytes before EOF");
                    break;
                }
            }
            byte[] chunk = currentChunk!;
            int n = Math.Min(toRead, chunk.Length - chunkPosition);
            Console.WriteLine("OER: copy " + n + " bytes chunk+" + chunkPosition + " -> buffer+" + read);
            new Span<byte>(chunk, chunkPosition, n).CopyTo(buffer.Slice(read));
            read += n;
            toRead -= n;
            chunkPosition += n;
            position += n;
            Console.WriteLine(string.Format("read={} toread={} pos={}", read, toRead, chunkPosition));
        }
        return read;
    }

    private void LoadAndDecryptNextChunk()
    {
        if (isCurrentChunkLast)
            return;
        var chunk = PassStoreContentChunk.GetFromStream(file, key, nextChunkOrdinal);
        nextChunkOrdinal += 1;
        isCurrentChunkLast = chunk.IsLast;
        var encryptedData = chunk.GetContent();
        EraseCurrentChunk();

        int decrypted = 0, read = 0;
        currentChunk = new byte[(encryptedData.Length + encryptedRemainderLength) / 16 * 16];
        if (encryptedRemainderLength > 0 && encryptedData.Length >= 16 - encryptedRemainderLength)
        {
            encryptedData.Slice(0, 16 - encryptedRemainderLength)
                    .CopyTo(new Span<byte>(encryptedRemainder, encryptedRemainderLength, 16 - encryptedRemainderLength));
            decryptor.TransformBlock(encryptedRemainder, 0, 16, currentChunk, 0);
            decrypted = 16;
            read = 16 - encryptedRemainderLength;
            encryptedRemainderLength = 0;
        }
        if (!isCurrentChunkLast)
        {
            int wholeBlocksLen = (encryptedData.Length - decrypted) / 16 * 16;
            if (wholeBlocksLen > 0)
            {
                byte[] blocks = new byte[wholeBlocksLen];
                encryptedData.Slice(read, wholeBlocksLen).CopyTo(blocks);
                decryptor.TransformBlock(blocks, 0, wholeBlocksLen, currentChunk, decrypted);
                decrypted += wholeBlocksLen;
                read += wholeBlocksLen;
            }
            if (read < encryptedData.Length)
            {
                encryptedRemainderLength = encryptedData.Length - read;
                encryptedData.Slice(read, encryptedRemainderLength).CopyTo(encryptedRemainder);
            }
        } else
        {
            byte[] finalData = new byte[encryptedData.Length - read];
            encryptedData.Slice(read).CopyTo(finalData);
            byte[] decryptedFinalData = decryptor.TransformFinalBlock(finalData, 0, finalData.Length);
            decryptedFinalData.CopyTo(currentChunk, decrypted);
            Array.Fill<byte>(decryptedFinalData, 0);
        }
        chunkPosition = 0;
    }

    private void EraseCurrentChunk()
    {
        if (currentChunk == null) return;
        Array.Fill<byte>(currentChunk, 0);
        currentChunk = null;
    }
}
