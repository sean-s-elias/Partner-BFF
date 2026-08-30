using Polly;

namespace PartnerBFF.Persistence;

public class ResiliencePolicyFactory
{
    public static ResiliencePipeline<HttpResponseMessage> CreatePartnerVerificationPipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<TimeoutException>()
            })
            .AddCircuitBreaker(new()
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 5,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<TimeoutException>()
            })
            .Build();
    }
}