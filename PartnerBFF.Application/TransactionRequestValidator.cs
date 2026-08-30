using FluentValidation;
using PartnerBFF.Domain;

namespace PartnerBFF.Application;

public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
{
    public TransactionRequestValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEmpty().WithMessage("PartnerId is required.");

        RuleFor(x => x.TransactionReference)
            .NotEmpty().WithMessage("TransactionReference is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(x => Enum.TryParse<Currency>(x, ignoreCase: true, out _))
            .WithMessage("Currency is not a valid currency code.");

        RuleFor(x => x.Timestamp)
            .NotEqual(default(DateTime)).WithMessage("Timestamp is required.");
    }
}