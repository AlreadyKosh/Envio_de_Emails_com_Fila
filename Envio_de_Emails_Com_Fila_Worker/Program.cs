using Envio_de_Emails_Com_Fila_Worker;
using Envio_de_Emails_Com_Fila_Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<EmailService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
