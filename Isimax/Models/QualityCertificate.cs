using System.ComponentModel.DataAnnotations;

namespace Isimax.Models
{
    public class QualityCertificate
    {
        [Key]
        public int Id { get; set; }
        public string Image { get; set; }
    }
}
