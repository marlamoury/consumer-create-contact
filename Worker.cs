namespace Consumer.Create.Contact
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public class Worker : BackgroundService
    {
        private IConnection _connection;
        private RabbitMQ.Client.IModel _channel;
        private const string QUEUE_NAME = "fila_criar_contato";

        public Worker()
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };


            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QUEUE_NAME, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var contato = JsonSerializer.Deserialize<ContatoDto>(message);

                Console.WriteLine($" [x] Processando contato: {contato?.Nome} - {contato?.Email}");

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: QUEUE_NAME, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }

    public class ContatoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
