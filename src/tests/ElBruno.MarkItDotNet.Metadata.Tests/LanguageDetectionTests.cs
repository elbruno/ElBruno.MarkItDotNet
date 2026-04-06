// Copyright (c) Bruno Capuano. All rights reserved.
// Licensed under the MIT License.

using ElBruno.MarkItDotNet.CoreModel;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Metadata.Tests;

public class LanguageDetectionTests
{
    private readonly DocumentMetadataExtractor _extractor = new();

    [Fact]
    public void DetectsEnglish()
    {
        var doc = CreateDocumentWithText(
            "The committee will review the proposed changes to the agreement and provide their recommendations. " +
            "This is an important decision that will affect all members of the organization.");

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("en");
    }

    [Fact]
    public void DetectsSpanish()
    {
        var doc = CreateDocumentWithText(
            "El comité revisará los cambios propuestos al acuerdo y proporcionará sus recomendaciones. " +
            "Esta es una decisión importante que afectará a todos los miembros de la organización.");

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("es");
    }

    [Fact]
    public void DetectsFrench()
    {
        var doc = CreateDocumentWithText(
            "Le comité examinera les modifications proposées à l'accord et fournira ses recommandations. " +
            "Il est très important de prendre une décision dans les plus brefs délais pour tous les membres.");

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("fr");
    }

    [Fact]
    public void DetectsGerman()
    {
        var doc = CreateDocumentWithText(
            "Der Ausschuss wird die vorgeschlagenen Änderungen der Vereinbarung prüfen und seine Empfehlungen abgeben. " +
            "Dies ist eine wichtige Entscheidung für alle Mitglieder der Organisation.");

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("de");
    }

    [Fact]
    public void DetectsPortuguese()
    {
        var doc = CreateDocumentWithText(
            "O comitê vai revisar as mudanças propostas ao acordo e fornecer suas recomendações. " +
            "Esta é uma decisão importante que vai afetar todos os membros da organização.");

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("pt");
    }

    [Fact]
    public void DetectsItalian()
    {
        var doc = CreateDocumentWithText(
            "Il comitato esaminerà le modifiche proposte all'accordo e fornirà le sue raccomandazioni. " +
            "Questa è una decisione molto importante che riguarda tutti i membri della organizzazione.");

        var result = _extractor.Extract(doc);

        result.Language.Should().Be("it");
    }

    [Fact]
    public void ReturnsNullForEmptyContent()
    {
        var doc = CreateDocumentWithText(string.Empty);

        var result = _extractor.Extract(doc);

        result.Language.Should().BeNull();
    }

    [Fact]
    public void ReturnsNullForNonLanguageContent()
    {
        var doc = CreateDocumentWithText("12345 67890 !@#$% ^&*()");

        var result = _extractor.Extract(doc);

        result.Language.Should().BeNull();
    }

    private static Document CreateDocumentWithText(string text)
    {
        var blocks = string.IsNullOrEmpty(text)
            ? Array.Empty<DocumentBlock>()
            : new DocumentBlock[] { new ParagraphBlock { Text = text } };

        return new Document
        {
            Sections =
            [
                new DocumentSection { Blocks = blocks },
            ],
        };
    }
}
