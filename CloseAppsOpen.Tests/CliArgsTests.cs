using CloseAppsOpen;
using Xunit;

namespace CloseAppsOpen.Tests;

public class CliArgsTests
{
    [Fact]
    public void Parse_NoArgs_DefaultValues()
    {
        var args = CliArgs.Parse([]);

        Assert.False(args.Help);
        Assert.False(args.Version);
        Assert.False(args.CloseAll);
        Assert.False(args.List);
        Assert.False(args.Force);
        Assert.Equal(2000, args.Timeout);
        Assert.Empty(args.Kill);
        Assert.Empty(args.Exclude);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    public void Parse_HelpFlag_SetsHelp(string flag)
    {
        var args = CliArgs.Parse([flag]);
        Assert.True(args.Help);
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--version")]
    public void Parse_VersionFlag_SetsVersion(string flag)
    {
        var args = CliArgs.Parse([flag]);
        Assert.True(args.Version);
    }

    [Theory]
    [InlineData("-a")]
    [InlineData("--all")]
    public void Parse_AllFlag_SetsCloseAll(string flag)
    {
        var args = CliArgs.Parse([flag]);
        Assert.True(args.CloseAll);
    }

    [Theory]
    [InlineData("-l")]
    [InlineData("--list")]
    public void Parse_ListFlag_SetsList(string flag)
    {
        var args = CliArgs.Parse([flag]);
        Assert.True(args.List);
    }

    [Theory]
    [InlineData("-f")]
    [InlineData("--force")]
    public void Parse_ForceFlag_SetsForce(string flag)
    {
        var args = CliArgs.Parse([flag]);
        Assert.True(args.Force);
    }

    [Theory]
    [InlineData("-t", "5000", 5000)]
    [InlineData("--timeout", "1500", 1500)]
    [InlineData("-t", "0", 0)]
    public void Parse_TimeoutFlag_SetsTimeout(string flag, string value, int expected)
    {
        var args = CliArgs.Parse([flag, value]);
        Assert.Equal(expected, args.Timeout);
    }

    [Fact]
    public void Parse_TimeoutWithInvalidValue_KeepsDefault()
    {
        var args = CliArgs.Parse(["-t", "notanumber"]);
        Assert.Equal(2000, args.Timeout);
    }

    [Fact]
    public void Parse_TimeoutWithoutValue_KeepsDefault()
    {
        var args = CliArgs.Parse(["-t"]);
        Assert.Equal(2000, args.Timeout);
    }

    [Theory]
    [InlineData("-k")]
    [InlineData("--kill")]
    public void Parse_KillFlag_AddsToKillList(string flag)
    {
        var args = CliArgs.Parse([flag, "chrome"]);
        Assert.Single(args.Kill);
        Assert.Equal("chrome", args.Kill[0]);
    }

    [Fact]
    public void Parse_MultipleKillFlags_AddsAll()
    {
        var args = CliArgs.Parse(["-k", "chrome", "-k", "slack", "--kill", "firefox"]);
        Assert.Equal(3, args.Kill.Count);
        Assert.Contains("chrome", args.Kill);
        Assert.Contains("slack", args.Kill);
        Assert.Contains("firefox", args.Kill);
    }

    [Theory]
    [InlineData("-e")]
    [InlineData("--exclude")]
    public void Parse_ExcludeFlag_AddsToExcludeList(string flag)
    {
        var args = CliArgs.Parse([flag, "explorer"]);
        Assert.Single(args.Exclude);
        Assert.Equal("explorer", args.Exclude[0]);
    }

    [Fact]
    public void Parse_MultipleExcludeFlags_AddsAll()
    {
        var args = CliArgs.Parse(["-e", "explorer", "--exclude", "taskmgr"]);
        Assert.Equal(2, args.Exclude.Count);
        Assert.Contains("explorer", args.Exclude);
        Assert.Contains("taskmgr", args.Exclude);
    }

    [Fact]
    public void Parse_CombinedFlags_SetsAll()
    {
        var args = CliArgs.Parse(["--all", "--force", "-e", "explorer", "-t", "3000"]);

        Assert.True(args.CloseAll);
        Assert.True(args.Force);
        Assert.Single(args.Exclude);
        Assert.Equal("explorer", args.Exclude[0]);
        Assert.Equal(3000, args.Timeout);
    }

    [Fact]
    public void Parse_KillWithoutValue_DoesNotAdd()
    {
        var args = CliArgs.Parse(["-k"]);
        Assert.Empty(args.Kill);
    }

    [Fact]
    public void Parse_ExcludeWithoutValue_DoesNotAdd()
    {
        var args = CliArgs.Parse(["-e"]);
        Assert.Empty(args.Exclude);
    }

    [Fact]
    public void Parse_FlagsAreCaseInsensitive()
    {
        var args = CliArgs.Parse(["--ALL"]);
        Assert.True(args.CloseAll);
    }
}
