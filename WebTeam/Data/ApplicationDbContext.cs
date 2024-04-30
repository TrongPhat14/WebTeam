using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace WebTeam.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FeedBack> FeedBacks { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        /*public IQueryable<FeedBack> SearchUsers(string searchString)
        {
            var query = FeedBacks.Where(c => c.Article.Contains(searchString));
            return query;
        }*/

        public IQueryable<ApplicationUser> SearchUsers(string searchString)
        {
            var query = ApplicationUsers.Where(c => c.Name.Contains(searchString));
            return query;
        }
        public IQueryable<Category> SearchCategories(string searchString)
        {
            // Lọc Category có tên chứa chuỗi tìm kiếm
            var query = Categories.Where(c => c.CategoryName.Contains(searchString));
            return query;
        }
        public IQueryable<FeedBack> SearchFeedBacks(string searchString)
        {
            // Lọc author có tên chứa chuỗi tìm kiếm
            var query = FeedBacks.Include(a => a.Article)
                                              .ThenInclude(a => a.Author)
                                 .Where(c => c.Article.Author.Name.Contains(searchString));
            return query;
        }

        public IQueryable<Faculty> SearchFaculties(string searchString)
        {
            // Lọc Faculty có tên chứa chuỗi tìm kiếm
            var query = Faculties.Where(f => f.FacultyName.Contains(searchString));
            return query;
        }

        public IQueryable<Semester> SearchSemesters(string searchString)
        {
            // Lọc Faculty có tên chứa chuỗi tìm kiếm
            var query = Semesters.Where(f => f.SemesterName.Contains(searchString));
            return query;
        }

        public IQueryable<AcademicYear> SearchAcademicYear(string searchString)
        {
            // Lọc Category có tên chứa chuỗi tìm kiếm
            var query = AcademicYears.Where(c => c.AcademicYears.Contains(searchString));
            return query;
        }

    }
}