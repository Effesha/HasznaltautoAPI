namespace HasznaltAuto.API.Repositories.Interfaces
{
    public interface IListOnlyRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> ListAllAsync();
    }
}
