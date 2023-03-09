using Microsoft.AspNetCore.Mvc;
using Isimax.Models;
using Isimax.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Web.Helpers;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Isimax.Controllers
{
    public class AdminController : Controller
    {

        private ApplicationDbContext Context { get; }
        private readonly IWebHostEnvironment _hostEnvironment;
        public AdminController(ApplicationDbContext _context, IWebHostEnvironment hostEnvironment)
        {
            this._hostEnvironment = hostEnvironment;
            this.Context = _context;
        }

        public class SerializerOptions {
            public JsonSerializerOptions options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)};
        }


        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                return View();
            }
        }


        public class LogInModelForm
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        public IActionResult LogIn(LogInModelForm model)
        {
            List<AdminUser> users = this.Context.AdminUsers.Where(c => c.Email == model.Email).ToList();
            if (users.Count() != 0)
            {
                if (Crypto.VerifyHashedPassword(users[0].Password, model.Password))
                {
                    Authenticate(model.Email);
                    string hostName = Dns.GetHostName();
                    string Ip = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    string date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")).ToString("M/dd/yyyy HH:mm:ss");
                    LoginHistory loginHistory = new LoginHistory
                    {
                        Email = model.Email,
                        Date = date,
                        Ip = Ip
                    };
                    this.Context.LoginHistory.Add(loginHistory);
                    this.Context.SaveChanges();
                    return RedirectToAction("Dashboard");
                }
            }
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View("Views/Admin/Index.cshtml");
        }

        private void Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(200) });
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Authorize]
        public IActionResult EditProductView()
        {
            List<Product> products = this.Context.Products.Select(a => a).ToList();
            return View(products);
        }

        [Authorize]
        public IActionResult EditCategoryView()
        {
            List<Category> categories = this.Context.Categories.Select(a => a).ToList();
            return View(categories);
        }

        [Authorize]
        public IActionResult EditContactFormsView()
        {
            List<ContactForm> contactForms = this.Context.ContactForms.Select(a => a).OrderByDescending(a => a).ToList();
            return View(contactForms);
        }

        [Authorize]
        public IActionResult EditQualityCertificateView()
        {
            List<QualityCertificate> qualityCertificates = this.Context.QualityCertificates.Select(a => a).ToList();
            return View(qualityCertificates);
        }

        public class EditProductDetailResponse
        {
            public Product product { get; set; }
            public string[] recommProducts { get; set; }
            public List<Category> categories { get; set; }
            public List<string> ProductParameterName { get; set; }
            public List<string> ProductParameterValue { get; set; }
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditProductDetail(int productId)
        {
            List<Category> categories = this.Context.Categories.Select(a => a).ToList();
            Product product = this.Context.Products.Where(c => c.Id == productId).First();
            Category productCategory = this.Context.Categories.Where(c => c.Id == product.CategoryId).ToList().First();
            categories.Remove(productCategory);
            categories.Add(productCategory);
            categories.Reverse();

            string productsId = product.RecommendedProducts.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            string[] recommProducts = productsId.Split(',');
            List<Product> products = new List<Product>();
            List<string> productParameterName = ConvertStringToList(product.ProductParameterName);
            List<string> productParameterValue = ConvertStringToList(product.ProductParameterValue);

            return View("Views/Admin/EditProductDetailView.cshtml",
                new EditProductDetailResponse 
                { 
                    product = product,
                    categories = categories,
                    recommProducts = recommProducts,
                    ProductParameterName = productParameterName,
                    ProductParameterValue = productParameterValue
                });
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditCategoryDetail(int categoryId)
        {
            Category category = new Category();
            foreach (var row in this.Context.Categories)
                if (row.Id == categoryId)
                {
                    category.Id = row.Id;
                    category.Name = row.Name;
                    category.Image = row.Image;
                }

            return View("Views/Admin/EditCategoryDetailView.cshtml", category);
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditUserDetail(int userId)
        {
            AdminUser adminUser = this.Context.AdminUsers.Where(a => a.Id == userId).First();
            return View("Views/Admin/EditUserDetailView.cshtml", adminUser);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ViewContactFormDetail(int contactFormId)
        {
            ContactForm contactForm = this.Context.ContactForms.Where(a => a.Id == contactFormId).First();
            return View("Views/Admin/ContactFormDetailView.cshtml", contactForm);
        }

        [Authorize]
        public IActionResult CreateProductView()
        {
            List<Category> categories = this.Context.Categories.Select(a => a).ToList();
            return View(categories);
        }

        [Authorize]
        public IActionResult CreateCategoryView()
        {
            return View();
        }


        [Authorize]
        public IActionResult CreateQualityCertificateView()
        {
            return View();
        }

        public class HomePageData
        {
            public int Id { get; set; }
            public string SliderImage1 { get; set; }
            public string SliderImage2 { get; set; }
            public string SliderImage3 { get; set; }
            public string Category1 { get; set; }
            public string Category2 { get; set; }
            public string Category3 { get; set; }
            public string Category4 { get; set; }
            public string[] Products { get; set; }
            public string BannerTitle { get; set; }
            public string BannerText { get; set; }
            public string BannerImage { get; set; }
        }


        public void DeleteOldImage(string ImagePath)
        {
            FileInfo file = new FileInfo(ImagePath);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        [Authorize]
        public IActionResult EditHomePageView()
        {
            if (this.Context.HomePage.Select(c => c).ToList().Count() > 0)
            {
                HomePage homePage = this.Context.HomePage.Select(c => c).ToList().Last();
                string productsId = homePage.Products.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
                string[] result = productsId.Split(',');
                List<Product> products = new List<Product>();

                HomePageData homePageData = new HomePageData
                {
                    Id = homePage.Id,
                    SliderImage1 = homePage.SliderImage1,
                    SliderImage2 = homePage.SliderImage2,
                    SliderImage3 = homePage.SliderImage3,
                    Category1 = homePage.Category1,
                    Category2 = homePage.Category2,
                    Category3 = homePage.Category3,
                    Category4 = homePage.Category4,
                    Products = result,
                    BannerTitle = homePage.BannerTitle,
                    BannerText = homePage.BannerText,
                    BannerImage = homePage.BannerImage,
                };

                return View(homePageData);
            }
            else
            {
                string[] products = { };
                HomePageData homePageData = new HomePageData
                {
                    SliderImage1 = "",
                    SliderImage2 = "",
                    SliderImage3 = "",
                    Category1 = "",
                    Category2 = "",
                    Category3 = "",
                    Category4 = "",
                    Products = products,
                    BannerTitle = "",
                    BannerText = "",
                    BannerImage = "",
                };

                HomePage homePage = new HomePage
                {
                    Category1 = "",
                    Category2 = "",
                    Category3 = "",
                    Category4 = "",
                    Products = "",
                    BannerTitle = "",
                    BannerText = "",
                    BannerImage = ""
                };

                this.Context.HomePage.Add(homePage);
                this.Context.SaveChanges();
                return View(homePageData);
            }
        }


        [Authorize]
        public IActionResult EditAboutUsPageView()
        {
            if (this.Context.AboutUsPage.Count() > 0)
            {
                return View(this.Context.AboutUsPage.Select(c => c).ToList().First());
            }
            else
            {
                AboutUsPage aboutUsPage = new AboutUsPage
                {
                    BannerImage = "",
                    Text = "",
                    MissionText = "",
                    VisionText = "",
                    ValuesText = ""
                };
                this.Context.AboutUsPage.Add(aboutUsPage);
                this.Context.SaveChanges();
                return View(aboutUsPage);
            }
        }

        public class CreateUserForm
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreateUser(CreateUserForm model)
        {
            AdminUser adminUser = new AdminUser
            {
                Email = model.Email,
                Password = Crypto.HashPassword(model.Password)
            };

            this.Context.AdminUsers.Add(adminUser);
            this.Context.SaveChanges();
            return View("Views/Admin/Dashboard.cshtml");
        }

        [Authorize]
        public IActionResult CreateUserView()
        {
            return View();
        }

        [Authorize]
        public IActionResult EditUserView()
        {
            List<AdminUser> adminUsers = this.Context.AdminUsers.Select(a => a).ToList();
            return View(adminUsers);
        }

        [Authorize]
        public IActionResult LoginHistoryView()
        {
            List<LoginHistory> loginHistories = this.Context.LoginHistory.Select(a => a).OrderByDescending(a => a).ToList();
            return View(loginHistories);
        }

        [Authorize]
        [HttpPost]
        public IActionResult SearchCategory(string query)
        {
            List<Category> categories = this.Context.Categories.Where(c => c.Name.Contains(query) || query.Contains(c.Id.ToString())).ToList();
            return View("Views/Admin/EditCategoryView.cshtml", categories);
        }

        [Authorize]
        [HttpPost]
        public IActionResult SearchProduct(string query)
        {
            List<Product> products = this.Context.Products.Where(c => c.Title.Contains(query) || query.Contains(c.Code)).ToList();
            return View("Views/Admin/EditProductView.cshtml", products);
        }

        [Authorize]
        [HttpPost]
        public IActionResult SearchUser(string query)
        {
            List<AdminUser> adminUsers = this.Context.AdminUsers.Where(c => c.Email.Contains(query) || query.Contains(c.Id.ToString())).ToList();
            return View("Views/Admin/EditUserView.cshtml", adminUsers);
        }

        [Authorize]
        [HttpPost]
        public IActionResult SearchContactForm(string query)
        {
            List<ContactForm> contactForms = this.Context.ContactForms.Where(c => c.Email.Contains(query) || c.Name.Contains(query) || query.Contains(c.Id.ToString()) || c.Msg.Contains(query)).ToList();
            return View("Views/Admin/EditContactFormsView.cshtml", contactForms);
        }

        [Authorize]
        [HttpPost]
        public IActionResult SearchQualityCertificate(string query)
        {
            List<QualityCertificate> qualityCertificates = this.Context.QualityCertificates.Where(c => query.Contains(c.Id.ToString())).ToList();
            return View("Views/Admin/EditQualityCertificateView.cshtml", qualityCertificates);
        }

        string GenerateImagePath(string fileName)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string path = Path.Combine(wwwRootPath + "/uploads/", fileName);
            return path;
        }


        string GenerateImageUniqName(string fileName)
        {
            string newFileName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            newFileName = newFileName + DateTime.Now.ToString("yymmssfff") + extension;
            return newFileName;
        }

        public class EditHomePageForm
        {
            public IFormFile SliderImage1 { get; set; }
            public IFormFile SliderImage2 { get; set; }
            public IFormFile SliderImage3 { get; set; }
            public List<string> HomePageCategory { get; set; }

            public List<string> HomePageProduct { get; set; }
            public string BannerTitle { get; set; }
            public string BannerText { get; set; }
            [NotMapped]
            [DisplayName("Upload File")]
            public IFormFile BannerImage { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditHomePageDataAsync(EditHomePageForm homePage)
        {
            HomePage oldHomePage = this.Context.HomePage.Select(h => h).ToList().First();

            if (homePage.SliderImage1 != null)
            {
                string ImageUniqName = "sliderImage1.jpg";
                string ImagePath = GenerateImagePath(ImageUniqName);
                DeleteOldImage(ImagePath);
                using (var fileStream = new FileStream(ImagePath, FileMode.Create))
                {
                    await homePage.SliderImage1.CopyToAsync(fileStream);
                }
                Console.WriteLine(ImagePath);
                oldHomePage.SliderImage1 = ImageUniqName;
            }

            if (homePage.SliderImage2 != null)
            {
                string ImageUniqName = "sliderImage2.jpg";
                string ImagePath = GenerateImagePath(ImageUniqName);
                DeleteOldImage(ImagePath);
                using (var fileStream = new FileStream(ImagePath, FileMode.Create))
                {
                    await homePage.SliderImage2.CopyToAsync(fileStream);
                }
                oldHomePage.SliderImage2 = ImageUniqName;
            }

            if (homePage.SliderImage3 != null)
            {
                string ImageUniqName = "sliderImage3.jpg";
                string ImagePath = GenerateImagePath(ImageUniqName);
                DeleteOldImage(ImagePath);
                using (var fileStream = new FileStream(ImagePath, FileMode.Create))
                {
                    await homePage.SliderImage3.CopyToAsync(fileStream);
                }
                oldHomePage.SliderImage3 = ImageUniqName;
            }

            if (homePage.BannerImage != null)
            {
                string bannerUniqName = GenerateImageUniqName(homePage.BannerImage.FileName);
                string bannerImagePath = GenerateImagePath(bannerUniqName);
                DeleteOldImage(bannerImagePath);
                using (var fileStream = new FileStream(bannerImagePath, FileMode.Create))
                {
                    await homePage.BannerImage.CopyToAsync(fileStream);
                }
                oldHomePage.BannerImage = bannerUniqName;
            }


            oldHomePage.Category1 = homePage.HomePageCategory[0];
            oldHomePage.Category2 = homePage.HomePageCategory[1];
            oldHomePage.Category3 = homePage.HomePageCategory[2];
            oldHomePage.Category4 = homePage.HomePageCategory[3];
            oldHomePage.Products = JsonSerializer.Serialize(homePage.HomePageProduct);
            oldHomePage.BannerTitle = homePage.BannerTitle;
            oldHomePage.BannerText = homePage.BannerText;

            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }


        public class EditAboutUsPageForm
        {
            [NotMapped]
            [DisplayName("Upload File")]
            public IFormFile BannerImage { get; set; }
            public string Text { get; set; }
            public string MissionText { get; set; }
            public string VisionText { get; set; }
            public string ValuesText { get; set; }
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditAboutUsPageDataAsync(EditAboutUsPageForm form)
        {
            AboutUsPage aboutUsPage = this.Context.AboutUsPage.Select(a => a).ToList().First();
            if (form.BannerImage != null)
            {
                string bannerUniqName = GenerateImageUniqName(form.BannerImage.FileName);
                string bannerImagePath = GenerateImagePath(bannerUniqName);

                DeleteOldImage(GenerateImagePath(aboutUsPage.BannerImage));

                using (var fileStream = new FileStream(bannerImagePath, FileMode.Create))
                {
                    await form.BannerImage.CopyToAsync(fileStream);
                }
                aboutUsPage.BannerImage = bannerUniqName;
            }

            if (form.Text != null)
            {
                aboutUsPage.Text = form.Text;
            }

            if (form.MissionText != null)
            {
                aboutUsPage.MissionText = form.MissionText;
            }

            if (form.VisionText != null)
            {
                aboutUsPage.VisionText = form.VisionText;
            }

            if (form.ValuesText != null)
            {
                aboutUsPage.ValuesText = form.ValuesText;
            }

            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }

        public class ProductForm
        {
            public string Code { get; set; }
            public string Title { get; set; }
            public string Price { get; set; }
            public string Desc { get; set; }

            [NotMapped]
            [DisplayName("Upload File")]
            public IFormFile Image { get; set; }
            public int CategoryId { get; set; }
            public List<string> ProductParameterName { get; set; }
            public List<string> ProductParameterValue { get; set; }
            public List<string> RecommendedProduct { get; set; }
        }


        List<Product> GetProductsByCategory(int categoryId)
        {
            List<Product> products = this.Context
                                         .Products
                                         .Where(c => c.CategoryId == categoryId)
                                         .ToList();

            return products;
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProductAsync(ProductForm product)
        {
            string imageUniqName = GenerateImageUniqName(product.Image.FileName);
            string productimagePath = GenerateImagePath(imageUniqName);

            using (var fileStream = new FileStream(productimagePath, FileMode.Create))
            {
                await product.Image.CopyToAsync(fileStream);
            }

            if (product.RecommendedProduct[0] == null)
            {
                product.RecommendedProduct = new List<string>();
            }

            int recommProductsNum = product.RecommendedProduct.Count();
            if (recommProductsNum < 4)
            {
                List<Category> categories = this.Context.Categories.Select(c => c).ToList();
                Category category = this.Context.Categories.Where(c => c.Id == product.CategoryId).ToList().First();
                categories.Remove(category);
                categories.Add(category);
                categories.Reverse();
                for (int i = 0; i < categories.Count(); i++)
                {
                    if (recommProductsNum < 4)
                    {
                        List<Product> products = GetProductsByCategory(categories[i].Id);
                        if (recommProductsNum < 4)
                        {
                            for (int k = 0; k < products.Count(); k++)
                            {
                                if (recommProductsNum < 4)
                                {
                                    product.RecommendedProduct.Add(products[k].Id.ToString());
                                    recommProductsNum++;
                                }

                            }
                        }
                    }
                }
                if (recommProductsNum == 0)
                {
                    product.RecommendedProduct = new List<string>();
                }
                recommProductsNum = 4;
            }

            bool priceIsVisible = false;
            List<Product> pdts = this.Context.Products.Select(c => c).ToList();
            if (pdts.Count() > 0)
            {
                priceIsVisible = pdts[0].PriceIsVisible;
            }
            else
            {
                priceIsVisible = true;
            }

            Product newProduct = new Product
            {
                Code = product.Code,
                Title = product.Title,
                Price = product.Price,
                Desc = product.Desc,
                Image = imageUniqName,
                CategoryName = this.Context
                                   .Categories
                                   .Where(c => c.Id == product.CategoryId)
                                   .ToList()
                                   .First()
                                   .Name,

                CategoryId = product.CategoryId,
                ProductParameterName = JsonSerializer.Serialize(product.ProductParameterName, new SerializerOptions().options),
                ProductParameterValue = JsonSerializer.Serialize(product.ProductParameterValue, new SerializerOptions().options),
                RecommendedProducts = JsonSerializer.Serialize(product.RecommendedProduct),
                DateTime = DateTime.Now,
                PriceIsVisible = priceIsVisible
            };

            foreach (var row in this.Context.Categories)
            {
                if (row.Id == product.CategoryId)
                {
                    row.NumberOfProducts += 1;
                }
            }

            this.Context.Products.Add(newProduct);
            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }

        [Authorize]
        [HttpPost]
        public IActionResult DeleteAllProducts()
        {
            List<Product> products = this.Context.Products.Select(a => a).ToList();
            for (int i = 0; i < products.Count(); i++)
            {
                this.Context.Products.Remove(products[i]);
            }
            this.Context.SaveChanges();

            products = new List<Product>();
            return View("Views/Admin/EditProductView.cshtml", products);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ShowAllProductsPrice()
        {
            foreach (var row in this.Context.Products)
            {
                row.PriceIsVisible = true;
            }

            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }


        [Authorize]
        [HttpPost]
        public IActionResult HideAllProductsPrice()
        {
            foreach (var row in this.Context.Products)
            {
                row.PriceIsVisible = false;
            }

            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }

        public class CategoryForm
        {
            public string Name { get; set; }
            [NotMapped]
            [DisplayName("Upload File")]
            public IFormFile Image { get; set; }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCategoryAsync(CategoryForm category)
        {
            string imageUniqName = GenerateImageUniqName(category.Image.FileName);
            string imagePath = GenerateImagePath(imageUniqName);

            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await category.Image.CopyToAsync(fileStream);
            }

            Category newCategory = new Category
            {
                Name = category.Name,
                Image = imageUniqName
            };


            this.Context.Categories.Add(newCategory);
            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }


        public class QualityCertificateForm
        {
            [NotMapped]
            [DisplayName("Upload File")]
            public IFormFile Image { get; set; }
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateQualityCertificateAsync(QualityCertificateForm form)
        {
            string imageUniqName = GenerateImageUniqName(form.Image.FileName);
            string imagePath = GenerateImagePath(imageUniqName);

            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await form.Image.CopyToAsync(fileStream);
            }

            QualityCertificate qualityCertificate = new QualityCertificate
            {
                Image = imageUniqName
            };


            this.Context.QualityCertificates.Add(qualityCertificate);
            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }

        public class UpdateProductForm
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Price { get; set; }
            public string Title { get; set; }
            public string Desc { get; set; }
            public IFormFile Image { get; set; }
            public int CategoryId { get; set; }
            public List<string> ProductParameterName { get; set; }
            public List<string> ProductParameterValue { get; set; }
            public List<string> RecommendedProduct { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(UpdateProductForm newProduct)
        {
            Product oldProduct = this.Context.Products.Where(c => c.Id == newProduct.Id).First();

            if (newProduct.Image != null)
            {
                string oldProductImagePath = GenerateImagePath(oldProduct.Image);

                DeleteOldImage(oldProductImagePath);

                string newProductimageUniqName = GenerateImageUniqName(newProduct.Image.FileName);
                string newProductimagePath = GenerateImagePath(newProductimageUniqName);

                using (var fileStream = new FileStream(newProductimagePath, FileMode.Create))
                {
                    await newProduct.Image.CopyToAsync(fileStream);
                }

                oldProduct.Image = newProductimageUniqName;
            }

            for(int i = 0; i < newProduct.ProductParameterName.Count(); i++)
            {
                newProduct.ProductParameterName[i] = newProduct.ProductParameterName[i].Trim();
            }

            List<Category> categories = this.Context.Categories.Where(c => c.Id == newProduct.CategoryId).ToList();

            oldProduct.Code = newProduct.Code;
            oldProduct.Title = newProduct.Title;
            oldProduct.Price = newProduct.Price;
            oldProduct.Desc = newProduct.Desc;
            oldProduct.CategoryName = categories[0].Name;
            oldProduct.CategoryId = newProduct.CategoryId;
            oldProduct.ProductParameterName = JsonSerializer.Serialize(newProduct.ProductParameterName, new SerializerOptions().options);
            oldProduct.ProductParameterValue = JsonSerializer.Serialize(newProduct.ProductParameterValue, new SerializerOptions().options);
            oldProduct.RecommendedProducts = JsonSerializer.Serialize(newProduct.RecommendedProduct);
            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }


        public class UpdateCategoryForm
        {
            public int Id { get; set; }
            public string Name { get; set; }
            [NotMapped]
            [DisplayName("Upload File")]
            public IFormFile Image { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryForm newCategory)
        {
            Category oldCategory = this.Context.Categories.Where(c => c.Id == newCategory.Id).First();
            string oldCategoryImagePath = GenerateImagePath(oldCategory.Image);
            if (newCategory.Image != null)
            {
                DeleteOldImage(oldCategoryImagePath);

                string newCategoryimageUniqName = GenerateImageUniqName(newCategory.Image.FileName);
                string newCategoryimagePath = GenerateImagePath(newCategoryimageUniqName);


                using (var fileStream = new FileStream(newCategoryimagePath, FileMode.Create))
                {
                    await newCategory.Image.CopyToAsync(fileStream);
                }

                oldCategory.Image = newCategoryimageUniqName;
            }

            oldCategory.Name = newCategory.Name;


            this.Context.SaveChanges();

            return View("Views/Admin/Dashboard.cshtml");
        }

        public class UpdateUserForm
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUser(UpdateUserForm newAdminUser)
        {
            AdminUser oldUser = this.Context
                                    .AdminUsers
                                    .Where(c => c.Id == newAdminUser.Id)
                                    .First();

            oldUser.Email = newAdminUser.Email;
            oldUser.Password = Crypto.HashPassword(newAdminUser.Password);
            this.Context.SaveChanges();
            return View("Views/Admin/Dashboard.cshtml");
        }


        public List<string> ConvertStringToList(string arrayString)
        {
            string replacedArrString = arrayString.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            List<string> list = replacedArrString.Split(',').ToList();
            return list;
        }

        public void DeleteHomePageCategories(int categoryId)
        {
            HomePage homePage = this.Context.HomePage.Select(h => h).First();
            if (homePage.Category1 == categoryId.ToString())
            {
                homePage.Category1 = "";
            }

            if (homePage.Category2 == categoryId.ToString())
            {
                homePage.Category2 = "";
            }

            if (homePage.Category3 == categoryId.ToString())
            {
                homePage.Category3 = "";
            }

            if (homePage.Category4 == categoryId.ToString())
            {
                homePage.Category4 = "";
            }

            this.Context.SaveChanges();
        }

        public void DeleteHomePageProducts(Product product)
        {
            HomePage homePage = this.Context.HomePage.Select(h => h).First();
            List<string> recommProducts = ConvertStringToList(homePage.Products);
            for (int k = 0; k < recommProducts.Count(); k++)
            {
                if (recommProducts[k] == product.Id.ToString())
                {
                    recommProducts.RemoveAt(k);
                }
            }
            homePage.Products = String.Join(",", recommProducts.ToArray());
            this.Context.SaveChanges();
        }


        public void DeleteRecommProducts(Product product)
        {
            List<Product> products = this.Context.Products.Select(p => p).ToList();
            for (int i = 0; i < products.Count(); i++)
            {
                List<string> recommProducts = ConvertStringToList(products[i].RecommendedProducts);
                for (int k = 0; k < recommProducts.Count(); k++)
                {
                    if (recommProducts[k] == product.Id.ToString())
                    {
                        recommProducts.RemoveAt(k);
                    }
                }
                products[i].RecommendedProducts = String.Join(",", recommProducts.ToArray());
                this.Context.SaveChanges();
            }
        }


        public void DeleteProducts(int CategoryId)
        {
            List<Product> categoryProducts = this.Context.Products.Where(p => p.CategoryId == CategoryId).ToList();
            for (int i = 0; i < categoryProducts.Count(); i++)
            {
                Product product = categoryProducts[i];
                DeleteRecommProducts(product);
                DeleteHomePageProducts(product);
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string path = wwwRootPath + "/uploads/" + product.Image;
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                }
                this.Context.Products.Remove(product);
            }
            this.Context.SaveChanges();
        }


        [Authorize]
        [HttpPost]
        public ActionResult DeleteCategory(int categoryId)
        {
            Category deletedCategory = this.Context
                                           .Categories
                                           .Where(c => c.Id == categoryId)
                                           .First();

            string wwwRootPath = _hostEnvironment.WebRootPath;
            string path = wwwRootPath + "/uploads/" + deletedCategory.Image;
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
            }

            this.Context.Categories.Remove(deletedCategory);
            this.Context.SaveChanges();
            DeleteHomePageCategories(categoryId);
            DeleteProducts(categoryId);

            return Json("success");
        }


        [Authorize]
        [HttpPost]
        public ActionResult DeleteProduct(int productId)
        {
            Product deletedProduct = this.Context
                                         .Products
                                         .Where(c => c.Id == productId)
                                         .First();

            string wwwRootPath = _hostEnvironment.WebRootPath;
            string path = wwwRootPath + "/uploads/" + deletedProduct.Image;
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
            }

            this.Context.Products.Remove(deletedProduct);
            this.Context.SaveChanges();
            DeleteRecommProducts(deletedProduct);
            DeleteHomePageProducts(deletedProduct);
            return Json("success");
        }


        public void FillMissingRecommendedProducts(Product product)
        {
            string productsId = product.RecommendedProducts.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            string[] recommProductsArray = productsId.Split(',');
            List<string> recommProducts = recommProductsArray.ToList();

            if (recommProducts[0] == null)
            {
                recommProducts = new List<string>();
            }

            int recommProductsNum = recommProducts.Count();
            if (recommProductsNum < 4)
            {
                List<Category> categories = this.Context.Categories.Select(c => c).ToList();
                Category category = this.Context.Categories.Where(c => c.Id == product.CategoryId).ToList().First();
                categories.Remove(category);
                categories.Add(category);
                categories.Reverse();
                for (int i = 0; i < categories.Count(); i++)
                {
                    if (recommProductsNum < 4)
                    {
                        List<Product> products = GetProductsByCategory(categories[i].Id);
                        if (recommProductsNum < 4)
                        {
                            for (int k = 0; k < products.Count(); k++)
                            {
                                if (recommProductsNum < 4)
                                {
                                    recommProducts.Add(products[k].Id.ToString());
                                    recommProductsNum++;
                                }

                            }
                        }
                    }
                }
                if (recommProductsNum == 0)
                {
                    recommProducts = new List<string>();
                }
                recommProductsNum = 4;

            }
            Product oldProduct = this.Context.Products.Where(p => p.Id == product.Id).First();
            oldProduct.RecommendedProducts = JsonSerializer.Serialize(recommProducts);
        }


        [Authorize]
        [HttpPost]
        public ActionResult DeleteUser(int userId)
        {
            AdminUser deletedAdminUser = this.Context
                                             .AdminUsers
                                             .Where(c => c.Id == userId)
                                             .First();

            this.Context.AdminUsers.Remove(deletedAdminUser);
            this.Context.SaveChanges();
            return Json("success");
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteContactForm(int contactFormId)
        {
            ContactForm deletedContactForm = this.Context
                                                 .ContactForms
                                                 .Where(c => c.Id == contactFormId)
                                                 .First();

            this.Context.ContactForms.Remove(deletedContactForm);
            this.Context.SaveChanges();
            return Json("success");
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteQualityCertificate(int qualityCertificateId)
        {
            QualityCertificate deletedQualityCertificate = this.Context
                                                               .QualityCertificates
                                                               .Where(c => c.Id == qualityCertificateId)
                                                               .First();


            this.Context.QualityCertificates.Remove(deletedQualityCertificate);
            this.Context.SaveChanges();
            return Json("success");
        }


        [Authorize]
        [HttpPost]
        public IActionResult DeleteLoginHistory()
        {
            var last100 = this.Context
                              .LoginHistory
                              .Select(l => l)
                              .Take(100);

            this.Context.LoginHistory.RemoveRange(last100);
            this.Context.SaveChanges();
            List<LoginHistory> loginHistories = this.Context
                                                    .LoginHistory
                                                    .Select(l => l)
                                                    .OrderByDescending(l => l)
                                                    .ToList();

            return View("Views/Admin/LoginHistoryView.cshtml", loginHistories);
        }

    }
}
