using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using X.PagedList;

using BlogDotNet8.Data;
using BlogDotNet8.Enums;
using BlogDotNet8.Models;
using BlogDotNet8.Services.Interfaces;
using BlogDotNet8.Services;
using BlogDotNet8.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BlogSecond.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;

        public PostsController(ApplicationDbContext context, ISlugService slugService, IImageService imageService, UserManager<BlogUser> userManager, BlogSearchService blogSearchService)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
        }



        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm;

            var pageNumber = page ?? 1;
            var pageSize = 5;
            //Changed to AsQueryable in case user clicks on button with nothing selected it will return all production ready posts
            var posts = _blogSearchService.Search(searchTerm);
            return View(await posts.ToPagedListAsync(pageNumber, pageSize));
        }


        // GET: Posts
        public async Task<IActionResult> Index()
        {
            //allows you to get the actual data and not just the foreign keys and a null object (eager loading)
            var applicationDbContext = _context.Posts.Include(p => p.Blog).Include(p => p.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }

        ////BlogPostINdex
        //public async Task<IActionResult> BlogPostIndex(int? id)
        //{
        //  if (id is null)
        //  {
        //    return NotFound();
        //  }

        //  var posts = _context.Posts.Where(p => p.BlogId == id).ToList();
        //  return View("Index", posts);

        //}

        //BlogPostINdex Second
        public async Task<IActionResult> BlogPostIndex(int? id, int? page)
        {
            if (id is null)
            {
                return NotFound();
            }

            var pageNumber = page ?? 1;
            var pageSize = 5;

            //var posts = _context.Posts.Where(p => p.BlogId == id).ToList();
            var posts = await _context.Posts
                                .Where(p => p.BlogId == id && p.ReadyStatus == ReadyStatus.ProductionReady)
                                .OrderByDescending(p => p.Created)
                                .ToPagedListAsync(pageNumber, pageSize);
            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            ViewData["HeaderImage"] = _imageService.DecodeImage(blog.ImageData, blog.ContentType);
            ViewData["MainText"] = "Blogs By Wells";
            ViewData["SubText"] = "Code Build...Code Again";
            return View(posts);
        }

        //// GET: Posts/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //  if (id == null)
        //  {
        //    return NotFound();
        //  }

        //  var post = await _context.Posts
        //      .Include(p => p.Blog)
        //      .Include(p => p.BlogUser)
        //      .Include(p =>p.Tags)
        //      .FirstOrDefaultAsync(m => m.Id == id);
        //  if (post == null)
        //  {
        //    return NotFound();
        //  }

        //  return View(post);
        //}

        // GET: Posts/Details/5
        //public async Task<IActionResult> Details(string slug)
        //{
        //  if (string.IsNullOrEmpty(slug))
        //  {
        //    return NotFound();
        //  }

        //  var post = await _context.Posts
        //      .Include(p => p.Blog)
        //      .Include(p => p.BlogUser)
        //      .Include(p => p.Tags)
        //      .Include(p => p.Comments)
        //      .ThenInclude(c => c.BlogUser)
        //      .FirstOrDefaultAsync(m => m.Slug == slug);
        //  if (post == null)
        //  {
        //    return NotFound();
        //  }

        //  return View(post);
        //}
        // GET: Posts/Details/5
        public async Task<IActionResult> Details(string slug)
        {
            ViewData["Title"] = "Post Details Page";
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var post = await _context.Posts
                                     .Include(p => p.BlogUser)
                                     .Include(p => p.Tags)
                                     .Include(p => p.Comments)
                                     .ThenInclude(c => c.BlogUser)
                                     .Include(p => p.Comments)
                                     .ThenInclude(c => c.Moderator)
                                     .FirstOrDefaultAsync(m => m.Slug == slug);

            if (post == null) return NotFound();

            var dataVM = new PostDetailViewModel()
            {
                Post = post,
                Tags = _context.Tags
                               .Select(t => t.Text.ToLower())
                               .Distinct().ToList()
            };

            ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ContentType);
            ViewData["MainText"] = post.Title;
            ViewData["SubText"] = post.Abstract;
            return View(dataVM);


        }

        // GET: Posts/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,ReadyStatus,Image")] Post post, List<string> tagValues)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.Now.ToUniversalTime();
                var authorId = _userManager.GetUserId(User);
                post.BlogUserId = authorId;

                //use the _imageservice to store incoming user specified image
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ContentType = _imageService.ContentType(post.Image);

                // create slug and determine if it is unique
                var slug = _slugService.UrlFriendly(post.Title);

                //Create a variable to storewhether an error has occurred
                var validationError = false;

                if (string.IsNullOrEmpty(slug))
                {
                    validationError = true;
                    ModelState.AddModelError("", "The Title you provided cannot be used as it results in an empty slug.");
                }

                else if (!_slugService.IsUnique(slug))
                {
                    validationError = true;
                    ModelState.AddModelError("Title", "The Title you provided cannot be used as it results in a duplicate slug.");
                }

                else if (slug.Contains("test"))
                {
                    validationError = true;
                    ModelState.AddModelError("", "Uh-oh are you testing again??");
                    ModelState.AddModelError("Title", "The Title cannot contain the word test.");
                }

                if (validationError)
                {
                    ViewData["TagValues"] = string.Join(",", tagValues);
                    return View(post);
                }


                post.Slug = slug;

                _context.Add(post);
                await _context.SaveChangesAsync();


                //loop over incoming list of string
                foreach (var tagText in tagValues)
                {
                    _context.Add(new Tag()
                    {
                        PostId = post.Id,
                        BlogUserId = authorId,
                        Text = tagText

                    });
                }


                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);

            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                                     .Include(p => p.Tags)
                                     .FirstOrDefaultAsync(p => p.Id == id);



            if (post == null)
            {
                return NotFound();
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));

            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogId,Title,Abstract,Content,ReadyStatus")] Post post, IFormFile? newImage, List<string> tagValues)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //original Post
                    var newPost = await _context.Posts
                                                .Include(p => p.Tags)
                                                .FirstOrDefaultAsync(p => p.Id == post.Id);

                    newPost.Updated = DateTime.Now.ToUniversalTime();
                    newPost.Title = post.Title;
                    newPost.Abstract = post.Abstract;
                    newPost.Content = post.Content;
                    newPost.ReadyStatus = post.ReadyStatus;

                    var newSlug = _slugService.UrlFriendly(post.Title);
                    if (newSlug != newPost.Slug)
                    {
                        if (_slugService.IsUnique(newSlug))
                        {
                            newPost.Title = post.Title;
                            newPost.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "This Title cannot be used as it results in a duplicate slug.");
                            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
                            return View(post);
                        }
                    }

                    if (newImage is not null)
                    {
                        newPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newPost.ContentType = _imageService.ContentType(newImage);
                    }

                    //remove all tags previously asscociated with this post
                    _context.Tags.RemoveRange(newPost.Tags);
                    //Add new tags from the edite
                    foreach (var tagText in tagValues)
                    {
                        _context.Add(new Tag()
                        {
                            PostId = post.Id,
                            BlogUserId = newPost.BlogUserId,
                            Text = tagText
                        });

                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }

        // GET: Posts/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}