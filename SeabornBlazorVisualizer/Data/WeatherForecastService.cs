using IronPython.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Scripting.Hosting;
using Python.Runtime;

namespace SeabornBlazorVisualizer.Data
{
    public class WeatherForecastService
    {

        static bool runtime_initialized = false;

        static WeatherForecastService()
        {

            string pythonDll = @"C:\Users\Tore\AppData\Local\Programs\Python\Python310\python310.dll";
            // Set environment variables
            Environment.SetEnvironmentVariable("PYTHONHOME", @"C:\programdata\anaconda3", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", @"c:\programdata\anaconda3\lib\site-packages", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);

            //Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", @"C:\ProgramData\Anaconda3\python310.dll");
            Environment.SetEnvironmentVariable("PYTHONNET_PYVER", "3.10");

            PythonEngine.Initialize();
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


        public Task<string?> GetSomethingFromPython()
        {
            //// Initialize the Python engine
            //PythonEngine.Initialize();

            ////using (Py.GIL())
            ////{
            ////    // Import numpy
            ////    dynamic np = Py.Import("numpy");

            ////    // Create a numpy array
            ////    dynamic array = np.array(new int[] { 1, 2, 3, 4 });

            ////    // Perform operations
            ////    Console.WriteLine($"Sum: {array.sum()}");
            ////}

            //// Shutdown the Python engine
            //PythonEngine.Shutdown();

            // Python.Runtime.Runtime.PythonDLL = @"C:\Users\Tore\.conda\envs\py310\python310.dll";

            string? result = null;

          
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {
                if (!runtime_initialized)
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(@"C:\ProgramData\Anaconda3\Lib\site-packages");
                    Console.WriteLine(sys.path);

                    //add folders in solution too

                    sys.path.append(@"Data/");
                    runtime_initialized = true;
                }

                Py.Import("numpy");
                //Py.Import("matplotlib");
               // Py.Import("seaborn");
               

              
                dynamic script = Py.Import(@"hello");

                result = script.InvokeMethod("get_hello_world");



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
