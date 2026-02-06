using CrestApps.OrchardCore.AgentSkills.Mcp.Providers;
using CrestApps.OrchardCore.AgentSkills.Mcp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CrestApps.OrchardCore.AgentSkills.Mcp.Extensions;

/// <summary>
/// Extension methods for registering Orchard Core agent skills with an MCP server.
/// </summary>
public static class OrchardCoreSkillMcpExtensions
{
    private const string DefaultSkillsRelativePath = ".agents/skills";

    /// <summary>
    /// Registers Orchard Core agent skills as MCP prompts and resources.
    /// Skills are loaded at runtime from the NuGet package output directory.
    /// The <see cref="IMcpResourceFileStore"/>, <see cref="FileSystemSkillPromptProvider"/>,
    /// and <see cref="FileSystemSkillResourceProvider"/> are registered as singletons.
    /// </summary>
    /// <param name="builder">The MCP server builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static IMcpServerBuilder AddOrchardCoreSkills(this IMcpServerBuilder builder)
    {
        return builder.AddOrchardCoreSkills(_ => { });
    }

    /// <summary>
    /// Registers Orchard Core agent skills as MCP prompts and resources
    /// with optional configuration. All services are registered as singletons.
    /// </summary>
    /// <param name="builder">The MCP server builder.</param>
    /// <param name="configure">A delegate to configure skill options.</param>
    /// <returns>The builder for chaining.</returns>
    public static IMcpServerBuilder AddOrchardCoreSkills(
        this IMcpServerBuilder builder,
        Action<OrchardCoreSkillOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new OrchardCoreSkillOptions();
        configure(options);

        var skillsPath = options.Path
            ?? Path.Combine(AppContext.BaseDirectory, DefaultSkillsRelativePath);

        // Register the file store as a singleton so all providers share one instance.
        builder.Services.AddSingleton<IMcpResourceFileStore>(
            _ => new McpSkillFileStore(skillsPath));

        // Register providers as singletons to avoid repeated file reads.
        builder.Services.AddSingleton<FileSystemSkillPromptProvider>();
        builder.Services.AddSingleton<FileSystemSkillResourceProvider>();

        return builder;
    }
}
