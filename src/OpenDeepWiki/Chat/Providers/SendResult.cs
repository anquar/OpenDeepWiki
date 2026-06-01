namespace OpenDeepWiki.Chat.Providers;

/// <summary>
/// Result of a message send operation.
/// </summary>
public record SendResult(
    bool Success,
    string? MessageId = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    bool ShouldRetry = true);
