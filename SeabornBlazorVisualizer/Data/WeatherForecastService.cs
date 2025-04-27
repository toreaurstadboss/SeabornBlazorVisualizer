using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace SeabornBlazorVisualizer.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate)
        {
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray());
        }

        public Task<string?> GetSomethingFromPython()
        {

            //Create a Python runtime
            ScriptRuntime pyRuntime = Python.CreateRuntime();

            //Load the Python script
            dynamic pyScript = pyRuntime.UseFile(@"Data\hello.py");

            //Call the method and get the result
            string? result = pyScript.get_hello_world() as string;

            return Task.FromResult(result);      
        }
    }
}
