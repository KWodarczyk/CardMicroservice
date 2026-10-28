using CardMicroserviceApi.DTOs;
using CardMicroserviceApi.Models;
using CardMicroserviceApi.Services;
using CardMicroserviceApi.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CardMicroserviceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardActionsController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ICardActionsCalculator _cardActionsCalculator;
    private readonly ILogger<CardActionsController> _logger;

    public CardActionsController(
        ICardService cardService,
        ICardActionsCalculator cardActionsCalculator,
        ILogger<CardActionsController> logger)
    {
        _cardService = cardService;
        _cardActionsCalculator = cardActionsCalculator;
        _logger = logger;
    }

    /// <summary>
    /// Returns the actions allowed for the given user's card.
    /// </summary>
    /// <param name="userId">The id of the card owner.</param>
    /// <param name="cardNumber">The card number.</param>
    [HttpGet]
    [ProducesResponseType(typeof(CardActionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CardActionsResponse>> GetAllowedActions(
        [FromQuery] string userId,
        [FromQuery] string cardNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new ApiErrorResponse(ApiErrorCode.MissingUserId, "'userId' is required."));
            }

            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return BadRequest(new ApiErrorResponse(ApiErrorCode.MissingCardNumber, "'cardNumber' is required."));
            }

            if (!CardRequestValidator.IsValidUserId(userId))
            {
                return BadRequest(new ApiErrorResponse(ApiErrorCode.InvalidUserId,
                    $"'userId' must be alphanumeric (letters, digits, '-', '_') and at most {CardRequestValidator.MaxUserIdLength} characters."));
            }

            if (!CardRequestValidator.IsValidCardNumber(cardNumber))
            {
                return BadRequest(new ApiErrorResponse(ApiErrorCode.InvalidCardNumber,
                    $"'cardNumber' must be alphanumeric (letters, digits, '-', '_') and at most {CardRequestValidator.MaxCardNumberLength} characters."));
            }

            CardDetailsResult result = await _cardService.GetCardDetails(userId, cardNumber);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    CardServiceErrorCode.CardNotFound => NotFoundResponse(userId, cardNumber),
                    CardServiceErrorCode.BackendUnavailable => StatusCode(
                        StatusCodes.Status503ServiceUnavailable,
                        new ApiErrorResponse(ApiErrorCode.BackendUnavailable,"The card data service is currently unavailable.")),
                    _ => StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new ApiErrorResponse(ApiErrorCode.UnexpectedError,"An unexpected error occurred while retrieving card details.")),
                };
            }

            CardDetails cardDetails = result.CardDetails!;
            IReadOnlyList<CardAction> allowedActions = _cardActionsCalculator.GetAllowedActions(
                cardDetails.CardType,
                cardDetails.CardStatus,
                cardDetails.IsPinSet);

            return Ok(new CardActionsResponse(userId, cardNumber, allowedActions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while getting allowed actions for user {UserId}, card {CardNumber}.", userId, cardNumber);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(ApiErrorCode.UnexpectedError, "An unexpected error occurred."));
        }
    }

    /// <summary>
    /// Evaluates the actions allowed for an explicit combination of card type,
    /// card status and PIN-set flag, without looking up an actual card. Useful
    /// for verifying the rule engine (e.g. in Swagger/manual testing).
    /// </summary>
    [HttpPost("evaluate")]
    [ProducesResponseType(typeof(CardActionsEvaluationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<CardActionsEvaluationResponse> EvaluateAllowedActions(
        [FromBody] CardActionsEvaluationRequest request)
    {
        try
        {
            IReadOnlyList<CardAction> allowedActions = _cardActionsCalculator.GetAllowedActions(
                request.CardType,
                request.CardStatus,
                request.IsPinSet);

            return Ok(new CardActionsEvaluationResponse(
                request.CardType,
                request.CardStatus,
                request.IsPinSet,
                allowedActions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while evaluating allowed actions for {CardType}/{CardStatus}.", request.CardType, request.CardStatus);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(ApiErrorCode.UnexpectedError, "An unexpected error occurred."));
        }
    }

    private ActionResult<CardActionsResponse> NotFoundResponse(string userId, string cardNumber)
    {
        _logger.LogInformation("Card {CardNumber} not found for user {UserId}.", cardNumber, userId);
        return NotFound(new ApiErrorResponse(ApiErrorCode.CardNotFound,$"Card '{cardNumber}' was not found for user '{userId}'."));
    }
}
