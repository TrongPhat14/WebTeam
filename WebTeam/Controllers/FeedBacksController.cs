using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTeam.Data;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using WebTeam.Constants;

namespace WebTeam.Controllers
{
    [Authorize(Roles = "Marketing_Coordinator")]

    public class FeedBacksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public FeedBacksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: FeedBacks
        public async Task<IActionResult>Index(int? page)
        {
            var currentStudent = await _userManager.GetUserAsync(User);
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.FeedBacks != null)
            {
                var feedbacks = await _context.FeedBacks
                .Include(f => f.Article)
                .ThenInclude(a => a.Author)
                .Include(f => f.Article)
                .ThenInclude(a => a.Faculty)
                .OrderByDescending(a => a.FeedBackID)
                .Include(f => f.Marketing_coordinator)
                .Where(a => a.Article.FacultyId == currentStudent.FacultyId && a.Article.Isbool == false && a.Article.boolIs == false)

                .ToListAsync();

                var pagedArticles = feedbacks.ToPagedList(pageNumber, pageSize);

                return View(pagedArticles);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<FeedBack>
                    ());
            }
        }
        
        public async Task<IActionResult> DashboardAsync(int? page, string semesterName)
        {
            // Lấy thông tin về người dùng và vai trò của họ
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);


            // Lấy thông tin về khoa của người dùng
            var userFacultyId = user.FacultyId;

            // Lấy danh sách tất cả các kì từ cơ sở dữ liệu
            var allSemesters = _context.Semesters.Include(s => s.Faculty).ToList();

            // Lọc ra các kì có cùng khoa với người dùng
            var semestersWithUserFaculty = allSemesters.Where(s => s.FacultyID == userFacultyId).ToList();

            ViewData["semestersWithUserFaculty"] = semestersWithUserFaculty;

            // Lấy query feedbacks từ database
            var query = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Faculty)
                .Include(a => a.Category)
                .Include(a => a.Semester)
                .Where(a => semestersWithUserFaculty.Select(s => s.SemesterName).Contains(a.Semester.SemesterName));

            // Lọc theo semesterName nếu được chỉ định
            if (!string.IsNullOrEmpty(semesterName))
            {
                query = query.Where(a => a.Semester.SemesterName == semesterName);
            }

            // Thực hiện phân trang
            var pageNumber = page ?? 1; // Số trang mặc định
            var pageSize = 10; // Số lượng mục trên mỗi trang
            var pagedFeedbacks = await query.ToPagedListAsync(pageNumber, pageSize);
            ViewData["semesterName"] = semesterName;
            return View(pagedFeedbacks);
        }


        public async Task<IActionResult> Chart2Data()
        {
            // Lấy thông tin người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(User);
            // Lấy số ID của khoa của người dùng hiện tại
            var userFacultyId = currentUser.FacultyId;

            // Đếm số lượng bài báo theo từng trạng thái có cùng khoa với người dùng hiện tại
            var pendingCount = await _context.Articles
                .Where(article => article.Isbool != true && article.FacultyId == userFacultyId && article.boolIs != true)
                .CountAsync();

            var approvedCount = await _context.Articles
                .Where(article => article.Isbool == true && article.FacultyId == userFacultyId)
                .CountAsync();

            var deniedCount = await _context.Articles
                .Where(article => article.boolIs == true && article.FacultyId == userFacultyId)
                .CountAsync();

            // Tạo một dictionary để chứa dữ liệu cho biểu đồ
            var data = new Dictionary<string, int>
            {
                { "Pending", pendingCount },
                { "Approved", approvedCount },
                { "Denied", deniedCount }
            };

            // Trả về dữ liệu dưới dạng JSON
            return Json(data);
        }
        public async Task<IActionResult> Approved(int? page)
        {
            var currentStudent = await _userManager.GetUserAsync(User);
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.Articles != null)
            {
/*                var feedbacks = await _context.FeedBacks
                    .Include(f => f.Article)
                    .ThenInclude(a => a.Author)
                    .Include(f => f.Article)
                    .ThenInclude(a => a.Faculty)
                    .Include(f => f.Marketing_coordinator)
                    .Where(a => a.Article.FacultyId == currentStudent.FacultyId && a.Article.Isbool)// Filter trạng thái approved
                    .OrderByDescending(a => a.ArticleId)
                    .ToListAsync();*/
                var article = await _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Faculty)
                    .Include(a => a.FeedBacks)
                                .ThenInclude(f => f.Marketing_coordinator)
                    .Where(a => a.FacultyId == currentStudent.FacultyId && a.Isbool == true)// Filter trạng thái approved
                    .OrderByDescending(a => a.ArticleDate)
                    .ToListAsync();
                var pagedArticles = article.ToPagedList(pageNumber, pageSize);

                return View(pagedArticles);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<FeedBack>());
            }
        }
        public async Task<IActionResult> Reject(int? page)
        {
            var currentStudent = await _userManager.GetUserAsync(User);
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.FeedBacks != null)
            {
/*                var feedbacks = await _context.FeedBacks
                    .Include(f => f.Article)
                    .ThenInclude(a => a.Author)
                    .Include(f => f.Article)
                    .ThenInclude(a => a.Faculty)
                    .Include(f => f.Marketing_coordinator)
                    .Where(a => a.Article.FacultyId == currentStudent.FacultyId && a.Article.boolIs)// Filter trạng thái approved
                    .OrderByDescending(a => a.)
                    .ToListAsync();*/
                var article = await _context.Articles
                        .Include(a => a.Author)
                        .Include(a => a.Faculty)
                        .Include(a => a.FeedBacks)
                                    .ThenInclude(f => f.Marketing_coordinator)
                        .Where(a => a.FacultyId == currentStudent.FacultyId && a.boolIs == true)// Filter trạng thái approved
                        .OrderByDescending(a => a.ArticleDate)
                        .ToListAsync();
                var pagedArticles = article.ToPagedList(pageNumber, pageSize);

                return View(pagedArticles);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<FeedBack>());
            }
        }

        public async Task<IActionResult> Deadline()
        {
            var semester = _context.Semesters.Include(u => u.AcademicYear)
                .OrderByDescending(a => a.CreatedDate) ;

            return View(await semester.ToListAsync());
        }
        private bool SemesterExists(int id)
        {
            return (_context.Semesters?.Any(e => e.SemesterID == id)).GetValueOrDefault();
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
        // GET: FeedBacks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Semesters == null)
            {
                return NotFound();
            }
            var semester = await _context.Semesters.FirstOrDefaultAsync(f => f.SemesterID == id);

            if (semester == null)
            {
                return NotFound();
            }
            return View("~/Views/FeedBacks/Details.cshtml", semester);
        }
        public async Task<IActionResult> SearchFeedBack(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            var feedback= _context.FeedBacks.Include(a => a.Article)
             .ThenInclude(a => a.Author). 
                AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                feedback = feedback.Where(f => f.Article.Author.Name.Contains(searchString));
            }
            switch (orderBy)
            {
                case "user_desc":
                    feedback = feedback.OrderByDescending(a => a.Article.submissionDate);
                    break;

                default:
                    feedback = feedback.OrderBy(a => a.Article.submissionDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var searchResult = await feedback.ToPagedListAsync(pageNumber, pageSize);

            return View("Index",searchResult);
        }
        public async Task<IActionResult> SearchFeedBackApp(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            var feedback = _context.Articles.Include(a => a.Author).
                AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                feedback = feedback.Where(f => f.Author.Name.Contains(searchString));
            }
            switch (orderBy)
            {
                case "user_desc":
                    feedback = feedback.OrderByDescending(a => a.submissionDate);
                    break;

                default:
                    feedback = feedback.OrderBy(a => a.submissionDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var searchResult = await feedback.ToPagedListAsync(pageNumber, pageSize);

            return View("~/Views/FeedBacks/Approved.cshtml", searchResult);
        }
        public async Task<IActionResult> SearchFeedBackRe(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            var feedback = _context.Articles.Include(a => a.Author).
                AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                feedback = feedback.Where(f => f.Author.Name.Contains(searchString));
            }
            switch (orderBy)
            {
                case "user_desc":
                    feedback = feedback.OrderByDescending(a => a.submissionDate);
                    break;

                default:
                    feedback = feedback.OrderBy(a => a.submissionDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var searchResult = await feedback.ToPagedListAsync(pageNumber, pageSize);

            return View("~/Views/FeedBacks/Reject.cshtml", searchResult);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, [Bind("SemesterID,SemesterName,Notification,CreatedDate,ClosureDate,Dl1,DL2,FacultyID,AcademicYearID")] Semester semester)
        {
            if (id != semester.SemesterID)
            {
                return NotFound();
            }

            try
            {
                if (semester.Dl1 < semester.CreatedDate )
                {
                    TempData["ErrorMessage"] = "Create Date must be greater than Deadline1.";
                    return RedirectToAction(nameof(Details));
                }
                else if (semester.DL2 < semester.Dl1)
                {
                    TempData["ErrorMessage"] = "Deadline 2 must be greater than Deadline 1.";
                    return RedirectToAction(nameof(Details));
                }
                else if (semester.DL2 > semester.ClosureDate)
                {
                    TempData["ErrorMessage"] = "Closure Date must be greater than Deadline 2.";
                    return RedirectToAction(nameof(Details));
                }

                _context.Update(semester);
                await _context.SaveChangesAsync();
                TempData["EditMessage"] = "Edit successful";
                return Redirect("/FeedBacks/Deadline");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SemesterExists(semester.SemesterID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return View(semester);
        }


        // GET: FeedBacks/Edit/5

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
            /*            ViewBag.DocxContent = htmlContent;
                        // Trả về view chứa nội dung của tệp DOCX
                        return View(article);*/
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
        // POST: FeedBacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    article.ArticleDate =   DateTime.Now;
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
                TempData["1"] = "Update Success";
                return RedirectToAction(nameof(Index));
            }
            return View(article);
        }
        public async Task<IActionResult> Upload1(int? id)
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
                    article.boolIs = true;
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
                TempData["1"] = "Reject Success";
                return RedirectToAction(nameof(Index));
            }
            return View(article);
        }
        private bool ArticleExists(int id)
        {
            return (_context.Articles?.Any(e => e.ArticleId == id)).GetValueOrDefault();
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FeedBacks == null)
            {
                return NotFound();
            }

            var feedBack = await _context.FeedBacks
                              .Include(f => f.Article)
                                     .ThenInclude(f => f.Author)
                              .FirstOrDefaultAsync(f => f.FeedBackID == id);
            /*            var feedBack = await _context.FeedBacks.
                            FindAsync(id);*/

            feedBack.IsCheckMes = false;
            await _context.SaveChangesAsync();
            if (feedBack == null)
            {
                return NotFound();
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "Title", feedBack.ArticleId);
            ViewData["Marketing_coordinatorID"] = new SelectList(_context.Users, "Id", "Name", feedBack.Marketing_coordinatorID);
            return View(feedBack);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FeedBackID,Content,SentDate,Marketing_coordinatorID,ArticleId,IsCheckMes")] FeedBack feedBack)
        {
            if (id != feedBack.FeedBackID)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                try
                {

                    // Update the UpdateTime with the current time
                    feedBack.SentDate = DateTime.Now;
                    feedBack.Marketing_coordinatorID = userId;
                    feedBack.IsCheckMes = false;
                    var currentArticleId = feedBack.ArticleId;
                    var article = _context.Articles.FirstOrDefault(a => a.ArticleId == feedBack.ArticleId);
                    // Lấy thông tin bài viết từ phản hồi
                    /*                    var article = feedBack.ArticleId;*/
                    var article2 = await _context.Articles
                            .Where(a => a.ArticleId == currentArticleId)
                            .FirstOrDefaultAsync();
                    article2.IsCheckMes = true;
/*                    var article2 = await _context.Articles.FindAsync(currentArticleId);*/
/*                    article2.IsCheckMes = true;*/
                    if (article != null && article.submissionDate != null && DateTime.Now.Subtract((DateTime)article.submissionDate).TotalDays > 14)
                    {
                        // If the article is older than 14 days, return error message or handle as needed
                        ModelState.AddModelError("", "Feedback cannot be edited as it has exceeded the time limit.");
                        return View(feedBack);

                    }
                    // Update Feedback
                    _context.Update(feedBack);
                    await _context.SaveChangesAsync();

                    // Restore the original ArticleId
                    feedBack.ArticleId = currentArticleId;

                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedBackExists(feedBack.FeedBackID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["UpdateMes"] = "Update Successfully";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "Title", feedBack.ArticleId);
            ViewData["Marketing_coordinatorID"] = new SelectList(_context.Users, "Id", "Name", feedBack.Marketing_coordinatorID);
            return View(feedBack);
        }

        // GET: FeedBacks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FeedBacks == null)
            {
                return NotFound();
            }

            var feedBack = await _context.FeedBacks
                .Include(f => f.Article)
                .Include(f => f.Marketing_coordinator)
                .FirstOrDefaultAsync(m => m.FeedBackID == id);
            if (feedBack == null)
            {
                return NotFound();
            }

            return View(feedBack);
        }

        // POST: FeedBacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FeedBacks == null)
            {
                return Problem("Entity set 'ApplicationDbContext.FeedBacks'  is null.");
            }
            var feedBack = await _context.FeedBacks.FindAsync(id);
            if (feedBack != null)
            {
                _context.FeedBacks.Remove(feedBack);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedBackExists(int id)
        {
            return (_context.FeedBacks?.Any(e => e.FeedBackID == id)).GetValueOrDefault();
        }
    }
}
