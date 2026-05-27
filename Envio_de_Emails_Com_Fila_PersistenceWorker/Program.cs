using Envio_de_Emails_Com_Fila_PersistenceWorker;
using Envio_de_Emails_Com_Fila_PersistenceWorker.Services;
using Envio_de_Emails_Com_Fila_Shared.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.AddEmailQueueOpenTelemetry("envio-emails-persistence-worker");

builder.Services.AddSingleton<EmailPersistenceService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
