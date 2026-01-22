using FitnessCenterProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Linq;

namespace FitnessCenterProject.Controllers
{
    [Route("sitemap.xml")]
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SitemapController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var baseUrl = _configuration["SiteSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = $"{Request.Scheme}://{Request.Host}";
            }

            baseUrl = baseUrl.TrimEnd('/');

            var urls = new List<string>
            {
                "/",
                "/portfolio",
                "/services",
                "/about",
                "/contact",
                "/quote"
            };

            var projectSlugs = await _context.Projects
                .Select(p => p.Slug)
                .ToListAsync();

            foreach (var slug in projectSlugs)
            {
                urls.Add($"/portfolio/{slug}");
            }

            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var sitemap = new XElement(ns + "urlset",
                urls.Select(url => new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}{url}"))));

            var xml = new XDocument(sitemap);
            var content = xml.ToString(SaveOptions.DisableFormatting);

            return Content(content, "application/xml", Encoding.UTF8);
        }
    }
}
