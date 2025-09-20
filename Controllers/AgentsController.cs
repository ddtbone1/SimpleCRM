using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SimpleCRM.Models;

namespace SimpleCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AgentsController : Controller
    {
        public static List<Agent> agents = new List<Agent>();
        private static int nextId = 1;

        static AgentsController()
        {
            // Add mock agents
            for (int i = 1; i <= 20; i++)
            {
                agents.Add(new Agent
                {
                    Id = nextId++,
                    Name = $"Agent {i}",
                    Email = $"agent{i}@crm.com",
                    Phone = $"555-{i:D4}",
                    Department = i % 2 == 0 ? "Sales" : "Support",
                    Position = i % 3 == 0 ? "Senior Agent" : "Agent",
                    HireDate = DateTime.Now.AddDays(-Random.Shared.Next(30, 365))
                });
            }
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;
            var totalAgents = agents.Count;
            var totalPages = (int)Math.Ceiling((double)totalAgents / pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedAgents = agents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAgents = totalAgents;

            return View(paginatedAgents);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Agent agent)
        {
            if (ModelState.IsValid)
            {
                agent.Id = nextId++;
                agents.Add(agent);
                return RedirectToAction("Index");
            }
            return View(agent);
        }

        public IActionResult Edit(int id)
        {
            var agent = agents.FirstOrDefault(a => a.Id == id);
            if (agent == null) return NotFound();
            return View(agent);
        }

        [HttpPost]
        public IActionResult Edit(Agent agent)
        {
            var existing = agents.FirstOrDefault(a => a.Id == agent.Id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                existing.Name = agent.Name;
                existing.Email = agent.Email;
                existing.Phone = agent.Phone;
                existing.Department = agent.Department;
                existing.Position = agent.Position;
                existing.IsActive = agent.IsActive;
                return RedirectToAction("Index");
            }
            return View(agent);
        }

        public IActionResult Delete(int id)
        {
            var agent = agents.FirstOrDefault(a => a.Id == id);
            if (agent == null) return NotFound();
            
            agents.Remove(agent);
            return RedirectToAction("Index");
        }
    }
}
