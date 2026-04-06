// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Represents a specific quality issue found during document analysis.
/// </summary>
/// <param name="Code">Machine-readable issue code (e.g., "EMPTY_BLOCK", "OCR_GARBLED").</param>
/// <param name="Severity">Severity level of the issue.</param>
/// <param name="Message">Human-readable description of the issue.</param>
/// <param name="BlockId">Optional identifier of the block where the issue was found.</param>
/// <param name="SectionId">Optional identifier of the section where the issue was found.</param>
public record QualityIssue(
    string Code,
    QualitySeverity Severity,
    string Message,
    string? BlockId = null,
    string? SectionId = null);
