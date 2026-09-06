using Microsoft.AspNetCore.Mvc;
using PartnerBFF.Application;

namespace PartnerBFF.Api.Controllers;

[ApiController]
[Route("partnerVerify")]
public class PartnerVerificationController : ControllerBase
{
    [HttpGet]
    public IActionResult Verify([FromQuery] string partnerId)
    {
        return Ok(new PartnerVerificationResponse
        {
            IsVerified =  true,
            PartnerId = partnerId
        });
    }
}