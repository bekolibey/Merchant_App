namespace merchant.domain.Constants;

public static class MerchantApplicationStatuses
{
    public const string Pending = "Onay Bekliyor";
    public const string InReview = "Incelemede";
    public const string Approved = "Onaylandi";
    public const string Rejected = "Reddedildi";

    public static readonly string[] All =
    [
        Pending,
        InReview,
        Approved,
        Rejected
    ];
}
