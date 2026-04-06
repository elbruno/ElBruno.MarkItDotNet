// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Simple heuristic language detector based on common word frequency fingerprinting.
/// Supports English, Spanish, French, German, Portuguese, and Italian.
/// </summary>
internal static partial class LanguageDetector
{
    private static readonly Dictionary<string, HashSet<string>> LanguageFingerprints = new(StringComparer.Ordinal)
    {
        ["en"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "be", "to", "of", "and", "a", "in", "that", "have", "i",
            "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
            "this", "but", "his", "by", "from", "they", "we", "her", "she", "an",
            "will", "my", "one", "all", "would", "there", "their", "what", "so", "if",
            "is", "are", "was", "were", "been", "has", "had", "did", "can", "could",
        },
        ["es"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "de", "la", "que", "el", "en", "y", "a", "los", "del", "se",
            "las", "por", "un", "para", "con", "no", "una", "su", "al", "es",
            "lo", "como", "más", "pero", "sus", "le", "ya", "o", "este", "si",
            "porque", "esta", "entre", "cuando", "muy", "sin", "sobre", "también", "me", "hasta",
            "hay", "donde", "quien", "desde", "todo", "nos", "durante", "todos", "uno", "les",
        },
        ["fr"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "de", "la", "le", "et", "les", "des", "en", "un", "du", "une",
            "que", "est", "dans", "qui", "par", "pour", "au", "il", "sur", "ne",
            "se", "pas", "plus", "ce", "avec", "ou", "sont", "son", "cette", "aux",
            "ont", "ses", "mais", "comme", "été", "aussi", "nous", "même", "fait", "elle",
            "peut", "très", "tous", "leur", "bien", "ces", "vous", "ils", "entre", "être",
        },
        ["de"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "der", "die", "und", "in", "den", "von", "zu", "das", "mit", "sich",
            "des", "auf", "für", "ist", "im", "dem", "nicht", "ein", "eine", "als",
            "auch", "es", "an", "werden", "aus", "er", "hat", "dass", "sie", "nach",
            "wird", "bei", "einer", "um", "am", "sind", "noch", "wie", "einem", "über",
            "so", "zum", "war", "haben", "nur", "oder", "aber", "vor", "zur", "bis",
        },
        ["pt"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "de", "a", "o", "que", "e", "do", "da", "em", "um", "para",
            "com", "não", "uma", "os", "no", "se", "na", "por", "mais", "as",
            "dos", "como", "mas", "foi", "ao", "ele", "das", "tem", "à", "seu",
            "sua", "ou", "ser", "quando", "muito", "há", "nos", "já", "está", "eu",
            "também", "só", "pelo", "pela", "até", "isso", "ela", "entre", "era", "depois",
        },
        ["it"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "di", "e", "il", "la", "che", "in", "un", "è", "per", "non",
            "una", "del", "si", "le", "da", "con", "dei", "al", "sono", "ha",
            "lo", "gli", "anche", "come", "nel", "più", "questo", "su", "se", "della",
            "ma", "alla", "delle", "nella", "cui", "tutti", "essere", "stato", "molto", "dopo",
            "già", "era", "tutto", "tra", "fatto", "modo", "quella", "suo", "loro", "ogni",
        },
    };

    /// <summary>
    /// Detects the most likely language of the given text using word frequency fingerprinting.
    /// Returns null if no confident match is found.
    /// </summary>
    internal static string? Detect(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var words = WordSplitRegex().Split(text.ToLowerInvariant())
            .Where(w => w.Length > 0)
            .ToList();

        if (words.Count == 0)
        {
            return null;
        }

        var bestLanguage = (string?)null;
        var bestScore = 0.0;

        foreach (var (language, fingerprint) in LanguageFingerprints)
        {
            var matchCount = words.Count(w => fingerprint.Contains(w));
            var score = (double)matchCount / words.Count;

            if (score > bestScore)
            {
                bestScore = score;
                bestLanguage = language;
            }
        }

        // Require at least 10% of words to match a language fingerprint
        return bestScore >= 0.10 ? bestLanguage : null;
    }

    [GeneratedRegex(@"[^\p{L}\p{M}''-]+")]
    private static partial Regex WordSplitRegex();
}
