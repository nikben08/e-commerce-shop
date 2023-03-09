using Microsoft.AspNetCore.Mvc;
using Isimax.Data;
using Isimax.Models;
using System.Linq;
using System.Collections.Generic;

namespace Isimax.Controllers
{
    public class QualityCertificatesController : Controller
    {

        private ApplicationDbContext Context { get; }
        public QualityCertificatesController(ApplicationDbContext _context)
        {
            this.Context = _context;
        }

        public IActionResult Index()
        {
            List<QualityCertificate> qualityCertificates = this.Context.QualityCertificates.Select(c => c).ToList();
            return View(qualityCertificates);
        }
    }
}
