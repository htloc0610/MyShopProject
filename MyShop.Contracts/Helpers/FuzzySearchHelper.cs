using System;
using FuzzySharp;

namespace MyShop.Contracts.Helpers;

/// <summary>
/// Shared Fuzzy search helper
/// Uses Levenshtein distance algorithm to find similar strings.
/// </summary>
public static class FuzzySearchHelper
{
    /// <summary>
    /// Minimum similarity score (0-100) to consider a match
    /// 80+ = High similarity (recommended)
    /// 60-79 = Medium similarity
    /// Below 60 = Low similarity (may produce too many false positives)
    /// </summary>
    public const int DEFAULT_THRESHOLD = 70;

    /// <summary>
    /// Checks if the search keyword matches the target text using fuzzy matching
    /// </summary>
    /// <param name="keyword">Search keyword</param>
    /// <param name="target">Target text to match against</param>
    /// <param name="threshold">Minimum similarity score (0-100)</param>
    /// <returns>True if similarity score is above threshold</returns>
    public static bool IsMatch(string? keyword, string? target, int threshold = DEFAULT_THRESHOLD)
    {
        if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(target))
            return false;

        // Normalize strings (lowercase, trim)
        var normalizedKeyword = keyword.Trim().ToLowerInvariant();
        var normalizedTarget = target.Trim().ToLowerInvariant();

        // Exact match - always return true
        if (normalizedTarget.Contains(normalizedKeyword))
            return true;

        // Fuzzy match using ratio algorithm
        var score = Fuzz.PartialRatio(normalizedKeyword, normalizedTarget);
        
        return score >= threshold;
    }

    /// <summary>
    /// Gets the similarity score between two strings
    /// </summary>
    /// <param name="keyword">Search keyword</param>
    /// <param name="target">Target text</param>
    /// <returns>Similarity score (0-100)</returns>
    public static int GetSimilarityScore(string? keyword, string? target)
    {
        if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(target))
            return 0;

        var normalizedKeyword = keyword.Trim().ToLowerInvariant();
        var normalizedTarget = target.Trim().ToLowerInvariant();

        return Fuzz.PartialRatio(normalizedKeyword, normalizedTarget);
    }
}
