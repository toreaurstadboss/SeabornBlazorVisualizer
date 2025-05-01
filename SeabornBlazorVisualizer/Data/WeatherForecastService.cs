using IronPython.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;
using Python.Runtime;
using static IronPython.Modules._ast;

namespace SeabornBlazorVisualizer.Data
{
    public class WeatherForecastService
    {

        static bool runtime_initialized = false;

        private IOptions<PythonConfig>? _pythonConfig;

        public WeatherForecastService(IOptions<PythonConfig> pythonConfig)
        {
            _pythonConfig = pythonConfig;
            PythonInitializer.InitializePythonRuntime(_pythonConfig);
        }

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


        public Task<string> GetSomethingFromPython()
        {
            string? result = null;

          
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {
                
                dynamic np = Py.Import("numpy");

                Py.Import("pandas");
                Py.Import("scipy");
                Py.Import("datetime");
                dynamic os = Py.Import("os");

                dynamic mpl = Py.Import("matplotlib");
                dynamic plt = Py.Import("matplotlib.pyplot");

                mpl.use("Agg");


                // Generate data
                dynamic x = np.arange(0, 10, 0.1);
                dynamic y = np.multiply(2, x); // Use NumPy's multiply function

                dynamic values = np.cumsum(np.random.randn(1000, 1));

                // Plot data
                plt.plot(values);

                string cwd = os.getcwd();

                result = cwd;

                // Save plot to PNG file
                string savePath = $@"{cwd}\GeneratedImages\{Guid.NewGuid().ToString("N")}_plotimg.png";

                plt.savefig(savePath);



                //Py.Import("seaborn");

                //Py.Import("seaborn");

                //Py.Import("_imaging");
                //Py.Import("matplotlib");
                // Py.Import("seaborn");
                // 

               // result = "hullo";
           
                //dynamic script = Py.Import(@"hello");

                //result = script.InvokeMethod("get_hello_world");



                // Import seaborn and matplotlib
                // Import Seaborn
                //dynamic sys = Py.Import("sys");
                //sys.path.append(@"Data/");
                //dynamic script = Py.Import(@"hello");

                //result = script.InvokeMethod("get_hello_world");


                //dynamic sns = Py.Import("numpy");

                //// Example: Use Seaborn's functionality (e.g., create a dataset)
                //dynamic dataset = sns.load_dataset("iris");
                //System.Console.WriteLine(dataset.head()); // Display the first few rows

                //dynamic plt = Py.Import("matplotlib.pyplot");

                //// Create a simple plot
                //dynamic data = sns.load_dataset("penguins");
                //sns.histplot(data: data, x: "flipper_length_mm", hue: "species", multiple: "stack");

                //// Show the plot
                //plt.figure.savefig("output.png");

            }

            //PythonEngine.Shutdown();

            return Task.FromResult(result);
        }
            
        

        //public Task<string?> GetSomethingFromPython()
        //{

        //    //Create a Python runtime
        //    ScriptRuntime pyRuntime = Python.CreateRuntime();

        //    //Load the Python script
        //    dynamic pyScript = pyRuntime.UseFile(@"Data\hello.py");

        //    //Call the method and get the result
        //    string? result = pyScript.get_hello_world() as string;

        //    return Task.FromResult(result);      
        //}
    }
}
