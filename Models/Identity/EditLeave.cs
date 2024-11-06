namespace HrInternWebApp.Models.Identity
{
    public class EditLeave
    {
        public int leaveId { get; set; }                    
        public string leaveType { get; set; }      
        public DateTime? startDate { get; set; }   
        public DateTime? endDate { get; set; }        
        public string reason { get; set; }             
        public string status { get; set; }       
        public string approver { get; set; }        
        public int empId { get; set; }            
        public string username { get; set; }
    }
}
