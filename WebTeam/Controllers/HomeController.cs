using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebTeam.Data;
using WebTeam.Models;
using WebTeam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebTeam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ISearch _search;
        private readonly ICommentsService _commentsService;
        private readonly IArticleService _articleService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ISearch search, IArticleService articleService, ICommentsService commentsService)
        {
            _logger = logger;
            _context = context;
            _search = search;
            _commentsService = commentsService;
            _articleService = articleService;

        }

        public IActionResult Index()
        {
            var currentDateTime = DateTime.Now;
            var nearestSemester = _context.Semesters
                .Where(s => s.CreatedDate >= currentDateTime)
                .OrderBy(s => s.CreatedDate)
                .FirstOrDefault();

            ViewData["NearestSemester"] = nearestSemester;

            var articles = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Semester)
                .Include(a => a.Faculty)
                .ToList();

            var faculties = _context.Faculties.ToList(); // Lấy danh sách các khoa từ cơ sở dữ liệu

            ViewData["Faculties"] = faculties; // Truyền danh sách khoa đến view

            return View(articles);
        }
        public async Task<IActionResult> Search(string Search)
        {
            var applicationDbContext = _search.GetAll();


            if (!string.IsNullOrEmpty(Search))
            {
                // Kiểm tra searchString có phải là số không
                if (int.TryParse(Search, out int authorId))
                {
                    // Chuyển đổi authorId thành kiểu string
                    string authorIdString = authorId.ToString();
                    // Tìm kiếm theo author Id trước
                    applicationDbContext = applicationDbContext.Where(a => a.AuthorID == authorIdString);
                }
                else
                {
                    // Nếu không phải là author id, sẽ tìm kiếm theo Title
                    applicationDbContext = applicationDbContext.Where(a => a.Title.Contains(Search));
                }
            }

            var listings = applicationDbContext.ToList();

            return View(listings);
        }

        public IActionResult Category()
        {
            return View();
        }
        public IActionResult Detail()
        {
            return View();
        }
/*        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }

            var article = await _articleService.GetById(id);
            *//*var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(m => m.ArticleId == id);*//*
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }*/

        [HttpPost]
        public async Task<ActionResult> AddComment([Bind("CommentID, CommentContent, UserID, ArticleID")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CommentDate = DateTime.Now; 
                await _commentsService.Add(comment);
            }
            var listing = await _articleService.GetById(comment.ArticleID);
            return View("Detail", listing);
        }
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingLike = await _context.Likes
                .Where(l => l.ArticleID == id && l.UserID == userId)
                .FirstOrDefaultAsync();

            if (existingLike == null)
            {
                _context.Likes.Add(new Like { ArticleID = id, UserID = userId });
                article.NoOfLike++;
            }
            else
            {
                _context.Likes.Remove(existingLike);
                article.NoOfLike--;
            }

            await _context.SaveChangesAsync();

            return Json(new { noOfLike = article.NoOfLike });
        }
        [HttpPost]
        public async Task<IActionResult> Unlike(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingLike = await _context.Likes
                .Where(l => l.ArticleID == id && l.UserID == userId)
                .FirstOrDefaultAsync();

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                article.NoOfLike--;
                await _context.SaveChangesAsync();
            }

            return Json(new { noOfLike = article.NoOfLike });
        }
        /*    [HttpPost]
            public async Task<IActionResult> Like(int id)
            {
                var article = await _context.Articles.FindAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var existingLike = await _context.Likes
                    .Where(l => l.ArticleID == id && l.UserID == userId)
                    .FirstOrDefaultAsync();

                if (existingLike == null)
                {
                    _context.Likes.Add(new Like { ArticleID = id, UserID = userId });
                    article.NoOfLike++;
                }
                else
                {
                    _context.Likes.Remove(existingLike);
                    article.NoOfLike--;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { id });
            }*/
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}