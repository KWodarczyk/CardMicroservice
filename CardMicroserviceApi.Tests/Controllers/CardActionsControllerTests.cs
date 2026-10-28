using CardMicroserviceApi.Controllers;
using CardMicroserviceApi.DTOs;
using CardMicroserviceApi.Models;
using CardMicroserviceApi.Services;
using CardMicroserviceApi.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CardMicroserviceApi.Tests.Controllers;

public class CardActionsControllerTests
{
    private const string ExpectedUserId = "User1";
    private const string ExpectedCardNumber = "Card11";
    private const string ExpectedUnknownCardNumber = "UnknownCard";

    private static readonly CardAction[] ExpectedAllowedActions =
        [CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION9];

    private readonly Mock<ICardService> _cardServiceMock = new();
    private readonly Mock<ICardActionsCalculator> _calculatorMock = new();
    private readonly CardActionsController _cardActionsController;

    public CardActionsControllerTests()
    {
        _cardActionsController = new CardActionsController(
            _cardServiceMock.Object,
            _calculatorMock.Object,
            NullLogger<CardActionsController>.Instance);
    }

    [Fact]
    public async Task GetAllowedActions_CardExists_ReturnsOkWithAllowedActions()
    {
        var cardDetails = new CardDetails(ExpectedCardNumber, CardType.Prepaid, CardStatus.Closed, IsPinSet: false);
        _cardServiceMock
            .Setup(s => s.GetCardDetails(ExpectedUserId, ExpectedCardNumber))
            .ReturnsAsync(CardDetailsResult.Success(cardDetails));
        _calculatorMock
            .Setup(c => c.GetAllowedActions(CardType.Prepaid, CardStatus.Closed, false))
            .Returns(ExpectedAllowedActions);

        var result = await _cardActionsController.GetAllowedActions(ExpectedUserId, ExpectedCardNumber);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CardActionsResponse>(okResult.Value);
        Assert.Equal(ExpectedUserId, response.UserId);
        Assert.Equal(ExpectedCardNumber, response.CardNumber);
        Assert.Equal(ExpectedAllowedActions, response.AllowedActions);
    }

    [Fact]
    public async Task GetAllowedActions_CardNotFound_ReturnsNotFound()
    {
        _cardServiceMock
            .Setup(s => s.GetCardDetails(ExpectedUserId, ExpectedUnknownCardNumber))
            .ReturnsAsync(CardDetailsResult.Failure(CardServiceErrorCode.CardNotFound));

        var result = await _cardActionsController.GetAllowedActions(ExpectedUserId, ExpectedUnknownCardNumber);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(notFoundResult.Value);
        Assert.Equal(ApiErrorCode.CardNotFound, error.ErrorCode);
        _calculatorMock.Verify(
            c => c.GetAllowedActions(It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllowedActions_BackendUnavailable_ReturnsServiceUnavailable()
    {
        _cardServiceMock
            .Setup(s => s.GetCardDetails(ExpectedUserId, ExpectedCardNumber))
            .ReturnsAsync(CardDetailsResult.Failure(CardServiceErrorCode.BackendUnavailable));

        var result = await _cardActionsController.GetAllowedActions(ExpectedUserId, ExpectedCardNumber);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusResult.StatusCode);
        var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
        Assert.Equal(ApiErrorCode.BackendUnavailable, error.ErrorCode);
    }

    [Fact]
    public async Task GetAllowedActions_UnexpectedServiceError_ReturnsInternalServerError()
    {
        _cardServiceMock
            .Setup(s => s.GetCardDetails(ExpectedUserId, ExpectedCardNumber))
            .ReturnsAsync(CardDetailsResult.Failure(CardServiceErrorCode.UnexpectedError));

        var result = await _cardActionsController.GetAllowedActions(ExpectedUserId, ExpectedCardNumber);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
        Assert.Equal(ApiErrorCode.UnexpectedError, error.ErrorCode);
    }

    [Fact]
    public async Task GetAllowedActions_CardServiceThrows_ReturnsInternalServerError()
    {
        _cardServiceMock
            .Setup(s => s.GetCardDetails(ExpectedUserId, ExpectedCardNumber))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await _cardActionsController.GetAllowedActions(ExpectedUserId, ExpectedCardNumber);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
        Assert.Equal(ApiErrorCode.UnexpectedError, error.ErrorCode);
    }

    [Theory]
    [InlineData(null, ExpectedCardNumber, ApiErrorCode.MissingUserId)]
    [InlineData("", ExpectedCardNumber, ApiErrorCode.MissingUserId)]
    [InlineData(" ", ExpectedCardNumber, ApiErrorCode.MissingUserId)]
    [InlineData(ExpectedUserId, null, ApiErrorCode.MissingCardNumber)]
    [InlineData(ExpectedUserId, "", ApiErrorCode.MissingCardNumber)]
    [InlineData(ExpectedUserId, " ", ApiErrorCode.MissingCardNumber)]
    public async Task GetAllowedActions_MissingRequiredParameter_ReturnsBadRequest(
        string? userId, string? cardNumber, ApiErrorCode expectedErrorCode)
    {
        var result = await _cardActionsController.GetAllowedActions(userId!, cardNumber!);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
        Assert.Equal(expectedErrorCode, error.ErrorCode);
        _cardServiceMock.Verify(
            s => s.GetCardDetails(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData("User 1", ExpectedCardNumber, ApiErrorCode.InvalidUserId)]
    [InlineData("User;DROP TABLE", ExpectedCardNumber, ApiErrorCode.InvalidUserId)]
    [InlineData("User1\r\nInjectedLogLine", ExpectedCardNumber, ApiErrorCode.InvalidUserId)]
    [InlineData("User1<script>", ExpectedCardNumber, ApiErrorCode.InvalidUserId)]
    [InlineData(ExpectedUserId, "Card 11", ApiErrorCode.InvalidCardNumber)]
    [InlineData(ExpectedUserId, "Card;11", ApiErrorCode.InvalidCardNumber)]
    public async Task GetAllowedActions_MalformedParameter_ReturnsBadRequest(
        string userId, string cardNumber, ApiErrorCode expectedErrorCode)
    {
        var result = await _cardActionsController.GetAllowedActions(userId, cardNumber);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
        Assert.Equal(expectedErrorCode, error.ErrorCode);
        _cardServiceMock.Verify(
            s => s.GetCardDetails(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllowedActions_UserIdTooLong_ReturnsBadRequest()
    {
        string oversizedUserId = new('a', CardRequestValidator.MaxUserIdLength + 1);

        var result = await _cardActionsController.GetAllowedActions(oversizedUserId, ExpectedCardNumber);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
        Assert.Equal(ApiErrorCode.InvalidUserId, error.ErrorCode);
    }

    [Fact]
    public async Task GetAllowedActions_CardNumberTooLong_ReturnsBadRequest()
    {
        string oversizedCardNumber = new('1', CardRequestValidator.MaxCardNumberLength + 1);

        var result = await _cardActionsController.GetAllowedActions(ExpectedUserId, oversizedCardNumber);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
        Assert.Equal(ApiErrorCode.InvalidCardNumber, error.ErrorCode);
    }

    [Fact]
    public void EvaluateAllowedActions_ReturnsOkWithAllowedActions()
    {
        CardAction[] expectedAllowedActions = [CardAction.ACTION1, CardAction.ACTION3, CardAction.ACTION5];
        _calculatorMock
            .Setup(c => c.GetAllowedActions(CardType.Credit, CardStatus.Active, true))
            .Returns(expectedAllowedActions);

        var request = new CardActionsEvaluationRequest(CardType.Credit, CardStatus.Active, IsPinSet: true);

        var result = _cardActionsController.EvaluateAllowedActions(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CardActionsEvaluationResponse>(okResult.Value);
        Assert.Equal(CardType.Credit, response.CardType);
        Assert.Equal(CardStatus.Active, response.CardStatus);
        Assert.True(response.IsPinSet);
        Assert.Equal(expectedAllowedActions, response.AllowedActions);
    }

    [Fact]
    public void EvaluateAllowedActions_CalculatorThrows_ReturnsInternalServerError()
    {
        _calculatorMock
            .Setup(c => c.GetAllowedActions(It.IsAny<CardType>(), It.IsAny<CardStatus>(), It.IsAny<bool>()))
            .Throws(new InvalidOperationException("boom"));

        var request = new CardActionsEvaluationRequest(CardType.Debit, CardStatus.Ordered, IsPinSet: false);

        var result = _cardActionsController.EvaluateAllowedActions(request);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
        Assert.Equal(ApiErrorCode.UnexpectedError, error.ErrorCode);
    }
}
