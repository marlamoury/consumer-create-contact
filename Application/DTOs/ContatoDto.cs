using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Consumer.Create.Contact.Application.DTOs
{
    public class ContatoDto
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("ddd")]
        public int Ddd { get; set; }

        [JsonPropertyName("regiao")]
        public int Regiao { get; set; }

        [JsonPropertyName("DataHoraRegistro")]
        public DateTime DataHoraRegistro { get; set; }
    }
}
