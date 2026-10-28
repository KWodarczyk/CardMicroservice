using CardDataApi.DTOs;
using CardDataApi.Services;
using CardDataApi.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CardDataApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardInfoController : ControllerBase
{
    private readonly ICardDataProvider _cardDataProvider;
    private readonly ILogger<CardInfoController> _logger;

    public CardInfoController(ICardDataProvider cardDataProvider, ILogger<CardInfoController> logger)
    {
        _cardDataProvider = cardDataProvider;
        _logger = logger;
    }

    /// <summary>
    /// Returns the card data for the given user's card.
    /// </summary>
    /// <param name="userId">The id of the card owner.</param>
    /// <param name="cardNumber">The card number.</param>
    [HttpGet]
    [ProducesResponseType(typeof(CardInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CardInfoDto> GetCardInfo([FromQuery] string userId, [FromQuery] string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("'userId' is required.");
        }

        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return BadRequest("'cardNumber' is required.");
        }

        if (!CardRequestValidator.IsValidUserId(userId))
        {
            return BadRequest(
                $"'userId' must be alphanumeric (letters, digits, '-', '_') and at most {CardRequestValidator.MaxUserIdLength} characters.");
        }

        if (!CardRequestValidator.IsValidCardNumber(cardNumber))
        {
            return BadRequest(
                $"'cardNumber' must be alphanumeric (letters, digits, '-', '_') and at most {CardRequestValidator.MaxCardNumberLength} characters.");
        }

        var card = _cardDataProvider.GetCard(userId, cardNumber);

        if (card is null)
        {
            _logger.LogInformation("Card {CardNumber} not found for user {UserId}.", cardNumber, userId);
            return NotFound($"Card '{cardNumber}' was not found for user '{userId}'.");
        }

        return Ok(new CardInfoDto(card.CardNumber, card.CardType, card.CardStatus, card.IsPinSet));
    }
}
