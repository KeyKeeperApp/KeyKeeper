using System;
using System.Security.Cryptography;

namespace KeyKeeper.PasswordStore;

/// <summary>
/// RFC 6238 Time-based One-Time Password (TOTP) code generator.
/// </summary>
public static class TotpCodeGenerator
{
    private const long UnixEpochTicks = 621355968000000000L;

    /// <summary>
    /// Generates a TOTP code for the given parameters at the current time.
    /// </summary>
    public static string GenerateCode(TotpParameters totp)
    {
        return GenerateCodeAtTime(totp, DateTime.UtcNow);
    }

    /// <summary>
    /// Generates a TOTP code for the given parameters at a specific time.
    /// </summary>
    public static string GenerateCodeAtTime(TotpParameters totp, DateTime utcTime)
    {
        try
        {
            byte[] secretBytes = Convert.FromHexString(totp.Secret);
            long timeCounter = GetTimeCounter(utcTime, totp.Period);
            byte[] counterBytes = BitConverter.GetBytes(timeCounter);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            byte[] hash = ComputeHmac(secretBytes, counterBytes, totp.Algorithm);
            int code = DynamicTruncate(hash);
            int digits = Math.Min(totp.Digits, 10);
            int modulo = (int)Math.Pow(10, digits);
            code %= modulo;

            return code.ToString().PadLeft(digits, '0');
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Gets the remaining seconds until the next TOTP code change.
    /// </summary>
    public static int GetSecondsUntilNextCode(TotpParameters totp)
    {
        return GetSecondsUntilNextCodeAtTime(totp, DateTime.UtcNow);
    }

    /// <summary>
    /// Gets the remaining seconds until the next code change at a specific time.
    /// </summary>
    public static int GetSecondsUntilNextCodeAtTime(TotpParameters totp, DateTime utcTime)
    {
        long unixTimestamp = GetUnixTimestamp(utcTime);
        long secondsInCurrentPeriod = unixTimestamp % totp.Period;
        return totp.Period - (int)secondsInCurrentPeriod;
    }

    private static long GetTimeCounter(DateTime utcTime, int period)
    {
        long unixTimestamp = GetUnixTimestamp(utcTime);
        return unixTimestamp / period;
    }

    private static long GetUnixTimestamp(DateTime utcTime)
    {
        return (utcTime.Ticks - UnixEpochTicks) / TimeSpan.TicksPerSecond;
    }

    private static byte[] ComputeHmac(byte[] secret, byte[] counter, TotpAlgorithm algorithm)
    {
        return algorithm switch
        {
            TotpAlgorithm.SHA256 => HMACSHA256.HashData(secret, counter),
            TotpAlgorithm.SHA512 => HMACSHA512.HashData(secret, counter),
            _ => HMACSHA1.HashData(secret, counter),
        };
    }

    /// <summary>
    /// Dynamic truncate per RFC 6238 section 5.4.
    /// </summary>
    private static int DynamicTruncate(byte[] hash)
    {
        int offset = hash[^1] & 0xf;
        int p = (hash[offset] & 0x7f) << 24
              | (hash[offset + 1] & 0xff) << 16
              | (hash[offset + 2] & 0xff) << 8
              | (hash[offset + 3] & 0xff);
        return p;
    }
}
