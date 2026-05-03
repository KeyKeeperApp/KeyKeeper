using System;
using KeyKeeper.PasswordStore;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.ViewModels;

public class EntryEditViewModel : ViewModelBase
{
    private string _entryName = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isTotpConfigured;
    private string _totpSecret = string.Empty;
    private TotpAlgorithm _totpAlgorithm = TotpAlgorithm.SHA1;
    private string _totpDigits = "6";
    private string _totpPeriod = "30";
    private string _totpIssuer = string.Empty;
    private string _totpAccountName = string.Empty;
    private string _secretValidationError = string.Empty;
    private string _digitsValidationError = string.Empty;
    private string _periodValidationError = string.Empty;
    private PassStoreEntryPassword? _editedEntry;
    private Guid? _existingId;
    private DateTime? _createdAt;

    public string EntryName
    {
        get => _entryName;
        set
        {
            _entryName = value;
            OnPropertyChanged(nameof(EntryName));
            OnPropertyChanged(nameof(SaveAllowed));
        }
    }

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(SaveAllowed));
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(SaveAllowed));
        }
    }

    public bool IsTotpConfigured
    {
        get => _isTotpConfigured;
        set
        {
            _isTotpConfigured = value;
            OnPropertyChanged(nameof(IsTotpConfigured));
            ValidateTotpSecret();
            ValidateTotpDigits();
            ValidateTotpPeriod();
        }
    }

    public string TotpSecret
    {
        get => _totpSecret;
        set
        {
            _totpSecret = value;
            OnPropertyChanged(nameof(TotpSecret));
            ValidateTotpSecret();
        }
    }

    public TotpAlgorithm TotpAlgorithm
    {
        get => _totpAlgorithm;
        set { _totpAlgorithm = value; OnPropertyChanged(nameof(TotpAlgorithm)); }
    }

    public string TotpDigits
    {
        get => _totpDigits;
        set
        {
            _totpDigits = value;
            OnPropertyChanged(nameof(TotpDigits));
            ValidateTotpDigits();
        }
    }

    public string TotpPeriod
    {
        get => _totpPeriod;
        set
        {
            _totpPeriod = value;
            OnPropertyChanged(nameof(TotpPeriod));
            ValidateTotpPeriod();
        }
    }

    public string TotpIssuer
    {
        get => _totpIssuer;
        set { _totpIssuer = value; OnPropertyChanged(nameof(TotpIssuer)); }
    }

    public string TotpAccountName
    {
        get => _totpAccountName;
        set { _totpAccountName = value; OnPropertyChanged(nameof(TotpAccountName)); }
    }

    public string SecretValidationError
    {
        get => _secretValidationError;
        private set { _secretValidationError = value; OnPropertyChanged(nameof(SecretValidationError)); }
    }

    public string DigitsValidationError
    {
        get => _digitsValidationError;
        private set { _digitsValidationError = value; OnPropertyChanged(nameof(DigitsValidationError)); }
    }

    public string PeriodValidationError
    {
        get => _periodValidationError;
        private set { _periodValidationError = value; OnPropertyChanged(nameof(PeriodValidationError)); }
    }

    public PassStoreEntryPassword? EditedEntry
    {
        get => _editedEntry;
        private set { _editedEntry = value; OnPropertyChanged(nameof(EditedEntry)); }
    }

    public bool SaveAllowed
    {
        get
        {
            bool loginFieldsValid = !string.IsNullOrEmpty(EntryName?.Trim())
                && !string.IsNullOrEmpty(Username?.Trim())
                && !string.IsNullOrEmpty(Password);

            if (!loginFieldsValid)
                return false;

            if (IsTotpConfigured)
            {
                bool totpValid = string.IsNullOrEmpty(SecretValidationError)
                    && string.IsNullOrEmpty(DigitsValidationError)
                    && string.IsNullOrEmpty(PeriodValidationError);
                return totpValid;
            }
            return true;
        }
    }

    public void LoadEntry(PassStoreEntryPassword entry)
    {
        _existingId = entry.Id;
        _createdAt = entry.CreationDate;

        EntryName = entry.Name;
        Username = entry.Username.Value;
        Password = entry.Password.Value;

        if (entry.Totp != null)
        {
            IsTotpConfigured = true;
            TotpSecret = entry.Totp.GetBase32Secret();
            TotpAlgorithm = entry.Totp.Algorithm;
            TotpDigits = entry.Totp.Digits.ToString();
            TotpPeriod = entry.Totp.Period.ToString();
            TotpIssuer = entry.Totp.Issuer ?? string.Empty;
            TotpAccountName = entry.Totp.AccountName ?? string.Empty;
        }
        else
        {
            IsTotpConfigured = false;
        }
    }

    public void ConfigureTotp()
    {
        if (!IsTotpConfigured)
        {
            IsTotpConfigured = true;
            TotpSecret = string.Empty;
            TotpAlgorithm = TotpAlgorithm.SHA1;
            TotpDigits = "6";
            TotpPeriod = "30";
            TotpIssuer = string.Empty;
            TotpAccountName = string.Empty;
        }
    }

    public void RemoveTotp()
    {
        IsTotpConfigured = false;
        TotpSecret = string.Empty;
        TotpAlgorithm = TotpAlgorithm.SHA1;
        TotpDigits = "6";
        TotpPeriod = "30";
        TotpIssuer = string.Empty;
        TotpAccountName = string.Empty;
        SecretValidationError = string.Empty;
        DigitsValidationError = string.Empty;
        PeriodValidationError = string.Empty;
    }

    public bool ParseOtpauthUrl(string uri)
    {
        try
        {
            if (string.IsNullOrEmpty(uri))
                return false;

            TotpParameters totp = TotpParameters.FromUri(uri);
            TotpSecret = totp.GetBase32Secret();
            TotpAlgorithm = totp.Algorithm;
            TotpDigits = totp.Digits.ToString();
            TotpPeriod = totp.Period.ToString();
            TotpIssuer = totp.Issuer ?? string.Empty;
            TotpAccountName = totp.AccountName ?? string.Empty;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void ValidateTotpSecret()
    {
        if (!IsTotpConfigured)
        {
            SecretValidationError = string.Empty;
        }
        else if (string.IsNullOrWhiteSpace(TotpSecret))
        {
            SecretValidationError = "Secret is required";
        }
        else if (!Base32.Validate(TotpSecret))
        {
            SecretValidationError = "Secret must be valid Base32 (A-Z, 2-7, and = padding)";
        }
        else
        {
            SecretValidationError = string.Empty;
        }

        OnPropertyChanged(nameof(SaveAllowed));
    }

    private void ValidateTotpDigits()
    {
        if (!IsTotpConfigured)
        {
            DigitsValidationError = string.Empty;
        }
        else if (!int.TryParse(TotpDigits, out int digits) || digits < 6 || digits > 8)
        {
            DigitsValidationError = "Digits must be 6, 7, or 8";
        }
        else
        {
            DigitsValidationError = string.Empty;
        }

        OnPropertyChanged(nameof(SaveAllowed));
    }

    private void ValidateTotpPeriod()
    {
        if (!IsTotpConfigured)
        {
            PeriodValidationError = string.Empty;
        }
        else if (!int.TryParse(TotpPeriod, out int period) || period <= 0)
        {
            PeriodValidationError = "Period must be a positive number (seconds)";
        }
        else
        {
            PeriodValidationError = string.Empty;
        }

        OnPropertyChanged(nameof(SaveAllowed));
    }

    public void CreateEntry()
    {
        if (!SaveAllowed)
            return;

        Guid id = _existingId ?? Guid.NewGuid();
        DateTime created = _createdAt ?? DateTime.UtcNow;

        TotpParameters? totp = null;
        if (IsTotpConfigured && !string.IsNullOrEmpty(TotpSecret))
        {
            try
            {
                totp = TotpParameters.FromBase32Secret(
                    TotpSecret,
                    TotpAlgorithm,
                    int.Parse(TotpDigits),
                    int.Parse(TotpPeriod),
                    string.IsNullOrEmpty(TotpIssuer) ? null : TotpIssuer,
                    string.IsNullOrEmpty(TotpAccountName) ? null : TotpAccountName
                );
            }
            catch (Exception)
            {
                totp = null;
            }
        }

        EditedEntry = new PassStoreEntryPassword(
            id,
            created,
            DateTime.UtcNow,
            BuiltinEntryIconType.DEFAULT,
            EntryName.Trim(),
            new LoginField() { Type = LOGIN_FIELD_USERNAME_ID, Value = Username.Trim() },
            new LoginField() { Type = LOGIN_FIELD_PASSWORD_ID, Value = Password },
            null,
            totp
        );
    }
}
