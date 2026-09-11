using Microsoft.AspNetCore.Mvc;
using PartnerBFF.Application;

namespace PartnerBFF.Api.Controllers;

[ApiController]
[Route("partnerVerify")]
public class PartnerVerificationController : ControllerBase
{
    private static readonly Random RandomGenerator = new();
    
    [HttpGet]
    public IActionResult Verify([FromQuery] string partnerId)
    {
        if (RandomGenerator.Next(1, 101) <= 30)
        {
            throw new TimeoutException($"Partner verification timed out for {partnerId}");
        }
        
        return Ok(new PartnerVerificationResponse
        {
            IsVerified =  true,
            PartnerId = partnerId
        });
    }
}