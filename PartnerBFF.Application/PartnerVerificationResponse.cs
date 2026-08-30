namespace PartnerBFF.Application;

public class PartnerVerificationResponse
{
    public bool IsVerified { get; set; }
    public string PartnerId { get; set; } = string.Empty;
}