using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CrestApps.AgentSkills.Mcp.Services;

/// <summary>
/// Parses skill definitions from YAML (<c>.yaml</c> / <c>.yml</c>) files.
/// Expects a YAML document with at least <c>name</c> and <c>description</c> fields.
/// An optional <c>body</c> field provides the skill body content.
/// </summary>
public static class SkillYamlParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Attempts to parse a YAML skill file, extracting the required fields.
    /// </summary>
    /// <param name="content">The full content of the YAML file.</param>
    /// <param name="name">The parsed <c>name</c> field.</param>
    /// <param name="description">The parsed <c>description</c> field.</param>
    /// <param name="body">The parsed <c>body</c> field, or empty if not present.</param>
    /// <returns><c>true</c> if valid YAML with required fields was found; otherwise <c>false</c>.</returns>
    public static bool TryParse(string content, out string name, out string description, out string body)
    {
        name = string.Empty;
        description = string.Empty;
        body = string.Empty;

        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        try
        {
            var skill = Deserializer.Deserialize<SkillYamlModel>(content);

            if (skill is null
                || string.IsNullOrWhiteSpace(skill.Name)
                || string.IsNullOrWhiteSpace(skill.Description))
            {
                return false;
            }

            name = skill.Name.Trim();
            description = skill.Description.Trim();
            body = skill.Body?.Trim() ?? string.Empty;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private sealed class SkillYamlModel
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Body { get; set; }
    }
}
