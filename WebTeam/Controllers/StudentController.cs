using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using WebTeam.Data;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using WebTeam.Constants;

namespace WebTeam.Controllers
{
    [Authorize(Roles = "Student")]

    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentController> _logger;



        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<StudentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Semesters.Include(s => s.AcademicYear).Include(s => s.Faculty);
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id, int? page)
        {
            if (id == null || _context.Semesters == null)
            {
                return NotFound();
            }
            // Lấy thông tin về Semester từ cơ sở dữ liệu hoặc các phương thức khác
            var semesters = await _context.Semesters.Include(S => S.Faculty)
                                          .FirstOrDefaultAsync(f => f.SemesterID == id);
            var currentStudent = await _userManager.GetUserAsync(User);
            // Kiểm tra xem Semester có tồn tại không
            if (semesters == null)
            {
                return NotFound();
            }

            // Truyền Semester vào ViewBag để sử dụng trong view
            ViewBag.Semester = semesters;
            int pageNumber = page ?? 1;
            int pageSize = 3; // Số lượng mục trên mỗi trang

            var currentDate = DateTime.Now;
            bool isPastDl1 = currentDate > semesters.Dl1;
            bool isPastDl2 = currentDate > semesters.DL2;
            // Truyền giá trị boolean sang View
            ViewData["IsPastDl1"] = isPastDl1;
            ViewData["IsPastDl2"] = isPastDl2;

            var articles = await _context.Articles
                .Include(s => s.Faculty)
                .Include(s => s.Category)
                .Include(s => s.Author)
                .OrderByDescending(a => a.ArticleId)
                .Where(a => a.SemesterId == id && a.Author.Id == currentStudent.Id)
                .ToListAsync();
            var pagedArticles = articles.ToPagedList(pageNumber, pageSize);

            return View(pagedArticles);
        }
        public IActionResult Download(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = _context.Articles.FirstOrDefault(m => m.ArticleId == id);
            if (article == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", article.Content);

            // Kiểm tra định dạng của tệp
            var fileExtension = Path.GetExtension(filePath);
            if (fileExtension != ".docx")
            {
                return BadRequest("Only .docx files can be downloaded.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", article.Content);
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
            var se = article.SemesterId;
            var semester = await _context.Semesters.FindAsync(se);
            article.IsCheckMes = false;
            await _context.SaveChangesAsync();

            if (article == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                // Xử lý trường hợp không tìm thấy người dùng
                // Có thể chuyển hướng đến một trang lỗi hoặc thực hiện xử lý khác tùy theo yêu cầu của bạn
            }
            ViewBag.CurrentSemester = semester;

            // Lấy thông tin khoa của người dùng
            var userFacultyId = user.FacultyId;

            // Lấy danh sách các thể loại có cùng khoa với người dùng
            var accessibleCategories = await _context.Categories
                                                .Where(c => c.FacultyId == userFacultyId)
                                                .ToListAsync();
            var feedback = await _context.FeedBacks
                                    .Where(c => c.ArticleId == article.ArticleId)
                                    .ToListAsync();
            ViewData["Feedback"] = feedback;
            ViewData["AuthorID"] = new SelectList(_context.Users, "Id", "Name", article.AuthorID);
            ViewData["CategoryId"] = new SelectList(accessibleCategories, "CategoryId", "CategoryName", article.CategoryId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", article.FacultyId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "SemesterID", "SemesterID", article.SemesterId);
            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("ArticleId,Title,Content,MagazineCover,AuthorID,CategoryId,FacultyId,SemesterId,submissionDate,Comment,ArticleDate,NoOfLike,Isbool,Description,IsCheckMes")] Article article, IFormFile file, IFormFile coverFile)
        {
            if (id != article.ArticleId)
            {
                return NotFound();
            }

            // Load the original article from the database
            var originalArticle = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.ArticleId == id);
            article.IsCheckMes = false;
            // Lấy thông tin bài viết từ phản hồi
            var feedBack = article.ArticleId;
            var article2 = await _context.FeedBacks
                    .Where(a => a.ArticleId == feedBack)
                    .FirstOrDefaultAsync();
            article2.IsCheckMes = true;
            var se = article.SemesterId;
            var semester = await _context.Semesters.FindAsync(se);
            ViewBag.CurrentSemester = semester;
            /*            if (article2 != null)
                        {
                            if (article2.IsCheckMes == true)
                            {
                                // Do something
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            // Xử lý trường hợp article2 là null
                        }*/

            if (file != null && file.Length > 0)
            {
                // Handle file upload for content
                var fileExtension = System.IO.Path.GetExtension(file.FileName);
                if (fileExtension != ".docx" && fileExtension != ".pdf")
                {
                    ModelState.AddModelError("File", "Only .docx and .pdf files are allowed for content.");
                    return View(article);
                }

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
                // Keep the original content if no new file is uploaded
                article.Content = originalArticle.Content;
            }

            if (coverFile != null && coverFile.Length > 0)
            {
                // Handle file upload for magazine cover
                var maxFileSize = 5 * 1024 * 1024; // 5MB
                if (coverFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("CoverFile", "File size exceeds the limit.");
                    return View(article);
                }

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
                // Keep the original magazine cover if no new file is uploaded
                article.MagazineCover = originalArticle.MagazineCover;
            }

            // Update the article fields you want to change
            article.IsCheckMes = true;
            article.submissionDate = DateTime.Now;

            try
            {
                _context.Update(article);
                await _context.SaveChangesAsync();
                TempData["UpdateMes2"] = "Edit Successfully";
                return RedirectToAction("Details", new { id = article.SemesterId });
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
        }
        public async Task<IActionResult> Create(int? semesterId)
        {

            if (semesterId == null || _context.Semesters == null)
            {
                return NotFound();
            }
            var semester = await _context.Semesters.FindAsync(semesterId);
            if (semester == null)
            {
                return NotFound();
            }

            ViewBag.CurrentSemester = semester;
            var newArticle = new Article
            {
                SemesterId = semesterId.Value // Gán SemesterID của bài viết bằng ID của Semester
            };

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
            return View(newArticle);
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArticleId,Title,Content,MagazineCover,AuthorID,CategoryId,FacultyId,SemesterId,submissionDate,Comment,ArticleDate,NoOfLike,Isbool,Description,IsCheckMes")] Article article, IFormFile file, IFormFile coverFile)
        {
            if (ModelState.IsValid)
            {
                // Set submission date
                article.submissionDate = DateTime.Now;
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var se = article.SemesterId;
                var semester = await _context.Semesters.FindAsync(se);
                ViewBag.CurrentSemester = semester;

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
                    article.IsCheckMes = false;
                    // Lấy kì hiện tại đang chọn để tạo bài
                    var semesterId = article.SemesterId;
                    var currentSemester = await _context.Semesters.FindAsync(semesterId);

                    if (currentSemester != null)
                    {
                        // Gán semesterId trong article
                        article.SemesterId = currentSemester.SemesterID;

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid semester selected.");
                        return View(article);
                    }
                    // Lấy thông tin của khoa của người dùng
                    var userFacultyId = user.FacultyId;

                    // Truy vấn UserManager để lấy danh sách điều phối viên trong vai trò "Marketing_coordinator"
                    var marketingCoordinators = await _userManager.GetUsersInRoleAsync("Marketing_coordinator");

                    // Lọc danh sách điều phối viên cùng khoa với người dùng đang đăng nhập
                    var coordinator = marketingCoordinators.FirstOrDefault(mc => mc.FacultyId == userFacultyId);


                    if (coordinator != null)
                    {
                        var emailModel = new EmailModel
                        {
                            FromEmail = "nguyenhoangtrongphat@gmail.com",
                            ToEmails = $"{coordinator.Email}",
                            Subject = "New article submission",
                            Body = $"A new article titled '{article.Title}' has been submitted by {user.Name}. Please review it."
                        };
                        await SendEmailAsync(emailModel);

                    }
                    else
                    {
                        // Xử lý trường hợp không tìm thấy điều phối viên
                        ModelState.AddModelError(string.Empty, "Coordinator not found for the user's faculty.");
                        return View(article);
                    }

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
                    ArticleId = article.ArticleId,
                    IsCheckMes = true
                };
                _context.FeedBacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["UpdateMes2"] = "Add Successfully";
                return RedirectToAction("Details", new { id = article.Semester.SemesterID });
            }
            ViewData["AuthorID"] = new SelectList(_context.Users, "Id", "Id", article.AuthorID);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", article.FacultyId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "SemesterID", "SemesterID", article.SemesterId);
            return View(article);
        }
        private async Task SendEmailAsync(EmailModel emailData)
        {
            try
            {
                var message = new MailMessage()
                {
                    From = new MailAddress(emailData.FromEmail),
                    Subject = emailData.Subject,
                    IsBodyHtml = true,
                    Body = $@"
                        <html>
                            <body>
                                <h3>{emailData.Body}</h3>
                            </body> 
                        </html>"
                };

                foreach (var toEmail in emailData.ToEmails.Split(";"))
                {
                    message.To.Add(new MailAddress(toEmail));
                }

                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailData.FromEmail, "ehkj dldg rmtd xqsg"),
                    EnableSsl = true,
                };

                await smtp.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
            }
        }
        private bool ArticleExists(int id)
        {
            return (_context.Articles?.Any(e => e.ArticleId == id)).GetValueOrDefault();
        }
    }
}
