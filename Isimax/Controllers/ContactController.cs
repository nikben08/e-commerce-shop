using Isimax.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Isimax.Models;
using System;
using System.Net.Mail;
using System.Net;

namespace Isimax.Controllers
{
    public class ContactController : Controller
    {

        private ApplicationDbContext Context { get; }
        public ContactController(ApplicationDbContext _context)
        {
            this.Context = _context;
        }

        public class ContactUsForm
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Tel { get; set; }
            public string Msg { get; set; }

        }


        [HttpPost]
        public IActionResult AddContactForm(ContactUsForm form)
        {
            ContactForm contact = new ContactForm
            {
                Name = form.Name,
                Email = form.Email,
                Tel = form.Tel,
                Msg = form.Msg,
                Date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")).ToString("M/dd/yyyy HH:mm:ss"),
        };

            this.Context.ContactForms.Add(contact);
            this.Context.SaveChanges();

            var fromAddress = new MailAddress("isimaxsmtp@gmail.com", "From Name");
            var toAddress = new MailAddress("info@isimaxaydinlatma.com", "To Name");
            string fromPassword = "ppesfdhlnkgawsin";
            string subject = "İletişim";
            string body = "Adı ve Soyadı: " + form.Name + "\nTelefon Numarası: " + form.Tel + "\nE-mail: " + form.Email + "\nMesaj: " + form.Msg;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }

            return View("/Views/Contact/SuccessContactView.cshtml");
        }

        public ActionResult Index()
        {
            return View();
        }

    }
}
