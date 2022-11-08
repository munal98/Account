using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Login.Filter;
using Login.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils.Messaging;

namespace Login.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private string code = null;

        public AccountController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("id").HasValue)
            {
                return Redirect("/Home/Index");
            }
            return View();
        }
        public IActionResult ForgotPassword()
        {
            if (HttpContext.Session.GetInt32("id").HasValue)
            {
                return Redirect("/Home/Index");
            }
            return View();
        }

        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetInt32("id").HasValue)
            {
                return Redirect("/Home/Index");
            }
            return View();
        }

        public IActionResult SendCode(string email)
        {
            var user = _context.User.FirstOrDefault(w => w.Email.Equals(email));
            if(user != null)
            {
                _context.Add(new PasswordCode { UserId=user.Id,Code=getCode()});
                _context.SaveChanges();
                string text = "<h1>Sıfırlama için kodunuz:</h1>"+ getCode()+" ";
                string subject = "Parola sıfırlama";
                MailMessage msg = new MailMessage("noreplytaskcore@gmail.com",email,subject,text);
                msg.IsBodyHtml = true;
                SmtpClient sc = new SmtpClient("smtp.gmail.com", 587);
                sc.UseDefaultCredentials = false;
                NetworkCredential cre = new NetworkCredential("noreplytaskcore@gmail.com","Taskcore2811");
                sc.Credentials = cre;
                sc.EnableSsl = true;
                sc.Send(msg);
                return Redirect("ResetPassword");

            }
            return Redirect("Index");
        }

        public IActionResult ResetPasswordCode(string code,string newPassword)
        {
            var passwordcode = _context.PasswordCode.FirstOrDefault(w => w.Code.Equals(code));
            if (passwordcode != null)
            {
                var user = _context.User.Find(passwordcode.UserId);
                user.Password = newPassword;
                _context.Update(user);
                _context.Remove(passwordcode);
                _context.SaveChanges();
                return Redirect("Index");
            }
            return Redirect("Index");

        }


        public IActionResult Login(string email,string pass)
        {
            var user = _context.User.FirstOrDefault(w => w.Email.Equals(email) && w.Password.Equals(pass));
            if (user != null) {
                HttpContext.Session.SetInt32("id",user.Id);
                HttpContext.Session.SetString("fullname",user.Name+" "+user.Surname);
                return Redirect("/Home/Index");
            }

            return Redirect("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("Index");
        }

        public IActionResult SignUp()
        {
            if (HttpContext.Session.GetInt32("id").HasValue)
            {
                return Redirect("/Home/Index");
            }
            return View();
        }

      
        public async Task<IActionResult> Register(User model)
        {
            await _context.AddAsync(model);
            await _context.SaveChangesAsync();

            return Redirect("Index");
        }
        
        public string getCode()
        {
            if(code == null)
            {
                Random rand = new Random();
                code = "";
                for(int i = 0; i < 6; i++)
                {
                    char tmp = Convert.ToChar(rand.Next(48, 58));
                    code += tmp;
                }

            }
            return code;
        }
       
       
    }
}