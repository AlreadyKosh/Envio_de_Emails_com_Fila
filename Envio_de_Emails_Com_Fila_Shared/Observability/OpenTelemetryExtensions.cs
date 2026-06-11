using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Envio_de_Emails_Com_Fila_Shared.Observability;

public static class OpenTelemetryExtensions
{
    private const string DefaultOtlpEndpoint = "http://localhost:4317";

    public static IHostApplicationBuilder AddEmailQueueOpenTelemetry(
        this IHostApplicationBuilder builder,
        string serviceName,
        bool includeAspNetCore = false)
    {
        var otlpEndpoint =
            builder.Configuration["OpenTelemetry:OtlpEndpoint"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? DefaultOtlpEndpoint;

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(EmailQueueActivitySource.Name)
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });

                if (includeAspNetCore)
                {
                    tracing.AddAspNetCoreInstrumentation();
                }
            });

        return builder;
    }
}
