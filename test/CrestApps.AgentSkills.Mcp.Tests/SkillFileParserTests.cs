using CrestApps.AgentSkills.Mcp.Services;
using Xunit;

namespace CrestApps.AgentSkills.Mcp.Tests;

public sealed class SkillFileParserTests
{
    [Fact]
    public void TryParse_MarkdownFile_DelegatesToFrontMatterParser()
    {
        var content = "---\nname: test-skill\ndescription: A test.\n---\n# Body";

        var result = SkillFileParser.TryParse("SKILL.md", content, out var name, out var description, out var body);

        Assert.True(result);
        Assert.Equal("test-skill", name);
        Assert.Equal("A test.", description);
        Assert.Equal("# Body", body);
    }

    [Fact]
    public void TryParse_YamlFile_DelegatesToYamlParser()
    {
        var content = "name: test-skill\ndescription: A test skill.";

        var result = SkillFileParser.TryParse("SKILL.yaml", content, out var name, out var description, out _);

        Assert.True(result);
        Assert.Equal("test-skill", name);
        Assert.Equal("A test skill.", description);
    }

    [Fact]
    public void TryParse_YmlFile_DelegatesToYamlParser()
    {
        var content = "name: test-skill\ndescription: A test skill.";

        var result = SkillFileParser.TryParse("SKILL.yml", content, out var name, out var description, out _);

        Assert.True(result);
        Assert.Equal("test-skill", name);
        Assert.Equal("A test skill.", description);
    }

    [Fact]
    public void TryParse_UnsupportedExtension_ReturnsFalse()
    {
        var content = "name: test-skill\ndescription: A test.";

        var result = SkillFileParser.TryParse("SKILL.json", content, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_NullFileName_ReturnsFalse()
    {
        var result = SkillFileParser.TryParse(null!, "content", out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_EmptyFileName_ReturnsFalse()
    {
        var result = SkillFileParser.TryParse("", "content", out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_NullContent_ReturnsFalse()
    {
        var result = SkillFileParser.TryParse("SKILL.md", null!, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_EmptyContent_ReturnsFalse()
    {
        var result = SkillFileParser.TryParse("SKILL.yaml", "", out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_CaseInsensitiveExtension_Works()
    {
        var content = "name: test-skill\ndescription: A test skill.";

        var resultYaml = SkillFileParser.TryParse("SKILL.YAML", content, out _, out _, out _);
        var resultYml = SkillFileParser.TryParse("SKILL.YML", content, out _, out _, out _);

        Assert.True(resultYaml);
        Assert.True(resultYml);
    }

    [Fact]
    public void TryParse_ConsistentOutputForMarkdownAndYaml()
    {
        var mdContent = "---\nname: my-skill\ndescription: My description.\n---\nBody text.";
        var yamlContent = "name: my-skill\ndescription: My description.\nbody: Body text.";

        SkillFileParser.TryParse("SKILL.md", mdContent, out var mdName, out var mdDesc, out var mdBody);
        SkillFileParser.TryParse("SKILL.yaml", yamlContent, out var yamlName, out var yamlDesc, out var yamlBody);

        Assert.Equal(mdName, yamlName);
        Assert.Equal(mdDesc, yamlDesc);
        Assert.Equal(mdBody, yamlBody);
    }

    [Fact]
    public void SupportedSkillFileNames_ContainsExpectedEntries()
    {
        var fileNames = SkillFileParser.SupportedSkillFileNames;

        Assert.Contains("SKILL.md", fileNames);
        Assert.Contains("SKILL.yaml", fileNames);
        Assert.Contains("SKILL.yml", fileNames);
        Assert.Equal(3, fileNames.Count);
    }
}
