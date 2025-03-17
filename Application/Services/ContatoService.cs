using Consumer.Create.Contact.Domain.Entities;
using Consumer.Create.Contact.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace Consumer.Create.Contact.Application.Services
{
    public class ContatoService : IContatoService
    {
        private readonly IContatoRepository _contatoRepository;

        public ContatoService(IContatoRepository contatoRepository)
        {
            _contatoRepository = contatoRepository;
        }

        public async Task SalvarContatoAsync(Contato contato)
        {
            await _contatoRepository.AddContatoAsync(contato);
        }
    }
}
