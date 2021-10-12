using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Infrastructure;
using Web.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Web.Controllers
{
    public class PortfolioItemsController : Controller
    {
        private readonly IUnitOfWork<PortfolioItem> _portfolio;
        private readonly IWebHostEnvironment _hosting;
        private readonly UserManager<Owner> _userManager;

        public PortfolioItemsController(IUnitOfWork<PortfolioItem> portfolio, IWebHostEnvironment hosting, UserManager<Owner> userManager)
        {
            _portfolio = portfolio;
            _hosting = hosting;
            _userManager = userManager;
        }
    


        // GET: PortfolioItems
        public IActionResult Index()
        {
            HttpContext.Session.SetString("userName", _userManager.GetUserName(HttpContext.User));

            return View(_portfolio.Entity.GetAll().Where(o=>o.OwnerId == _userManager.GetUserId(HttpContext.User)));
        }

        // GET: PortfolioItems/Details/5
        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioItem = _portfolio.Entity.GetById(id);
            if (portfolioItem == null)
            {
                return NotFound();
            }

            return View(portfolioItem);
        }

        // GET: PortfolioItems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PortfolioItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult Create(PortfolioViewModel model)
        {
            if (ModelState.IsValid)
            {

                string uniqueFileName = ProcessUploadedFile(model);
                PortfolioItem portfolioItem = new PortfolioItem
                {
                    ProjectName = model.ProjectName,
                    Description = model.Description,
                    ImageUrl = uniqueFileName,
                    ProjectLink = model.ProjectLink,
                    OwnerId = _userManager.GetUserId(HttpContext.User)

                };

                _portfolio.Entity.Insert(portfolioItem);
                _portfolio.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: PortfolioItems/Edit/5
        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioItem = _portfolio.Entity.GetById(id);
            if (portfolioItem == null)
            {
                return NotFound();
            }

            PortfolioViewModel portfolioViewModel = new PortfolioViewModel
            {
                Id = portfolioItem.Id,
                Description = portfolioItem.Description,
                ImageUrl = portfolioItem.ImageUrl,
                ProjectLink = portfolioItem.ProjectLink,
                ProjectName = portfolioItem.ProjectName
            };

            return View(portfolioViewModel);
        }

        // POST: PortfolioItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, PortfolioViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                



                    string uniqueFileName = ProcessUploadedFile(model);

                    PortfolioItem portfolioItem = new PortfolioItem
                    {
                        Id = model.Id,
                        ProjectName = model.ProjectName,
                        Description = model.Description,
                        ProjectLink = model.ProjectLink,
                        ImageUrl = uniqueFileName
                    };

                    _portfolio.Entity.Update(portfolioItem);
                    _portfolio.Save();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PortfolioItemExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: PortfolioItems/Delete/5
        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolioItem = _portfolio.Entity.GetById(id);
            if (portfolioItem == null)
            {
                return NotFound();
            }

            return View(portfolioItem);
        }

        // POST: PortfolioItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            _portfolio.Entity.Delete(id);
            _portfolio.Save();
            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioItemExists(Guid id)
        {
            return _portfolio.Entity.GetAll().Any(e => e.Id == id);
        }











        private string ProcessUploadedFile(PortfolioViewModel model)
        {
            string uniqueFileName = null;
            if (model.File != null)
            {
                string uploadsFolder = Path.Combine(_hosting.WebRootPath, @"img\portfolio");   // The image must be uploaded to the images folder in wwwroot
                                                                                               // To get the path of the wwwroot folder we are using the inject
                                                                                               // IWebHostEnvironment service provided by ASP.NET Core

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName; // To make sure the file name is unique we are appending a new
                                                                                        // GUID value and an underscore to the file name

                string FilePath = Path.Combine(uploadsFolder, uniqueFileName);    // To Combine the Uploaded Folder and Path
                using (var fileStream = new FileStream(FilePath, FileMode.Create))
                {
                    model.File.CopyTo(fileStream);   // Use CopyTo() method provided by IFormFile interface to
                }                                                                      // copy the file to wwwroot/images folder
            }

            return uniqueFileName;
        }





    }



}
