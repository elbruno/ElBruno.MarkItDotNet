// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Metadata;

/// <summary>
/// Classification of the document type based on format and content structure.
/// </summary>
public enum DocumentType
{
    /// <summary>Unknown or unclassified document type.</summary>
    Unknown = 0,

    /// <summary>A structured report document.</summary>
    Report,

    /// <summary>An article or blog post.</summary>
    Article,

    /// <summary>A presentation or slide deck.</summary>
    Presentation,

    /// <summary>A spreadsheet or tabular data document.</summary>
    Spreadsheet,

    /// <summary>A technical manual or user guide.</summary>
    Manual,

    /// <summary>A legal document such as a contract or agreement.</summary>
    Legal,

    /// <summary>A letter or correspondence.</summary>
    Letter,
}
