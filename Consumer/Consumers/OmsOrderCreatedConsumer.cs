using System.Text;
using Common;
using Microsoft.Extensions.Options;
using Messages;
using Models.Dto.V1.Requests;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebApi.Consumer.Clients;
using WebApi.Consumer.Config;

namespace WebApi.Consumer.Consumers;

public class OmsOrderCreatedConsumer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RabbitMqSettings> _rabbitMqSettings;
    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private IChannel _channel;
    private AsyncEventingBasicConsumer _consumer;
    
    public OmsOrderCreatedConsumer(IOptions<RabbitMqSettings> rabbitMqSettings, IServiceProvider serviceProvider)
    {
        _rabbitMqSettings = rabbitMqSettings;
        _serviceProvider = serviceProvider;
        _factory = new ConnectionFactory { HostName = rabbitMqSettings.Value.HostName, Port = rabbitMqSettings.Value.Port };
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await _factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(
            queue: _rabbitMqSettings.Value.OrderCreatedQueue, 
            durable: false, 
            exclusive: false,
            autoDelete: false,
            arguments: null, 
            cancellationToken: cancellationToken);

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var order = message.FromJson<OrderCreatedMessage>();

                Console.WriteLine("Received: " + message);
                
                // Проверяем, что OrderItems не пустые
                if (order.OrderItems == null || order.OrderItems.Length == 0)
                {
                    Console.WriteLine($"Warning: Order {order.Id} has no OrderItems, skipping audit log");
                    return;
                }
                
                using var scope = _serviceProvider.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<OmsClient>();
                
                var request = new V1AuditLogOrderRequest
                {
                    Orders = order.OrderItems.Select(x => 
                        new V1AuditLogOrderRequest.LogOrder
                        {
                            OrderId = order.Id,
                            OrderItemId = x.Id,
                            CustomerId = order.CustomerId,
                            OrderStatus = nameof(OrderStatus.Created)
                        }).ToArray()
                };
                
                Console.WriteLine($"Sending audit log request for order {order.Id} with {request.Orders.Length} items");
                
                var response = await client.LogOrder(request, CancellationToken.None);
                
                Console.WriteLine($"Successfully logged audit for order {order.Id}: {response.Success}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        };
        
        await _channel.BasicConsumeAsync(
            queue: _rabbitMqSettings.Value.OrderCreatedQueue, 
            autoAck: true, 
            consumer: _consumer,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _connection?.Dispose();
        _channel?.Dispose();
    }
}