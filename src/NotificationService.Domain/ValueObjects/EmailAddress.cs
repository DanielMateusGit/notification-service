using System.Text.RegularExpressions;

namespace NotificationService.Domain.ValueObjects;

/// <summary>
/// Value Object: Indirizzo email validato
/// Immutabile, self-validating, equality by value
/// </summary>
public partial class EmailAddress : IEquatable<EmailAddress>
{
    // Regex semplificata ma efficace per email
    // Non copre il 100% dei casi RFC 5322, ma è pragmatica
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();

    /// <summary>
    /// L'indirizzo email (normalizzato in lowercase)
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Crea un nuovo EmailAddress validato
    /// </summary>
    /// <param name="email">L'indirizzo email</param>
    /// <exception cref="ArgumentException">Se l'email non è valida</exception>
    public EmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var normalized = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));

        Value = normalized;
    }

    /// <summary>
    /// Estrae il dominio dall'email
    /// </summary>
    public string Domain => Value.Split('@')[1];

    /// <summary>
    /// Estrae la parte locale (prima della @)
    /// </summary>
    public string LocalPart => Value.Split('@')[0];

    // ===== EQUALITY (Value Object) =====

    public override bool Equals(object? obj) => Equals(obj as EmailAddress);

    public bool Equals(EmailAddress? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(EmailAddress? left, EmailAddress? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(EmailAddress? left, EmailAddress? right) => !(left == right);

    // ===== CONVERSION =====

    public override string ToString() => Value;

    /// <summary>
    /// Conversione implicita a string per comodità
    /// </summary>
    public static implicit operator string(EmailAddress email) => email.Value;
}
