using PartnerBFF.Persistence;
using Xunit;
using Polly;

namespace PartnerBFF.Tests;

public class ResiliencePolicyTests
{
    private ResiliencePipeline<HttpResponseMessage>? _pipeline;
    private int _attemptCount;
    
    [Fact]
    public async Task Should_Retry_On_TimeoutException_And_Eventually_Succeed()
    {
        Setup();

        var result = await _pipeline!.ExecuteAsync(async _ =>
        {
            _attemptCount++;
            if (_attemptCount < 3) throw new TimeoutException("Simulated timeout");
            await Task.CompletedTask;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        });

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(3, _attemptCount);
    }
    
    [Fact]
    public async Task Should_Throw_After_Exhausting_All_Retries()
    {
        Setup();

        await Assert.ThrowsAsync<TimeoutException>(async () =>
        {
            await _pipeline!.ExecuteAsync<HttpResponseMessage>(_ =>
            {
                _attemptCount++;
                throw new TimeoutException("Always fails");
            });
        });

        Assert.Equal(4, _attemptCount);
    }
    
    private void Setup()
    {
        _pipeline = ResiliencePolicyFactory.CreatePartnerVerificationPipeline();
        _attemptCount = 0;
    }
}