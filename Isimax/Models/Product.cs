using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Isimax.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }
        public string Desc { get; set; }
        public string Image { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string ProductParameterName { get; set; }
        public string ProductParameterValue { get; set; }
        public string RecommendedProducts { get; set; }
        public DateTime DateTime { get; set; }
        public bool PriceIsVisible { get; set; }
    }
}
