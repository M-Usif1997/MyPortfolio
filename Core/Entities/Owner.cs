using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class Owner : IdentityUser
    {
  
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Profile { get; set; }
        public string Avatar { get; set; }
        public string City { get; set; }

        public ICollection<PortfolioItem> portfolioItems { get; set; }


    }
}
