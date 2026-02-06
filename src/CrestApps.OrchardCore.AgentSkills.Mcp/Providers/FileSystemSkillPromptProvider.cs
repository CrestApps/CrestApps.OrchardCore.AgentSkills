using CrestApps.OrchardCore.AgentSkills.Mcp.Services;
using ModelContextProtocol.Server;

namespace CrestApps.OrchardCore.AgentSkills.Mcp.Providers;

/// <summary>
/// Loads Orchard Core skill prompt files via <see cref="IMcpResourceFileStore"/>
/// and produces <see cref="McpServerPrompt"/> instances for MCP registration.
/// Each <c>prompts.md</c> file found in a skill directory becomes a prompt.
/// Registered as a singleton to avoid repeated file enumeration.
/// </summary>
public sealed class FileSystemSkillPromptProvider
{
    private readonly IMcpResourceFileStore _fileStore;
    private IReadOnlyList<McpServerPrompt>? _prompts;

    public FileSystemSkillPromptProvider(IMcpResourceFileStore fileStore)
    {
        ArgumentNullException.ThrowIfNull(fileStore);
        _fileStore = fileStore;
    }

    /// <summary>
    /// Discovers all <c>prompts.md</c> files under the skills directory and
    /// creates MCP prompt instances from their contents.
    /// Results are cached after the first call. File content is read once
    /// and stored in memory to avoid repeated I/O.
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

            var skillName = skillDir.Name;
            var promptPath = $"{skillName}/prompts.md";
            var promptInfo = await _fileStore.GetFileInfoAsync(promptPath);

            if (promptInfo is null)
            {
                continue;
            }

            // Read file content once and cache it.
            string content;
            await using (var stream = await _fileStore.GetFileStreamAsync(promptPath))
            using (var reader = new StreamReader(stream))
            {
                content = await reader.ReadToEndAsync();
            }

            var prompt = McpServerPrompt.Create(
                () => content,
                new McpServerPromptCreateOptions
                {
                    Name = skillName,
                    Description = $"Orchard Core prompt template for {skillName}",
                });

            prompts.Add(prompt);
        }

        _prompts = prompts;

        return _prompts;
    }
}
