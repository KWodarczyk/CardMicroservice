using CardMicroserviceApi.CardDataApiClient;
using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

/// <summary>
/// Executes a CardDataApi call and converts its outcome (success or exception) into a
/// <see cref="CardDetailsResult"/>. Extracted from CardServiceClient so the try/catch
/// error-mapping logic can be unit tested directly, by passing a fake delegate that
/// throws, without needing to mock the generated HTTP client.
/// </summary>
public static class CardServiceResultHandler
{
    public static async Task<CardDetailsResult> Execute(
        Func<Task<CardInfoDto>> apiCall,
        Func<CardInfoDto, CardDetails> map)
    {
        try
        {
            var cardInfo = await apiCall();
            return CardDetailsResult.Success(map(cardInfo));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return CardDetailsResult.Failure(CardServiceErrorCode.CardNotFound);
        }
        catch (ApiException)
        {
            return CardDetailsResult.Failure(CardServiceErrorCode.UnexpectedError);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return CardDetailsResult.Failure(CardServiceErrorCode.BackendUnavailable);
        }
    }
}
