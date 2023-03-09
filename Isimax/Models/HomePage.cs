using System.ComponentModel.DataAnnotations;

namespace Isimax.Models
{
    public class HomePage
    {
        [Key]
        public int Id { get; set; }
        public string SliderImage1 { get; set; }
        public string SliderImage2 { get; set; }
        public string SliderImage3 { get; set; }
        public string Category1 { get; set; }
        public string Category2 { get; set; }
        public string Category3 { get; set; }
        public string Category4 { get; set; }   
        public string Products { get; set; }
        public string BannerTitle { get; set; }
        public string BannerText { get; set; }
        public string BannerImage { get; set; }
    }
}
