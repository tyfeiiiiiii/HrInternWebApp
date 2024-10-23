﻿using HrInternWebApp.Models.Identity;
using System.ComponentModel.DataAnnotations;

public class Employee
{
    public virtual int  EmpId { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    public virtual string Username { get; set; } 

    [Required(ErrorMessage = "Password is required.")]
    public virtual string Password { get; set; } 

    public virtual IList<Leave> Leave { get; set; } = new List<Leave>(); 

    //Add a role session to check whether admin or user
    public virtual string Role { get; set; }
}