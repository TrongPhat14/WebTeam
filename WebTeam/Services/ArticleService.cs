using Microsoft.EntityFrameworkCore;
using WebTeam.Data;

namespace WebTeam.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Article> Articles { get; set; }
        public string TitleSortOrder { get; set; }
        public string AuthorSortOrder { get; set; }

        public IQueryable<Article> GetAll()
        {
            var applicationDbContext = _context.Articles.Include(l => l.Author);
            return applicationDbContext;
        }

        public async Task<Article> GetById(int? id)
        {
            var article = await _context.Articles.Include(l => l.Author)
                .Include(l => l.Comments)
                .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            
            return article;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
