using Popocatepetl.CLI.Menus.PowerUser;

namespace Popocatepetl.CLI.Tests.Menus.PowerUser;

public class SendReportActionTests
{
    [Theory]
    [InlineData("a@b.com", new[] { "a@b.com" })]
    [InlineData("a@b.com,c@d.com", new[] { "a@b.com", "c@d.com" })]
    [InlineData("a@b.com;c@d.com", new[] { "a@b.com", "c@d.com" })]
    [InlineData("a@b.com, c@d.com ; e@f.com", new[] { "a@b.com", "c@d.com", "e@f.com" })]
    [InlineData("  a@b.com  ", new[] { "a@b.com" })]
    public void ParseRecipients_SplitsOnCommaAndSemicolonAndTrims(string input, string[] expected)
    {
        SendReportAction.ParseRecipients(input).Should().Equal(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseRecipients_EmptyInput_ReturnsEmpty(string input)
    {
        SendReportAction.ParseRecipients(input).Should().BeEmpty();
    }

    [Fact]
    public void ValidateList_EmptyInput_AllowsItForCancelPath()
    {
        var result = SendReportAction.ValidateList("");
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateList_ValidList_Passes()
    {
        var result = SendReportAction.ValidateList("alice@example.com, bob@example.com");
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateList_InvalidEmail_FailsAndNamesTheBadAddress()
    {
        var result = SendReportAction.ValidateList("alice@example.com, not-an-email");

        result.IsValid.Should().BeFalse();
        result.ErrorKey.Should().Be("menu.poweruser.send.invalid");
        result.ErrorArgs.Should().NotBeNull().And.ContainSingle().Which.Should().Be("not-an-email");
    }

    [Fact]
    public void ValidateList_TooManyRecipients_Fails()
    {
        var emails = string.Join(",", Enumerable.Range(1, 101).Select(i => $"u{i}@example.com"));

        var result = SendReportAction.ValidateList(emails);

        result.IsValid.Should().BeFalse();
        result.ErrorKey.Should().Be("menu.poweruser.send.too-many");
    }

    [Fact]
    public void ValidateList_ExactlyMaxRecipients_Passes()
    {
        var emails = string.Join(",", Enumerable.Range(1, 100).Select(i => $"u{i}@example.com"));

        var result = SendReportAction.ValidateList(emails);

        result.IsValid.Should().BeTrue();
    }
}
