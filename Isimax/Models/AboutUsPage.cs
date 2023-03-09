using System.ComponentModel.DataAnnotations;

namespace Isimax.Models
{
    public class AboutUsPage
    {
        [Key]
        public int Id { get; set; }
        public string BannerImage { get; set; }
        public string Text { get; set; }
        public string MissionText { get; set; }
        public string VisionText { get; set; }
        public string ValuesText { get; set; }
    }
}
