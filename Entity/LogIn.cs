using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Entity
{
    public class LogIn
    {
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
    }
}
