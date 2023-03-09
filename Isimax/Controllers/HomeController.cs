using Isimax.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Isimax.Data;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Isimax.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext Context { get; }
        public HomeController(ApplicationDbContext _context)
        {
            this.Context = _context;
        }

        public class HomePageData
        {
            public int Id { get; set; }
            public string SliderImage1 { get; set;}
            public string SliderImage2 { get; set;}
            public string SliderImage3 { get; set;}
            public Category Category1 { get; set; }
            public Category Category2 { get; set; }
            public Category Category3 { get; set; }
            public Category Category4 { get; set; }
            public List<Category> Categories { get; set; }
            public List<Product> Products { get; set; }
            public string BannerTitle { get; set; }
            public string BannerText { get; set; }
            public string BannerImage { get; set; }
        }

        public class MyClass
        {
            int value { get; set; }
        }
        public IActionResult Index()
        {
            List<HomePage> homePage = this.Context.HomePage.Select(c => c).ToList();
            string productsId = homePage.Last().Products.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            string[] result = productsId.Split(',');
            List<Product> products = new List<Product>();

            if (result[0] != "null" && result[0].Length > 0)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    Product product = this.Context.Products.Where(c => c.Id.ToString() == result[i]).First();
                    products.Add(product);
                }
            } 



            List<Category> categories = this.Context.Categories.Select(a => a).OrderByDescending(a => a.NumberOfProducts).ToList();
            if (categories.Count >= 5)
            {
                categories = categories.GetRange(0, 5);
            }
            else
            {
                categories = categories.GetRange(0, categories.Count());
            }

            if(homePage.First().Category1 == "")
            {
                homePage.First().Category1 = categories[0].Id.ToString();
            }

            if (homePage.First().Category2 == "")
            {
                homePage.First().Category2 = categories[0].Id.ToString();
            }

            if (homePage.First().Category3 == "")
            {
                homePage.First().Category3 = categories[0].Id.ToString();
            }

            if (homePage.First().Category4 == "")
            {
                homePage.First().Category4 = categories[0].Id.ToString();
            }

            HomePageData homePageData = new HomePageData
            {
                Id = homePage.Last().Id,
                SliderImage1 = homePage[0].SliderImage1,
                SliderImage2 = homePage[0].SliderImage2,
                SliderImage3 = homePage[0].SliderImage3,
                Category1 = this.Context.Categories.Where(c => c.Id.ToString() == homePage.First().Category1).ToList().FirstOrDefault(),
                Category2 = this.Context.Categories.Where(c => c.Id.ToString() == homePage.First().Category2).ToList().FirstOrDefault(),
                Category3 = this.Context.Categories.Where(c => c.Id.ToString() == homePage.First().Category3).ToList().FirstOrDefault(),
                Category4 = this.Context.Categories.Where(c => c.Id.ToString() == homePage.First().Category4).ToList().FirstOrDefault(),
                Products = products,
                Categories = categories,
                BannerTitle = homePage.First().BannerTitle,
                BannerText = homePage.First().BannerText,
                BannerImage = homePage.First().BannerImage,
            };
            return View(homePageData);
        }
    }
}