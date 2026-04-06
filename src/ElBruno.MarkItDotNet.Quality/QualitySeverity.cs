// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Quality;

/// <summary>
/// Severity level for a quality issue detected during analysis.
/// </summary>
public enum QualitySeverity
{
    /// <summary>Informational observation, not necessarily a problem.</summary>
    Info,

    /// <summary>Potential quality concern that may warrant attention.</summary>
    Warning,

    /// <summary>Significant quality problem that likely affects usability.</summary>
    Error,
}
