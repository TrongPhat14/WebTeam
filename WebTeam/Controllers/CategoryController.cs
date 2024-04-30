using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTeam.Data;
using WebTeam.Services;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using WebTeam.Data.Migrations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using MailKit.Search;
using System.Linq;

namespace WebTeam.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IArticleService _articleService;

        public CategoryController(ApplicationDbContext context, IArticleService articleService)
        {
            _context = context;
            _articleService = articleService;
        }
        public async Task<IActionResult> Index(int? page, string orderBy)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var articles = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Faculty)
                .Where(a => a.Isbool == true)
                .ToListAsync();
            switch (orderBy)
            {
                case "art_desc":
                    articles = articles.OrderByDescending(a => a.Title).ToList();
                    break;
                case "art_asc":
                    articles = articles.OrderBy(a => a.Title).ToList();
                    break;
                case "date_desc":
                    articles = articles.OrderByDescending(a => a.ArticleDate).ToList();
                    break;
                default:
                    articles = articles.OrderBy(a => a.ArticleDate).ToList();
                    break;
            }


            var pagedArticles = articles.ToPagedList(pageNumber, pageSize);
            var faculties = _context.Faculties.Include(f => f.Categories).
                ToList();

            ViewData["Faculties"] = faculties;
            return View(pagedArticles);
        }
        public IActionResult FilterByCategory(string categoryNames, int? page, string orderBy)
        {
            ViewBag.SearchString = categoryNames;

            if (string.IsNullOrEmpty(categoryNames))
            {
                // Nếu không có category nào được chọn, trả về tất cả các bài báo
                return RedirectToAction("Index");
            }

            string[] categoryNameArray = categoryNames.Split(',');

            // Lấy danh sách tất cả các bài báo từ cơ sở dữ liệu
            var allArticles = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Faculty)
                .ToList();

            // Lọc danh sách bài báo theo các danh mục được chọn
            var filteredArticles = allArticles
                .Where(a => a.Category != null && categoryNameArray.Contains(a.Category.CategoryName))
                .Where(a => a.Isbool == true)
                .ToList();
            switch (orderBy)
            {
                case "art_desc":
                    allArticles = allArticles.OrderByDescending(a => a.Title).ToList();
                    break;
                case "art_asc":
                    allArticles = allArticles.OrderBy(a => a.Title).ToList();
                    break;
                case "date_desc":
                    allArticles = allArticles.OrderByDescending(a => a.ArticleDate).ToList();
                    break;
                default:
                    allArticles = allArticles.OrderBy(a => a.ArticleDate).ToList();
                    break;
            }
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var faculties = _context.Faculties.Include(a => a.Categories)
                .ToList();
            var pagedArticles = filteredArticles.ToPagedList(pageNumber, pageSize);
            // Truyền danh sách khoa và danh sách bài báo đã lọc sang view, cùng với danh sách các danh mục được chọn
            ViewData["Faculties"] = faculties;
            ViewData["SelectedCategories"] = categoryNames;

            // Trả về view "Index" với danh sách bài báo đã lọc
            return View("Index", pagedArticles);
        }

    }
}

