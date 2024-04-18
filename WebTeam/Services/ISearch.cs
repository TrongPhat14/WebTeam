using WebTeam.Data;

namespace WebTeam.Services
{
    public interface ISearch
    {
        IQueryable<Article> GetAll();

        Task<Article> GetById(int? id);

        Task SaveChanges();
    }
}
