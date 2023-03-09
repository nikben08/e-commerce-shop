using Isimax.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Isimax.Models;
using System;
using System.Linq;

namespace Isimax.Controllers
{
    public class AboutUsController : Controller
    {

        private ApplicationDbContext Context { get; }
        public AboutUsController(ApplicationDbContext _context)
        {
            this.Context = _context;
        }

        public ActionResult Index()
        {
            AboutUsPage aboutUsPage = this.Context.AboutUsPage.Select(c => c).ToList().Last();
            return View(aboutUsPage);
        }

    }
}
