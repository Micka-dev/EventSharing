using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventSharing.Data;
using EventSharing.Models;
using AutoMapper;
using EventSharing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace EventSharing.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;

        public EventsController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
        }

        // POST: Events/Register
        [HttpPost]
        [Authorize(Roles = "Admin, Organizer, Participant")]
        public async Task<IActionResult> Register(int eventId)
        {
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var @event = await _context.Events.Include(e => e.Participants).FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null)
            {
                ModelState.AddModelError("", "L'événement est inexistant.");
                return View("Details", _mapper.Map<EventViewModel>(@event)); 
            }
            if (@event.Participants.Count >= @event.Capacity)
            {
                ModelState.AddModelError("", "L'événement est complet.");
                return View("Details", _mapper.Map<EventViewModel>(@event));
            }
            if (user == null)
            {
                ModelState.AddModelError("", "Utilisateur introuvable.");
                return View("Details", _mapper.Map<EventViewModel>(@event));
            }
            if (@event.Participants.Any(p => p.Id == user.Id))
            {
                ModelState.AddModelError("", "Vous êtes déjà inscrit à cet événement.");
                return View("Details", _mapper.Map<EventViewModel>(@event));
            }

            @event.Participants.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = eventId });
        }

        // GET: Events
        [Authorize(Roles = "Admin, Organizer, Participant")]
        public async Task<IActionResult> Index()
        {
            List<Event> events;
            events = await _context.Events
                    .Include(e => e.Category)
                    .ToListAsync();

            return _context.Events != null ?
                View(_mapper.Map<List<EventViewModel>>(events)) :
                Problem("Entity set 'ApplicationDbContext.Events' is null.");
        }

        // GET: Events/Details
        [Authorize(Roles = "Admin, Organizer, Participant")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (_context.Events == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Events' is null.");
            }

            var eventVm = _mapper.Map<EventViewModel>(await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Participants)
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(m => m.Id == id));
            if (eventVm == null)
            {
                return NotFound();
            }
            return View(eventVm);
        }

        // GET: Events/Create
        [Authorize(Roles = "Admin, Organizer")]
        public IActionResult Create()
        {
            var eventVm = new EventViewModel
            {
                CategoriesVm = _mapper.Map<List<CategoryViewModel>>(_context.Categories.ToList())
            };
            return View(eventVm);
        }

        // POST: Events/Create
        [Authorize(Roles = "Admin, Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,Capacity,CategoryId")] EventViewModel eventVm)
        {
            if (!ModelState.IsValid)
            {
                eventVm.CategoriesVm = _mapper.Map<List<CategoryViewModel>>(_context.Categories.ToList());
                return View(eventVm);
            }

            eventVm.CategoryName = _context.Categories.FirstOrDefault(c => c.Id == eventVm.CategoryId)?.Name;

            var @event = _mapper.Map<Event>(eventVm);
            @event.Category = _context.Categories
                .FirstOrDefault(c => c.Id.Equals(eventVm.CategoryId));
            @event.Creator = _context.Set<User>()
                .FirstOrDefault(o => o.Email.Equals(User.FindFirstValue(ClaimTypes.Email)));
            @event.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            _context.Add(@event);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Organizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.Include(e => e.Category)
                                              .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && @event.CreatorId != currentUserId)
            {
                return Forbid(); 
            }

            var eventVm = _mapper.Map<EventViewModel>(@event);
            eventVm.CategoriesVm = _mapper.Map<List<CategoryViewModel>>(_context.Categories.ToList());

            return View(eventVm);
        }


        [Authorize(Roles = "Admin, Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,Capacity,CategoryId,CreatorId")] EventViewModel eventVm)
        {
            if (id != eventVm.Id)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && @event.CreatorId != currentUserId)
            {
                return Forbid(); 
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _mapper.Map(eventVm, @event);
                    @event.Category = _context.Categories.FirstOrDefault(c => c.Id.Equals(eventVm.CategoryId));
                    _context.Update(@event);

                    if (string.IsNullOrEmpty(@event.CreatorId))
                    {
                        @event.CreatorId = _context.Entry(@event).Property(e => e.CreatorId).OriginalValue?.ToString();
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventVm.Id))
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

            eventVm.CategoriesVm = _mapper.Map<List<CategoryViewModel>>(_context.Categories.ToList());
            return View(eventVm);
        }


        // GET: Events/Delete/5
        [Authorize(Roles = "Admin, Organizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (_context.Events == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Events' is null.");
            }

            var @event = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);

            if (!User.IsInRole("Admin") && (@event.Creator?.Email != currentUserEmail))
            {
                return Forbid();
            }

            return View(_mapper.Map<EventViewModel>(@event));
        }


        [Authorize(Roles = "Admin, Organizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.Include(e => e.Creator)
                                               .FirstOrDefaultAsync(e => e.Id == id);

            if (@event == null)
            {
                return NotFound();
            }

            if (@event.Creator == null)
            {
                return Problem("Impossible de vérifier le créateur de l'événement.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && (@event.CreatorId != currentUserId))
            {
                return Forbid(); 
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}

