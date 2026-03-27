using System;
using System.Collections.Generic;

namespace KeyKeeper.PasswordStore;

public static class Base32
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static bool Validate(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        foreach (char c in input)
        {
            if (!Base32Alphabet.Contains(c) && c != '=')
                return false;
        }

        return IsValidPadding(input);
    }

    private static bool IsValidPadding(string input)
    {
        if (input.Length % 8 != 0)
            return false;

        int paddingCount = 0;
        for (int i = input.Length - 1; i >= 0; i--)
        {
            if (input[i] == '=')
                paddingCount++;
            else if (paddingCount > 0)
                return false;
        }

        return paddingCount <= 6;
    }

    public static byte[] Decode(string input)
    {
        if (!Validate(input))
            throw new ArgumentException("Invalid Base32 string", nameof(input));

        input = input.TrimEnd('=');

        var output = new List<byte>();
        int bitCount = 0;
        int bitBuffer = 0;

        foreach (char c in input)
        {
            int value = Base32Alphabet.IndexOf(c);
            if (value < 0)
                throw new ArgumentException($"Invalid Base32 character: {c}", nameof(input));

            bitBuffer = (bitBuffer << 5) | value;
            bitCount += 5;

            if (bitCount >= 8)
            {
                bitCount -= 8;
                output.Add((byte)((bitBuffer >> bitCount) & 0xFF));
            }
        }

        return output.ToArray();
    }

    public static string Encode(byte[] input)
    {
        if (input == null || input.Length == 0)
            return string.Empty;

        var output = new System.Text.StringBuilder();
        int bitCount = 0;
        int bitBuffer = 0;

        foreach (byte b in input)
        {
            bitBuffer = (bitBuffer << 8) | b;
            bitCount += 8;

            while (bitCount >= 5)
            {
                bitCount -= 5;
                int index = (bitBuffer >> bitCount) & 0x1F;
                output.Append(Base32Alphabet[index]);
            }
        }

        if (bitCount > 0)
        {
            bitBuffer <<= (5 - bitCount);
            int index = bitBuffer & 0x1F;
            output.Append(Base32Alphabet[index]);
        }

        int paddingCount = (8 - (output.Length % 8)) % 8;
        output.Append(new string('=', paddingCount));

        return output.ToString();
    }
}
