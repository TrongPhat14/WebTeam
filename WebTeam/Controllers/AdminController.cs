using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTeam.Data;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using WebTeam.Data.Migrations;

namespace WebTeam.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }


        ////////////////////////////// Faculties Controller ///////////////////////////////////
/*        public async Task<IActionResult> Faculties()
        {
            if (_context.Faculties != null)
            {
                var faculties = await _context.Faculties.ToListAsync();
                return View("~/Views/Admin/Faculties/Index.cshtml", faculties);
            }
            else
            {
                return Problem("Entity set 'ApplicationDbContext.Faculties' is null.");
            }
        }*/
        public async Task<IActionResult> Faculties(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.Faculties != null)
            {
                var faculties = await _context.Faculties
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Faculties/Index.cshtml", faculties);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<Faculty>());
            }
        }
        // GET: Faculties/Details/5
        public async Task<IActionResult> FacultyDetails(int? id)
        {
            if (id == null || _context.Faculties == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(m => m.FacultyId == id);
            if (faculty == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Faculties/Details.cshtml", faculty);
        }

        // GET: Faculties/Create
        public IActionResult FacultyCreate()
        {
            return View("~/Views/Admin/Faculties/Create.cshtml");
        }

        // POST: Faculties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FacultyCreate([Bind("FacultyId,FacultyName")] Faculty faculty)
        {
            if (ModelState.IsValid)
            {
                _context.Add(faculty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Faculties));
            }
            return View(faculty);
        }

        // GET: Faculties/Edit/5
        public async Task<IActionResult> FacultyEdit(int? id)
        {
            if (id == null || _context.Faculties == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/Faculties/Edit.cshtml", faculty);
        }

        // POST: Faculties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FacultyEdit(int id, [Bind("FacultyId,FacultyName")] Faculty faculty)
        {
            if (id != faculty.FacultyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faculty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacultyExists(faculty.FacultyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Faculties));
            }
            return View(faculty);
        }

        // GET: Faculties/Delete/5
        public async Task<IActionResult> FacultyDelete(int? id)
        {
            if (id == null || _context.Faculties == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(m => m.FacultyId == id);
            if (faculty == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Faculties/Delete.cshtml", faculty);
        }

        // POST: Faculties/Delete/5
        [HttpPost, ActionName("FacultyDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FacultyDeleteConfirmed(int id)
        {
            if (_context.Faculties == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Faculties'  is null.");
            }
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty != null)
            {
                _context.Faculties.Remove(faculty);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Faculties));
        }

        private bool FacultyExists(int id)
        {
            return (_context.Faculties?.Any(e => e.FacultyId == id)).GetValueOrDefault();
        }


        ////////////////////////////// Semester Controller ///////////////////////////////////
/*        public async Task<IActionResult> Semesters()
        {
            var applicationDbContext = _context.Semesters.Include(s => s.AcademicYear).Include(s => s.Faculty);
            return View("~/Views/Admin/Semesters/Index.cshtml", await applicationDbContext.ToListAsync());
        }*/
        public async Task<IActionResult> Semesters(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.Semesters != null)
            {
                var semesters = await _context.Semesters
                    .Include(s => s.AcademicYear).Include(s => s.Faculty)
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Semesters/Index.cshtml", semesters);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<Semester>());
            }
        }
        // GET: Semesters/Details/5
        public async Task<IActionResult> SemesterDetails(int? id)
        {
            if (id == null || _context.Semesters == null)
            {
                return NotFound();
            }

            var semester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(m => m.SemesterID == id);
            if (semester == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Semesters/Details.cshtml", semester);
        }

        // GET: Semesters/Create
        public IActionResult SemesterCreate()
        {
            ViewData["AcademicYearID"] = new SelectList(_context.AcademicYears, "AcademicYearID", "AcademicYearID");
            ViewData["FacultyID"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId");
            return View("~/Views/Admin/Semesters/Create.cshtml");
        }

        // POST: Semesters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SemesterCreate([Bind("SemesterID,SemesterName,Notification,CreatedDate,ClosureDate,Dl1,DL2,FacultyID,AcademicYearID")] Semester semester)
        {
/*            if (semester.CreatedDate.HasValue) // Kiểm tra xem CreatedDate có giá trị không
            {
                var createdDate = semester.CreatedDate.Value; // Trích xuất ngày thực sự từ CreatedDate

                var year = createdDate.Year; // Trích xuất năm từ CreatedDate

                // Tìm AcademicYearID phù hợp với năm
                var academicYear = await _context.AcademicYears.FirstOrDefaultAsync(a => a.StartDate <= year && a.ClosureDate >= year);

                if (academicYear != null)
                {
                    // Gán AcademicYearID tìm được vào đối tượng semester
                    semester.AcademicYearID = academicYear.AcademicYearID;

                    // Thêm semester vào context và lưu thay đổi
                    _context.Add(semester);
                    await _context.SaveChangesAsync();

                    // Redirect đến action Semesters
                    return RedirectToAction(nameof(Semesters));
                }
                else
                {
                    // Trường hợp không tìm thấy AcademicYear phù hợp
                    ModelState.AddModelError(string.Empty, "Không tìm thấy AcademicYear phù hợp.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ngày tạo không hợp lệ.");
            }*/
            if (ModelState.IsValid)
            {
                _context.Add(semester);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AcademicYearID"] = new SelectList(_context.AcademicYears, "AcademicYearID", "AcademicYearID", semester.AcademicYearID);
            ViewData["FacultyID"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", semester.FacultyID);
            return View(semester);
        }

        // GET: Semesters/Edit/5
        public async Task<IActionResult> SemesterEdit(int? id)
        {
            if (id == null || _context.Semesters == null)
            {
                return NotFound();
            }

            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null)
            {
                return NotFound();
            }
            ViewData["AcademicYearID"] = new SelectList(_context.AcademicYears, "AcademicYearID", "AcademicYearID", semester.AcademicYearID);
            ViewData["FacultyID"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", semester.FacultyID);
            return View("~/Views/Admin/Semesters/Edit.cshtml", semester);
        }

        // POST: Semesters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SemesterEdit(int id, [Bind("SemesterID,SemesterName,Notification,CreatedDate,ClosureDate,Dl1,DL2,FacultyID,AcademicYearID")] Semester semester)
        {
            if (id != semester.SemesterID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(semester);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Semesters));
            }
            ViewData["AcademicYearID"] = new SelectList(_context.AcademicYears, "AcademicYearID", "AcademicYearID", semester.AcademicYearID);
            ViewData["FacultyID"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", semester.FacultyID);
            return View(semester);
        }

        // GET: Semesters/Delete/5
        public async Task<IActionResult> SemesterDelete(int? id)
        {
            if (id == null || _context.Semesters == null)
            {
                return NotFound();
            }

            var semester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(m => m.SemesterID == id);
            if (semester == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Semesters/Delete.cshtml", semester);
        }

        // POST: Semesters/Delete/5
        [HttpPost, ActionName("SemesterDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SemesterDeleteConfirmed(int id)
        {
            if (_context.Semesters == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Semesters'  is null.");
            }
            var semester = await _context.Semesters.FindAsync(id);
            if (semester != null)
            {
                _context.Semesters.Remove(semester);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Semesters));
        }

        private bool SemesterExists(int id)
        {
            return (_context.Semesters?.Any(e => e.SemesterID == id)).GetValueOrDefault();
        }

        ////////////////////////////// Categories Controller ///////////////////////////////////
        public async Task<IActionResult> Categories(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.Categories != null)
            {
                var categories = await _context.Categories
                                               .Include(c => c.Faculty)
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Categories/Index.cshtml", categories);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<Category>());
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> CategoryDetails(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Faculty)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Categories/Details.cshtml", category);
        }

        // GET: Categories/Create
        public IActionResult CategoryCreate()
        {
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId");
            return View("~/Views/Admin/Categories/Create.cshtml");
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate([Bind("CategoryId,CategoryName,FacultyId")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", category.FacultyId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> CategoryEdit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", category.FacultyId);
            return View("~/Views/Admin/Categories/Edit.cshtml", category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(int id, [Bind("CategoryId,CategoryName,FacultyId")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyId", category.FacultyId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> CategoryDelete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Faculty)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/Categories/Delete.cshtml", category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("CategoryDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.CategoryId == id)).GetValueOrDefault();
        }


        ////////////////////////////// AcademicYears Controller ///////////////////////////////////
        public async Task<IActionResult> AcademicYears(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.AcademicYears != null)
            {
                var academicYears = await _context.AcademicYears
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/AcademicYears/Index.cshtml", academicYears);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<AcademicYear>());
            }
        }
        // GET: AcademicYears/Details/5
        public async Task<IActionResult> AcademicYearDetails(int? id)
        {
            if (id == null || _context.AcademicYears == null)
            {
                return NotFound();
            }

            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(m => m.AcademicYearID == id);
            if (academicYear == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AcademicYears/Details.cshtml", academicYear);
        }

        // GET: AcademicYears/Create
        public IActionResult AcademicYearCreate()
        {
            return View("~/Views/Admin/AcademicYears/Create.cshtml");
        }

        // POST: AcademicYears/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcademicYearCreate([Bind("AcademicYearID,AcademicYears,StartDate,ClosureDate")] AcademicYear academicYear)
        {
            if (ModelState.IsValid)
            {
                _context.Add(academicYear);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AcademicYears));
            }
            return View(academicYear);
        }

        // GET: AcademicYears/Edit/5
        public async Task<IActionResult> AcademicYearEdit(int? id)
        {
            if (id == null || _context.AcademicYears == null)
            {
                return NotFound();
            }

            var academicYear = await _context.AcademicYears.FindAsync(id);
            if (academicYear == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/AcademicYears/Edit.cshtml", academicYear);
        }

        // POST: AcademicYears/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcademicYearEdit(int id, [Bind("AcademicYearID,AcademicYears,StartDate,ClosureDate")] AcademicYear academicYear)
        {
            if (id != academicYear.AcademicYearID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(academicYear);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcademicYearExists(academicYear.AcademicYearID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AcademicYears));
            }
            return View(academicYear);
        }

        // GET: AcademicYears/Delete/5
        public async Task<IActionResult> AcademicYearDelete(int? id)
        {
            if (id == null || _context.AcademicYears == null)
            {
                return NotFound();
            }

            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(m => m.AcademicYearID == id);
            if (academicYear == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AcademicYears/Delete.cshtml", academicYear);
        }

        // POST: AcademicYears/Delete/5
        [HttpPost, ActionName("AcademicYearDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcademicYearDeleteConfirmed(int id)
        {
            if (_context.AcademicYears == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AcademicYears'  is null.");
            }
            var academicYear = await _context.AcademicYears.FindAsync(id);
            if (academicYear != null)
            {
                _context.AcademicYears.Remove(academicYear);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AcademicYears));
        }

        private bool AcademicYearExists(int id)
        {
            return (_context.AcademicYears?.Any(e => e.AcademicYearID == id)).GetValueOrDefault();
        }
        ////////////////////////////// User Controller ///////////////////////////////////
        public async Task<IActionResult> Users(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.Users != null)
            {
                var users = await _context.Users.Include(u => u.Faculty)
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Users/Index.cshtml", users);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<user>());
            }
        }

        public async Task<IActionResult> UserEdit(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName", user.FacultyId);
            return View("~/Views/Admin/Users/Edit.cshtml", user);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(string id, [Bind("Id, Name, Email, FacultyId")] ApplicationUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            try
            {
                // Lấy người dùng từ cơ sở dữ liệu
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin người dùng từ dữ liệu được gửi
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.FacultyId = user.FacultyId;

                // Cập nhật và lưu người dùng
                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Users));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        public async Task<IActionResult> UserDelete(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Faculty)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }


            return View("~/Views/Admin/Users/Delete.cshtml", user);
        }

        [HttpPost, ActionName("UserDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserDeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Xử lý lỗi nếu không thể xóa người dùng
                // Ví dụ: hiển thị thông báo lỗi, ghi nhật ký, vv.
                return Problem("Error occurred while deleting the user.");
            }
            // Sau khi xóa, chuyển hướng người dùng đến trang chính
            return RedirectToAction(nameof(Users));
        }
        private bool UserExists(string id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

