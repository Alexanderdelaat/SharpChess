using System.Net;
using System.Text;
using System.Text.RegularExpressions;

try
{
    var options = SyncOptions.Parse(args);
    var outputPath = Path.GetFullPath(options.OutputPath ?? Path.Combine(Environment.CurrentDirectory, "docs", "kanban.md"));
    var notionEmbedUrl =
        OptionalEmbedUrlEnvironmentVariable("NOTION_EMBED_URL") ??
        OptionalEmbedUrlEnvironmentVariable("NOTION_PUBLIC_BOARD_URL") ??
        throw new InvalidOperationException(
            "Missing Notion embed configuration. Set 'NOTION_PUBLIC_BOARD_URL' or 'NOTION_EMBED_URL' in GitHub Actions before running the sync.");

    var embeddedMarkdown = BuildEmbeddedMarkdown(notionEmbedUrl);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    await File.WriteAllTextAsync(outputPath, embeddedMarkdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    Console.WriteLine($"Generated '{outputPath}' with the configured Notion embed.");
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

return;

static string? OptionalEmbedUrlEnvironmentVariable(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    var normalizedValue = StripWrappingQuotes(value.Trim());
    var iframeMatch = Regex.Match(
        normalizedValue,
        "src\\s*=\\s*[\"'](?<src>[^\"']+)[\"']",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    if (iframeMatch.Success)
    {
        normalizedValue = iframeMatch.Groups["src"].Value.Trim();
    }

    if (Uri.TryCreate(normalizedValue, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
    {
        return uri.ToString();
    }

    throw new InvalidOperationException(
        $"Environment variable '{name}' must be a published Notion page URL, an embeddable URL, or an iframe snippet that contains one.");
}

static string StripWrappingQuotes(string value)
{
    if (value.Length >= 2)
    {
        var first = value[0];
        var last = value[^1];
        if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
        {
            return value[1..^1].Trim();
        }
    }

    return value;
}

static string BuildEmbeddedMarkdown(string notionEmbedUrl)
{
    var builder = new StringBuilder();
    var escapedUrl = WebUtility.HtmlEncode(notionEmbedUrl);

    builder.AppendLine("---");
    builder.AppendLine("title: Project Board");
    builder.AppendLine("---");
    builder.AppendLine();
    builder.AppendLine("# Project Board");
    builder.AppendLine();
    builder.AppendLine("> This page embeds the published Notion board.");
    builder.AppendLine("> Notion is the live runtime source for this page, and DocFX only hosts the container page.");
    builder.AppendLine("> If the embed stays blank, open the direct Notion link below. That usually means Notion refused iframe rendering for the current URL.");
    builder.AppendLine();
    builder.AppendLine($"<p><a href=\"{escapedUrl}\" target=\"_blank\" rel=\"noopener noreferrer\">Open the board directly in Notion</a></p>");
    builder.AppendLine();
    builder.AppendLine("<iframe");
    builder.AppendLine($"  src=\"{escapedUrl}\"");
    builder.AppendLine("  width=\"100%\"");
    builder.AppendLine("  height=\"1200\"");
    builder.AppendLine("  frameborder=\"0\"");
    builder.AppendLine("  loading=\"lazy\"");
    builder.AppendLine("  allowfullscreen");
    builder.AppendLine("  referrerpolicy=\"strict-origin-when-cross-origin\"");
    builder.AppendLine("  sandbox=\"allow-scripts allow-same-origin allow-popups allow-popups-to-escape-sandbox allow-forms\"");
    builder.AppendLine("  style=\"border: 1px solid #d7dce2; border-radius: 12px; background: #ffffff;\"");
    builder.AppendLine("></iframe>");

    return builder.ToString().TrimEnd() + Environment.NewLine;
}

sealed record SyncOptions(string? OutputPath)
{
    public static SyncOptions Parse(string[] args)
    {
        string? outputPath = null;

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];

            if (string.Equals(argument, "--output", StringComparison.Ordinal))
            {
                if (index + 1 >= args.Length)
                {
                    throw new InvalidOperationException("Expected a path after '--output'.");
                }

                outputPath = args[++index];
                continue;
            }

            throw new InvalidOperationException($"Unknown argument '{argument}'. Supported arguments: --output <path>.");
        }

        return new SyncOptions(outputPath);
    }
}
