using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;

namespace Envio_de_Emails_Com_Fila_Shared.Observability;

public static class RabbitMqTraceContext
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public static void Inject(BasicProperties properties)
    {
        Propagator.Inject(
            new PropagationContext(System.Diagnostics.Activity.Current?.Context ?? default, Baggage.Current),
            properties,
            static (props, key, value) =>
            {
                props.Headers ??= new Dictionary<string, object?>();
                props.Headers[key] = value;
            });
    }

    public static PropagationContext Extract(IReadOnlyBasicProperties properties)
    {
        return Propagator.Extract(
            default,
            properties,
            static (props, key) =>
            {
                if (props.Headers == null || !props.Headers.TryGetValue(key, out var value) || value == null)
                {
                    return [];
                }

                return value switch
                {
                    byte[] bytes => [System.Text.Encoding.UTF8.GetString(bytes)],
                    string text => [text],
                    _ => [value.ToString() ?? string.Empty]
                };
            });
    }
}
