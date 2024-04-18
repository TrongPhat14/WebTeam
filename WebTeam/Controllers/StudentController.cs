using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTeam.Data;
using X.PagedList;

namespace WebTeam.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            var semesters = await _context.Semesters.FindAsync(id);

            // Kiểm tra xem Semester có tồn tại không
            if (semesters == null)
            {
                return NotFound();
            }

            // Truyền Semester vào ViewBag để sử dụng trong view
            ViewBag.Semester = semesters;
            int pageNumber = page ?? 1;
            int pageSize = 3; // Số lượng mục trên mỗi trang

            var semester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .Include(s => s.Faculty)
                .Include(s => s.Articles)
                    .ThenInclude(a => a.Author)
                .Include(s => s.Articles)
                    .ThenInclude(a => a.Category)
                .Where(s => s.SemesterID == id)
                .SingleOrDefaultAsync();

            if (semester == null)
            {
                return NotFound();
            }

            var articles = await _context.Articles
                .Where(a => a.SemesterId == id) // Filter the articles by semester ID
                .Include(a => a.Author)
                .ToPagedListAsync(pageNumber, pageSize);

            return View(articles);
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

                return Redirect(Request.Headers["Referer"].ToString());
            }
            ViewData["AuthorID"] = new SelectList(_context.Users, "Id", "Id", article.AuthorID);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", article.CategoryId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", article.FacultyId);
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "SemesterID", "SemesterID", article.SemesterId);
            return View(article);
        }

    }
}
