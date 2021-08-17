using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LoginReg.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace LoginReg.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;

        public HomeController(MyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid){
                if (_context.Users.Any(user => user.Email == newUser.Email))
                { 
                    ModelState.AddModelError("Email", "Email already in use!");

                    return View("Index", newUser);
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);

                    _context.Users.Add(newUser);
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                
            }
            return View("Index", newUser);
        }

        [HttpPost("checklogin")]
        public IActionResult CheckLogin(LoginUser login)
        {
            if(ModelState.IsValid){
                User userInDb = _context.Users.FirstOrDefault(user => user.Email == login.LoginEmail);
                
                if(userInDb == null){
                    ModelState.AddModelError("LoginEmail", "Invalid Login!");
                    
                    return View("Login", login);
                }

                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();

                var result = hasher.VerifyHashedPassword(login, userInDb.Password, login.LoginPassword);

                if (result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Login!");

                    return View("Login", login);
                }

                HttpContext.Session.SetInt32("userId", userInDb.UserId);
                return RedirectToAction("Success");
            }

            return View("Login", login);
        }

        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            return View("Login");
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            int? userID = HttpContext.Session.GetInt32("userId");
            if (userID > 0){
                return View();
            }

            return RedirectToAction("Index");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.SetInt32("userId", -1);

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
