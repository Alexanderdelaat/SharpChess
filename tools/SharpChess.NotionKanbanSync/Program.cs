using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

const string NotionApiVersion = "2026-03-11";

try
{
    var options = SyncOptions.Parse(args);
    var outputPath = Path.GetFullPath(options.OutputPath ?? Path.Combine(Environment.CurrentDirectory, "docs", "kanban.md"));

    var notionToken = RequiredEnvironmentVariable("NOTION_TOKEN");
    var notionDatabaseId = RequiredEnvironmentVariable("NOTION_DATABASE_ID");
    var notionDataSourceId = Environment.GetEnvironmentVariable("NOTION_DATA_SOURCE_ID");

    using var client = new HttpClient
    {
        BaseAddress = new Uri("https://api.notion.com/v1/")
    };

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", notionToken);
    client.DefaultRequestHeaders.Add("Notion-Version", NotionApiVersion);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SharpChess.NotionKanbanSync/1.0");

    var dataSourceId = !string.IsNullOrWhiteSpace(notionDataSourceId)
        ? notionDataSourceId.Trim()
        : await ResolveDataSourceIdAsync(client, notionDatabaseId);

    var dataSource = await GetJsonAsync(client, $"data_sources/{Uri.EscapeDataString(dataSourceId)}");
    var schema = BoardSchema.Create(dataSource);
    var cards = await ReadCardsAsync(client, dataSourceId, schema);
    var markdown = BuildMarkdown(cards, schema);

    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    await File.WriteAllTextAsync(outputPath, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    Console.WriteLine($"Generated '{outputPath}' from the configured Notion board.");
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

return;

static string RequiredEnvironmentVariable(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (!string.IsNullOrWhiteSpace(value))
    {
        return value.Trim();
    }

    throw new InvalidOperationException(
        $"Missing required environment variable '{name}'. " +
        "Set it locally before running the sync, or configure it as a GitHub Actions secret.");
}

static async Task<string> ResolveDataSourceIdAsync(HttpClient client, string databaseId)
{
    var database = await GetJsonAsync(client, $"databases/{Uri.EscapeDataString(databaseId)}");
    var dataSources = database["data_sources"]?.AsArray()
        ?? throw new InvalidOperationException(
            "The configured Notion database did not return any data sources.");

    if (dataSources.Count == 0)
    {
        throw new InvalidOperationException(
            "The configured Notion database does not contain any data sources.");
    }

    if (dataSources.Count > 1)
    {
        var availableSources = string.Join(
            ", ",
            dataSources.Select(source =>
            {
                var id = source?["id"]?.GetValue<string>() ?? "<missing-id>";
                var name = source?["name"]?.GetValue<string>() ?? "<unnamed>";
                return $"{name} ({id})";
            }));

        throw new InvalidOperationException(
            $"The configured Notion database contains multiple data sources: {availableSources}. " +
            "Set NOTION_DATA_SOURCE_ID to choose the board source explicitly.");
    }

    return dataSources[0]?["id"]?.GetValue<string>()
        ?? throw new InvalidOperationException(
            "The configured Notion database returned a data source without an id.");
}

static async Task<JsonObject> GetJsonAsync(HttpClient client, string relativePath)
{
    using var response = await client.GetAsync(relativePath);
    var body = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        throw new InvalidOperationException(
            $"Notion API request failed ({(int)response.StatusCode} {response.ReasonPhrase}) for '{relativePath}'. " +
            $"{ExtractNotionError(body)}");
    }

    return JsonNode.Parse(body)?.AsObject()
        ?? throw new InvalidOperationException($"Notion API returned invalid JSON for '{relativePath}'.");
}

static async Task<IReadOnlyList<BoardCard>> ReadCardsAsync(HttpClient client, string dataSourceId, BoardSchema schema)
{
    var cards = new List<BoardCard>();
    string? cursor = null;
    var filterProperties = BuildFilterProperties(schema);

    do
    {
        var queryPath = BuildDataSourceQueryPath(dataSourceId, filterProperties);
        var requestBody = new JsonObject
        {
            ["page_size"] = 100
        };

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            requestBody["start_cursor"] = cursor;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, queryPath)
        {
            Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Notion API request failed ({(int)response.StatusCode} {response.ReasonPhrase}) for '{queryPath}'. " +
                $"{ExtractNotionError(body)}");
        }

        var payload = JsonNode.Parse(body)?.AsObject()
            ?? throw new InvalidOperationException("Notion returned invalid JSON when querying the board.");

        var results = payload["results"]?.AsArray()
            ?? throw new InvalidOperationException("The Notion data source query response did not include results.");

        foreach (var page in results)
        {
            var pageObject = page?.AsObject();
            if (pageObject is null)
            {
                continue;
            }

            if (pageObject["archived"]?.GetValue<bool>() == true || pageObject["in_trash"]?.GetValue<bool>() == true)
            {
                continue;
            }

            cards.Add(BoardCard.Create(pageObject, schema));
        }

        cursor = payload["has_more"]?.GetValue<bool>() == true
            ? payload["next_cursor"]?.GetValue<string>()
            : null;
    }
    while (!string.IsNullOrWhiteSpace(cursor));

    return cards;
}

static string BuildDataSourceQueryPath(string dataSourceId, IReadOnlyList<string> filterProperties)
{
    var builder = new StringBuilder($"data_sources/{Uri.EscapeDataString(dataSourceId)}/query");
    if (filterProperties.Count == 0)
    {
        return builder.ToString();
    }

    builder.Append('?');
    builder.Append(string.Join(
        "&",
        filterProperties.Select(property => $"filter_properties[]={Uri.EscapeDataString(property)}")));

    return builder.ToString();
}

static IReadOnlyList<string> BuildFilterProperties(BoardSchema schema)
{
    var properties = new List<string>
    {
        schema.Title.Name,
        schema.Status.Name
    };

    if (schema.Priority is not null)
    {
        properties.Add(schema.Priority.Name);
    }

    if (schema.DueDate is not null)
    {
        properties.Add(schema.DueDate.Name);
    }

    return properties
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

static string BuildMarkdown(IReadOnlyList<BoardCard> cards, BoardSchema schema)
{
    var builder = new StringBuilder();
    var generatedAt = DateTimeOffset.UtcNow;
    var groupedCards = cards
        .GroupBy(card => card.Status, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(
            group => group.Key,
            group => group
                .OrderBy(card => card.DueDateSortKey is null)
                .ThenBy(card => card.DueDateSortKey)
                .ThenBy(card => card.Title, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            StringComparer.OrdinalIgnoreCase);

    builder.AppendLine("---");
    builder.AppendLine("title: Project Board");
    builder.AppendLine("---");
    builder.AppendLine();
    builder.AppendLine("# Project Board");
    builder.AppendLine();
    builder.AppendLine("> This page is generated from Notion by `scripts/sync-notion-kanban.sh`.");
    builder.AppendLine("> The integration is read-only: it only reads the board and writes Markdown for DocFX.");
    builder.AppendLine();
    builder.AppendLine($"_Generated at {generatedAt:yyyy-MM-dd HH:mm 'UTC'}._");
    builder.AppendLine();

    foreach (var status in OrderedStatuses(schema, groupedCards.Keys))
    {
        builder.AppendLine($"## {EscapeMarkdown(status)}");
        builder.AppendLine();

        if (!groupedCards.TryGetValue(status, out var statusCards) || statusCards.Count == 0)
        {
            builder.AppendLine("_No cards in this column._");
            builder.AppendLine();
            continue;
        }

        foreach (var card in statusCards)
        {
            builder.AppendLine($"- **{EscapeMarkdown(card.Title)}**");
            builder.AppendLine($"  {BuildCardMetadata(card)}");
            builder.AppendLine();
        }
    }

    return builder.ToString().TrimEnd() + Environment.NewLine;
}

static IEnumerable<string> OrderedStatuses(BoardSchema schema, IEnumerable<string> statusesFromCards)
{
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var status in schema.StatusesInDisplayOrder)
    {
        seen.Add(status);
        yield return status;
    }

    foreach (var status in statusesFromCards.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
    {
        if (seen.Add(status))
        {
            yield return status;
        }
    }
}

static string BuildCardMetadata(BoardCard card)
{
    var parts = new List<string>
    {
        $"Status: {EscapeMarkdown(card.Status)}"
    };

    if (!string.IsNullOrWhiteSpace(card.Priority))
    {
        parts.Add($"Priority: {EscapeMarkdown(card.Priority)}");
    }

    if (!string.IsNullOrWhiteSpace(card.DueDateLabel))
    {
        parts.Add($"Due: {EscapeMarkdown(card.DueDateLabel)}");
    }

    parts.Add($"[Open in Notion](<{card.Url}>)");
    return string.Join(" | ", parts);
}

static string EscapeMarkdown(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return string.Empty;
    }

    return value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("*", "\\*", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal)
        .Replace("]", "\\]", StringComparison.Ordinal)
        .Replace("<", "&lt;", StringComparison.Ordinal)
        .Replace(">", "&gt;", StringComparison.Ordinal);
}

static string ExtractNotionError(string body)
{
    if (string.IsNullOrWhiteSpace(body))
    {
        return "No response body was returned.";
    }

    try
    {
        var payload = JsonNode.Parse(body)?.AsObject();
        var code = payload?["code"]?.GetValue<string>();
        var message = payload?["message"]?.GetValue<string>();

        if (!string.IsNullOrWhiteSpace(code) || !string.IsNullOrWhiteSpace(message))
        {
            return $"{code}: {message}".Trim(':', ' ');
        }
    }
    catch (JsonException)
    {
    }

    return body;
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

sealed record BoardSchema(
    NotionProperty Title,
    NotionProperty Status,
    NotionProperty? Priority,
    NotionProperty? DueDate,
    IReadOnlyList<string> StatusesInDisplayOrder)
{
    public static BoardSchema Create(JsonObject dataSource)
    {
        var properties = dataSource["properties"]?.AsObject()
            ?? throw new InvalidOperationException("The Notion data source response did not include a properties schema.");

        var availableProperties = properties
            .Select(property => NotionProperty.Create(property.Key, property.Value?.AsObject()))
            .ToList();

        var title = availableProperties.FirstOrDefault(property => property.Type == "title")
            ?? throw new InvalidOperationException("The Notion data source does not contain a title property.");

        var status = FindStatusProperty(availableProperties)
            ?? throw new InvalidOperationException(
                "The Notion data source does not contain a status/select property for board columns.");

        var priority = FindPriorityProperty(availableProperties);
        var dueDate = FindDueDateProperty(availableProperties);

        return new BoardSchema(
            title,
            status,
            priority,
            dueDate,
            ReadStatusOptions(status));
    }

    static NotionProperty? FindStatusProperty(IEnumerable<NotionProperty> properties)
    {
        return properties
            .Where(property => property.Type is "status" or "select")
            .OrderBy(property => PropertyMatchScore(property.Name, "status"))
            .ThenBy(property => property.Type == "status" ? 0 : 1)
            .FirstOrDefault();
    }

    static NotionProperty? FindPriorityProperty(IEnumerable<NotionProperty> properties)
    {
        return properties
            .Where(property => property.Type is "select" or "status" or "multi_select" or "rich_text" or "number")
            .OrderBy(property => PropertyMatchScore(property.Name, "priority"))
            .FirstOrDefault(property => NameContains(property.Name, "priority"));
    }

    static NotionProperty? FindDueDateProperty(IEnumerable<NotionProperty> properties)
    {
        return properties
            .Where(property => property.Type == "date")
            .OrderBy(property => PropertyMatchScore(property.Name, "due"))
            .ThenBy(property => PropertyMatchScore(property.Name, "deadline"))
            .FirstOrDefault(property =>
                NameContains(property.Name, "due") ||
                NameContains(property.Name, "deadline") ||
                NameContains(property.Name, "target"));
    }

    static IReadOnlyList<string> ReadStatusOptions(NotionProperty statusProperty)
    {
        JsonArray? options = statusProperty.Type switch
        {
            "status" => statusProperty.Node["status"]?["options"]?.AsArray(),
            "select" => statusProperty.Node["select"]?["options"]?.AsArray(),
            _ => null
        };

        return options?
            .Select(option => option?["name"]?.GetValue<string>())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToArray()
            ?? Array.Empty<string>();
    }

    static bool NameContains(string value, string fragment) =>
        value.Contains(fragment, StringComparison.OrdinalIgnoreCase);

    static int PropertyMatchScore(string propertyName, string expectedName)
    {
        if (string.Equals(propertyName, expectedName, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (propertyName.Contains(expectedName, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return 2;
    }
}

sealed record NotionProperty(string Name, string Id, string Type, JsonObject Node)
{
    public static NotionProperty Create(string name, JsonObject? node)
    {
        if (node is null)
        {
            throw new InvalidOperationException($"The Notion property '{name}' was missing its schema definition.");
        }

        var id = node["id"]?.GetValue<string>() ?? name;
        var type = node["type"]?.GetValue<string>()
            ?? throw new InvalidOperationException($"The Notion property '{name}' did not include a property type.");

        return new NotionProperty(name, id, type, node);
    }
}

sealed record BoardCard(
    string Title,
    string Status,
    string? Priority,
    string? DueDateLabel,
    DateTimeOffset? DueDateSortKey,
    string Url)
{
    public static BoardCard Create(JsonObject page, BoardSchema schema)
    {
        var properties = page["properties"]?.AsObject()
            ?? throw new InvalidOperationException("A Notion page response did not include properties.");

        var title = ReadTitle(properties, schema.Title.Name);
        var status = ReadPropertyValue(properties, schema.Status) ?? "Unassigned";
        var priority = schema.Priority is null ? null : ReadPropertyValue(properties, schema.Priority);
        var dueDate = schema.DueDate is null ? null : ReadDateValue(properties, schema.DueDate);
        var url = page["url"]?.GetValue<string>()
            ?? throw new InvalidOperationException($"The Notion card '{title}' did not include a page URL.");

        return new BoardCard(
            string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
            string.IsNullOrWhiteSpace(status) ? "Unassigned" : status,
            priority,
            dueDate?.DisplayText,
            dueDate?.SortKey,
            url);
    }

    static string ReadTitle(JsonObject properties, string propertyName)
    {
        var titleArray = properties[propertyName]?["title"]?.AsArray();
        var title = ReadRichTextArray(titleArray);
        return string.IsNullOrWhiteSpace(title) ? "Untitled" : title;
    }

    static string? ReadPropertyValue(JsonObject properties, NotionProperty property)
    {
        var value = properties[property.Name]?.AsObject();
        if (value is null)
        {
            return null;
        }

        return property.Type switch
        {
            "status" => value["status"]?["name"]?.GetValue<string>(),
            "select" => value["select"]?["name"]?.GetValue<string>(),
            "multi_select" => string.Join(
                ", ",
                value["multi_select"]?.AsArray()
                    .Select(option => option?["name"]?.GetValue<string>())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Cast<string>() ?? Array.Empty<string>()),
            "rich_text" => ReadRichTextArray(value["rich_text"]?.AsArray()),
            "title" => ReadRichTextArray(value["title"]?.AsArray()),
            "number" => value["number"]?.GetValue<double>().ToString(CultureInfo.InvariantCulture),
            _ => null
        };
    }

    static NotionDateValue? ReadDateValue(JsonObject properties, NotionProperty property)
    {
        var dateObject = properties[property.Name]?["date"]?.AsObject();
        if (dateObject is null)
        {
            return null;
        }

        var start = dateObject["start"]?.GetValue<string>();
        var end = dateObject["end"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(start))
        {
            return null;
        }

        return NotionDateValue.Create(start, end);
    }

    static string ReadRichTextArray(JsonArray? values)
    {
        if (values is null)
        {
            return string.Empty;
        }

        return string.Concat(values.Select(value => value?["plain_text"]?.GetValue<string>() ?? string.Empty)).Trim();
    }
}

sealed record NotionDateValue(string DisplayText, DateTimeOffset? SortKey)
{
    public static NotionDateValue Create(string start, string? end)
    {
        var startDisplay = start;
        var endDisplay = end;
        var startSortKey = TryParseDate(start);

        return new NotionDateValue(
            string.IsNullOrWhiteSpace(endDisplay) || string.Equals(startDisplay, endDisplay, StringComparison.Ordinal)
                ? startDisplay
                : $"{startDisplay} to {endDisplay}",
            startSortKey);
    }

    static DateTimeOffset? TryParseDate(string value)
    {
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces,
            out var parsed)
            ? parsed
            : null;
    }
}
