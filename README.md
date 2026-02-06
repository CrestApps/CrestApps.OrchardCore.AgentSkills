# CrestApps.OrchardCore.AgentSkills

Reusable **Agent Skills for Orchard Core development**, distributed via NuGet. These skills follow [agentskills.io](https://agentskills.io/) standards, encapsulate Orchard Core best practices, and improve AI code generation quality.

Once installed, skills **automatically mount** into the consumer project's `.agents/skills` folder so AI agents discover and use them immediately—no manual copying required.

## Overview

- **Purpose**: Provide a centralized, versioned set of Orchard Core skills that AI agents can consume.
- **Orchard Core Specialization**: Covers content types, modules, recipes, deployments, and AI integrations.
- **agentskills.io Compliance**: Every skill includes `skill.yaml`, input/output schemas, prompt templates, examples, and follows naming and validation standards.
- **AI Quality Improvements**: Agents generate higher-quality Orchard Core code by leveraging standardized prompts and examples.

## Installation

```bash
dotnet add package CrestApps.OrchardCore.AgentSkills
```

## How Mounting Works

When you install the NuGet package, the skills are delivered via the `contentFiles` mechanism built into NuGet:

- Skills **auto-mount** into `.agents/skills` at build time.
- **No manual copying** is required.
- Agents treat the mounted skills as **local skills**.
- The **package remains the source of truth**—updates flow via NuGet restore.

After installation, your project will contain:

```
.agents/
  skills/
    orchardcore.content-types/
    orchardcore.modules/
    orchardcore.recipes/
    orchardcore.deployments/
    orchardcore.ai/
```

## Registering Skills

Use the builder extension to register the auto-mounted skills with your agent framework:

```csharp
agentBuilder.AddOrchardCoreSkills();
```

Full usage example:

```csharp
builder.Services.AddAgents(agent =>
{
    agent.AddOrchardCoreSkills();
});
```

## Keeping Projects Up To Date

### Floating Version

Always get the latest skills on restore/build with no manual updates:

```xml
<PackageReference
  Include="CrestApps.OrchardCore.AgentSkills"
  Version="*" />
```

or pin to a major version:

```xml
<PackageReference
  Include="CrestApps.OrchardCore.AgentSkills"
  Version="1.*" />
```

### Locked Version

Pin to a specific version for full control:

```xml
<PackageReference
  Include="CrestApps.OrchardCore.AgentSkills"
  Version="1.0.0" />
```

Update manually:

```bash
dotnet add package CrestApps.OrchardCore.AgentSkills --version 1.1.0
```

## Skill Categories

| Skill | Description |
|---|---|
| `orchardcore.content-types` | Creating and managing Orchard Core content types, parts, and fields |
| `orchardcore.modules` | Scaffolding modules, features, manifests, and startup configuration |
| `orchardcore.recipes` | Recipe structure, steps, content definitions, and content items |
| `orchardcore.deployments` | Deployment plans and import/export configuration |
| `orchardcore.ai` | AI service integration, MCP enablement, and agent framework setup |

## agentskills.io Compliance

All skills in this package comply with [agentskills.io](https://agentskills.io/) standards:

- Each skill includes a `skill.yaml` with metadata, inputs, and outputs.
- Prompt templates are provided in `prompts.md`.
- Examples are included where applicable.
- Naming follows the `orchardcore.<category>` convention.
- Input/output schemas are fully defined.

## License

This project is licensed under the [MIT License](LICENSE).
