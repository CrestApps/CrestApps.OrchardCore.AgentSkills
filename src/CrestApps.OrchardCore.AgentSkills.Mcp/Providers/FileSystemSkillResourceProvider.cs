using CrestApps.OrchardCore.AgentSkills.Mcp.Services;
using ModelContextProtocol.Server;

namespace CrestApps.OrchardCore.AgentSkills.Mcp.Providers;

/// <summary>
/// Loads Orchard Core skill resource files via <see cref="IMcpResourceFileStore"/>
/// and produces <see cref="McpServerResource"/> instances for MCP registration.
/// Each <c>skill.yaml</c> and <c>examples/*.md</c> file becomes a resource.
/// Registered as a singleton to avoid repeated file enumeration.
/// </summary>
public sealed class FileSystemSkillResourceProvider
{
    private readonly IMcpResourceFileStore _fileStore;
    private IReadOnlyList<McpServerResource>? _resources;

    public FileSystemSkillResourceProvider(IMcpResourceFileStore fileStore)
    {
        ArgumentNullException.ThrowIfNull(fileStore);
        _fileStore = fileStore;
    }

    /// <summary>
    /// Discovers all <c>skill.yaml</c> and example files under the skills
    /// directory and creates MCP resource instances from their contents.
    /// Results are cached after the first call. File content is read once
    /// and stored in memory to avoid repeated I/O.
    /// </summary>
    public async Task<IReadOnlyList<McpServerResource>> GetResourcesAsync()
    {
        if (_resources is not null)
        {
            return _resources;
        }

        var resources = new List<McpServerResource>();

        await foreach (var skillDir in _fileStore.GetDirectoryContentAsync(null, includeSubDirectories: false))
        {
            if (!skillDir.IsDirectory)
            {
                continue;
            }

            var skillName = skillDir.Name;

            // Register skill.yaml as a resource.
            var skillYamlPath = $"{skillName}/skill.yaml";
            var skillInfo = await _fileStore.GetFileInfoAsync(skillYamlPath);

            if (skillInfo is not null)
            {
                // Read file content once and cache it.
                string skillContent;
                await using (var stream = await _fileStore.GetFileStreamAsync(skillYamlPath))
                using (var reader = new StreamReader(stream))
                {
                    skillContent = await reader.ReadToEndAsync();
                }

                var resource = McpServerResource.Create(
                    () => skillContent,
                    new McpServerResourceCreateOptions
                    {
                        Name = $"{skillName}/skill.yaml",
                        Description = $"Orchard Core skill definition for {skillName}",
                        UriTemplate = $"skills://{skillName}/skill.yaml",
                        MimeType = "text/yaml",
                    });
                resources.Add(resource);
            }

            // Register example *.md files as resources.
            var examplesPath = $"{skillName}/examples";
            var examplesDir = await _fileStore.GetDirectoryInfoAsync(examplesPath);

            if (examplesDir is null)
            {
                continue;
            }

            await foreach (var entry in _fileStore.GetDirectoryContentAsync(examplesPath, includeSubDirectories: false))
            {
                if (entry.IsDirectory || !entry.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fileName = entry.Name;

                // Read file content once and cache it.
                string exampleContent;
                await using (var stream = await _fileStore.GetFileStreamAsync(entry.Path))
                using (var reader = new StreamReader(stream))
                {
                    exampleContent = await reader.ReadToEndAsync();
                }

                var resource = McpServerResource.Create(
                    () => exampleContent,
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

        _resources = resources;

        return _resources;
    }
}
