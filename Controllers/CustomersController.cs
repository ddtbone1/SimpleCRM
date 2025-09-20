using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SimpleCRM.Models;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCRM.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        public static List<Customer> customers = new List<Customer>();
        private static int nextId = 1;

        static CustomersController()
        {
            if (!customers.Any())
            {
                for (int i = 1; i <= 50; i++)
                {
                    customers.Add(new Customer
                    {
                        Id = i,
                        Name = $"Customer {i}",
                        Email = $"customer{i}@example.com",
                        Phone = $"555-010{i:D2}"
                    });
                }
                nextId = 51;
            }
        }

        public IActionResult Index()
        {
            int pageSize = 10;
            int page = 1;
            if (Request.Query.ContainsKey("page"))
            {
                int.TryParse(Request.Query["page"], out page);
                if (page < 1) page = 1;
            }
            int totalItems = customers.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagedCustomers = customers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
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
        public IActionResult Create(Customer customer)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
                
            if (ModelState.IsValid)
            {
                customer.Id = nextId++;
                customers.Add(customer);
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public IActionResult Edit(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(Customer customer)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
                
            var existing = customers.FirstOrDefault(c => c.Id == customer.Id);
            if (existing == null) return NotFound();
            if (ModelState.IsValid)
            {
                existing.Name = customer.Name;
                existing.Email = customer.Email;
                existing.Phone = customer.Phone;
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public IActionResult Delete(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
                
            var customer = customers.FirstOrDefault(c => c.Id == id);
            if (customer == null) return NotFound();
            customers.Remove(customer);
            return RedirectToAction("Index");
        }
    }
}
