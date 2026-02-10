using System.Text.RegularExpressions;

namespace NotificationService.Domain.ValueObjects;

/// <summary>
/// Value Object: Numero di telefono in formato E.164
/// Formato: +[country code][number] es. +391234567890
/// Immutabile, self-validating, equality by value
/// </summary>
public partial class PhoneNumber : IEquatable<PhoneNumber>
{
    // E.164: + seguito da 1-15 cifre
    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Regex();

    /// <summary>
    /// Il numero di telefono in formato E.164
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Crea un nuovo PhoneNumber validato
    /// </summary>
    /// <param name="phoneNumber">Il numero in formato E.164 (+391234567890)</param>
    /// <exception cref="ArgumentException">Se il formato non è valido</exception>
    public PhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        // Rimuovi spazi e trattini (normalizzazione)
        var normalized = phoneNumber
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        if (!E164Regex().IsMatch(normalized))
            throw new ArgumentException(
                $"Invalid phone number format: {phoneNumber}. Expected E.164 format (+391234567890)",
                nameof(phoneNumber));

        Value = normalized;
    }

    /// <summary>
    /// Estrae il country code (prime cifre dopo il +)
    /// Nota: semplificato, assume 2 cifre per country code
    /// </summary>
    public string CountryCode => Value.Substring(1, 2);

    /// <summary>
    /// Restituisce il numero senza il prefisso internazionale
    /// </summary>
    public string NationalNumber => Value.Substring(3);

    // ===== EQUALITY (Value Object) =====

    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

    public bool Equals(PhoneNumber? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !(left == right);

    // ===== CONVERSION =====

    public override string ToString() => Value;

    /// <summary>
    /// Conversione implicita a string per comodità
    /// </summary>
    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
