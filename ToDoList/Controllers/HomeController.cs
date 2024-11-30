using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using ToDoList.Dtos;
using ToDoList.Models;
using Microsoft.AspNetCore.Identity;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ToDoListContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

        public HomeController(ILogger<HomeController> logger, ToDoListContext toDoListContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = toDoListContext;
            _httpContextAccessor = httpContextAccessor;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [AllowAnonymous] // 不需要身分驗證時加
        public IActionResult Index()
        {
            var Claim = _httpContextAccessor.HttpContext.User.Claims.ToList();
            
            if (Claim.Count == 0) 
            {
                return View();
            }
            else 
            {
                var userId = int.Parse(Claim.Where(x => x.Type == "UserId").First().Value);
                var userStatus = Claim.Where(x => x.Type == "UserStatus").First().Value;
                var userName = Claim.Where(x => x.Type == "UserName").First().Value;
                UserDto user = new UserDto()
                {
                    Id = userId,
                    Status = userStatus,
                    Name = userName
                };

                return View(user);

            }
            
        }
        
        public IActionResult ListCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ListCreate(ListDto list)
        {
            if (ModelState.IsValid)
            {
                List insert = new List()
                {
                    uid = 1,
                    title = list.Title,
                    word = list.Word,
                };
                _context.List.Add(insert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List));
            }
            return View(list);
        }

        // GET: Home/List/5
        public async Task<IActionResult> List()
        {
            var Claim = _httpContextAccessor.HttpContext.User.Claims.ToList();
            var userId = int.Parse(Claim.Where(x => x.Type == "UserId").First().Value);
            var userStatus = Claim.Where(x => x.Type == "UserStatus").First().Value;

            List<ListDto> result;
            if (userStatus == "A") // 管理員身分
            {
                result = await (from list in _context.List
                                join user in _context.User on list.uid equals user.id
                                select new ListDto
                                {
                                    Id = list.id,
                                    Uid = list.uid,
                                    Title = list.title,
                                    Word = list.word,
                                    UserName = user.name 
                                }).ToListAsync();
            }
            else if (userStatus == "B") // 一班使用者
            {
                result = await (from a in _context.List
                                where a.uid == userId
                                select new ListDto
                                {
                                    Id = a.id,
                                    Uid = a.uid,
                                    Title = a.title,
                                    Word = a.word,
                                }).ToListAsync();
            }
            else
            {
                return BadRequest("無效的使用者狀態");
            }

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(UserCreateDto user) 
        {
            if (ModelState.IsValid) 
            {
                //檢查帳號是否重複
                var isUserExists = _context.User.Any(x => x.name == user.Name);
                if (isUserExists) 
                {
                    return RedirectToAction(nameof(Create));
                }
                var hashedPassword = _passwordHasher.HashPassword(null, user.Password); // 加密
                User insert = new User()
                {
                    name= user.Name,
                    password = hashedPassword,
                    status = "B",//A為管理員,B為一般使用者
                };
                _context.User.Add(insert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Home/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await (from a in _context.List
                                where a.id == id
                                select new ListDto
                                {
                                    Uid = a.uid,
                                    Title = a.title,
                                    Word = a.word,
                                }).SingleOrDefaultAsync();
            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ListDto list)
        {
            if (id != list.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var update = _context.List.Find(id);
                if (update != null) 
                {
                    update.title = list.Title;
                    update.word = list.Word;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(List));
                }
            }
            return View(list);
        }

        // GET: Home/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await (from a in _context.List
                                where a.id == id
                                select new ListDto
                                {
                                    Uid = a.uid,
                                    Title = a.title,
                                    Word = a.word,
                                }).SingleOrDefaultAsync();
            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var list = await _context.List.FindAsync(id);
            if (list != null)
            {
                _context.List.Remove(list);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }


        private bool ListExists(int? id)
        {
            return _context.List.Any(e => e.id == id);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(UserDto user)
        {
            if (ModelState.IsValid)
            {
                var result =  (from a in _context.User
                                    where a.name == user.Name 
                                    select a).SingleOrDefault();


                if (result == null)
                {
                    return RedirectToAction(nameof(Index));

                }

                var valid = _passwordHasher.VerifyHashedPassword(null,result.password,user.Password);
                if (valid == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,result.name),//帳號
                        new Claim("UserName",result.name),//帳號
                        new Claim("UserStatus",result.status),//身分
                        new Claim("UserId",result.id.ToString()),//會員ID


                        //new Claim(ClaimTypes.Role,"Admin")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction(nameof(List));

                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Logout() 
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public string noLogin() 
        {
            return "未登入";
        }

        
    }
}
