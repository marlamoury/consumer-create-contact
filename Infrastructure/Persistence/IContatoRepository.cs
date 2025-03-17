using Consumer.Create.Contact.Domain.Entities;
using System.Threading.Tasks;

namespace Consumer.Create.Contact.Infrastructure.Persistence
{
    public interface IContatoRepository
    {
        Task AddContatoAsync(Contato contato);
    }
}
