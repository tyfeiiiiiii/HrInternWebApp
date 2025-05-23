﻿
namespace HrInternWebApp.Entity
{
    public class Employee
    {
        public virtual int empId { get; set; }

        public virtual string username { get; set; }

        public virtual string password { get; set; }

        public virtual string Role { get; set; }

        public virtual string Department { get; set; }

        public virtual string email { get; set; }
        public virtual byte[] profilePic { get; set; }
        public virtual string  Gender{ get; set; }

        public virtual IList<Leave> Leave { get; set; } = new List<Leave>();
        public virtual LeaveBalance LeaveBalance { get; set; } // One-to-One Relationship
        public virtual IList<SurveyPredictionResults> SurveyPredictionResults { get; set; } = new List<SurveyPredictionResults>();  // One-to-Many Relationship
    }
}

