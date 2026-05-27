using Envio_de_Emails_Com_Fila_API.Services;
using Envio_de_Emails_Com_Fila_Shared.Observability;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddEmailQueueOpenTelemetry("envio-emails-api", includeAspNetCore: true);

builder.Services.AddSingleton<RabbitMQService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Envio de Emails")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
