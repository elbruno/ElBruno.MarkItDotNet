using ElBruno.MarkItDotNet.CoreModel;

namespace ElBruno.MarkItDotNet.Citations;

/// <summary>
/// Builds <see cref="CitationReference"/> instances from CoreModel documents, sections, and blocks.
/// </summary>
public static class CitationBuilder
{
    /// <summary>
    /// Extracts citations for every block in the given document.
    /// </summary>
    /// <param name="document">The document to extract citations from.</param>
    /// <returns>A list of citation references, one per block found in the document.</returns>
    public static IReadOnlyList<CitationReference> FromDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var citations = new List<CitationReference>();
        var filePath = document.Source?.FilePath;

        foreach (var section in document.Sections)
        {
            citations.AddRange(FromSection(section, filePath));
        }

        return citations;
    }

    /// <summary>
    /// Extracts citations for all blocks within a document section, including sub-sections.
    /// </summary>
    /// <param name="section">The section to extract citations from.</param>
    /// <param name="filePath">The file path of the source document, if known.</param>
    /// <returns>A list of citation references for the section's blocks.</returns>
    public static IReadOnlyList<CitationReference> FromSection(DocumentSection section, string? filePath)
    {
        ArgumentNullException.ThrowIfNull(section);

        var citations = new List<CitationReference>();
        var headingPath = section.Heading?.Text;

        foreach (var block in section.Blocks)
        {
            citations.Add(FromBlock(block, filePath, headingPath));
        }

        foreach (var subSection in section.SubSections)
        {
            var subHeadingPath = headingPath is not null && subSection.Heading?.Text is not null
                ? $"{headingPath} > {subSection.Heading.Text}"
                : subSection.Heading?.Text ?? headingPath;

            foreach (var block in subSection.Blocks)
            {
                citations.Add(FromBlock(block, filePath, subHeadingPath));
            }

            foreach (var nested in subSection.SubSections)
            {
                citations.AddRange(FromSection(nested, filePath));
            }
        }

        return citations;
    }

    /// <summary>
    /// Creates a citation reference for a single block.
    /// Uses <see cref="SourceReference"/> data from the block when available;
    /// falls back to coarse mode when exact source information is missing.
    /// </summary>
    /// <param name="block">The block to create a citation for.</param>
    /// <param name="filePath">The file path of the source document, if known.</param>
    /// <param name="headingPath">The heading path for context, if known.</param>
    /// <returns>A citation reference for the block.</returns>
    public static CitationReference FromBlock(DocumentBlock block, string? filePath, string? headingPath)
    {
        ArgumentNullException.ThrowIfNull(block);

        var source = block.Source;

        if (source is not null)
        {
            return new CitationReference
            {
                FilePath = source.FilePath ?? filePath,
                PageNumber = source.PageNumber,
                HeadingPath = source.HeadingPath ?? headingPath,
                BlockId = block.Id,
                Span = source.Span,
                Mode = CitationMode.Exact
            };
        }

        // No source reference — fall back to coarse mode
        return new CitationReference
        {
            FilePath = filePath,
            HeadingPath = headingPath,
            BlockId = block.Id,
            Mode = CitationMode.Coarse
        };
    }
}
