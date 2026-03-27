using System;
using System.Web;

namespace KeyKeeper.PasswordStore;

public enum TotpAlgorithm { SHA1, SHA256, SHA512 }

public class TotpParameters
{
    public string Secret { get; set; }
    public TotpAlgorithm Algorithm { get; set; }
    public int Digits { get; set; }
    public int Period { get; set; }
    public string? Issuer { get; set; }
    public string? AccountName { get; set; }

    public TotpParameters(string secret, TotpAlgorithm algorithm = TotpAlgorithm.SHA1,
                          int digits = 6, int period = 30,
                          string? issuer = null, string? accountName = null)
    {
        Secret = secret;
        Algorithm = algorithm;
        Digits = digits;
        Period = period;
        Issuer = issuer;
        AccountName = accountName;
    }

    public static TotpParameters FromBase32Secret(string base32Secret,
        TotpAlgorithm algorithm = TotpAlgorithm.SHA1,
        int digits = 6, int period = 30,
        string? issuer = null, string? accountName = null)
    {
        if (!Base32.Validate(base32Secret))
            throw new ArgumentException("Invalid Base32-encoded secret", nameof(base32Secret));

        byte[] secretBytes = Base32.Decode(base32Secret);
        string hexSecret = Convert.ToHexString(secretBytes);

        return new TotpParameters(hexSecret, algorithm, digits, period, issuer, accountName);
    }

    public string GetBase32Secret()
    {
        byte[] secretBytes = Convert.FromHexString(Secret);
        return Base32.Encode(secretBytes);
    }

    public static TotpParameters FromUri(string uri)
    {
        if (!uri.StartsWith("otpauth://totp/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("URI must start with otpauth://totp/", nameof(uri));

        Uri parsed = new(uri);

        string label = Uri.UnescapeDataString(parsed.AbsolutePath.TrimStart('/'));
        string? issuer = null;
        int colon = label.IndexOf(':');
        string? accountName;
        if (colon >= 0)
        {
            issuer = label[..colon];
            accountName = label[(colon + 1)..];
        }
        else
        {
            accountName = label;
        }

        var query = HttpUtility.ParseQueryString(parsed.Query);

        string base32Secret = query["secret"] ?? throw new ArgumentException("URI is missing required 'secret' parameter", nameof(uri));

        issuer = query["issuer"] ?? issuer;

        TotpAlgorithm algorithm = (query["algorithm"] ?? "SHA1").ToUpperInvariant() switch
        {
            "SHA256" => TotpAlgorithm.SHA256,
            "SHA512" => TotpAlgorithm.SHA512,
            _ => TotpAlgorithm.SHA1,
        };

        int digits = int.TryParse(query["digits"], out int d) ? d : 6;
        int period = int.TryParse(query["period"], out int p) ? p : 30;

        return FromBase32Secret(base32Secret, algorithm, digits, period, issuer, accountName);
    }
}
