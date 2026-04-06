// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;

namespace ElBruno.MarkItDotNet.Sync;

/// <summary>
/// Provides human-readable summaries of sync plans.
/// </summary>
public static class SyncSummary
{
    /// <summary>
    /// Produces a human-readable summary of a single sync plan.
    /// </summary>
    /// <param name="plan">The sync plan to summarize.</param>
    /// <returns>A summary string.</returns>
    public static string Summarize(SyncPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var sb = new StringBuilder();
        sb.Append($"[{plan.Action}] {plan.DocumentId} (v{plan.NewVersion})");

        if (plan.Action == SyncAction.Skip)
        {
            sb.Append(" — no changes");
            return sb.ToString();
        }

        var parts = new List<string>();
        if (plan.ChunksToAdd.Count > 0)
        {
            parts.Add($"+{plan.ChunksToAdd.Count} added");
        }

        if (plan.ChunksToUpdate.Count > 0)
        {
            parts.Add($"~{plan.ChunksToUpdate.Count} updated");
        }

        if (plan.ChunksToDelete.Count > 0)
        {
            parts.Add($"-{plan.ChunksToDelete.Count} deleted");
        }

        if (plan.ChunksUnchanged.Count > 0)
        {
            parts.Add($"={plan.ChunksUnchanged.Count} unchanged");
        }

        if (parts.Count > 0)
        {
            sb.Append($" — {string.Join(", ", parts)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Produces a human-readable summary for a batch of sync plans.
    /// </summary>
    /// <param name="plans">The sync plans to summarize.</param>
    /// <returns>A multi-line summary string.</returns>
    public static string SummarizeAll(IEnumerable<SyncPlan> plans)
    {
        ArgumentNullException.ThrowIfNull(plans);

        var planList = plans.ToList();
        if (planList.Count == 0)
        {
            return "No sync plans to summarize.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Sync summary ({planList.Count} documents):");

        foreach (var plan in planList)
        {
            sb.AppendLine($"  {Summarize(plan)}");
        }

        var adds = planList.Count(p => p.Action == SyncAction.Add);
        var updates = planList.Count(p => p.Action == SyncAction.Update);
        var deletes = planList.Count(p => p.Action == SyncAction.Delete);
        var skips = planList.Count(p => p.Action == SyncAction.Skip);

        sb.Append($"Totals: {adds} added, {updates} updated, {deletes} deleted, {skips} skipped");

        return sb.ToString();
    }
}
