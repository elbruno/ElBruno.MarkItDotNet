# Sample Document

This is a sample document used for testing the MarkItDotNet library conversion capabilities.

## Features

* Convert HTML to Markdown
* Support for **bold** and *italic* text
* Handle [hyperlinks](https://github.com/elbruno/markitdotnet)

## Code Example

```
var converter = new MarkdownConverter();
var result = await converter.ConvertAsync("sample.html");
```

## Table

| Format | Extension | Supported |
| --- | --- | --- |
| HTML | .html | Yes |
| PDF | .pdf | Yes |
| Word | .docx | Yes |

## Ordered List

1. First item
2. Second item
3. Third item

> This is a blockquote for testing purposes.