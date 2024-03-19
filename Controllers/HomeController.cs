using BlogDotNet8.Data;
using BlogDotNet8.Models;
using BlogDotNet8.Services.Interfaces;
using BlogDotNet8.ViewModels;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList;

namespace BlogDotNet8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;

        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender, ApplicationDbContext context, IImageService imageService)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _imageService = imageService;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
        public async Task<IActionResult> Index(int? page)
        {
            //sets page number to 1 if page is null and to the page if it isn't null
            var pageNumber = page ?? 1;
            var pageSize = 5;

            //var blogs = _context.Blogs.Where(
            //                          b => b.Posts.Any(p => p.ReadyStatus == Enums.ReadyStatus.ProductionReady))
            //                          .OrderByDescending(b => b.Created)
            //                          .ToPagedListAsync(pageNumber, pageSize);

            var blogs = _context.Blogs
                                .Include(b => b.BlogUser)
                                .OrderByDescending(b => b.Created)
                                .ToPagedListAsync(pageNumber, pageSize);

            //ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ContentType);
            ViewData["MainText"] = "Blogs By Wells";
            ViewData["SubText"] = "Code Build...Code Again";

            return View(await blogs);



        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMe model)
        {
            //Emails from contact form
            model.Message = $"{model.Message} <hr/> Phone: {model.Phone}";
            await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
