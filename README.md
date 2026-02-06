# CrestApps.OrchardCore.AgentSkills

Distributes shared **AI agent instruction and guardrail files** for Orchard Core development, available as two complementary NuGet packages:

| Package | Purpose |
|---|---|
| [`CrestApps.OrchardCore.AgentSkills`](src/CrestApps.OrchardCore.AgentSkills/) | **Dev / design-time** — copies skills into solution-root `.agents/skills` for local AI authoring tools (GitHub Copilot, Cursor, Cline) |
| [`CrestApps.OrchardCore.AgentSkills.Mcp`](src/CrestApps.OrchardCore.AgentSkills.Mcp/) | **Runtime / MCP server** — loads skills at runtime and exposes them as MCP prompts and resources |

## Quick Start

### Local AI Authoring

```bash
dotnet add package CrestApps.OrchardCore.AgentSkills
```

After install and restore, the solution root will contain:

```
.agents/
  skills/
    orchardcore.content-types/
    orchardcore.modules/
    orchardcore.recipes/
    orchardcore.deployments/
    orchardcore.ai/
```

- Files are copied on **install/update** (not build).
- **No runtime dependency** — purely for development and AI tooling guidance.
- Files are refreshed when the package is updated.

### MCP Server Hosting

```bash
dotnet add package CrestApps.OrchardCore.AgentSkills.Mcp
```

```csharp
builder.Services.AddMcpServer(mcp =>
{
    mcp.AddOrchardCoreSkills();
});
```

- Loads skills at runtime from the package output directory.
- Registers prompts and resources via the [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk).
- No file copying to solution.

### Full Experience

Install both packages to get local AI authoring **and** MCP server support.

## Skill Categories

| Skill | Description |
|---|---|
| `orchardcore.content-types` | Creating and managing Orchard Core content types, parts, and fields |
| `orchardcore.modules` | Scaffolding modules, features, manifests, and startup configuration |
| `orchardcore.recipes` | Recipe structure, steps, content definitions, and content items |
| `orchardcore.deployments` | Deployment plans and import/export configuration |
| `orchardcore.ai` | AI service integration, MCP enablement, and agent framework setup |

## Repository Structure

```
src/
├─ CrestApps.OrchardCore.AgentSkills/           ← Dev package
│  ├─ contentFiles/any/any/.agents/skills/       ← Skill content
│  ├─ buildTransitive/                           ← MSBuild .targets for solution-root copy
│  ├─ README.md
│  └─ CrestApps.OrchardCore.AgentSkills.csproj
│
└─ CrestApps.OrchardCore.AgentSkills.Mcp/       ← MCP runtime package
   ├─ contentFiles/any/any/.agents/skills/       ← Skill content (copied to output)
   ├─ Extensions/                                ← MCP extension methods
   ├─ README.md
   └─ CrestApps.OrchardCore.AgentSkills.Mcp.csproj
```

> **Note:**
> This project installs shared agent files into your local `.agents/` folder.
> If needed, it will replace common agent files (such as `Agents.md`) that already exist in your project.
> Do **not** modify files added by this package inside `.agents/`, as your changes will be lost after a NuGet package update.

## License

This project is licensed under the [MIT License](LICENSE).
