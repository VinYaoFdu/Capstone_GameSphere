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
    public class SearchPostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchPostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SearchPost
        /* public IActionResult Index(string searchString)
         {
             var posts = from p in _context.Post
                         select p;

             if (!String.IsNullOrEmpty(searchString))
             {
                 posts = posts.Where(p => p.Topic.Contains(searchString));
             }
             return View(posts.ToList());
         }
        */


        public IActionResult Index(string searchString)
        {
            IQueryable<Post> posts = Enumerable.Empty<Post>().AsQueryable();  // Empty list

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = _context.Post.Where(p => p.Topic.Contains(searchString));
            }

            return View(posts.ToList());
        }




        // GET: SearchPost/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: SearchPost/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SearchPost/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Topic,Message,PostedBy,MessaAt")] Post post)
        {
            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: SearchPost/Edit/5
        [Authorize]
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
        // POST: SearchPost/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Topic,Message,PostedBy,MessaAt")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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

        // GET: SearchPost/Delete/5
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

        // POST: SearchPost/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Post == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Post'  is null.");
            }
            var post = await _context.Post.FindAsync(id);
            if (post != null)
            {
                _context.Post.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Search(string searchString)
        {
            var posts = from p in _context.Post
                        select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(p => p.PostedBy.Contains(searchString));
            }

            return View(posts.ToList());
        }



        private bool PostExists(int id)
        {
          return (_context.Post?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
