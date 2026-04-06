// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Sync.Tests;

public class SyncSummaryTests
{
    [Fact]
    public void Summarize_AddPlan_ShowsAddedCount()
    {
        var plan = new SyncPlan
        {
            DocumentId = "doc-1",
            Action = SyncAction.Add,
            ChunksToAdd = ["c1", "c2", "c3"],
            NewVersion = 1
        };

        var summary = SyncSummary.Summarize(plan);

        summary.Should().Contain("[Add]");
        summary.Should().Contain("doc-1");
        summary.Should().Contain("+3 added");
    }

    [Fact]
    public void Summarize_SkipPlan_ShowsNoChanges()
    {
        var plan = new SyncPlan
        {
            DocumentId = "doc-1",
            Action = SyncAction.Skip,
            ChunksUnchanged = ["c1"],
            PreviousVersion = 2,
            NewVersion = 2
        };

        var summary = SyncSummary.Summarize(plan);

        summary.Should().Contain("[Skip]");
        summary.Should().Contain("no changes");
    }

    [Fact]
    public void Summarize_UpdatePlan_ShowsAllCategories()
    {
        var plan = new SyncPlan
        {
            DocumentId = "doc-1",
            Action = SyncAction.Update,
            ChunksToAdd = ["c4"],
            ChunksToUpdate = ["c2"],
            ChunksToDelete = ["c3"],
            ChunksUnchanged = ["c1"],
            PreviousVersion = 1,
            NewVersion = 2
        };

        var summary = SyncSummary.Summarize(plan);

        summary.Should().Contain("[Update]");
        summary.Should().Contain("+1 added");
        summary.Should().Contain("~1 updated");
        summary.Should().Contain("-1 deleted");
        summary.Should().Contain("=1 unchanged");
    }

    [Fact]
    public void Summarize_DeletePlan_ShowsDeletedCount()
    {
        var plan = new SyncPlan
        {
            DocumentId = "doc-1",
            Action = SyncAction.Delete,
            ChunksToDelete = ["c1", "c2"],
            NewVersion = 2
        };

        var summary = SyncSummary.Summarize(plan);

        summary.Should().Contain("[Delete]");
        summary.Should().Contain("-2 deleted");
    }

    [Fact]
    public void SummarizeAll_EmptyList_ReturnsNoPlansMessage()
    {
        var summary = SyncSummary.SummarizeAll([]);

        summary.Should().Be("No sync plans to summarize.");
    }

    [Fact]
    public void SummarizeAll_MultiplePlans_ShowsTotals()
    {
        var plans = new List<SyncPlan>
        {
            new() { DocumentId = "doc-1", Action = SyncAction.Add, ChunksToAdd = ["c1"], NewVersion = 1 },
            new() { DocumentId = "doc-2", Action = SyncAction.Skip, ChunksUnchanged = ["c2"], NewVersion = 1 },
            new() { DocumentId = "doc-3", Action = SyncAction.Update, ChunksToUpdate = ["c3"], NewVersion = 2 },
            new() { DocumentId = "doc-4", Action = SyncAction.Delete, ChunksToDelete = ["c4"], NewVersion = 2 }
        };

        var summary = SyncSummary.SummarizeAll(plans);

        summary.Should().Contain("Sync summary (4 documents):");
        summary.Should().Contain("1 added");
        summary.Should().Contain("1 updated");
        summary.Should().Contain("1 deleted");
        summary.Should().Contain("1 skipped");
    }
}
