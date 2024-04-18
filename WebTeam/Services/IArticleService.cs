using WebTeam.Data;

namespace WebTeam.Services
{
    public interface IArticleService
    {
        IQueryable<Article> GetAll();

        Task<Article> GetById(int? id);

        Task SaveChanges();

        public IQueryable<Article> Articles { get; set; }
        public string TitleSortOrder { get; set; }
        public string AuthorSortOrder { get; set; }
    }
}
