// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Represents the type of synchronization action to perform for a document.
/// </summary>
public enum SyncAction
{
    /// <summary>The document is new and all chunks should be added.</summary>
    Add,

    /// <summary>The document has changed and chunks should be diffed and updated.</summary>
    Update,

    /// <summary>The document has been removed and should be soft-deleted.</summary>
    Delete,

    /// <summary>The document is unchanged and no action is needed.</summary>
    Skip
}
