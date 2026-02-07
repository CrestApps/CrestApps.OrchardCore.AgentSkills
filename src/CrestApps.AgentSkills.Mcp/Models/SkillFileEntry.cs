namespace CrestApps.AgentSkills.Mcp.Models;

/// <summary>
/// Represents a file or directory entry in a skill file store.
/// </summary>
public sealed class SkillFileEntry
{
    /// <summary>
    /// Gets the name of the file or directory (e.g., <c>SKILL.md</c> or <c>my-skill</c>).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the relative path of the entry within the file store.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets a value indicating whether the entry is a directory.
    /// </summary>
    public required bool IsDirectory { get; init; }
}
