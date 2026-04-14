namespace MerchantApp.WebAPI.Services;

public sealed class MerchantApplicationOptions
{
    public string UploadRoot { get; set; } = "uploads/merchant-applications";
    public int PendingTimeoutHours { get; set; } = 48;
    public int PendingScanIntervalMinutes { get; set; } = 30;
}
