using System.Text.RegularExpressions;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Entity: Template di notifica
/// Rappresenta un formato riutilizzabile per generare contenuti di notifica
/// </summary>
public partial class Template
{
    // Regex per trovare i placeholder {{nome}}
    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderRegex();

    // ===== PROPERTIES (State) =====

    /// <summary>
    /// Identificatore univoco del template
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nome univoco del template (es. "order-shipped", "welcome-email")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Canale per cui è destinato il template
    /// </summary>
    public NotificationChannel Channel { get; private set; }

    /// <summary>
    /// Subject del messaggio (obbligatorio per Email, opzionale per altri canali)
    /// Può contenere placeholder {{nome}}
    /// </summary>
    public string? Subject { get; private set; }

    /// <summary>
    /// Body del messaggio con placeholder {{nome}}
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// Indica se il template è attivo e utilizzabile
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Data di creazione
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Data dell'ultimo aggiornamento
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // ===== CONSTRUCTOR =====

    /// <summary>
    /// Crea un nuovo template
    /// </summary>
    /// <param name="name">Nome univoco del template</param>
    /// <param name="channel">Canale di destinazione</param>
    /// <param name="body">Corpo del messaggio con placeholder</param>
    /// <param name="subject">Subject (obbligatorio per Email)</param>
    /// <exception cref="ArgumentException">Se i parametri non sono validi</exception>
    public Template(
        string name,
        NotificationChannel channel,
        string body,
        string? subject = null)
    {
        // VALIDAZIONI
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty", nameof(body));

        if (channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required for Email templates", nameof(subject));

        // INIZIALIZZAZIONE
        Id = Guid.NewGuid();
        Name = name.Trim().ToLowerInvariant(); // Normalizza il nome
        Channel = channel;
        Subject = subject?.Trim();
        Body = body;
        IsActive = true; // Template nasce attivo
        CreatedAt = DateTime.UtcNow;
    }

    // ===== BEHAVIORS (Business Rules) =====

    /// <summary>
    /// Esegue il rendering del template sostituendo i placeholder con i valori forniti
    /// </summary>
    /// <param name="data">Dati per i placeholder</param>
    /// <returns>Tupla (Subject renderizzato, Body renderizzato)</returns>
    /// <exception cref="InvalidOperationException">Se il template non è attivo</exception>
    /// <exception cref="ArgumentException">Se mancano placeholder richiesti</exception>
    public (string? Subject, string Body) Render(TemplateData data)
    {
        if (!IsActive)
            throw new InvalidOperationException(
                $"Cannot render inactive template '{Name}'. Activate it first.");

        var renderedSubject = Subject != null ? ReplacePlaceholders(Subject, data) : null;
        var renderedBody = ReplacePlaceholders(Body, data);

        return (renderedSubject, renderedBody);
    }

    /// <summary>
    /// Attiva il template rendendolo utilizzabile
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return; // Idempotente

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disattiva il template impedendone l'uso
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return; // Idempotente

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aggiorna il contenuto del template
    /// </summary>
    /// <param name="body">Nuovo body</param>
    /// <param name="subject">Nuovo subject (obbligatorio per Email)</param>
    /// <exception cref="ArgumentException">Se i parametri non sono validi</exception>
    public void UpdateContent(string body, string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty", nameof(body));

        if (Channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required for Email templates", nameof(subject));

        Body = body;
        Subject = subject?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restituisce l'elenco dei placeholder presenti nel template
    /// </summary>
    /// <returns>Lista di nomi placeholder (senza le parentesi graffe)</returns>
    public IReadOnlyList<string> GetPlaceholders()
    {
        var placeholders = new HashSet<string>();

        if (Subject != null)
        {
            foreach (Match match in PlaceholderRegex().Matches(Subject))
            {
                placeholders.Add(match.Groups[1].Value);
            }
        }

        foreach (Match match in PlaceholderRegex().Matches(Body))
        {
            placeholders.Add(match.Groups[1].Value);
        }

        return placeholders.ToList().AsReadOnly();
    }

    // ===== PRIVATE HELPERS =====

    private static string ReplacePlaceholders(string text, TemplateData data)
    {
        return PlaceholderRegex().Replace(text, match =>
        {
            var placeholderName = match.Groups[1].Value;

            if (!data.HasValue(placeholderName))
                throw new ArgumentException(
                    $"Missing required placeholder '{placeholderName}' in template data");

            return data.GetValue(placeholderName);
        });
    }
}
