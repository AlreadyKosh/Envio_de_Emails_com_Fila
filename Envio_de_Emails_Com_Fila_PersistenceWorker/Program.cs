using Envio_de_Emails_Com_Fila_PersistenceWorker;
using Envio_de_Emails_Com_Fila_PersistenceWorker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<EmailPersistenceService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
