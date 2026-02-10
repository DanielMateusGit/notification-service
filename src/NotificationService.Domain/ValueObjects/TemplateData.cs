namespace NotificationService.Domain.ValueObjects;

/// <summary>
/// Value Object: Dati per il rendering di un Template
/// Immutabile - contiene i valori da sostituire ai placeholder
/// </summary>
public class TemplateData
{
    private readonly Dictionary<string, string> _values;

    /// <summary>
    /// Crea un nuovo TemplateData con i valori specificati
    /// </summary>
    /// <param name="values">Dictionary con chiave = nome placeholder, valore = valore da sostituire</param>
    /// <exception cref="ArgumentNullException">Se values è null</exception>
    public TemplateData(Dictionary<string, string> values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        // Copia difensiva per garantire immutabilità
        _values = new Dictionary<string, string>(values);
    }

    /// <summary>
    /// Crea un TemplateData vuoto (nessun placeholder)
    /// </summary>
    public static TemplateData Empty => new(new Dictionary<string, string>());

    /// <summary>
    /// Ottiene il valore di un placeholder
    /// </summary>
    /// <param name="key">Nome del placeholder</param>
    /// <returns>Valore associato</returns>
    /// <exception cref="KeyNotFoundException">Se il placeholder non esiste</exception>
    public string GetValue(string key)
    {
        if (!_values.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Placeholder '{key}' not found in template data");

        return value;
    }

    /// <summary>
    /// Verifica se un placeholder esiste
    /// </summary>
    /// <param name="key">Nome del placeholder</param>
    /// <returns>True se esiste</returns>
    public bool HasValue(string key) => _values.ContainsKey(key);

    /// <summary>
    /// Restituisce tutti i placeholder disponibili
    /// </summary>
    public IReadOnlyCollection<string> Keys => _values.Keys.ToList().AsReadOnly();

    // ===== VALUE OBJECT EQUALITY =====

    public override bool Equals(object? obj)
    {
        if (obj is not TemplateData other)
            return false;

        if (_values.Count != other._values.Count)
            return false;

        foreach (var kvp in _values)
        {
            if (!other._values.TryGetValue(kvp.Key, out var otherValue) || kvp.Value != otherValue)
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var kvp in _values.OrderBy(x => x.Key))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(TemplateData? left, TemplateData? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(TemplateData? left, TemplateData? right) => !(left == right);
}
