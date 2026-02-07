using CrestApps.OrchardCore.AgentSkills.Mcp.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CrestApps.OrchardCore.AgentSkills.Mcp.Providers;

/// <summary>
/// Loads Orchard Core skill resource files via <see cref="IMcpResourceFileStore"/>
/// and produces <see cref="McpServerResource"/> instances for MCP registration.
/// Each <c>SKILL.md</c> and <c>references/*.md</c> file becomes a resource.
/// Registered as a singleton — results are lazily loaded and cached.
/// </summary>
public sealed class FileSystemSkillResourceProvider
{
    private readonly IMcpResourceFileStore _fileStore;
    private readonly ILogger<FileSystemSkillResourceProvider> _logger;
    private IReadOnlyList<McpServerResource>? _resources;

    public FileSystemSkillResourceProvider(
        IMcpResourceFileStore fileStore,
        ILogger<FileSystemSkillResourceProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(fileStore);
        ArgumentNullException.ThrowIfNull(logger);
        _fileStore = fileStore;
        _logger = logger;
    }

    /// <summary>
    /// Discovers all <c>SKILL.md</c> and reference files under the skills
    /// directory and creates MCP resource instances from their contents.
    /// Results are lazily loaded and cached after the first call.
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

            var skillDirName = skillDir.Name;

            // Register SKILL.md as a resource.
            var skillMdPath = NormalizePath($"{skillDirName}/SKILL.md");
            var skillInfo = await _fileStore.GetFileInfoAsync(skillMdPath);

            if (skillInfo is not null)
            {
                string skillContent;

                try
                {
                    await using var stream = await _fileStore.GetFileStreamAsync(skillMdPath);
                    using var reader = new StreamReader(stream);
                    skillContent = await reader.ReadToEndAsync();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Failed to read SKILL.md for skill '{SkillName}'.", skillDirName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(skillContent))
                {
                    _logger.LogWarning("SKILL.md for skill '{SkillName}' is empty, skipping.", skillDirName);
                }
                else if (!SkillFrontMatterParser.TryParse(skillContent, out var name, out var description, out _))
                {
                    _logger.LogWarning(
                        "SKILL.md for skill '{SkillName}' has invalid or missing front-matter (name and description are required), skipping.",
                        skillDirName);
                }
                else
                {
                    var resource = McpServerResource.Create(
                        () => skillContent,
                        new McpServerResourceCreateOptions
                        {
                            Name = $"{name}/SKILL.md",
                            Description = description,
                            UriTemplate = $"skills://{name}/SKILL.md",
                            MimeType = "text/markdown",
                        });
                    resources.Add(resource);
                }
            }
            else
            {
                _logger.LogDebug("No SKILL.md found for skill '{SkillName}'.", skillDirName);
            }

            // Register reference *.md files as resources.
            var referencesPath = NormalizePath($"{skillDirName}/references");
            var referencesDir = await _fileStore.GetDirectoryInfoAsync(referencesPath);

            if (referencesDir is null)
            {
                continue;
            }

            await foreach (var entry in _fileStore.GetDirectoryContentAsync(referencesPath, includeSubDirectories: false))
            {
                if (entry.IsDirectory || !entry.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fileName = entry.Name;
                var filePath = NormalizePath(entry.Path);
                string referenceContent;

                try
                {
                    await using var stream = await _fileStore.GetFileStreamAsync(filePath);
                    using var reader = new StreamReader(stream);
                    referenceContent = await reader.ReadToEndAsync();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Failed to read reference file '{FileName}' for skill '{SkillName}'.", fileName, skillDirName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(referenceContent))
                {
                    _logger.LogDebug("Reference file '{FileName}' for skill '{SkillName}' is empty, skipping.", fileName, skillDirName);
                    continue;
                }

                var resource = McpServerResource.Create(
                    () => referenceContent,
                    new McpServerResourceCreateOptions
                    {
                        Name = $"{skillDirName}/references/{fileName}",
                        Description = $"Orchard Core reference for {skillDirName}",
                        UriTemplate = $"skills://{skillDirName}/references/{fileName}",
                        MimeType = "text/markdown",
                    });
                resources.Add(resource);
            }
        }

        _logger.LogInformation("Loaded {Count} MCP resources from agent skills.", resources.Count);
        _resources = resources;

        return _resources;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }
}
