using PartnerBFF.Application;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

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
        try
        {
            var response = await _httpClient.GetAsync($"partnerVerify?partnerId={partnerId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PartnerVerificationResponse>() 
                       ?? new() { PartnerId = partnerId };
            }
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("Circuit breaker is OPEN: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Verification failed for {PartnerId}", partnerId);
        }

        // Fallback: One clean return statement for all failures/errors
        return new() { IsVerified = false, PartnerId = partnerId };
    }
}