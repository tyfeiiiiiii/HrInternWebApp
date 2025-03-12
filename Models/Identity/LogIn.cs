using System.ComponentModel.DataAnnotations;
namespace HrInternWebApp.Models.Identity
{
    public class LogIn
    {
        [Required] public virtual int EmpId { get; set; }
        [Required] public virtual string Username { get; set; }
        [Required] public virtual string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
