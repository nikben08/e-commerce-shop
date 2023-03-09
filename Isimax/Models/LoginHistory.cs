using System.ComponentModel.DataAnnotations;

namespace Isimax.Models
{
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Date { get; set; }
        public string Ip { get; set; }
    }
}
