using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Consumer.Create.Contact.Application.DTOs;
using Consumer.Create.Contact.Application.Services;
using Consumer.Create.Contact.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer.Create.Contact.Infrastructure.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;
        private const string QUEUE_NAME = "fila_criar_contato";

        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { HostName = "localhost" };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: QUEUE_NAME, durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("Conectado ao RabbitMQ e aguardando mensagens na fila '{0}'...", QUEUE_NAME);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao conectar ao RabbitMQ: {0}", ex.Message);
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Mensagem recebida: {0}", messageJson);

                    // Extrai a propriedade "message" do JSON antes de desserializar
                    var jsonObject = JsonNode.Parse(messageJson);
                    var messageNode = jsonObject?["message"];

                    if (messageNode != null)
                    {
                        var contatoJson = messageNode.ToString();
                        var contato = JsonSerializer.Deserialize<ContatoDto>(contatoJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true // Ignora diferença entre maiúsculas e minúsculas
                        });

                        if (contato != null)
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var contatoService = scope.ServiceProvider.GetRequiredService<IContatoService>();

                            var contatoEntity = contato.ToEntity();

                            await contatoService.SalvarContatoAsync(contatoEntity);

                            _channel.BasicAck(ea.DeliveryTag, false);
                            _logger.LogInformation("Contato {0} salvo no banco!", contato.Nome);
                        }
                        else
                        {
                            _logger.LogWarning("Falha ao desserializar o contato.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("JSON recebido não contém a propriedade 'message'.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao processar mensagem: {0}", ex.Message);
                }
            };

            _channel.BasicConsume(queue: QUEUE_NAME, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _logger.LogInformation("Finalizando RabbitMQConsumer...");
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
