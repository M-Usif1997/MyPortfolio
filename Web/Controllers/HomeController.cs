using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork<Owner> _owner;
        private readonly IUnitOfWork<PortfolioItem> _portfolio;
        private readonly UserManager<Owner> _userManager;

        public HomeController(
            IUnitOfWork<Owner> owner,
            IUnitOfWork<PortfolioItem> portfolio,UserManager<Owner> userManager )
        {
            _owner = owner;
            _portfolio = portfolio;
            _userManager = userManager;
        }
        [AllowAnonymous]
        public IActionResult Index(string username)
        
        
        {
           
           
            bool IsuserNameExist = _owner.Entity.Exists(whereCondition: o => o.UserName == username);
            if (!string.IsNullOrEmpty(username) && IsuserNameExist)
            {
                HomeViewModel homeViewModel = new HomeViewModel();


                homeViewModel.Owner = _owner.Entity.SingleOrDefault(whereCondition: o => o.UserName == username);   // you must edit the specific logic in repository of Entity itself not in Generic  GetByUserName
                homeViewModel.PortfolioItems = _portfolio.Entity.GetAll(whereCondition:p => p.owner.UserName == username).ToList();
                ViewBag.Username = homeViewModel.Owner.UserName;

                return View(homeViewModel);

            }


            return View("ErrorUserName",username);     //Must Handle Errors in Error Controller and ,  Handle Status Code and Make Logging 








        }

     

       

    }
}