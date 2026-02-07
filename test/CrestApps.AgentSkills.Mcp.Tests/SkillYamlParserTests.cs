using CrestApps.AgentSkills.Mcp.Services;
using Xunit;

namespace CrestApps.AgentSkills.Mcp.Tests;

public sealed class SkillYamlParserTests
{
    [Fact]
    public void TryParse_ValidYaml_ReturnsTrueAndExtractsFields()
    {
        var content = "name: test-skill\ndescription: A test skill for testing.";

        var result = SkillYamlParser.TryParse(content, out var name, out var description, out var body);

        Assert.True(result);
        Assert.Equal("test-skill", name);
        Assert.Equal("A test skill for testing.", description);
        Assert.Equal(string.Empty, body);
    }

    [Fact]
    public void TryParse_ValidYamlWithBody_ReturnsTrueAndExtractsBody()
    {
        var content = "name: test-skill\ndescription: A test skill.\nbody: |\n  # Body Content\n  Some text.";

        var result = SkillYamlParser.TryParse(content, out var name, out var description, out var body);

        Assert.True(result);
        Assert.Equal("test-skill", name);
        Assert.Equal("A test skill.", description);
        Assert.Contains("Body Content", body);
    }

    [Fact]
    public void TryParse_MissingNameField_ReturnsFalse()
    {
        var content = "description: A test skill.";

        var result = SkillYamlParser.TryParse(content, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_MissingDescriptionField_ReturnsFalse()
    {
        var content = "name: test-skill";

        var result = SkillYamlParser.TryParse(content, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_EmptyContent_ReturnsFalse()
    {
        var result = SkillYamlParser.TryParse("", out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_NullContent_ReturnsFalse()
    {
        var result = SkillYamlParser.TryParse(null!, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_WhitespaceOnlyContent_ReturnsFalse()
    {
        var result = SkillYamlParser.TryParse("   \n  \n  ", out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_InvalidYamlSyntax_ReturnsFalse()
    {
        var content = "name: [invalid\ndescription: broken yaml";

        var result = SkillYamlParser.TryParse(content, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_EmptyNameField_ReturnsFalse()
    {
        var content = "name: \ndescription: A test skill.";

        var result = SkillYamlParser.TryParse(content, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_EmptyDescriptionField_ReturnsFalse()
    {
        var content = "name: test-skill\ndescription: ";

        var result = SkillYamlParser.TryParse(content, out _, out _, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_AdditionalFieldsIgnored()
    {
        var content = "name: my-skill\ndescription: My skill.\nlicense: MIT\nauthor: test";

        var result = SkillYamlParser.TryParse(content, out var name, out var description, out _);

        Assert.True(result);
        Assert.Equal("my-skill", name);
        Assert.Equal("My skill.", description);
    }

    [Fact]
    public void TryParse_WindowsLineEndings_WorksCorrectly()
    {
        var content = "name: test-skill\r\ndescription: A test skill.\r\nbody: Some body content.";

        var result = SkillYamlParser.TryParse(content, out var name, out var description, out var body);

        Assert.True(result);
        Assert.Equal("test-skill", name);
        Assert.Equal("A test skill.", description);
        Assert.Equal("Some body content.", body);
    }

    [Fact]
    public void TryParse_MultiLineBody_ExtractsCorrectly()
    {
        var content = "name: test-skill\ndescription: A test.\nbody: |\n  Line 1\n  Line 2\n  Line 3";

        var result = SkillYamlParser.TryParse(content, out _, out _, out var body);

        Assert.True(result);
        Assert.Contains("Line 1", body);
        Assert.Contains("Line 2", body);
        Assert.Contains("Line 3", body);
    }
}
