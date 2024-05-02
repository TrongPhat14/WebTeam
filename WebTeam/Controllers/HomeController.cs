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
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Packaging;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebTeam.Data.Migrations;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace WebTeam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ISearch _search;
        private readonly ICommentsService _commentsService;
        private readonly IArticleService _articleService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signin;



        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, ISearch search, IArticleService articleService, ICommentsService commentsService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signin)
        {
            _logger = logger;
            _context = context;
            _search = search;
            _commentsService = commentsService;
            _articleService = articleService;
            _userManager = userManager;
            _signin = signin;
        }

        public async Task<IActionResult> Index()
        {
            if (_signin.IsSignedIn(User))
            {
                // Lấy thông tin về người dùng và vai trò của họ
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                // Lấy thời gian hiện tại
                var currentDateTime = DateTime.Now;

                // Lấy thông tin về khoa của người dùng
                var userFacultyId = user.FacultyId;

                // Lấy danh sách tất cả các kì từ cơ sở dữ liệu
                var allSemesters = _context.Semesters.Include(s => s.Faculty).ToList();

                // Lọc ra các kì có cùng khoa với người dùng
                var semestersWithUserFaculty = allSemesters.Where(s => s.FacultyID == userFacultyId).ToList();

                // Chọn kì gần nhất từ các kì có cùng khoa với người dùng
                var nearestSemester = semestersWithUserFaculty.OrderBy(s => s.CreatedDate)
                                                               .FirstOrDefault(s => s.CreatedDate <= currentDateTime && currentDateTime <= s.ClosureDate);

                // Nếu không có kì nào có cùng khoa với người dùng, chọn kì gần nhất từ tất cả các kì
                if (nearestSemester == null)
                {
                    nearestSemester = allSemesters.OrderByDescending(s => s.CreatedDate)
                                                   .FirstOrDefault(s => s.CreatedDate > currentDateTime);
                }

                // Nếu vẫn không có kì nào, chọn kì gần nhất từ tất cả các kì
                if (nearestSemester == null)
                {
                    nearestSemester = allSemesters.OrderByDescending(s => s.ClosureDate)
                                                   .FirstOrDefault();
                }

                // Truyền thông tin về kì gần nhất đến view
                ViewData["NearestSemester"] = nearestSemester;

                var articles = _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .Include(a => a.Semester)
                    .Include(a => a.Faculty)
                    .OrderByDescending(a => a.ArticleDate) // Sắp xếp bài viết theo thứ tự giảm dần của CreatedAt
                    .Where(a => a.Isbool == true)
                    .Take(6)
                    .ToList();
                var oneMonthAgo = currentDateTime.AddMonths(-1);
                var randomArticles = _context.Articles
                        .Include(a => a.Author)
                    .Where(a => a.Isbool == true)

                    .OrderBy(a => Guid.NewGuid())
                    .Take(3)
                    .ToList();

                // Thêm danh sách ba bài báo ngẫu nhiên vào ViewData để truyền đến view
                ViewData["SpotlightArticles"] = randomArticles;
                // Lấy thời điểm đầu tháng hiện tại
                var startOfCurrentMonth = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);

                // Kiểm tra xem có bài nào trong tháng hiện tại hay không
                var articlesInCurrentMonth = _context.Articles
                    .Where(a => a.ArticleDate >= startOfCurrentMonth && a.ArticleDate <= currentDateTime)
                    .Where(a => a.Isbool == true)

                    .Any();

                List<Article> topRankedArticles;

                if (articlesInCurrentMonth)
                {
                    // Nếu có bài trong tháng hiện tại, lấy top bài trong tháng này
                    topRankedArticles = _context.Articles
                        .Include(a => a.Author)
                        .Where(a => a.ArticleDate >= startOfCurrentMonth && a.ArticleDate <= currentDateTime)
                    .Where(a => a.Isbool == true)

                        .OrderByDescending(a => a.NoOfLike)
                        .Take(6)
                        .ToList();
                }
                else
                {
                    // Nếu không có bài trong tháng hiện tại, lấy top bài trong tháng gần nhất trước đó
                    var lastMonth = currentDateTime.AddMonths(-1);
                    var startOfLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);

                    topRankedArticles = _context.Articles
                        .Include(a => a.Author)
                        .Where(a => a.ArticleDate >= startOfLastMonth && a.ArticleDate <= lastMonth)
                    .Where(a => a.Isbool == true)

                        .OrderByDescending(a => a.NoOfLike)
                        .Take(6)
                        .ToList();
                }
                // Thêm danh sách ba bài báo được yêu thích nhất vào ViewData để truyền đến view
                ViewData["TopRankedArticles"] = topRankedArticles;
                var faculties = _context.Faculties.ToList(); // Lấy danh sách các khoa từ cơ sở dữ liệu

                ViewData["Faculties"] = faculties; // Truyền danh sách khoa đến view

                return View(articles);

            }
            else
            {
                var currentDateTime = DateTime.Now;

                var articles = _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .Include(a => a.Semester)
                    .Include(a => a.Faculty)
                    .OrderByDescending(a => a.ArticleDate) // Sắp xếp bài viết theo thứ tự giảm dần của CreatedAt
                    .Where(a => a.Isbool == true)

                    .Take(6)
                    .ToList();
                var oneMonthAgo = currentDateTime.AddMonths(-1);
                var randomArticles = _context.Articles
                        .Include(a => a.Author)
                    .Where(a => a.Isbool == true)

                    .OrderBy(a => Guid.NewGuid())
                    .Take(3)
                    .ToList();

                // Thêm danh sách ba bài báo ngẫu nhiên vào ViewData để truyền đến view
                ViewData["SpotlightArticles"] = randomArticles;
                var startOfCurrentMonth = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);

                // Kiểm tra xem có bài nào trong tháng hiện tại hay không
                var articlesInCurrentMonth = _context.Articles
                    .Where(a => a.ArticleDate >= startOfCurrentMonth && a.ArticleDate <= currentDateTime)
                    .Where(a => a.Isbool == true)

                    .Any();

                List<Article> topRankedArticles;

                if (articlesInCurrentMonth)
                {
                    // Nếu có bài trong tháng hiện tại, lấy top bài trong tháng này
                    topRankedArticles = _context.Articles
                        .Include(a => a.Author)
                        .Where(a => a.ArticleDate >= startOfCurrentMonth && a.ArticleDate <= currentDateTime)
                        .OrderByDescending(a => a.NoOfLike)
                    .Where(a => a.Isbool == true)

                        .Take(6)
                        .ToList();
                }
                else
                {
                    // Nếu không có bài trong tháng hiện tại, lấy top bài trong tháng gần nhất trước đó
                    var lastMonth = currentDateTime.AddMonths(-1);
                    var startOfLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);

                    topRankedArticles = _context.Articles
                        .Include(a => a.Author)
                        .Where(a => a.ArticleDate >= startOfLastMonth && a.ArticleDate <= lastMonth)
                    .Where(a => a.Isbool == true)

                        .OrderByDescending(a => a.NoOfLike)
                        .Take(6)
                        .ToList();
                }
                // Thêm danh sách ba bài báo được yêu thích nhất vào ViewData để truyền đến view
                ViewData["TopRankedArticles"] = topRankedArticles;
                var faculties = _context.Faculties.ToList(); // Lấy danh sách các khoa từ cơ sở dữ liệu

                ViewData["Faculties"] = faculties; // Truyền danh sách khoa đến view

                return View(articles);
            }
        }
        public async Task<IActionResult> DownloadZip(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the submission by ID
            var article = await _context.Articles.FirstOrDefaultAsync(m => m.ArticleId == id);
            if (article == null)
            {
                return NotFound();
            }

            // Get the file path of the submission's DOCX file
            var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", article.Content);

            // Check if the file exists and if it's a DOCX file
            if (System.IO.File.Exists(filePath) && System.IO.Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                // Create a unique temporary directory to store the DOCX file
                var tempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDirectory);

                try
                {
                    // Copy the DOCX file to the temporary directory
                    var uniqueFileName = $"{article.ArticleId}_{article.Content}";
                    var destinationFilePath = System.IO.Path.Combine(tempDirectory, uniqueFileName);
                    System.IO.File.Copy(filePath, destinationFilePath, true); // Set overwrite to true to replace existing files

                    // Generate the ZIP file name based on the submission's DOCX file name
                    var zipFileName = $"{System.IO.Path.GetFileNameWithoutExtension(article.Content)}_{DateTime.Now:yyyyMMddHHmmss}.zip";
                    var zipFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), zipFileName);

                    // Create a ZIP archive from the temporary directory containing only the current submission's DOCX file
                    ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);

                    // Read the ZIP file as a byte array
                    var zipFileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);

                    // Return the ZIP file as a file download
                    return File(zipFileBytes, "application/zip", zipFileName);
                }
                finally
                {
                    // Delete the temporary directory and its contents
                    Directory.Delete(tempDirectory, true);
                }
            }
            else
            {
                return BadRequest("The submission's file does not exist or is not a DOCX file.");
            }
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

            var listings = applicationDbContext.Where(a => a.Isbool == true).ToList();


            return View(listings);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AddComment([Bind("CommentID, CommentContent, UserID, ArticleID")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CommentDate = DateTime.Now; 
                await _commentsService.Add(comment);
            }
            var article = await _articleService.GetById(comment.ArticleID);
            ViewBag.Article = article;

            // Lấy lại dữ liệu của bài viết và truyền vào View
            return RedirectToAction("Detail", new { id = article.ArticleId });
/*            return View("Detail", listing);*/
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

/*            var feedBack = await _context.FeedBacks
                  .Include(f => f.Article)
                         .ThenInclude(f => f.Author)
                  .FirstOrDefaultAsync(f => f.FeedBackID == id);*/
            var article = await _articleService.GetById(id);

            if (article == null)
            {
                return NotFound();
            }

            var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", article.Content);


            // Kiểm tra định dạng của tệp
            var fileExtension = System.IO.Path.GetExtension(filePath);
            if (fileExtension != ".docx")
            {
                return BadRequest("Only .docx files can be parsed.");
            }

            // Phân tích tệp DOCX và trả về nội dung HTML
            var htmlContent = await ReadDocxAsHtml(filePath);
            /*            return Content(htmlContent, "text/html");*/
            ViewBag.DocxContent = htmlContent;
            // Trả về view chứa nội dung của tệp DOCX
            return View(article);
        }

        public async Task<string> ReadDocxAsHtml(string filePath)
        {
            StringBuilder htmlContent = new StringBuilder();

            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                var body = doc.MainDocumentPart.Document.Body;
                var paragraphs = body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();

                htmlContent.Append("<html><body>");

                foreach (var paragraph in paragraphs)
                {
                    htmlContent.Append("<p>");
                    var runs = paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>();

                    foreach (var run in runs)
                    {
                        foreach (var text in run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>())
                        {
                            htmlContent.Append(HttpUtility.HtmlEncode(text.Text));
                        }

                        // Handle images
                        var drawing = run.Descendants<DocumentFormat.OpenXml.Wordprocessing.Drawing>().FirstOrDefault();

                        if (drawing != null)
                        {
                            var imageData = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
                            if (imageData != null)
                            {
                                var imageId = imageData.Embed.Value;
                                var imagePart = doc.MainDocumentPart.GetPartById(imageId) as ImagePart;
                                if (imagePart != null)
                                {
                                    using (var stream = imagePart.GetStream())
                                    {
                                        byte[] byteArray = new byte[stream.Length];
                                        stream.Read(byteArray, 0, (int)stream.Length);

                                        // Convert image to base64 string
                                        var base64String = Convert.ToBase64String(byteArray);

                                        // Embed image into HTML with max width set to 500px and centered
                                        htmlContent.Append("<div style=\"text-align: center;\">");
                                        htmlContent.Append($"<img src=\"data:image/png;base64,{base64String}\" style=\"max-width: 500px;\" />");
                                        htmlContent.Append("</div>");
                                    }
                                }
                            }
                        }
                    }
                    htmlContent.Append("</p>");
                }

                htmlContent.Append("</body></html>");
            }

            return htmlContent.ToString();
        }
        [Authorize]
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
            return Json(new { likesCount = article.NoOfLike });

        }
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