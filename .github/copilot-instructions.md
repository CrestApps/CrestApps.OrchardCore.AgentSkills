# CrestApps.AgentSkills Development Instructions

**ALWAYS reference these instructions first and fall back to searching only if needed.**

## Project Overview

CrestApps.AgentSkills contains shared AI agent skills and MCP tooling for .NET applications and Orchard Core projects.

- **Target Framework**: .NET 10 (net10.0)
- **Skill source of truth**: `src/CrestApps.AgentSkills.Skills/orchardcore/`
- **Packages**:
  - `src/CrestApps.AgentSkills.Mcp/`
  - `src/CrestApps.OrchardCore.AgentSkills/`
  - `src/CrestApps.OrchardCore.AgentSkills.Mcp/`
- **Tests**: `test/CrestApps.AgentSkills.Mcp.Tests/`, `test/CrestApps.OrchardCore.AgentSkills.Mcp.Tests/`

## Build & Test

Run builds and tests from the repository root:

```bash
# Build (treat warnings as errors)
dotnet build -c Release -warnaserror /p:TreatWarningsAsErrors=true /p:RunAnalyzers=true /p:NuGetAudit=false

# Tests
dotnet test -c Release --no-build --verbosity normal
```

## Skill Validation

Each skill directory under `src/CrestApps.AgentSkills.Skills/orchardcore/` must contain a `SKILL.md` file with YAML front-matter (must include `name` and `description`). Validate locally before changes land:

**Bash:**
```bash
for dir in src/CrestApps.AgentSkills.Skills/orchardcore/*/; do
  name=$(basename "$dir")
  if [ ! -f "$dir/SKILL.md" ]; then echo "FAIL: $name missing SKILL.md"; fi
  if ! head -1 "$dir/SKILL.md" | grep -q "^---$"; then echo "FAIL: $name bad front-matter"; fi
  echo "OK: $name"
done
```

**PowerShell:**
```powershell
Get-ChildItem -Path "src\CrestApps.AgentSkills.Skills\orchardcore" -Directory | ForEach-Object {
    $skillFile = Join-Path $_.FullName "SKILL.md"
    if (-not (Test-Path $skillFile)) { Write-Host "FAIL: $($_.Name) missing SKILL.md" -ForegroundColor Red }
    elseif ((Get-Content $skillFile -First 1) -ne "---") { Write-Host "FAIL: $($_.Name) bad front-matter" -ForegroundColor Red }
    else { Write-Host "OK: $($_.Name)" -ForegroundColor Green }
}
```

## Packaging Notes

The solution is configured for preview packages by default (`VersionSuffix=preview` in `Directory.Build.props`).
For release builds, override the version (for example via CI) and publish with `dotnet pack`.

## Coding Standards

- Follow `.editorconfig` for formatting and naming rules.
- Prefer minimal, focused changes and keep documentation in sync with code updates.
