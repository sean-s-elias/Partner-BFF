using Polly;

namespace PartnerBFF.Persistence;

public class ResiliencePolicyFactory
{
    public static ResiliencePipeline<HttpResponseMessage> CreatePartnerVerificationPipeline()
    {
        var failurePredicate = new PredicateBuilder<HttpResponseMessage>()
            .Handle<TimeoutException>()          
            .Handle<HttpRequestException>()      
            .HandleResult(response => !response.IsSuccessStatusCode); 
        
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true, // Added: Best practice to prevent slamming servers simultaneously
                ShouldHandle = failurePredicate // Fixed: Actually using the variable here
            })
            .AddCircuitBreaker(new()
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = 5,
                ShouldHandle = failurePredicate // Fixed: Actually using the variable here
            })
            .Build();
    }
}