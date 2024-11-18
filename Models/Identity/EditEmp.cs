namespace HrInternWebApp.Models.Identity
{
    public class EditEmp
    {
        public virtual int empId { get; set; }
        public virtual string Role { get; set; }
        public virtual string Department { get; set; }
        public virtual string username { get; set; }

        public virtual string password { get; set; }
        public virtual string email { get; set; }
        public virtual byte[] profilePic { get; set; }
    }
}
