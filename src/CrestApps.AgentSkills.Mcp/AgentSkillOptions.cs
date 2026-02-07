namespace CrestApps.AgentSkills.Mcp;

/// <summary>
/// Configuration options for the Agent Skills MCP engine.
/// </summary>
public sealed class AgentSkillOptions
{
    /// <summary>
    /// Gets or sets the path to the skills directory.
    /// When <c>null</c>, the default path (<c>&lt;AppContext.BaseDirectory&gt;/.agents/skills</c>) is used.
    /// </summary>
    public string? Path { get; set; }
}
