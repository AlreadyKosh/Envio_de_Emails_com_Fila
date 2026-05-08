using Envio_de_Emails_Com_Fila_Worker;
using Envio_de_Emails_Com_Fila_Worker.Models.Email;
using Envio_de_Emails_Com_Fila_Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<EmailService>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient<CepService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var host = builder.Build();
host.Run();
