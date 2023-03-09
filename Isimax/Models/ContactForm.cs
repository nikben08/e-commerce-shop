using System.ComponentModel.DataAnnotations;

namespace Isimax.Models
{
    public class ContactForm

    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
        public string Msg { get; set; }
        public string Date { get; set; }
    }
}
