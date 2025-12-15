using System.Threading.Tasks;

namespace FitnessCenterProject.Services
{
    public interface IAiService
    {
        Task<string> GenerateWorkoutPlanAsync(string prompt);
    }
}
