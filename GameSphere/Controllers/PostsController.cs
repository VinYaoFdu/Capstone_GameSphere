using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameSphere.Data;
using GameSphere.Models;
using Microsoft.AspNetCore.Authorization;

namespace GameSphere.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PostsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize]
        // GET: Posts
        public async Task<IActionResult> Index()
        {
            if (_context.Post == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Post' is null.");
            }

            var posts = await _context.Post.OrderByDescending(p => p.MessaAt).ToListAsync();
            return View(posts);
        }

        public ApplicationDbContext Get_context()
        {
            return _context;
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            var model = new PostDetailsModel
            {
                Post = post,
                LikesCount = _context.Likes.Count(l => l.PostId == id),
                Replies = _context.Replies.Where(r => r.PostId == id).ToList(),
                HostingEnvironment = _hostingEnvironment
            };

            return View(model);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Topic,Message,PostedBy,MessaAt,ImageFile")] Post post)
        {
            if (!ModelState.IsValid)
            {
                // Handle the image file upload and save it to the server
                if (post.ImageFile != null && post.ImageFile.Length > 0)
                {
                    string uploadDir = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + post.ImageFile.FileName;
                    string filePath = Path.Combine(uploadDir, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await post.ImageFile.CopyToAsync(fileStream);
                    }

                    // Save the image file path to the database
                    post.ImagePath = "/uploads/" + uniqueFileName;
                }

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Posts/Edit/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the post from the database
            var post = await _context.Post.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            if (post.PostedBy != User.Identity.Name)
            {
                return Forbid();
            }

            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Topic,Message,PostedBy,MessaAt")] Post post, IFormFile imageFile)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    // Handle the new image file upload and save it to the server
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadDir = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadDir, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Save the new image file path to the database
                        post.ImagePath = "/uploads/" + uniqueFileName;
                    }

                    _context.Update(post);
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
            return View(post);
        }

        [Authorize(Roles = "admin")]
        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Post == null)
            {
                return NotFound();
            }

            var post = await _context.Post
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
            if (_context.Post == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Post' is null.");
            }
            var post = await _context.Post.FindAsync(id);
            if (post != null)
            {
                _context.Post.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Post?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> Member()
        {
            if (_context.Post == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Post' is null.");
            }

            // Sort topics by MessaAt
            var sortedPosts = await _context.Post.OrderByDescending(p => p.MessaAt).ToListAsync();
            return View(sortedPosts);
        }

        public async Task<IActionResult> FilterPosts(string Topic = "", string Member = "", string Date = "", string tab = "")
        {
            //Dropdown for Topics
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Select Topic", Value = "" });

            var topics = await _context.Post.GroupBy(x => x.Topic)
                .Select(y => new { Topic = y.Key }).ToListAsync();

            foreach (var x in topics)
            {
                items.Add(new SelectListItem { Text = x.Topic, Value = x.Topic, Selected = x.Topic == Topic });
            }
            ViewBag.Topics = items;

            //Dropdown for Member
            var memberItems = new List<SelectListItem>();
            memberItems.Add(new SelectListItem { Text = "Select Member", Value = "" });

            var members = await _context.Post.GroupBy(x => x.PostedBy)
                .Select(y => new { Member = y.Key }).ToListAsync();

            foreach (var x in members)
            {
                memberItems.Add(new SelectListItem { Text = x.Member, Value = x.Member, Selected = x.Member == Member });
            }
            ViewBag.Members = memberItems;

            //Filter Post
            var posts = new List<Post>();
            if (_context.Post != null)
            {
                if (!String.IsNullOrEmpty(Topic))
                {
                    posts = await _context.Post.Where(x => x.Topic == Topic).ToListAsync();
                }
                else if (!String.IsNullOrEmpty(Member))
                {
                    posts = await _context.Post.Where(x => x.PostedBy == Member).ToListAsync();
                }
                else if (!String.IsNullOrEmpty(Date))
                {
                    var startDate = DateTime.Parse(Date);
                    var endDate = startDate.AddDays(1);
                    posts = await _context.Post.Where(x => x.MessaAt >= startDate && x.MessaAt < endDate).ToListAsync();
                    ViewBag.Date = Date;
                }
                else
                {
                    posts = await _context.Post.ToListAsync();
                }
            }
            else
            {
                return Problem("Entity set 'ApplicationDbContext.Post'  is null.");
            }

            //Selected Tab
            if (tab == "Topic" || String.IsNullOrEmpty(tab))
            {
                ViewBag.topicTab = "active";
                ViewBag.topicTabContent = "show active";
            }
            else if (tab == "Member")
            {
                ViewBag.memberTab = "active";
                ViewBag.memberTabContent = "show active";
            }
            else if (tab == "Date")
            {
                ViewBag.dateTab = "active";
                ViewBag.dateTabContent = "show active";
            }


            return View(posts);

        }

        public async Task<IActionResult> SearchPosts(string key = "")
        {
            if (_context.Post == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Post'  is null.");
            }
            else
            {
                var posts = new List<Post>();
                if (string.IsNullOrEmpty(key))
                {
                    posts = await _context.Post.ToListAsync();
                }
                else
                {
                    posts = await _context.Post
                        .Where(x => x.Topic.ToLower().Contains(key.ToLower())
                                || x.Message.ToLower().Contains(key.ToLower())).ToListAsync();
                    ViewBag.key = key;
                }
                return View(posts);
            }
        }

        [HttpPost]
        public IActionResult Reply(int id, string comment)
        {
            var post = _context.Post.Find(id);
            if (post == null)
            {
                return NotFound();
            }

            var userName = User.Identity.Name;
            var reply = new Reply
            {
                PostId = id,
                UserName = userName,
                Message = comment
            };

            _context.Replies.Add(reply);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult LikePost(int id)
        {
            var post = _context.Post.Find(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check if the user has already liked the post (optional but recommended to prevent multiple likes from the same user)
            var userId = User.Identity.Name;
            var existingLike = _context.Likes.FirstOrDefault(l => l.PostId == id && l.UserId == userId);

            if (existingLike == null)
            {
                // Add a new Like record for the post and user
                var like = new Likes { PostId = id, UserId = userId };
                _context.Likes.Add(like);
                _context.SaveChanges();

                // Increment the LikesCount property for the post
                post.LikesCount = _context.Likes.Count(l => l.PostId == id);
                _context.SaveChanges();
            }

            // Return the updated like count to the client
            return Json(new { likeCount = post.LikesCount });
        }
    }
}
