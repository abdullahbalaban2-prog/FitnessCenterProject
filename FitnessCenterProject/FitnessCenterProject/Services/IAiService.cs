
namespace FitnessCenterProject.Services
{
    public interface IAiService
    {
        Task<string> GeneratePlanAsync(string prompt);
    }
}

