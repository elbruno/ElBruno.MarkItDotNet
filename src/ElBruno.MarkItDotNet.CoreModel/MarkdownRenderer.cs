// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using System.Text;

namespace ElBruno.MarkItDotNet.CoreModel;

/// <summary>
/// Renders a <see cref="Document"/> to a Markdown string representation.
/// Handles headings, paragraphs, tables, figures, lists, and recursive sections.
/// </summary>
public static class MarkdownRenderer
{
    /// <summary>
    /// Renders the entire document as a Markdown string.
    /// </summary>
    /// <param name="document">The document to render.</param>
    /// <returns>A Markdown string representation of the document.</returns>
    public static string Render(Document document)
    {
        var sb = new StringBuilder();

        if (document.Metadata.Title is not null)
        {
            sb.AppendLine($"# {document.Metadata.Title}");
            sb.AppendLine();
        }

        foreach (var section in document.Sections)
        {
            RenderSection(sb, section);
        }

        return sb.ToString().TrimEnd() + Environment.NewLine;
    }

    private static void RenderSection(StringBuilder sb, DocumentSection section)
    {
        if (section.Heading is not null)
        {
            RenderBlock(sb, section.Heading);
        }

        foreach (var block in section.Blocks)
        {
            RenderBlock(sb, block);
        }

        foreach (var sub in section.SubSections)
        {
            RenderSection(sb, sub);
        }
    }

    private static void RenderBlock(StringBuilder sb, DocumentBlock block)
    {
        switch (block)
        {
            case HeadingBlock heading:
                RenderHeading(sb, heading);
                break;
            case ParagraphBlock paragraph:
                RenderParagraph(sb, paragraph);
                break;
            case TableBlock table:
                RenderTable(sb, table);
                break;
            case FigureBlock figure:
                RenderFigure(sb, figure);
                break;
            case ListBlock list:
                RenderList(sb, list);
                break;
            case ListItemBlock listItem:
                RenderListItem(sb, listItem, indent: 0, ordered: false, index: 1);
                sb.AppendLine();
                break;
        }
    }

    private static void RenderHeading(StringBuilder sb, HeadingBlock heading)
    {
        var prefix = new string('#', Math.Clamp(heading.Level, 1, 6));
        sb.AppendLine($"{prefix} {heading.Text}");
        sb.AppendLine();
    }

    private static void RenderParagraph(StringBuilder sb, ParagraphBlock paragraph)
    {
        sb.AppendLine(paragraph.Text);
        sb.AppendLine();
    }

    private static void RenderTable(StringBuilder sb, TableBlock table)
    {
        if (table.Headers.Count == 0)
        {
            return;
        }

        // Header row
        sb.AppendLine("| " + string.Join(" | ", table.Headers) + " |");

        // Separator row
        sb.AppendLine("| " + string.Join(" | ", table.Headers.Select(_ => "---")) + " |");

        // Data rows
        foreach (var row in table.Rows)
        {
            sb.AppendLine("| " + string.Join(" | ", row) + " |");
        }

        sb.AppendLine();
    }

    private static void RenderFigure(StringBuilder sb, FigureBlock figure)
    {
        var alt = figure.AltText ?? string.Empty;
        var path = figure.ImagePath ?? string.Empty;
        sb.AppendLine($"![{alt}]({path})");

        if (figure.Caption is not null)
        {
            sb.AppendLine();
            sb.AppendLine($"*{figure.Caption}*");
        }

        sb.AppendLine();
    }

    private static void RenderList(StringBuilder sb, ListBlock list)
    {
        for (var i = 0; i < list.Items.Count; i++)
        {
            RenderListItem(sb, list.Items[i], indent: 0, ordered: list.IsOrdered, index: i + 1);
        }

        sb.AppendLine();
    }

    private static void RenderListItem(StringBuilder sb, ListItemBlock item, int indent, bool ordered, int index)
    {
        var indentation = new string(' ', indent * 2);
        var marker = ordered ? $"{index}." : "-";
        sb.AppendLine($"{indentation}{marker} {item.Text}");

        for (var i = 0; i < item.SubItems.Count; i++)
        {
            RenderListItem(sb, item.SubItems[i], indent + 1, ordered: false, index: i + 1);
        }
    }
}
