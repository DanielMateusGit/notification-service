using NotificationService.Domain.Enums;

namespace NotificationService.Domain.ValueObjects;

/// <summary>
/// Value Object: Destinatario di una notifica
/// Wrapper che incapsula il valore validato in base al canale
/// </summary>
public class Recipient : IEquatable<Recipient>
{
    /// <summary>
    /// Il valore del destinatario (email, phone, userId, webhookUrl)
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Il tipo/canale del destinatario
    /// </summary>
    public NotificationChannel Channel { get; }

    /// <summary>
    /// EmailAddress (solo se Channel = Email)
    /// </summary>
    public EmailAddress? Email { get; }

    /// <summary>
    /// PhoneNumber (solo se Channel = Sms)
    /// </summary>
    public PhoneNumber? Phone { get; }

    // ===== PRIVATE CONSTRUCTORS =====

    /// <summary>
    /// Costruttore privato per EF Core (materializzazione da database)
    /// </summary>
    private Recipient()
    {
        Value = string.Empty;
        Channel = NotificationChannel.Email;
    }

    private Recipient(string value, NotificationChannel channel, EmailAddress? email = null, PhoneNumber? phone = null)
    {
        Value = value;
        Channel = channel;
        Email = email;
        Phone = phone;
    }

    // ===== FACTORY METHODS =====

    /// <summary>
    /// Crea un Recipient per email
    /// </summary>
    public static Recipient ForEmail(string email)
    {
        var emailAddress = new EmailAddress(email); // Valida automaticamente
        return new Recipient(emailAddress.Value, NotificationChannel.Email, email: emailAddress);
    }

    /// <summary>
    /// Crea un Recipient per SMS
    /// </summary>
    public static Recipient ForSms(string phoneNumber)
    {
        var phone = new PhoneNumber(phoneNumber); // Valida automaticamente
        return new Recipient(phone.Value, NotificationChannel.Sms, phone: phone);
    }

    /// <summary>
    /// Crea un Recipient per Push notification (userId)
    /// </summary>
    public static Recipient ForPush(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        return new Recipient(userId.Trim(), NotificationChannel.Push);
    }

    /// <summary>
    /// Crea un Recipient per Webhook
    /// </summary>
    public static Recipient ForWebhook(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            throw new ArgumentException("Webhook URL cannot be empty", nameof(webhookUrl));

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid webhook URL: {webhookUrl}", nameof(webhookUrl));

        if (uri.Scheme != "https" && uri.Scheme != "http")
            throw new ArgumentException("Webhook URL must use HTTP or HTTPS", nameof(webhookUrl));

        return new Recipient(uri.ToString(), NotificationChannel.Webhook);
    }

    /// <summary>
    /// Factory method generico che determina il tipo dal canale
    /// </summary>
    public static Recipient Create(string value, NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Email => ForEmail(value),
            NotificationChannel.Sms => ForSms(value),
            NotificationChannel.Push => ForPush(value),
            NotificationChannel.Webhook => ForWebhook(value),
            _ => throw new ArgumentException($"Unknown channel: {channel}", nameof(channel))
        };
    }

    // ===== EQUALITY (Value Object) =====

    public override bool Equals(object? obj) => Equals(obj as Recipient);

    public bool Equals(Recipient? other)
    {
        if (other is null) return false;
        return Value == other.Value && Channel == other.Channel;
    }

    public override int GetHashCode() => HashCode.Combine(Value, Channel);

    public static bool operator ==(Recipient? left, Recipient? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Recipient? left, Recipient? right) => !(left == right);

    // ===== CONVERSION =====

    public override string ToString() => $"{Channel}: {Value}";

    /// <summary>
    /// Conversione implicita a string
    /// </summary>
    public static implicit operator string(Recipient recipient) => recipient.Value;
}
