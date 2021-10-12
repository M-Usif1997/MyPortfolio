using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.ViewModels
{
    public class RegisterViewModel
    {

        [Required]
        [MaxLength(30, ErrorMessage = "Name Cannot Be More Than 30 Character")]
        public string FirstName { get; set; }

        [MaxLength(30, ErrorMessage = "Name Cannot Be More Than 30 Character")]
        public string LastName { get; set; }


        [Required]
        [Remote(action: "IsUsernameInUse", controller: "Account")]
        [MaxLength(50, ErrorMessage = "Name Cannot Be More Than 50 Character")]
        public string UserName { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Profile Cannot Be More Than 50 Character")]
        public string Profile { get; set; }

        public string City { get; set; }      

        [Display(Name = "Email")]       
        [Required]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller: "Account")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).*$",ErrorMessage = "at least 8 characters, must contain at least 1 uppercase letter, 1 lowercase letter, and 1 number & may special characters")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public IFormFile Photo { get; set; }
    }
}
