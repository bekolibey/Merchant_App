namespace MerchantApp.WebAPI.Services;

public sealed class MerchantApplicationValidationException(Dictionary<string, string[]> errors) : Exception("Merchant application validation failed.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}
