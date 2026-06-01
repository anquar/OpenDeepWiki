using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenDeepWiki.Chat.Routing;
using OpenDeepWiki.EFCore;

namespace OpenDeepWiki.Chat.Execution;

/// <summary>
/// Resolves messaging platform user IDs to DeepWiki user IDs via email matching.
/// </summary>
public interface IChatUserResolver
{
    /// <summary>
    /// Resolves a platform user ID to a DeepWiki user ID.
    /// Returns null if the user cannot be mapped.
    /// </summary>
    Task<string?> ResolveDeepWikiUserIdAsync(string platformUserId, string platform, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves messaging platform user IDs to DeepWiki user IDs by email matching.
/// Singleton service with in-memory caching.
/// </summary>
public class ChatUserResolver : IChatUserResolver
{
    private readonly IContextFactory _contextFactory;
    private readonly ILogger<ChatUserResolver> _logger;

    // Cache: "platform:userId" -> DeepWiki user ID (or null for unmapped)
    private readonly ConcurrentDictionary<string, CachedResolution> _cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public ChatUserResolver(
        IContextFactory contextFactory,
        ILogger<ChatUserResolver> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<string?> ResolveDeepWikiUserIdAsync(
        string platformUserId,
        string platform,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(platformUserId) || string.IsNullOrWhiteSpace(platform))
            return null;

        var cacheKey = $"{platform}:{platformUserId}";

        // Check cache (with TTL)
        if (_cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
        {
            _logger.LogDebug("Cache hit for {CacheKey}: {UserId}", cacheKey, cached.DeepWikiUserId ?? "(unmapped)");
            return cached.DeepWikiUserId;
        }

        string? deepWikiUserId = null;

        // Cache the result (even null = "unmapped")
        _cache[cacheKey] = new CachedResolution(deepWikiUserId, DateTimeOffset.UtcNow.Add(CacheDuration));
        return deepWikiUserId;
    }

    /// <summary>
    /// Finds a DeepWiki user by email address.
    /// </summary>
    private async Task<string?> FindDeepWikiUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateContext();
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.Status == 1, cancellationToken);
        return user?.Id;
    }

    private record CachedResolution(string? DeepWikiUserId, DateTimeOffset ExpiresAt)
    {
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    }
}
