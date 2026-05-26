namespace Envio_de_Emails_Com_Fila_Shared.Messaging
{
    public static class RabbitMqTopology
    {
        public const string EmailExchangeName = "email-exchange";
        public const string EmailReceivedRoutingKey = "email.received";
        public const string EmailQueueName = "email-queue";
        public const string EmailPersistenceQueueName = "email-persistence-queue";
    }
}
