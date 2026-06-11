namespace Envio_de_Emails_Com_Fila_PersistenceWorker.Models.ViaCep
{
    public class ViaCepResponse
    {
        public string Logradouro { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public bool Erro { get; set; }
    }
}
