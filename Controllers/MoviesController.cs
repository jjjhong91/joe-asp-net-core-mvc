using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Joe.MVC.Data;
using Joe.MVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace Joe.MVC.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;
        private List<TagItemViewModel> _tags = new List<TagItemViewModel>
        {
            new TagItemViewModel{ Id = 1, Name = "A"},
            new TagItemViewModel{ Id = 2, Name = "B"},
        };

        public MoviesController(MvcMovieContext context)
        {
            _context = context;

        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            var movies = from m in _context.Movie
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = await movies.ToListAsync()
            };

            return View(movieGenreVM);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var tagViewModel = new TagViewModel();
            var movieViewModel = new MovieViewModel();
            movieViewModel.SelectTags = new SelectList(_tags, "Id", "Name");
            movieViewModel.TagVM = tagViewModel;
            return View(movieViewModel);
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price,Rating,Tags,TagId,SelectTags")] MovieViewModel vm)
        {
            if (vm.TagId > 0)
            {
                var tag = _tags.FirstOrDefault(t => t.Id == vm.TagId);

                if(vm.TagVM == null)
                    vm.TagVM = new TagViewModel();


                vm.TagVM.Tags.Add(tag);
                vm.TagId = 0;
                foreach (var vmTag in vm.TagVM.Tags)
                {
                    var removeTag = _tags.FirstOrDefault(t => t.Id == vmTag.Id);
                    _tags.Remove(removeTag);
                }

                vm.SelectTags = new SelectList(_tags, "Id", "Name");

                return View(vm);
            }

            if (ModelState.IsValid)
            {
                var movie = new Movie
                {
                    Id = vm.Id,
                    Title = vm.Title,
                    Price = vm.Price,
                    Genre = vm.Genre,
                    Tags = string.Join(",", vm.TagVM.Tags.Select(t => t.Name).ToList()),
                    ReleaseDate = vm.ReleaseDate,
                    Rating = vm.Rating
                };
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
