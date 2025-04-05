using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using tempfunctionapp.model;
namespace tempfunctionapp.repository
{
    //code for warrior data retrieval if database is available

    //public class WarriorRepository : IWarriorRepository
    //{
    //    private readonly string _connectionString;

    //    public WarriorRepository(IConfiguration configuration)
    //    {
    //        _connectionString = configuration["SqlConnectionString"];
    //    }

    //    public async Task<List<Warrior>> GetAllWarriorsAsync()
    //    {
    //        var warriors = new List<Warrior>();
    //        using var conn = new SqlConnection(_connectionString);
    //        var cmd = new SqlCommand("SELECT * FROM warriors", conn);

    //        await conn.OpenAsync();
    //        using var reader = await cmd.ExecuteReaderAsync();

    //        while (await reader.ReadAsync())
    //        {
    //            warriors.Add(new Warrior
    //            {
    //                Id = reader.GetInt32(0),
    //                Name = reader.GetString(1),
    //                Clan = reader.GetString(2),
    //                Strength = reader.GetInt32(3),
    //                Rank = reader.GetString(4),
    //                LastUpdated = reader.GetDateTime(5)
    //            });
    //        }

    //        return warriors;
    //    }
    //}

    //temporary code to retrieve data from json since database is not available
    /// <summary>
    /// Repository to retrieve warrior data from a JSON file stored in Azure Blob Storage.
    /// </summary>
    public class WarriorRepository : IWarriorRepository
    {
        private readonly BlobClient _blobClient;

        public WarriorRepository(IConfiguration configuration)
        {
            var blobUrl = configuration["WarriorJsonBlobUrl"]; // This should include SAS token if needed
            _blobClient = new BlobClient(new Uri(blobUrl));
        }

        public async Task<List<Warrior>> GetAllWarriorsAsync()
        {
            if (!await _blobClient.ExistsAsync())
            {
                return new List<Warrior>();
            }

            using var stream = new MemoryStream();
            await _blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            var warriors = await JsonSerializer.DeserializeAsync<List<Warrior>>(stream);
            return warriors ?? new List<Warrior>();
        }
    }




}
