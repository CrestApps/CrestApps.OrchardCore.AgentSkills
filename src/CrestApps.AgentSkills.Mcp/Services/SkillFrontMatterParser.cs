namespace CrestApps.AgentSkills.Mcp.Services;

/// <summary>
/// Parses YAML front-matter from SKILL.md files.
/// Front-matter is delimited by <c>---</c> markers at the start of the file.
/// </summary>
public static class SkillFrontMatterParser
{
    private const string FrontMatterDelimiter = "---";

    /// <summary>
    /// Attempts to parse a SKILL.md file, extracting the front-matter fields and body content.
    /// </summary>
    /// <param name="content">The full content of the SKILL.md file.</param>
    /// <param name="name">The parsed <c>name</c> field from front-matter.</param>
    /// <param name="description">The parsed <c>description</c> field from front-matter.</param>
    /// <param name="body">The body content after the closing <c>---</c> delimiter.</param>
    /// <returns><c>true</c> if valid front-matter with required fields was found; otherwise <c>false</c>.</returns>
    public static bool TryParse(string content, out string name, out string description, out string body)
    {
        name = string.Empty;
        description = string.Empty;
        body = string.Empty;

        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        var trimmedContent = content.TrimStart();

        if (!trimmedContent.StartsWith(FrontMatterDelimiter, StringComparison.Ordinal))
        {
            return false;
        }

        // Find the closing delimiter.
        var firstDelimiterEnd = trimmedContent.IndexOf('\n');
        if (firstDelimiterEnd < 0)
        {
            return false;
        }

        var afterFirstDelimiter = firstDelimiterEnd + 1;
        var closingIndex = trimmedContent.IndexOf(
            $"\n{FrontMatterDelimiter}",
            afterFirstDelimiter,
            StringComparison.Ordinal);

        if (closingIndex < 0)
        {
            return false;
        }

        var frontMatter = trimmedContent[afterFirstDelimiter..closingIndex];
        var bodyStart = closingIndex + 1 + FrontMatterDelimiter.Length;

        // Skip any trailing newline after the closing delimiter.
        if (bodyStart < trimmedContent.Length && trimmedContent[bodyStart] == '\r')
        {
            bodyStart++;
        }

        if (bodyStart < trimmedContent.Length && trimmedContent[bodyStart] == '\n')
        {
            bodyStart++;
        }

        body = bodyStart < trimmedContent.Length
            ? trimmedContent[bodyStart..]
            : string.Empty;

        // Parse the front-matter key-value pairs (simple line-based YAML parsing).
        foreach (var line in frontMatter.Split('\n'))
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.Length == 0 || trimmedLine.StartsWith('#'))
            {
                continue;
            }

            var colonIndex = trimmedLine.IndexOf(':');
            if (colonIndex <= 0)
            {
                continue;
            }

            var key = trimmedLine[..colonIndex].Trim();
            var value = trimmedLine[(colonIndex + 1)..].Trim();

            if (string.Equals(key, "name", StringComparison.OrdinalIgnoreCase))
            {
                name = value;
            }
            else if (string.Equals(key, "description", StringComparison.OrdinalIgnoreCase))
            {
                description = value;
            }
        }

        return name.Length > 0 && description.Length > 0;
    }
}
