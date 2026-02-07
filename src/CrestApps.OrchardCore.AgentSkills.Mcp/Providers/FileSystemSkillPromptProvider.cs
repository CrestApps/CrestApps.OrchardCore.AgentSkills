using CrestApps.OrchardCore.AgentSkills.Mcp.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CrestApps.OrchardCore.AgentSkills.Mcp.Providers;

/// <summary>
/// Loads Orchard Core skill files via <see cref="IMcpResourceFileStore"/>
/// and produces <see cref="McpServerPrompt"/> instances for MCP registration.
/// Each <c>SKILL.md</c> file found in a skill directory becomes a prompt
/// (using the body content after the YAML front-matter).
/// Registered as a singleton — results are lazily loaded and cached.
/// </summary>
public sealed class FileSystemSkillPromptProvider
{
    private readonly IMcpResourceFileStore _fileStore;
    private readonly ILogger<FileSystemSkillPromptProvider> _logger;
    private IReadOnlyList<McpServerPrompt>? _prompts;

    public FileSystemSkillPromptProvider(
        IMcpResourceFileStore fileStore,
        ILogger<FileSystemSkillPromptProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(fileStore);
        ArgumentNullException.ThrowIfNull(logger);
        _fileStore = fileStore;
        _logger = logger;
    }

    /// <summary>
    /// Discovers all <c>SKILL.md</c> files under the skills directory and
    /// creates MCP prompt instances from the body content (after front-matter).
    /// Results are lazily loaded and cached after the first call.
    /// </summary>
    public async Task<IReadOnlyList<McpServerPrompt>> GetPromptsAsync()
    {
        if (_prompts is not null)
        {
            return _prompts;
        }

        var prompts = new List<McpServerPrompt>();

        await foreach (var skillDir in _fileStore.GetDirectoryContentAsync(null, includeSubDirectories: false))
        {
            if (!skillDir.IsDirectory)
            {
                continue;
            }

            var skillDirName = skillDir.Name;
            var skillPath = NormalizePath($"{skillDirName}/SKILL.md");
            var skillInfo = await _fileStore.GetFileInfoAsync(skillPath);

            if (skillInfo is null)
            {
                _logger.LogDebug("No SKILL.md found for skill '{SkillName}', skipping.", skillDirName);
                continue;
            }

            string content;

            try
            {
                await using var stream = await _fileStore.GetFileStreamAsync(skillPath);
                using var reader = new StreamReader(stream);
                content = await reader.ReadToEndAsync();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "Failed to read SKILL.md for skill '{SkillName}'.", skillDirName);
                continue;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("SKILL.md for skill '{SkillName}' is empty, skipping.", skillDirName);
                continue;
            }

            if (!SkillFrontMatterParser.TryParse(content, out var name, out var description, out var body))
            {
                _logger.LogWarning(
                    "SKILL.md for skill '{SkillName}' has invalid or missing front-matter (name and description are required), skipping.",
                    skillDirName);
                continue;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("SKILL.md for skill '{SkillName}' has no body content after front-matter, skipping.", skillDirName);
                continue;
            }

            var prompt = McpServerPrompt.Create(
                () => body,
                new McpServerPromptCreateOptions
                {
                    Name = name,
                    Description = description,
                });

            prompts.Add(prompt);
        }

        _logger.LogInformation("Loaded {Count} MCP prompts from agent skills.", prompts.Count);
        _prompts = prompts;

        return _prompts;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }
}
