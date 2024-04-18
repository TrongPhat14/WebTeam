using Microsoft.EntityFrameworkCore;
using WebTeam.Data;

namespace WebTeam.Services
{
    public class Search : ISearch
    {
        private readonly ApplicationDbContext _context;

        public Search(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Article> GetAll()
        {
            var applicationDbContext = _context.Articles.Include(l => l.Author);
            return applicationDbContext;
        }

        public async Task<Article> GetById(int? id)
        {
            var article = await _context.Articles
                .Include(l => l.Author)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            return article;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
