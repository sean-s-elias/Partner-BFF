using System.Text;
using System.Text.Json;
using PartnerBFF.Application;
using PartnerBFF.Domain;
using RabbitMQ.Client;

namespace PartnerBFF.Persistence;

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private const string QueueName = "partner-transactions";
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqPublisher(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqPublisher> CreateAsync(string hostName = "localhost")
    {
        var connection = await new ConnectionFactory { HostName = hostName }.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);

        return new RabbitMqPublisher(connection, channel);
    }

    public async Task PublishAsync(TransactionRequest transaction)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transaction));
        await _channel.BasicPublishAsync(exchange: "", routingKey: QueueName, body: body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}