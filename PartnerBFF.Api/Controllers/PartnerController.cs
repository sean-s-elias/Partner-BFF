using Microsoft.AspNetCore.Mvc;
using PartnerBFF.Application;
using PartnerBFF.Domain;

namespace PartnerBFF.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PartnerController : ControllerBase
{
    private readonly IPartnerVerificationService _verificationService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<PartnerController> _logger;
    
    public PartnerController
    (IPartnerVerificationService verificationService, 
        IMessagePublisher  messagePublisher, 
        ILogger<PartnerController> logger)
    {
        _verificationService = verificationService;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> Transactions([FromBody]TransactionRequest  request)
    {
        var verification = await _verificationService.VerifyPartnerAsync(request.PartnerId);

        if (!verification.IsVerified)
        {
            _logger.LogWarning("Partner verification failed for {PartnerId}", request.PartnerId);
            return BadRequest(new {  message = "Partner verification failed." });
        }

        await _messagePublisher.PublishAsync(request);
        
        return Ok(new {message = "Partner verification completed." });    
    }
}