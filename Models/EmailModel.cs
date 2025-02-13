using System.ComponentModel.DataAnnotations;

namespace To_Do_UI.Models
{
    public class EmailModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        
    }
   
}
