using HrInternWebApp.Entity;
using System.ComponentModel.DataAnnotations;

namespace HrInternWebApp.Models.Identity
{
    public class ApplyLeave
    {
        [Required(ErrorMessage = "Please select a leave type.")]
        public string leaveType { get; set; }

        [Required(ErrorMessage = "Please fill in the start date of your leave.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(LeaveDateValidator), nameof(LeaveDateValidator.ValidateStartDate))]
        public DateTime? startDate { get; set; }

        [Required(ErrorMessage = "Please fill in the end date of your leave.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(LeaveDateValidator), nameof(LeaveDateValidator.ValidateEndDate))]
        public DateTime? endDate { get; set; }

        [Required(ErrorMessage = "Please provide a reason for your leave.")]
        [StringLength(255)]
        public string reason { get; set; }

        public string Gender { get; set; }
        public virtual Employee employee { get; set; }
    }

    #region Start & EndDate Validation
    public static class LeaveDateValidator
    {
        public static ValidationResult ValidateStartDate(DateTime? startDate, ValidationContext context)
        {
            if (startDate.HasValue && startDate.Value.Date < DateTime.Today)
            {
                return new ValidationResult("Start date cannot be earlier than today.");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult ValidateEndDate(DateTime? endDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as ApplyLeave;

            if (instance == null)
            {
                return new ValidationResult("An error occurred while validating the end date.");
            }

            if (!instance.startDate.HasValue)
            {
                return ValidationResult.Success; // Allow endDate to be set before startDate
            }

            if (!endDate.HasValue)
            {
                return new ValidationResult("End date is required.");
            }

            if (endDate.Value < instance.startDate.Value)
            {
                return new ValidationResult("End date cannot be earlier than start date.");
            }

            return ValidationResult.Success;
        }
    }
    #endregion
}

