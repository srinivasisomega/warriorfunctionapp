using tempfunctionapp.model;

namespace tempfunctionapp.repository
{
    /// <summary>
    /// Repository to retrieve warrior data from a JSON file stored in Azure Blob Storage.
    /// </summary>
    public interface IWarriorRepository
    {
        Task<List<Warrior>> GetAllWarriorsAsync();
    }
}
