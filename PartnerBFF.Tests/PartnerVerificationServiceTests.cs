using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PartnerBFF.Persistence;
using Xunit;

namespace PartnerBFF.Tests;

public class PartnerVerificationServiceTests
{
    private Mock<HttpMessageHandler> _handlerMock;
    private Mock<ILogger<PartnerVerificationService>> _loggerMock;
    private PartnerVerificationService _service;
    
    [Fact]
    public async Task Should_Return_Verification_When_Response_Is_Valid()
    {
        Setup();

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{"isVerified":true,"partnerId":"P-1001"}""")
            });

        var result = await _service.VerifyPartnerAsync("P-1001");

        Assert.True(result.IsVerified);
        Assert.Equal("P-1001", result.PartnerId);
    }

    [Fact]
    public async Task Should_Throw_When_Response_Is_Null()
    {
        Setup();

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null")
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.VerifyPartnerAsync("P-1001"));
        
        Assert.Equal("Partner verification returned no data.", exception.Message);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Could not verify partner with id P-1001"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void Setup()
    {
        _handlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        _loggerMock = new Mock<ILogger<PartnerVerificationService>>();
        _service = new PartnerVerificationService(httpClient, _loggerMock.Object);
    }
}