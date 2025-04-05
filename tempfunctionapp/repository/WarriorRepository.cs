using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using tempfunctionapp.model;
namespace tempfunctionapp.repository
{
    

    public class WarriorRepository : IWarriorRepository
    {
        private readonly string _connectionString;

        public WarriorRepository(IConfiguration configuration)
        {
            _connectionString = configuration["SqlConnectionString"];
        }

        public async Task<List<Warrior>> GetAllWarriorsAsync()
        {
            var warriors = new List<Warrior>();
            using var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand("SELECT * FROM warriors", conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                warriors.Add(new Warrior
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Clan = reader.GetString(2),
                    Strength = reader.GetInt32(3),
                    Rank = reader.GetString(4),
                    LastUpdated = reader.GetDateTime(5)
                });
            }

            return warriors;
        }
    }

}
