using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using System.Threading.Channels;
using MemberService.DTOs;

namespace MemberService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"]!)
            };
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown!;

                Console.WriteLine("Connected to MessageBus");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not connect to the Message Bus: {ex.Message}");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "trigger",
                            routingKey: "",
                            basicProperties: null,
                            body: body);

            Console.WriteLine($"RabbitMQ message sent {message}");
        }

        public Task<bool> CheckConference(CheckConferenceDTO checkConferenceDTO)
        {
            if (_connection.IsOpen)
            {
                Console.WriteLine("RabbitMQ Connection Open, sending message...");
                SendMessage(JsonSerializer.Serialize(checkConferenceDTO));
                return Task.FromResult(true);
            }
            else
            {
                Console.WriteLine("RabbitMQ connectionis closed, not sending");
                return Task.FromResult(false);
            }
        }

        public void Dispose()
        {
            Console.WriteLine("MessageBus Disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("RabbitMQ Connection Shutdown");
        }
    }
}
