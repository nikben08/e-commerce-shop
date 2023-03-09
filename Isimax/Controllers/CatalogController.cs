using Isimax.Data;
using Isimax.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Isimax.Controllers
{
    public class CatalogController : Controller
    {

        private ApplicationDbContext Context { get; }
        public CatalogController(ApplicationDbContext _context)
        {
            this.Context = _context;
        }


        public class CatalogIndexResponse
        {
            public List<Product> Products { get; set; }
            public List<Category> Categories { get; set; }
        }

        public List<Product> ProductsListPerPage(List<Product> products, int page)
        {
            if (products.Count >= (page - 1) * 15 + 15)
            {
                products = products.GetRange((page - 1) * 15, 15);
                ViewBag.lastPage = false;
            }
            else
            {
                ViewBag.lastPage = true;
                products = products.GetRange((page - 1) * 15, products.Count() - (page - 1) * 15);
            }

            return products;
        }

        [HttpGet]
        public ActionResult Index(int CategoryId, int Sort, int Page)
        {
            int page = Page;

            if (Page == 0)
            {
                page = 1;
            }

            if (page == 1)
            {
                ViewBag.firstPage = true;
            }
            else
            {
                ViewBag.firstPage = false;
            }

            if (CategoryId > 0)
            {
                ViewBag.categoryName = this.Context.Categories.Where(c => c.Id == CategoryId).ToList().First().Name;
            }
            else
            {
                ViewBag.categoryName = null;
            }


            List<Category> categories = this.Context.Categories.Select(c => c).ToList();
            if (CategoryId > 0)
            {
                List<Product> products = this.Context.Products.Where(c => c.CategoryId == CategoryId).ToList();
                if (Sort == 2)
                {
                    products = this.Context.Products.Where(c => c.CategoryId == CategoryId).OrderBy(c => c.Price).ToList();
                }
                else if (Sort == 3)
                {
                    products = this.Context.Products.Where(c => c.CategoryId == CategoryId).OrderByDescending(c => c.Price).ToList();
                }
                else if (Sort == 4)
                {
                    products = this.Context.Products.Where(c => c.CategoryId == CategoryId).OrderByDescending(c => c.DateTime).ToList();
                }

                return View("Views/Catalog/index.cshtml", new CatalogIndexResponse { Products = ProductsListPerPage(products, page), Categories = categories });
            }
            else
            {
                List<Product> products = this.Context.Products.Select(c => c).ToList();
                if (Sort == 2)
                {
                    products = this.Context.Products.Select(c => c).OrderBy(c => c.Price).ToList();
                }
                else if (Sort == 3)
                {
                    products = this.Context.Products.Select(c => c).OrderByDescending(c => c.Price).ToList();
                }
                else if (Sort == 4)
                {
                    products = this.Context.Products.Select(c => c).OrderByDescending(c => c.DateTime).ToList();
                }
                return View("Views/Catalog/index.cshtml", new CatalogIndexResponse { Products = ProductsListPerPage(products, page), Categories = categories });
            }

        }

        List<Product> GetProductsByCategory(int categoryId, Product product, List<string> recommProducts)
        {
            List<Product> products = this.Context
                                         .Products
                                         .Where(c => c.CategoryId == categoryId)
                                         .ToList();

            for (int i = 0; i < recommProducts.Count(); i++)
            {
                Product recommProduct = this.Context
                                            .Products
                                            .Where(p => p.Id.ToString() == recommProducts[i])
                                            .First();

                if (products.Contains(recommProduct))
                {
                    products.Remove(recommProduct);
                }
            }

            products.Remove(product);
            return products;
        }


        public Product FillMissingRecommendedProducts(Product product)
        {

            string productsId = product.RecommendedProducts.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            string[] recommProductsArray = productsId.Split(',');

            List<string> recommProducts = recommProductsArray.ToList();

            if (recommProducts[0] == null || recommProducts[0].Length == 0)
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
                        List<Product> products = GetProductsByCategory(categories[i].Id, product, recommProducts);
                        if (recommProductsNum < 4)
                        {
                            for (int k = 0; k < products.Count(); k++)
                            {
                                if (recommProductsNum < 4)
                                {
                                    Console.WriteLine(products[k].Id.ToString());
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
            this.Context.SaveChanges();
            return oldProduct;
        }


        public class DetailViewResponse
        {
            public Product product { get; set; }
            public string productCategoryName { get; set; }
            public List<Product> recommendedProducts { get; set; }
            public List<string> productParametersName { get; set; }
            public List<string> productParametersValue { get; set; }
        };

        public List<string> ConvertStringToList(string arrayString)
        {
            string replacedArrString = arrayString.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            List<string> list = replacedArrString.Split(',').ToList();
            return list;
        }

        [HttpGet]
        public IActionResult DetailView(int ProductId)
        {
            Product product = FillMissingRecommendedProducts(this.Context.Products.Where(c => c.Id == ProductId).First());
            string productsId = product.RecommendedProducts.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            string[] recommProductIds = productsId.Split(',');

            List<string> productParametersName = ConvertStringToList(product.ProductParameterName);
            List<string> productParametersValue = ConvertStringToList(product.ProductParameterValue);

            string productCategoryName = this.Context
                                             .Categories
                                             .Where(c => c.Id == product.CategoryId)
                                             .FirstOrDefault()
                                             .Name;

            List<Product> recommendedProducts = new List<Product>();

            if (recommProductIds[0] != "null" && recommProductIds[0].Length > 0)
            {
                for (int i = 0; i < recommProductIds.Length; i++)
                {
                    Product recommendedProduct = this.Context.Products.Where(c => c.Id.ToString() == recommProductIds[i]).FirstOrDefault();
                    recommendedProducts.Add(recommendedProduct);
                }
            }
            else
            {
                recommProductIds = new string[] { };
            }
            return View("Views/Catalog/DetailView.cshtml",
                new DetailViewResponse
                {
                    product = product,
                    productCategoryName = productCategoryName,
                    recommendedProducts = recommendedProducts,
                    productParametersName = productParametersName,
                    productParametersValue = productParametersValue
                });
        }

    }
}
