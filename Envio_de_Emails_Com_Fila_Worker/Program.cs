using Envio_de_Emails_Com_Fila_Shared.Observability;
using Envio_de_Emails_Com_Fila_Worker;
using Envio_de_Emails_Com_Fila_Worker.Models.Email;
using Envio_de_Emails_Com_Fila_Worker.Services;
using Envio_de_Emails_Com_Fila_Worker.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.AddEmailQueueOpenTelemetry("envio-emails-worker");

builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<IEmailContentEnricher, EmailContentEnricher>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient<ICepService, CepService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var host = builder.Build();
host.Run();
