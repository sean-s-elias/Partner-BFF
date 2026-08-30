using PartnerBFF.Application;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PartnerBFF.Persistence;

public class PartnerVerificationService : IPartnerVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PartnerVerificationService> _logger;
    
    public PartnerVerificationService(HttpClient httpClient,  ILogger<PartnerVerificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<PartnerVerificationResponse> VerifyPartnerAsync(string partnerId)
    {
        var response = await _httpClient.GetFromJsonAsync<PartnerVerificationResponse>(
            $"partnerVerify?partnerId={partnerId}");

        if (response == null)
        {
            _logger.LogError("Could not verify partner with id {partnerId}", partnerId);
        }

        return response ?? throw new InvalidOperationException("Partner verification returned no data.");
    }
}