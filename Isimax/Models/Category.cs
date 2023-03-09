using System.ComponentModel.DataAnnotations;

namespace Isimax.Models
{
    public class Category
    {
        // Category Id
        [Key]
        public int Id { get; set; }
        // Category name
        public string Name { get; set; }
        public string Image { get; set; }
        public int NumberOfProducts { get; set; }

    }
}
