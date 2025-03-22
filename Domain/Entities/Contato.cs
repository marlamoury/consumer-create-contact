using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.Create.Contact.Domain.Entities
{
    public class Contato
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public int Ddd { get; set; }
        public int Regiao { get; set; }
        public DateTime DataHoraRegistro { get; set; } = DateTime.UtcNow;
    }
}
