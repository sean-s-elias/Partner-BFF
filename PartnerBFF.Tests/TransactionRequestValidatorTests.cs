using PartnerBFF.Application;
using PartnerBFF.Domain;
using Xunit;

namespace PartnerBFF.Tests;

public class TransactionRequestValidatorTests
{
    private readonly TransactionRequestValidator _validator = new();
    private TransactionRequest _request = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        Setup();

        var result = _validator.Validate(_request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Zero_Or_Negative()
    {
        Setup();
        _request.Amount = 0;
        
        var result = _validator.Validate(_request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.Amount));
    }

    [Fact]
    public void Should_Fail_When_Currency_Is_Invalid()
    {
        Setup();
        _request.Amount = 100;
        _request.Currency = "XXX";
        
        var result = _validator.Validate(_request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.Currency));
    }

    [Fact]
    public void Should_Fail_When_PartnerId_Is_Missing()
    {
        Setup();
        _request.PartnerId = "";
        _request.Amount = 100;
        
        var result = _validator.Validate(_request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(TransactionRequest.PartnerId));
    }

    private void Setup()
    {
        _request = new TransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TXN-99823",
            Amount = 250.00m,
            Currency = nameof(Currency.Usd),
            Timestamp = DateTime.UtcNow
        };
    }
}