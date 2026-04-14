namespace merchant.domain.Entities;

public sealed class UserClaim
{
    public Guid Id { get; set; }
    public Guid AppUserId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}
