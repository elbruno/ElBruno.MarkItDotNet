# Acknowledgements

ElBruno.MarkItDotNet is built on the shoulders of outstanding open-source libraries. We are grateful to the maintainers and contributors of every project listed below.

## Core Library Dependencies

| Library | NuGet | Used For | License |
|---------|-------|----------|---------|
| [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net) | [![NuGet](https://img.shields.io/nuget/v/ReverseMarkdown.svg?style=flat-square)](https://www.nuget.org/packages/ReverseMarkdown) | HTML → Markdown conversion (HtmlConverter, UrlConverter, EpubConverter, RtfConverter) | MIT |
| [Open XML SDK](https://github.com/dotnet/Open-XML-SDK) | [![NuGet](https://img.shields.io/nuget/v/DocumentFormat.OpenXml.svg?style=flat-square)](https://www.nuget.org/packages/DocumentFormat.OpenXml) | Word (DOCX) document parsing (DocxConverter) | MIT |
| [PdfPig](https://github.com/UglyToad/PdfPig) | [![NuGet](https://img.shields.io/nuget/v/PdfPig.svg?style=flat-square)](https://www.nuget.org/packages/PdfPig) | PDF text extraction (PdfConverter) | Apache 2.0 |
| [RtfPipe](https://github.com/erdomke/RtfPipe) | [![NuGet](https://img.shields.io/nuget/v/RtfPipe.svg?style=flat-square)](https://www.nuget.org/packages/RtfPipe) | RTF → HTML conversion (RtfConverter) | MIT |
| [VersOne.Epub](https://github.com/vers-one/EpubReader) | [![NuGet](https://img.shields.io/nuget/v/VersOne.Epub.svg?style=flat-square)](https://www.nuget.org/packages/VersOne.Epub) | EPUB reading and parsing (EpubConverter) | MIT |
| [System.Text.Encoding.CodePages](https://github.com/dotnet/runtime) | [![NuGet](https://img.shields.io/nuget/v/System.Text.Encoding.CodePages.svg?style=flat-square)](https://www.nuget.org/packages/System.Text.Encoding.CodePages) | Extended text encoding support | MIT |
| [Microsoft.Extensions.DependencyInjection.Abstractions](https://github.com/dotnet/runtime) | [![NuGet](https://img.shields.io/nuget/v/Microsoft.Extensions.DependencyInjection.Abstractions.svg?style=flat-square)](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions) | Dependency injection extensions | MIT |

## Satellite Package Dependencies

### ElBruno.MarkItDotNet.Excel

| Library | NuGet | Used For | License |
|---------|-------|----------|---------|
| [ClosedXML](https://github.com/ClosedXML/ClosedXML) | [![NuGet](https://img.shields.io/nuget/v/ClosedXML.svg?style=flat-square)](https://www.nuget.org/packages/ClosedXML) | Excel (.xlsx) spreadsheet parsing | MIT |

### ElBruno.MarkItDotNet.PowerPoint

| Library | NuGet | Used For | License |
|---------|-------|----------|---------|
| [Open XML SDK](https://github.com/dotnet/Open-XML-SDK) | [![NuGet](https://img.shields.io/nuget/v/DocumentFormat.OpenXml.svg?style=flat-square)](https://www.nuget.org/packages/DocumentFormat.OpenXml) | PowerPoint (.pptx) slide parsing | MIT |

### ElBruno.MarkItDotNet.AI

| Library | NuGet | Used For | License |
|---------|-------|----------|---------|
| [Microsoft.Extensions.AI.Abstractions](https://github.com/dotnet/extensions) | [![NuGet](https://img.shields.io/nuget/v/Microsoft.Extensions.AI.Abstractions.svg?style=flat-square)](https://www.nuget.org/packages/Microsoft.Extensions.AI.Abstractions) | `IChatClient` interface for AI-powered converters | MIT |
| [PdfPig](https://github.com/UglyToad/PdfPig) | [![NuGet](https://img.shields.io/nuget/v/PdfPig.svg?style=flat-square)](https://www.nuget.org/packages/PdfPig) | PDF page rendering for AI OCR | Apache 2.0 |

### ElBruno.MarkItDotNet.Whisper

| Library | NuGet | Used For | License |
|---------|-------|----------|---------|
| [ElBruno.Whisper](https://github.com/elbruno/ElBruno.Whisper) | [![NuGet](https://img.shields.io/nuget/v/ElBruno.Whisper.svg?style=flat-square)](https://www.nuget.org/packages/ElBruno.Whisper) | Local audio transcription via OpenAI Whisper ONNX | MIT |

## Test & Tooling Dependencies

| Library | NuGet | Used For | License |
|---------|-------|----------|---------|
| [xUnit](https://github.com/xunit/xunit) | [![NuGet](https://img.shields.io/nuget/v/xunit.svg?style=flat-square)](https://www.nuget.org/packages/xunit) | Unit testing framework | Apache 2.0 |
| [FluentAssertions](https://github.com/fluentassertions/fluentassertions) | [![NuGet](https://img.shields.io/nuget/v/FluentAssertions.svg?style=flat-square)](https://www.nuget.org/packages/FluentAssertions) | Expressive test assertions | Apache 2.0 |
| [NSubstitute](https://github.com/nsubstitute/NSubstitute) | [![NuGet](https://img.shields.io/nuget/v/NSubstitute.svg?style=flat-square)](https://www.nuget.org/packages/NSubstitute) | Test mocking | BSD 3-Clause |
| [coverlet](https://github.com/coverlet-coverage/coverlet) | [![NuGet](https://img.shields.io/nuget/v/coverlet.collector.svg?style=flat-square)](https://www.nuget.org/packages/coverlet.collector) | Code coverage collection | MIT |

## Inspiration

This project is inspired by [markitdown](https://github.com/microsoft/markitdown), a Python library by Microsoft for converting files to Markdown. ElBruno.MarkItDotNet brings the same concept to the .NET ecosystem with native performance and a plugin-based architecture.

---

Thank you to all the open-source maintainers who make projects like this possible. 🙏
