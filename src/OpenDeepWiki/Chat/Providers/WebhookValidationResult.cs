namespace OpenDeepWiki.Chat.Providers;

/// <summary>
/// Result of a webhook validation operation.
/// </summary>
public record WebhookValidationResult(
    bool IsValid,
    string? Challenge = null,
    string? ErrorMessage = null);
