using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.ViewModels
{
    public class HomeViewModel
    {
        public Owner Owner { get; set; }
        public ICollection<PortfolioItem> PortfolioItems { get; set; }
    }
}
