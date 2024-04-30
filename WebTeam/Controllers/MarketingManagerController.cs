using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebTeam.Constants;
using WebTeam.Data;

namespace WebTeam.Controllers
{
    [Authorize(Roles = "Marketing_manager")]

    public class MarketingManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MarketingManagerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string semesterName)
        {
            // Lấy danh sách tất cả các kì từ cơ sở dữ liệu
            var allSemesters = await _context.Semesters.Include(s => s.Faculty).ToListAsync();
            ViewData["semestersWithUserFaculty"] = allSemesters;

            // Lấy danh sách các bài báo từ cơ sở dữ liệu
            var articles = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Faculty)
                .Include(a => a.Author)
                .Include(a => a.Semester)
                .Where(a => allSemesters.Select(s => s.SemesterName).Contains(a.Semester.SemesterName))
                .ToListAsync();

            var faculties = await _context.Faculties.ToListAsync(); // Lấy danh sách các khoa từ cơ sở dữ liệu
            ViewData["Faculties"] = faculties;

            // Lọc theo semesterName nếu được chỉ định
            if (!string.IsNullOrEmpty(semesterName))
            {
                articles = articles.Where(a => a.Semester.SemesterName == semesterName).ToList();
            }

            // Truyền danh sách các bài báo vào view
            ViewData["semesterName"] = semesterName;

            return View(articles);
        }



        [HttpPost]
        public async Task<IActionResult> GetTotalStudent()
        {
            try
            {
                // Lấy danh sách tất cả người dùng từ UserManager
                var allUsers = await _userManager.Users.ToListAsync();

                // Lọc danh sách người dùng để chỉ lấy những người dùng có vai trò là "Student"
                var studentUsers = allUsers.Where(u => _userManager.IsInRoleAsync(u, "Student").Result).ToList();

                // Đếm số lượng người dùng thuộc vai trò "Student"
                var totalStudents = studentUsers.Count;

                return Json(totalStudents);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult GetTotalData()
        {
            try
            {
                // Tính tổng số lượng ArticleId
                int totalArticles = _context.Articles.Count();
                return Json(totalArticles);

            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult GetUserData()
        {
            try
            {
                List<object> data = new List<object>();

                var groupedArticles = _context.Articles
                    .Where(a => a.ArticleDate != null) // Kiểm tra xem ArticleDate có null không
                    .GroupBy(p => new { p.ArticleDate.Value.Year, p.ArticleDate.Value.Month }) // Sử dụng .Value để truy cập giá trị thực sự của ArticleDate
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalAuthor = g.Select(a => a.ArticleId).Distinct().Count()
                    })
                    .ToList();

                List<string> labels = groupedArticles.Select(p => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.Month)} {p.Year}").ToList();
                List<int> AuthorNumber = groupedArticles.Select(p => p.TotalAuthor).ToList();

                data.Add(labels);
                data.Add(AuthorNumber);

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }



        [HttpPost]
        public IActionResult GetFacultyData()
        {
            try
            {
                List<object> data = new List<object>();

                var faculties = _context.Faculties.Select(f => new
                {
                    FacultyId = f.FacultyId,
                    FacultyName = f.FacultyName,
                    TotalArticles = f.Categories.SelectMany(c => c.Articles).Where(a => a.Isbool == true)
                    .Count()
                }).ToList();

                int totalArticles = faculties.Sum(f => f.TotalArticles);

                List<string> labels = new List<string>();
                List<double> percentages = new List<double>();

                foreach (var faculty in faculties)
                {
                    labels.Add(faculty.FacultyName);
                    double percentage = (double)faculty.TotalArticles / totalArticles * 100;
                    percentages.Add(percentage);
                }

                data.Add(labels);
                data.Add(percentages);

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult GetStatisticalData()
        {
            try
            {
                List<object> data = new List<object>();

                var authors = _context.Users.Select(u => new
                {
                    AuthorID = u.Id,
                    AuthorName = u.UserName,
                    TotalArticles = u.Articles.Count()
                }).ToList();

                List<string> labels = new List<string>();
                List<int> articleNumbers = new List<int>();

                foreach (var author in authors)
                {
                    labels.Add(author.AuthorName);
                    articleNumbers.Add(author.TotalArticles);
                }

                data.Add(labels);
                data.Add(articleNumbers);

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}
