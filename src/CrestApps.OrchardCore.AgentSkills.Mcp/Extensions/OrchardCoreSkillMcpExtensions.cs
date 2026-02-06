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
    /// with optional configuration. The <see cref="IMcpResourceFileStore"/>,
    /// <see cref="FileSystemSkillPromptProvider"/>, and <see cref="FileSystemSkillResourceProvider"/>
    /// are registered as singletons and immediately used to populate the MCP server.
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

        // Create singleton instances.
        var fileStore = new McpSkillFileStore(skillsPath);
        var promptProvider = new FileSystemSkillPromptProvider(fileStore);
        var resourceProvider = new FileSystemSkillResourceProvider(fileStore);

        // Register singletons in DI for consumer injection.
        builder.Services.AddSingleton<IMcpResourceFileStore>(fileStore);
        builder.Services.AddSingleton(promptProvider);
        builder.Services.AddSingleton(resourceProvider);

        // Eagerly load and register prompts/resources with the MCP server.
        var prompts = promptProvider.GetPromptsAsync().GetAwaiter().GetResult();
        if (prompts.Count > 0)
        {
            builder.WithPrompts(prompts);
        }

        var resources = resourceProvider.GetResourcesAsync().GetAwaiter().GetResult();
        if (resources.Count > 0)
        {
            builder.WithResources(resources);
        }

        return builder;
    }
}
