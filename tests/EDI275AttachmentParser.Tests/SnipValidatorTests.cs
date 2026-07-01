using Xunit;
using EDI275AttachmentParser.Services;

public class SnipValidatorTests
{
    [Theory]
    [InlineData("123-45-6789", "123456789", true)]
    [InlineData("000000000", "000000000", true)]
    [InlineData("12A345678", "12345678", false)]
    [InlineData("abcdef", "", false)]
    public void NormalizeAndValidateBehavior(string input, string expectedNormalized, bool expectedIsValid)
    {
        var v = new SnipValidator();
        var normalized = v.Normalize(input);
        Assert.Equal(expectedNormalized, normalized);
        Assert.Equal(expectedIsValid, v.IsValid(input));
    }
}