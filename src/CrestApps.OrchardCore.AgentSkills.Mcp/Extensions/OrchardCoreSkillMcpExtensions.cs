using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

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
    /// </summary>
    /// <param name="builder">The MCP server builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static IMcpServerBuilder AddOrchardCoreSkills(this IMcpServerBuilder builder)
    {
        return builder.AddOrchardCoreSkills(_ => { });
    }

    /// <summary>
    /// Registers Orchard Core agent skills as MCP prompts and resources
    /// with optional configuration.
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

        var skillsPath = options.Path ?? Path.Combine(AppContext.BaseDirectory, DefaultSkillsRelativePath);

        if (!Directory.Exists(skillsPath))
        {
            return builder;
        }

        var prompts = new List<McpServerPrompt>();
        var resources = new List<McpServerResource>();

        foreach (var skillDir in Directory.EnumerateDirectories(skillsPath))
        {
            var skillName = Path.GetFileName(skillDir);

            // Register prompts.md files as MCP prompts
            var promptsFile = Path.Combine(skillDir, "prompts.md");
            if (File.Exists(promptsFile))
            {
                var content = File.ReadAllText(promptsFile);
                var prompt = McpServerPrompt.Create(
                    () => content,
                    new McpServerPromptCreateOptions
                    {
                        Name = skillName,
                        Description = $"Orchard Core prompt template for {skillName}",
                    });
                prompts.Add(prompt);
            }

            // Register skill.yaml files as MCP resources
            var skillFile = Path.Combine(skillDir, "skill.yaml");
            if (File.Exists(skillFile))
            {
                var content = File.ReadAllText(skillFile);
                var resource = McpServerResource.Create(
                    () => content,
                    new McpServerResourceCreateOptions
                    {
                        Name = $"{skillName}/skill.yaml",
                        Description = $"Orchard Core skill definition for {skillName}",
                        UriTemplate = $"skills://{skillName}/skill.yaml",
                        MimeType = "text/yaml",
                    });
                resources.Add(resource);
            }

            // Register example files as MCP resources
            var examplesDir = Path.Combine(skillDir, "examples");
            if (Directory.Exists(examplesDir))
            {
                foreach (var exampleFile in Directory.EnumerateFiles(examplesDir))
                {
                    var fileName = Path.GetFileName(exampleFile);
                    var content = File.ReadAllText(exampleFile);
                    var resource = McpServerResource.Create(
                        () => content,
                        new McpServerResourceCreateOptions
                        {
                            Name = $"{skillName}/examples/{fileName}",
                            Description = $"Orchard Core example for {skillName}",
                            UriTemplate = $"skills://{skillName}/examples/{fileName}",
                            MimeType = "text/markdown",
                        });
                    resources.Add(resource);
                }
            }
        }

        if (prompts.Count > 0)
        {
            builder.WithPrompts(prompts);
        }

        if (resources.Count > 0)
        {
            builder.WithResources(resources);
        }

        return builder;
    }
}
