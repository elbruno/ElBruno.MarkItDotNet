// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Suggested action based on quality analysis results.
/// </summary>
public enum QualityAction
{
    /// <summary>No action needed — document quality is acceptable.</summary>
    None,

    /// <summary>Document should be reviewed by a human before use.</summary>
    Review,

    /// <summary>Document quality suggests re-processing with OCR.</summary>
    FallbackToOcr,

    /// <summary>Document quality suggests re-processing with Azure Document Intelligence.</summary>
    FallbackToDocumentIntelligence,

    /// <summary>Document quality is too low for use; reject entirely.</summary>
    Reject,
}
