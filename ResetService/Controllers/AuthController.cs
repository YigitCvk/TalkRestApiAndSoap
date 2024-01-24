using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResetService.Models.Entity;
using ResetService.Models.ViewModels;
using ResetService.Services;

namespace RestService.Controllers
{
    public class AuthController : Controller
    {
        // private readonly IIdentityService _identityService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebSocketHandler _websocketHandler;
        public AuthController(UserManager<AppUser> userManager, ILogger<AuthController> logger, SignInManager<AppUser> signInManager, IWebSocketHandler websocketHandler)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _websocketHandler = websocketHandler;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new UserRegisterVM());
        }
        [HttpPost]
        public async Task<IActionResult> Index(UserRegisterVM p)
        {
            AppUser w = new AppUser()
            {
                FirstName = p.Name,
                LastName = p.Surname,
                UserName = p.UserName,
                Email = p.Mail,
                City = p.DefaultCity,
                TcNo = p.TcNo
            };

            if (p.Password == p.ConfirmPassword)
            {
                var result = await _userManager.CreateAsync(w);

                if (result.Succeeded)
                {
                    // Kullanıcı başarıyla oluşturuldu, şimdi WebSocketHandler kullanarak SOAP servisine veri gönderelim
                    // WebSocketHandler'ı DI ile alıyoruz
                    // Kullanıcı adını SOAP servisine iletebilirsiniz
                    var userName = w.UserName;

                    // SOAP servisi için kullanılacak mesajı oluşturun
                    var soapMessage = _websocketHandler.BuildSoapXmlContent(userName);

                    // WebSocketHandler ile SOAP servisine mesajı gönderin
                    await _websocketHandler.SendMessage(soapMessage);

                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item.Description);
                        _logger.LogInformation(item.Description);
                    }
                    _logger.LogInformation(" " + result);
                }
            }

            _logger.LogInformation("${result} ", p, ModelState.IsValid);
            return View(p);
        }
        [HttpGet]
        public async Task<IActionResult> Login(UserLoginVm p)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(p.Email);

                if (user != null)
                {
                    // Diğer kontroller...

                    var result = await _signInManager.PasswordSignInAsync(p.Email, p.Password, true, true);

                    if (result.Succeeded)
                    {
                        // Başarılı giriş işlemleri
                        await _userManager.ResetAccessFailedCountAsync(user);


                        // Kullanıcının "Admin" rolü var mı kontrol et
                        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                        if (isAdmin)
                        {
                            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Profile", new { area = "User" });
                        }
                    }
                    else
                    {
                        // Diğer kontroller...
                    }
                }
                else
                {

                    //_logger.LogInformation("Giriş Başarısız");
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                }
            }
            return View();
        }
    }

   
}
