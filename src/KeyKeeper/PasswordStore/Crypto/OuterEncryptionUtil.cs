using System;
using System.IO;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.PasswordStore.Crypto;

public static class OuterEncryptionUtil
{
    /// <summary>
    /// Проверяет корректность заголовка внешнего шифрования,
    /// который содержит соль для мастер-ключа и параметры шифрования +
    /// генерации ключа. Сдвигает указатель потока f на первый байт после
    /// заголовка.
    /// </summary>
    /// <param name="f">Поток, указатель которого стоит на начале заголовка
    /// внешнего шифрования.</param>
    /// <exception cref="PassStoreFileException">Если заголовок содержит некорректные поля или неполный</exception>
    public static void CheckOuterEncryptionHeader(FileStream f)
    {
        BinaryReader rd = new(f);
        try
        {
            byte masterSaltLen = rd.ReadByte();

            if (masterSaltLen < MIN_MASTER_SALT_LEN || masterSaltLen > MAX_MASTER_SALT_LEN)
            {
                throw PassStoreFileException.InvalidCryptoHeader;
            }

            f.Seek(masterSaltLen, SeekOrigin.Current);

            byte encryptAlgo = rd.ReadByte();

            if (encryptAlgo == ENCRYPT_ALGO_AES)
            {
                // пропустить 16 байт вектора инициализации AES
                f.Seek(16, SeekOrigin.Current);
            }
            else
            {
                throw PassStoreFileException.InvalidCryptoHeader;
            }

            byte keyDerivationFunctionType = rd.ReadByte();

            if (keyDerivationFunctionType == KDF_TYPE_AESKDF)
            {
                int nRounds;
                try
                {
                    nRounds = rd.Read7BitEncodedInt();
                }
                catch (FormatException)
                {
                    throw PassStoreFileException.InvalidCryptoHeader;
                }
                if (nRounds < MIN_AESKDF_ROUNDS || nRounds > MAX_AESKDF_ROUNDS)
                {
                    throw PassStoreFileException.InvalidCryptoHeader;
                }
                // пропустить 32 байта сида AES-KDF
                f.Seek(32, SeekOrigin.Current);
            }
        }
        catch (EndOfStreamException)
        {
            throw PassStoreFileException.UnexpectedEndOfFile;
        }
    }
}
