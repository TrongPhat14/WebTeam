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

namespace WebTeam.Controllers
{
    public class FeedBacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedBacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FeedBacks
        public async Task<IActionResult> Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.FeedBacks != null)
            {
                var feedbacks = await _context.FeedBacks
                                               .Include(f => f.Article)
                                                   .ThenInclude(a => a.Author)
                                               .Include(f => f.Marketing_coordinator)
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View(feedbacks);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<FeedBack>());
            }
        }
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult Deadline()
        {
            return View();
        }
        // GET: FeedBacks/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: FeedBacks/Create
        public IActionResult Create()
        {
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "Title");
            ViewData["Marketing_coordinatorID"] = new SelectList(_context.Users, "Id", "Name");
            return View();
        }

        // POST: FeedBacks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FeedBackID,Content,SentDate,Marketing_coordinatorID,ArticleId")] FeedBack feedBack)
        {
            feedBack.SentDate = DateTime.Now;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (ModelState.IsValid)
            {
                _context.Add(feedBack);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", feedBack.ArticleId);
            ViewData["Marketing_coordinatorID"] = new SelectList(_context.Users, "Id", "Id", feedBack.Marketing_coordinatorID);
            return View(feedBack);
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
                              .FirstOrDefaultAsync(f => f.FeedBackID == id);
/*            var feedBack = await _context.FeedBacks.
                FindAsync(id);*/
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
        public async Task<IActionResult> Edit(int id, [Bind("FeedBackID,Content,SentDate,Marketing_coordinatorID,ArticleId")] FeedBack feedBack)
        {
            if (id != feedBack.FeedBackID)
            {
                return NotFound();
            }
             

            /*var article = _context.Articles.FirstOrDefault(a => a.ArticleId == feedBack.ArticleId); // Giả sử có một trường Id trong bảng article, bạn cần thay thế bằng trường tương ứng
            if (article != null)
            {
                if (DateTime.Now.Subtract((DateTime)article.submissionDate).TotalDays > 14) // Giả sử có một trường PublishedDate trong bảng article, bạn cần thay thế bằng trường tương ứng
                {
                    // If the article is older than 14 days, return error message or handle as needed
                    ModelState.AddModelError("", "Feedback cannot be edited as it has exceeded the time limit.");
                    return View(feedBack);
                }
            }
            else
            {
                // Handle the case where the article doesn't exist
                ModelState.AddModelError("", "Article not found.");
                return View(feedBack);
            }*/
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                try
                {

                    // Update the UpdateTime with the current time
                    feedBack.SentDate = DateTime.Now;
                    feedBack.Marketing_coordinatorID = userId;
                    var currentArticleId = feedBack.ArticleId;
                    var article = _context.Articles.FirstOrDefault(a => a.ArticleId == feedBack.ArticleId);
                    if (DateTime.Now.Subtract((DateTime)article.submissionDate).TotalDays > 14) // Giả sử có một trường PublishedDate trong bảng article, bạn cần thay thế bằng trường tương ứng
                    {
                        // If the article is older than 14 days, return error message or handle as needed
                        ModelState.AddModelError("", "Feedback cannot be edited as it has exceeded the time limit.");
                        return View(article);
                    }
                    // Update Feedback
                    _context.Update(feedBack);
                    await _context.SaveChangesAsync();

                    // Restore the original ArticleId
                    feedBack.ArticleId = currentArticleId;

                    // Get the related Submission
/*                    var article = await _context.Articles.FirstOrDefaultAsync(s => s.ArticleId == feedBack.ArticleId);
                    if (article != null)
                    {
                        // Update CommentSubContent in Submission
                        article.CommentSubContent = feedBack.Content;
                        article.LastFeedbackUpdateTime = feedBack.UpdateTime;
                        _context.Update(article);
                        await _context.SaveChangesAsync();
                    }
*/
                    // Redirect to Submission Index
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", feedBack.ArticleId);
            ViewData["Marketing_coordinatorID"] = new SelectList(_context.Users, "Id", "Id", feedBack.Marketing_coordinatorID);
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
