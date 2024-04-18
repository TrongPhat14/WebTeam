using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Packaging;
using System.IO.Compression;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTeam.Data;
using System.Text;
using System.Web;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using DocumentFormat.OpenXml.Vml;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace WebTeam.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender, ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {

            var applicationDbContext = _context.Articles.Include(a => a.Author).
                ThenInclude(f => f.Faculty).
                Include(a => a.Category).Include(a => a.Faculty).Include(a => a.Semester).Include(a => a.FeedBacks); ;
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> Semester()
        {
            {
                var applicationDbContext = _context.Semesters.Include(s => s.AcademicYear).Include(s => s.Faculty);
                return View( await applicationDbContext.ToListAsync());
            }
        }
        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Faculty)
                .Include(a => a.Semester)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        public async Task<IActionResult> ShowDocx(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FirstOrDefaultAsync(m => m.ArticleId == id);

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
            return Content(htmlContent, "text/html");
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

                                        // Embed image into HTML
                                        htmlContent.Append($"<img src=\"data:image/png;base64,{base64String}\" />");
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
        // GET: Articles/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                // Xử lý trường hợp không tìm thấy người dùng
                // Có thể chuyển hướng đến một trang lỗi hoặc thực hiện xử lý khác tùy theo yêu cầu của bạn
            }

            // Lấy thông tin khoa của người dùng
            var userFacultyId = user.FacultyId;

            // Lấy danh sách các thể loại có cùng khoa với người dùng
            var accessibleCategories = await _context.Categories
                                                .Where(c => c.FacultyId == userFacultyId)
                                                .ToListAsync();


            var student = await _userManager.GetUsersInRoleAsync("Student");
            ViewData["AuthorID"] = new SelectList(student, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(accessibleCategories, "CategoryId", "CategoryName");
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId");
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "SemesterID", "SemesterID");
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleId,Title,Content,MagazineCover,AuthorID,CategoryId,FacultyId,SemesterId,submissionDate,Comment,ArticleDate,NoOfLike,Isbool")] Article article, IFormFile file, IFormFile coverFile)
        {
            if (ModelState.IsValid)
            {
                // Set submission date
                article.submissionDate = DateTime.Now;

                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    // Assign author ID
                    article.AuthorID = userId;
                    // Assign author's faculty ID
                    article.FacultyId = user.FacultyId;
                    article.Isbool = false;
                    article.submissionDate = DateTime.Now;
                    article.ArticleDate = null;
                    article.NoOfLike = 0;
                    /*                  // Lấy thông tin của khoa của người dùng
                                      var userFacultyId = user.FacultyId;

                                      // Truy vấn UserManager để lấy danh sách điều phối viên trong vai trò "Marketing_coordinator"
                                      var marketingCoordinators = await _userManager.GetUsersInRoleAsync("Marketing_coordinator");

                                      // Lọc danh sách điều phối viên cùng khoa với người dùng đang đăng nhập
                                      var coordinator = marketingCoordinators.FirstOrDefault(mc => mc.FacultyId == userFacultyId);


                                      if (coordinator != null)
                                      {
                                          // Gán địa chỉ email của điều phối viên vào biến recipientEmail
                                          var recipientEmail = coordinator.Email;

                                          // Tiếp tục với phần gửi email
                                          var subject = "New article submission";
                                          var emailMessage = $"A new article titled '{article.Title}' has been submitted by {user.Name}. Please review it.";
                                          await SendEmailAsync(recipientEmail, subject, emailMessage);
                                      }
                                      else
                                      {
                                          // Xử lý trường hợp không tìm thấy điều phối viên
                                          ModelState.AddModelError(string.Empty, "Coordinator not found for the user's faculty.");
                                          return View(article);
                                      }*/
                }
                else
                {
                    // Xử lý trường hợp không tìm thấy người dùng
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(article);
                }

                // Check if a file is uploaded for content
                if (file != null && file.Length > 0)
                {
                    var fileExtension = System.IO.Path.GetExtension(file.FileName);
                    if (fileExtension != ".docx" && fileExtension != ".pdf")
                    {
                        ModelState.AddModelError("File", "Only .docx and .pdf files are allowed for content.");
                        return View(article);
                    }

                    // Handle file upload for content
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    article.Content = fileName;
                }
                else
                {
                    ModelState.AddModelError("File", "Content file is required.");
                    return View(article);
                }

                // Check if a file is uploaded for magazine cover
                if (coverFile != null && coverFile.Length > 0)
                {
                    var maxFileSize = 5 * 1024 * 1024; // 5MB
                    if (coverFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("CoverFile", "File size exceeds the limit.");
                        return View(article);
                    }

                    // Handle file upload for magazine cover
                    var fileName = System.IO.Path.GetFileName(coverFile.FileName);
                    var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverFile.CopyToAsync(fileStream);
                    }
                    article.MagazineCover = fileName;
                }
                else
                {
                    ModelState.AddModelError("CoverFile", "Magazine cover file is required.");
                    return View(article);
                }

                // Add the article to the context and save changes
                _context.Add(article);
                await _context.SaveChangesAsync();

                // Create feedback related to the article
                FeedBack feedback = new FeedBack
                {
                    Content = null,
                    SentDate = null,
                    ArticleId = article.ArticleId
                };
                _context.FeedBacks.Add(feedback);
                await _context.SaveChangesAsync();

/*                var recipientEmail = "nguyenhoangtrongphat@gmail.com"; // Replace with the recipient's email address
                var emailSubject = "New article submission";
                var emailMessage = $"A new article titled '{article.Title}' has been submitted by {user.Name}. Please review it.";
                await SendEmailAsync(recipientEmail, emailSubject, emailMessage);*/

                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorID"] = new SelectList(_context.Users, "Id", "Id", article.AuthorID);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", article.FacultyId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "SemesterID", "SemesterID", article.SemesterId);
            return View(article);
        }
        public async Task SendEmailAsync(string recipient, string subject, string message)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("sinhvien@demomailtrap.com"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(recipient);

                using (var smtpClient = new System.Net.Mail.SmtpClient("live.smtp.mailtrap.io"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new System.Net.NetworkCredential("api", "875dc069f22948871800206e9f57d02c");
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine(ex.Message);
                // Log the error
                _logger.LogError(ex, "Error occurred while sending email");
            }
        }

        public async Task<IActionResult> Upload(int? id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    article.ArticleDate = DateTime.Now;
                    article.Isbool = true;
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(article.ArticleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(article);
        }
        // GET: Articles/Edit/5
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            ViewData["AuthorID"] = new SelectList(_context.Users, "Id", "Id", article.AuthorID);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", article.FacultyId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "SemesterID", "SemesterID", article.SemesterId);
            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,Title,Content,MagazineCover,AuthorID,CategoryId,FacultyId,SemesterId,submissionDate,Comment,ArticleDate,NoOfLike,Isbool")] Article article)
        {
            if (id != article.ArticleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(article.ArticleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorID"] = new SelectList(_context.Users, "Id", "Id", article.AuthorID);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", article.FacultyId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "SemesterID", "SemesterID", article.SemesterId);
            return View(article);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Articles == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Faculty)
                .Include(a => a.Semester)
                .FirstOrDefaultAsync(m => m.ArticleId == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Articles == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Articles'  is null.");
            }
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            var feedbacks = _context.FeedBacks.Where(c => c.ArticleId == id);
            _context.FeedBacks.RemoveRange(feedbacks);
            // Xóa các bình luận liên quan đến bài viết
            var comments = _context.Comments.Where(c => c.ArticleID == id);
            _context.Comments.RemoveRange(comments);

            // Xóa các lượt thích liên quan đến bài viết
            var likes = _context.Likes.Where(l => l.ArticleID == id);
            _context.Likes.RemoveRange(likes);

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
          return (_context.Articles?.Any(e => e.ArticleId == id)).GetValueOrDefault();
        }
    }
}
