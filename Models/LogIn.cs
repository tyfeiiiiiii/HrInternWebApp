using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Models
{
    public class LogIn
    {
        [Required] public virtual string Username { get; set; }
        [Required] public virtual string Password { get; set; }
    }
}
