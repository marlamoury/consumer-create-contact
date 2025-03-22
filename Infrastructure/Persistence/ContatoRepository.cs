using Consumer.Create.Contact.Domain.Entities;
using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Consumer.Create.Contact.Infrastructure.Persistence
{
    public class ContatoRepository : IContatoRepository
    {
        private readonly string _connectionString;

        public ContatoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task AddContatoAsync(Contato contato)
        {

            // Garantir que a DataHoraRegistro seja atualizada antes de salvar
            contato.DataHoraRegistro = DateTime.UtcNow;

            const string query = @"
            INSERT INTO contatos (nome, telefone, email, ddd, regiao, DataHoraRegistro) 
            VALUES (@Nome, @Telefone, @Email, @Ddd, @Regiao, @DataHoraRegistro);";

            Console.WriteLine($"Nome: {contato.Nome}, Telefone: {contato.Telefone}, Email: {contato.Email}, DDD: {contato.Ddd}, Região: {contato.Regiao}, CriadoEm: {contato.DataHoraRegistro}");

            using var connection = new MySqlConnection(_connectionString);
            await connection.ExecuteAsync(query, contato);
        }

    }
}
