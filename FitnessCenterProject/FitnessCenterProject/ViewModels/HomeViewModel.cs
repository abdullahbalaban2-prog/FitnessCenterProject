using FitnessCenterProject.Models;

namespace FitnessCenterProject.ViewModels
{
    public class HomeViewModel
    {
        public List<Project> FeaturedProjects { get; set; } = new();
        public List<Service> Services { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
    }
}
