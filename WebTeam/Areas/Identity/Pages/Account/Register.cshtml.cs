// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebTeam.Constants;
using WebTeam.Data;
using WebTeam.Data.Migrations;
using System.Net.Mail;
using System.Net;
using WebTeam.Controllers;



namespace WebTeam.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            public string? Name { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
/*            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
*/
            [Required]
            [Display(Name = "roles")]
            [Compare("Roles", ErrorMessage = "please choose 1 role")]
            [BindProperty]
            public List<string> Roles { get; set; }

            /*            [Required]
                        [Display(Name = "Faculties")]
                        [Compare("Faculties", ErrorMessage = "please choose 1 role")]
                        public List<string> Faculties { get; set; }*/
            
            [Display(Name = "Faculties")]
            public List<int> SelectedFaculties { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
/*            ViewData["Faculties"] = await _context.Faculties.ToListAsync();*/
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                if (Input.Roles.Contains(Roles.Marketing_Coordinator.ToString()))
                {
                    // Lấy ID của ngành học được chọn
                    var facultyId = Input.SelectedFaculties.FirstOrDefault();

                    // Kiểm tra xem trong ngành học đó đã có tài khoản với vai trò Marketing_Coordinator chưa
                    var existingMarketingCoordinatorsForFaculty = await _userManager.GetUsersInRoleAsync(Roles.Marketing_Coordinator.ToString());
                    var userInSameFaculty = existingMarketingCoordinatorsForFaculty.FirstOrDefault(u => u.FacultyId == facultyId);

                    if (userInSameFaculty != null)
                    {
                        // Nếu đã có tài khoản với vai trò Marketing_Coordinator trong ngành học này, thông báo và quay lại trang đăng ký
                        ModelState.AddModelError(string.Empty, "There is already a Marketing Coordinator for the selected faculty.");
                        ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
                        return Page();
                    }
                }
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Name = Input.Name,
                };

                if (Input.SelectedFaculties != null && Input.SelectedFaculties.Count > 1)
                {     
                    // Set the selected faculty to the user
                    ModelState.AddModelError(string.Empty, "Please select only one faculty.");
                    // Cập nhật danh sách khoa để hiển thị lại trên trang đăng ký
                    ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
                    // Trả về trang đăng ký với thông báo lỗi
                    return Page();

                }
                else 
                {
                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        // Tạo mật khẩu ngẫu nhiên
                        string password = GenerateRandomPassword();

                        // Đặt mật khẩu cho tài khoản mới
                        await _userManager.AddPasswordAsync(user, password);
                        foreach (var role in Input.Roles)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                        if (Input.Roles.Contains(Roles.Marketing_manager.ToString()))
                        {
                            // Không cần xử lý về khoa, nên không cần thiết lập user.FacultyId
                            // Không cần lưu thay đổi vào cơ sở dữ liệu (_context.SaveChangesAsync())
                            // Không gửi email xác nhận

                            // Log thông tin
                            _logger.LogInformation("Marketing Manager created a new account with password.");


                        }
                        else
                        {
                            // Assuming Input.SelectedFaculties contains only one selected faculty

                            // Kiểm tra trường Faculty Name như thông thường
                            if (Input.SelectedFaculties == null || !Input.SelectedFaculties.Any())
                            {
                                // Nếu người dùng chưa chọn khoa, hiển thị thông báo lỗi
                                ModelState.AddModelError(string.Empty, "The Faculties field is required.");
                                // Cập nhật danh sách khoa để hiển thị lại trên trang đăng ký
                                ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
                                // Trả về trang đăng ký với thông báo lỗi
                                return Page();
                            }

                            var selectedFacultyId = Input.SelectedFaculties.FirstOrDefault();
                            if (selectedFacultyId != null)
                            {
                                // Thiết lập faculty được chọn cho người dùng
                                user.FacultyId = selectedFacultyId;
                            }

                            else
                            {
                                user.FacultyId = null;
                            }
                        }


                        // Save changes to the database

                        var emailModel = new EmailModel
                        {
                            FromEmail = "nguyenhoangtrongphat@gmail.com",
                            ToEmails = $"{user.Email}",
                            Subject = "Password account",
                            Body = $"Welcome user, this is your password (Copy the word after the sign: {password})"
                        };
                        await SendEmailAsync(emailModel);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("User created a new account with password.");
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm Student email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            /*                        await _signInManager.SignInAsync(user, isPersistent: false);*/
                            return LocalRedirect("~/Admin/Users");
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyName");
            // If we got this far, something failed, redisplay form
            return Page();
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
        private string GenerateRandomPassword(int length = 10)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-+=<>?";
            const string validCharsWithoutSpecial = "abcdefghijklmnopqrstuvwxyz";
            const string NumberWithoutSpecial = "1234567890";
            const string CharSpecial = "!@#$%^&*()_-+=<>?";
            const string HCharSpecial = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";


            var random = new Random();
            var password = new StringBuilder();

            // Append a random character from validCharsWithoutSpecial first
            password.Append(validCharsWithoutSpecial[random.Next(validCharsWithoutSpecial.Length)]);
            password.Insert(1, NumberWithoutSpecial[random.Next(NumberWithoutSpecial.Length)]);
            password.Insert(2, CharSpecial[random.Next(CharSpecial.Length)]);
            password.Insert(3, HCharSpecial[random.Next(HCharSpecial.Length)]);



            for (int i = 4; i < length; i++)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }

            return password.ToString();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private ApplicationUser CreateMarketing_coordinator()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private ApplicationUser CreateMarketing_manager()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
