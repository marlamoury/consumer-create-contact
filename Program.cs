using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Consumer.Create.Contact.Infrastructure.Messaging;
using Consumer.Create.Contact.Infrastructure.Persistence;
using Consumer.Create.Contact.Application.Services;
using Consumer.Create.Contact.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<IContatoRepository, ContatoRepository>();
        services.AddScoped<IContatoService, ContatoService>();
        services.AddSingleton<RabbitMQConsumer>();  // Registramos como Singleton
        services.AddHostedService<Worker>();  // Mantemos apenas o Worker
    })
    .Build();

await host.RunAsync();
