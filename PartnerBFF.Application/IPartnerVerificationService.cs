namespace PartnerBFF.Application;

public interface IPartnerVerificationService
{
    Task<PartnerVerificationResponse> VerifyPartnerAsync(string partnerId);
}