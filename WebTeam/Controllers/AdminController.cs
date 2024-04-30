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
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using System.Security.Claims;
using DocumentFormat.OpenXml.Spreadsheet;
using WebTeam.Models;

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
        ////////////////////////////// Faculties Controller ///////////////////////////////////
        public async Task<IActionResult> Faculties(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.Faculties != null)
            {
                var faculties = await _context.Faculties
                .OrderByDescending(a => a.FacultyId)
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Faculties/Index.cshtml", faculties);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<Faculty>());
            }
        }
        public async Task<IActionResult> SearchFaculty(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            var faculties = _context.Faculties.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                faculties = faculties.Where(f => f.FacultyName.Contains(searchString));
            }
            switch (orderBy)
            {
                case "fac_desc":
                    faculties = faculties.OrderByDescending(a => a.FacultyName);
                    break;

                default:
                    faculties = faculties.OrderBy(a => a.FacultyName);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var searchResult = await faculties.ToPagedListAsync(pageNumber, pageSize);

            return View("~/Views/Admin/Faculties/Index.cshtml", searchResult);
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

                // Đặt thông báo vào TempData
                TempData["SuccessMessage"] = "Faculty has been added successfully.";

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

                    TempData["UpdateSuccessMessage"] = "Faculty has been edited successfully!";
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
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null)
            {
                return NotFound();
            }

            var semester = _context.Semesters.Where(c => c.FacultyID == id);

            var user = _context.Users.Where(c => c.FacultyId == id);

            var categories = _context.Categories.Where(l => l.FacultyId == id);

            var article = _context.Articles.Where(l => l.FacultyId == id);


            if (faculty != null  && !user.Any()  && !article.Any())
            {
                _context.Semesters.RemoveRange(semester);
                _context.Categories.RemoveRange(categories);
                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();
                TempData["DeleteSuccessMessage"] = "Faculty has been deleted successfully!";
                return RedirectToAction(nameof(Faculties));
            }
            else if(faculty != null && !user.Any())
            {
                TempData["DeleteMessage"] = "Faculty cannot be deleted! Because it is having articles of use!";
            }
            else if(faculty != null && !article.Any())
            {
                TempData["DeleteMessage"] = "Faculty cannot be deleted! Because there are users using it!";
            }
            return RedirectToAction(nameof(FacultyDelete));
        }


        private bool FacultyExists(int id)
        {
            return (_context.Faculties?.Any(e => e.FacultyId == id)).GetValueOrDefault();
        }


        ////////////////////////////// Semester Controller ///////////////////////////////////
        public async Task<IActionResult> Semesters(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số lượng mục trên mỗi trang
            if (_context != null && _context.Semesters != null)
            {
                var semesters = await _context.Semesters
                    .Include(s => s.AcademicYear).Include(s => s.Faculty)
                .OrderByDescending(a => a.SemesterID)
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Semesters/Index.cshtml", semesters);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<Semester>());
            }
        }
        public async Task<IActionResult> SearchSemester(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            var semesters = _context.Semesters.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                semesters = semesters.Where(f => f.SemesterName.Contains(searchString));
            }
            switch (orderBy)
            {
                case "sem_desc":
                    semesters = semesters.OrderByDescending(a => a.SemesterName);
                    break;

                default:
                    semesters = semesters.OrderBy(a => a.SemesterName);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var searchResult = await semesters.ToPagedListAsync(pageNumber, pageSize);

            return View("~/Views/Admin/Semesters/Index.cshtml", searchResult);
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
            ViewData["AcademicYearID"] = new SelectList(_context.AcademicYears, "AcademicYearID", "AcademicYears");
            ViewData["FacultyID"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
            return View("~/Views/Admin/Semesters/Create.cshtml");
        }

        // POST: Semesters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SemesterCreate([Bind("SemesterID,SemesterName,Notification,CreatedDate,ClosureDate,Dl1,DL2,FacultyID,AcademicYearID")] Semester semester)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem CloseDate và DL2 có nhỏ hơn CreateDate và Dl1 không
                if (semester.ClosureDate < semester.CreatedDate)
                {
                    // Nếu CloseDate nhỏ hơn CreateDate, đặt thông báo lỗi vào TempData
                    TempData["ErrorMessage"] = "Closure Date must be greater than Create Date.";
                    return RedirectToAction(nameof(SemesterCreate));
                }
                else if (semester.DL2 < semester.Dl1)
                {
                    // Nếu DL2 nhỏ hơn Dl1, đặt thông báo lỗi vào TempData
                    TempData["ErrorMessage"] = "Deadline 2 must be greater than Deadline 1.";
                    return RedirectToAction(nameof(SemesterCreate));
                }

                _context.Add(semester);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Semesters));
            }


            ViewData["AcademicYearID"] = new SelectList(_context.AcademicYears, "AcademicYearID", "AcademicYears", semester.AcademicYear);
            ViewData["FacultyID"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName", semester.Faculty);
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
            ViewData["AcademicYearID"] = new SelectList(_context.AcademicYears, "AcademicYearID", "AcademicYears", semester.AcademicYearID);
            ViewData["FacultyID"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName", semester.FacultyID);
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

                    TempData["SuccessMessage"] = "Semester has been edited successfully!";
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
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null)
            {
                return NotFound();
            }
            var article = _context.Articles.Where(c => c.SemesterId == id);

            if (semester != null && !article.Any())
            {
                _context.Semesters.Remove(semester);
                TempData["DeleteSuccessMessage"] = "Semester has been deleted successfully!";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Semesters));
            }
            else
            {
                TempData["DeleteSuccessMessage"] = "Error";

            }

            return RedirectToAction(nameof(SemesterDelete));

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
                                               .OrderByDescending(a => a.CategoryId)

                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Categories/Index.cshtml", categories);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<Category>());
            }
        }
        public async Task<IActionResult> SearchCategory(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            IQueryable<Category> categories = _context.Categories.Include(c => c.Faculty);

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.CategoryName.Contains(searchString));
            }

            switch (orderBy)
            {
                case "category_desc":
                    categories = categories.OrderByDescending(a => a.CategoryName);
                    break;

                default:
                    categories = categories.OrderBy(a => a.CategoryName);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var searchResult = await categories.ToPagedListAsync(pageNumber, pageSize);

            return View("~/Views/Admin/Categories/Index.cshtml", searchResult);
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
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
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

                TempData["SuccessMessage"] = "A Category has been created successfully!";

                return RedirectToAction(nameof(Categories)); // Chuyển hướng người dùng về trang Categories
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
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName", category.FacultyId);
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

                    // Thêm thông báo thành công vào TempData
                    TempData["SuccessMessage"] = "Category has been edited successfully!";
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
                // Chuyển hướng về trang index của category
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
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var article = _context.Articles.Where(c => c.CategoryId == id);

            if (category != null && !article.Any())
            {
                _context.Categories.Remove(category);
                TempData["DeleteSuccessMessage"] = "Category has been deleted successfully!";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            else
            {
                TempData["DeleteSuccessMessage"] = "Error";

            }
            return RedirectToAction(nameof(CategoryDelete));
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
                                               .OrderByDescending(a => a.AcademicYearID)

                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/AcademicYears/Index.cshtml", academicYears);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<AcademicYear>());
            }
        }
        public async Task<IActionResult> SearchAcademicYear(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            var academicYear = _context.AcademicYears.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                academicYear = academicYear.Where(f => f.AcademicYears.Contains(searchString));
            }
            switch (orderBy)
            {
                case "academic_desc":
                    academicYear = academicYear.OrderByDescending(a => a.AcademicYears);
                    break;

                default:
                    academicYear = academicYear.OrderBy(a => a.AcademicYears);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var searchResult = await academicYear.ToPagedListAsync(pageNumber, pageSize);

            return View("~/Views/Admin/AcademicYears/Index.cshtml", searchResult);
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
                // Kiểm tra ngày bắt đầu và ngày kết thúc
                if (academicYear.StartDate >= academicYear.ClosureDate)
                {
                    TempData["ErrorMessage1"] = "Closure Date must be greater than Start Date.";
                    return RedirectToAction(nameof(AcademicYearCreate));
                }

                _context.Add(academicYear);
                await _context.SaveChangesAsync();

                // Đặt thông báo thành công vào TempData
                TempData["SuccessMessage"] = "An Academic has been created successfully!";

                return RedirectToAction(nameof(AcademicYears)); // Chuyển hướng người dùng về trang Index
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
                TempData["UpdateeSuccessMessage"] = "An Academic has been created successfully!";
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
            if (academicYear == null)
            {
                return NotFound();

            }
            var semesterIds = await _context.Semesters
                .Where(c => c.AcademicYearID == id)
                .Select(s => (int?)s.SemesterID) // Chuyển đổi thành int? (nullable int)
                .ToListAsync();
            var semester = _context.Semesters.Where(c => c.AcademicYearID == id);

            var articles = _context.Articles
                .Where(a => semesterIds.Contains(a.SemesterId));



            if (academicYear != null && !semesterIds.Any() )
            {
                _context.AcademicYears.Remove(academicYear);
                await _context.SaveChangesAsync();
                TempData["DeleteSuccessMessage"] = "Faculty has been deleted successfully!";
                return RedirectToAction(nameof(AcademicYears));
            }
            else if (academicYear != null && !articles.Any())
            {
                _context.Semesters.RemoveRange(semester);
                _context.AcademicYears.Remove(academicYear);
                await _context.SaveChangesAsync();
                TempData["DeleteSuccessMessage"] = "Faculty has been deleted successfully!";
                return RedirectToAction(nameof(AcademicYears));
            }
            else
            {
                TempData["DeleteSuccessMessage"] = "The school year cannot be erased! Because there are articles being posted!";

            }
            return RedirectToAction(nameof(AcademicYearDelete));
/*            return RedirectToAction(nameof(AcademicYears), new { deleted = true });*/
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
                                               .OrderByDescending(a => a.Id)
                                               .ToPagedListAsync(pageNumber, pageSize);

                return View("~/Views/Admin/Users/Index.cshtml", users);
            }
            else
            {
                // Xử lý trường hợp khi _context hoặc _context.FeedBacks là null
                return View(new List<user>());
            }
        }
        public async Task<IActionResult> SearchUser(string searchString, int? page, string orderBy)
        {
            ViewBag.SearchString = searchString;

            IQueryable<ApplicationUser> users = _context.ApplicationUsers.Include(u => u.Faculty);
            



            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Name.Contains(searchString));
            }

            // Sắp xếp nếu có
            switch (orderBy)
            {
                case "user_desc":
                    users = users.OrderByDescending(u => u.Name);
                    break;
                default:
                    users = users.OrderBy(u => u.Name);
                    break;
            }

            // Phân trang
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var paginatedUsers = await users.ToPagedListAsync(pageNumber, pageSize);

            return View("~/Views/Admin/Users/Index.cshtml", paginatedUsers);
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

