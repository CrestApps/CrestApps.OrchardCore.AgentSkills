# CrestApps.OrchardCore.AgentSkills

A development-only NuGet package that distributes shared **AI agent instruction and guardrail files** for Orchard Core development.

When installed, agent skill files are automatically copied into the **solution-root** `.agents/skills` folder on package install, update, and restore. There is **no runtime dependency** — this package is used purely for development and AI tooling guidance.

## Install

```bash
dotnet add package CrestApps.OrchardCore.AgentSkills
```

## Behavior

- Skills are **auto-mounted** into `.agents/skills` at the solution root.
- Used by **GitHub Copilot**, **Cursor**, **Cline**, and other AI agents for code-generation guidance.
- Files are **refreshed automatically** when the NuGet package is updated.
- There is **no runtime dependency** and **no deployment impact**.
- Copying happens via MSBuild targets on install/update — **not at build time**.

After install, the solution root will contain:

```
.agents/
  skills/
    orchardcore.content-types/
    orchardcore.modules/
    orchardcore.recipes/
    orchardcore.deployments/
    orchardcore.ai/
```

## Updating

```bash
dotnet add package CrestApps.OrchardCore.AgentSkills --version x.x.x
```

Files in `.agents/skills` are replaced with the latest version from the package.

> **Note:**
> This project installs shared agent files into your local `.agents/` folder.
> If needed, it will replace common agent files (such as `Agents.md`) that already exist in your project.
> Do **not** modify files added by this package inside `.agents/`, as your changes will be lost after a NuGet package update.

## License

This project is licensed under the [MIT License](../../LICENSE).
