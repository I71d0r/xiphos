using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xiphos.Data;
using Xiphos.Models;

namespace Xiphos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProductDbContext _dbContext;
        private readonly bool _displayDebugMessages;

        public HomeController(ILogger<HomeController> logger, ProductDbContext dbContext, IWebHostEnvironment env)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _displayDebugMessages = env.IsDevelopment();
        }

        public async Task<IActionResult> Index()
        {
            var model = new ToneZoneModel()
            {
                DisplayDebugMessages = _displayDebugMessages,
                Melodies = await _dbContext.Melodies.ToArrayAsync()
            };

            ViewBag.Filter = string.Empty;

            return View(model);
        }

        /// <summary>
        /// API returning parametrized melody list
        /// </summary>
        /// <param name="limit">Maximal number of records returned</param>
        /// <param name="filter">Name filtering string</param>
        /// <returns>Name and Id pairs as Json formatted array</returns>
        public async Task<ActionResult> FetchMelodies(int limit, string filter)
        {
            var query = _dbContext.Melodies.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(m => m.Name.Contains(filter));
            }

            query = query.OrderBy(m => m.Name);

            var count = await query.CountAsync();
            var melodyTable = await query.Take(limit)
                .Select(m => new {name = m.Name, id = m.Id})
                .ToArrayAsync();

            var result = new
            {
                filteredCount = count - melodyTable.Length,
                melodies = melodyTable
            };
                
            return Json(result);
        }

        /// <summary>
        /// Returns melody with given Id
        /// </summary>
        /// <param name="id">MelodyModel Id</param>
        /// <returns>MelodyModel model</returns>
        public async Task<ActionResult> FetchMelody(int id)
            => Json(await _dbContext.Melodies.FirstOrDefaultAsync(m => m.Id == id));

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
