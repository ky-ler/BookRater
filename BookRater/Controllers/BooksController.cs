using BookRater.Authorization;
using BookRater.Data;
using BookRater.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookRater.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IAuthorizationService AuthorizationService { get; }
        private UserManager<IdentityUser> UserManager { get; }

        public BooksController(ApplicationDbContext context, IAuthorizationService authorizationService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            AuthorizationService = authorizationService;
            UserManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return _context.BookReview != null ?
                        View(await _context.BookReview.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Book'  is null.");
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BookReview == null)
            {
                return NotFound();
            }

            var book = await _context.BookReview
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Rating,DateStartedRead,DateCompletedRead")] BookReview book)
        {
            if (ModelState.IsValid)
            {
                var bookExists = _context.BookReview.FirstOrDefault(
                    m => m.OwnerID == UserManager.GetUserName(User) &&
                    m.Title == book.Title
                );

                if (bookExists != null)
                {
                    // Redirect to edit existing review if one already exists
                    TempData["error"] = $"Review for {book.Title} already exists! Redirecting to edit page.";
                    return RedirectToAction("Edit", new { bookExists.Id });
                }


                book.OwnerID = UserManager.GetUserName(User);
                var isAuthorized = await AuthorizationService.AuthorizeAsync(User, book, BookOperations.Create);

                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                TempData["success"] = "Successfully created review!";
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BookReview == null)
            {
                return NotFound();
            }

            var book = await _context.BookReview.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, book, BookOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                TempData["error"] = "You are not authorized to view this page!";
                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,OwnerID,Rating,DateRated,DateStartedRead,DateCompletedRead")] BookReview book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    book.OwnerID = UserManager.GetUserName(User);
                    var isAuthorized = await AuthorizationService.AuthorizeAsync(User, book, BookOperations.Update);

                    if (!isAuthorized.Succeeded)
                    {
                        TempData["error"] = "You are not authorized to view this page!";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Successfully updated review!";
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BookReview == null)
            {
                return NotFound();
            }

            var book = await _context.BookReview
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, book, BookOperations.Create);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BookReview == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Book'  is null.");
            }
            var book = await _context.BookReview.FindAsync(id);
            if (book != null)
            {
                var isAuthorized = await AuthorizationService.AuthorizeAsync(User, book, BookOperations.Create);

                if (!isAuthorized.Succeeded)
                {
                    TempData["error"] = "You are not authorized to view this page!";
                    return RedirectToAction(nameof(Index));
                }
                _context.BookReview.Remove(book);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Successfully deleted review!";
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return (_context.BookReview?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
