using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ProcessManager.Services
{
    public class WeatherForecastService
    {
        int count = 5;
        public WeatherForecastService()
        {
            var timer = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds) {AutoReset = false};
            timer.Elapsed += async (s, e) =>
            {
                count++;
                await OnChange();
                timer.Start();
            };
            timer.Start();
        }
        
        public Func<Task> OnChange = ()=> Task.CompletedTask;

        static string[] Summaries = 
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, count).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray());
        }
    }
}
