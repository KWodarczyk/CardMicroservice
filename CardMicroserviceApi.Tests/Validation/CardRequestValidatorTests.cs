using CardMicroserviceApi.Validation;
using Xunit;

namespace CardMicroserviceApi.Tests.Validation;

public class CardRequestValidatorTests
{
    [Theory]
    [InlineData("User1")]
    [InlineData("user-1")]
    [InlineData("user_1")]
    [InlineData("ABC123")]
    public void IsValidUserId_AcceptsAlphanumericIds(string userId)
    {
        Assert.True(CardRequestValidator.IsValidUserId(userId));
    }

    [Theory]
    [InlineData("User 1")]
    [InlineData("User;DROP TABLE")]
    [InlineData("User1\r\nInjected")]
    [InlineData("User1<script>")]
    [InlineData("")]
    public void IsValidUserId_RejectsMalformedIds(string userId)
    {
        Assert.False(CardRequestValidator.IsValidUserId(userId));
    }

    [Fact]
    public void IsValidUserId_RejectsIdLongerThanMax()
    {
        string oversized = new('a', CardRequestValidator.MaxUserIdLength + 1);

        Assert.False(CardRequestValidator.IsValidUserId(oversized));
    }

    [Fact]
    public void IsValidUserId_AcceptsIdAtMaxLength()
    {
        string maxLength = new('a', CardRequestValidator.MaxUserIdLength);

        Assert.True(CardRequestValidator.IsValidUserId(maxLength));
    }

    [Theory]
    [InlineData("Card11")]
    [InlineData("Card-11")]
    [InlineData("CARD_42")]
    public void IsValidCardNumber_AcceptsAlphanumericCardNumbers(string cardNumber)
    {
        Assert.True(CardRequestValidator.IsValidCardNumber(cardNumber));
    }

    [Theory]
    [InlineData("Card 11")]
    [InlineData("Card;11")]
    [InlineData("Card11\r\nInjected")]
    [InlineData("")]
    public void IsValidCardNumber_RejectsMalformedCardNumbers(string cardNumber)
    {
        Assert.False(CardRequestValidator.IsValidCardNumber(cardNumber));
    }

    [Fact]
    public void IsValidCardNumber_RejectsCardNumberLongerThanMax()
    {
        string oversized = new('1', CardRequestValidator.MaxCardNumberLength + 1);

        Assert.False(CardRequestValidator.IsValidCardNumber(oversized));
    }

    [Fact]
    public void IsValidCardNumber_AcceptsCardNumberAtMaxLength()
    {
        string maxLength = new('1', CardRequestValidator.MaxCardNumberLength);

        Assert.True(CardRequestValidator.IsValidCardNumber(maxLength));
    }
}
