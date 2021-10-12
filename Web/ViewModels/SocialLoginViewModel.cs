using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.ViewModels
{
    public class SocialLoginViewModel
    {
        public SocialLoginViewModel()
        {
                
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        [Required]
        [Remote(action: "IsUsernameInUse", controller: "Account")]
        [MaxLength(50, ErrorMessage = "Name Cannot Be More Than 50 Character")]
        public string UserName { get; set; }


        public string City { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Profile Cannot Be More Than 50 Character")]
        public string Profile { get; set; }


    }
}
