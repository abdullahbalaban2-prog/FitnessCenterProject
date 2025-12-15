using System;
using System.Threading.Tasks;

namespace FitnessCenterProject.Services
{
    public class FakeAiService : IAiService
    {
        public Task<string> GenerateWorkoutPlanAsync(string prompt)
        {
            // Basit ama teslim için “AI önerisi” mantığını gösteren içerik
            var text =
$@"(FAKE AI) Kişiselleştirilmiş Plan

Girdi Özeti:
{prompt}

Haftalık Örnek Program:
- Pzt: Full body (squat, row, push-up) 3x10
- Sal: 30 dk yürüyüş + core 10 dk
- Çar: Üst vücut (bench, lat pulldown, shoulder press) 3x10
- Per: Dinlenme / esneme
- Cum: Alt vücut (deadlift hafif, lunge, calf) 3x10
- Cmt: 25 dk kardiyo + mobilite
- Paz: Dinlenme

Not: Ağrı/rahatsızlık varsa antrenörü bilgilendir.";

            return Task.FromResult(text);
        }
    }
}
