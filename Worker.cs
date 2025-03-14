using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer.Create.Contact
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConnection _connection;
        private IModel _channel;
        private const string QUEUE_NAME = "fila_criar_contato";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

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
                            await ProcessarContatoAsync(contato);
                            _channel.BasicAck(ea.DeliveryTag, false);
                            _logger.LogInformation("Mensagem processada e ACK enviado.");
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

        private async Task ProcessarContatoAsync(ContatoDto contato)
        {
            _logger.LogInformation("Processando contato: Nome={0}, Telefone={1}, Email={2}, DDD={3}, Regiao={4}, CriadoEm={5}",
                contato.Nome, contato.Telefone, contato.Email, contato.Ddd, contato.Regiao, contato.CreatedAt);

            await Task.Delay(500);

            _logger.LogInformation("Contato {0} processado com sucesso!", contato.Nome);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Finalizando Worker...");
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }

    public class ContatoDto
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("ddd")]
        public int Ddd { get; set; }

        [JsonPropertyName("regiao")]
        public int Regiao { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
