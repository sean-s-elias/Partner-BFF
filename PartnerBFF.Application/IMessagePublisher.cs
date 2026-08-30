using PartnerBFF.Domain;

namespace PartnerBFF.Application;

public interface IMessagePublisher
{
    Task PublishAsync(TransactionRequest transaction);
}