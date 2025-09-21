using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SimpleCRM.Models;
using SimpleCRM.Data;

namespace SimpleCRM.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly CrmDbContext _context;

        public CustomersController(CrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            if (page < 1) page = 1;
            
            var totalItems = await _context.Customers.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var pagedCustomers = await _context.Customers
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            return View(pagedCustomers);
        }

        public IActionResult Create()
        {
            // Only Admin can create customers based on .copilot-rules.md
            if (!User.IsInRole("Admin"))
                return Forbid();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
                
            if (ModelState.IsValid)
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
                
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
                
            if (ModelState.IsValid)
            {
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
                
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
