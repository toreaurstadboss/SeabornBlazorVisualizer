using IronPython.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;
using Python.Runtime;
using System.Drawing;
using System.IO.Pipelines;
using static IronPython.Modules._ast;

namespace SeabornBlazorVisualizer.Data
{
    public class MatplotPlotImageService
    {

        private IOptions<PythonConfig>? _pythonConfig;

        private static readonly object _lock = new object();

        public MatplotPlotImageService(IOptions<PythonConfig> pythonConfig)
        {
            _pythonConfig = pythonConfig;
            PythonInitializer.InitializePythonRuntime(_pythonConfig);
        }

        public Task<string> GenerateHistogram(List<double> values)
        {
            string? result = null;
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {
                var (np, os, scipy, mpl, plt) = PythonHelper.ImportPythonModules();

                var distribution = np.array(values.ToArray());

                // Ensure clearing the plot
                plt.clf();

                plt.hist(distribution, edgecolor: "black");

                string cwd = os.getcwd();

                // Calculate average and standard deviation
                var average = np.mean(distribution);
                var std_dev = np.std(distribution);
                var total_count = np.size(distribution);

                // Format average and standard deviation to two decimal places
                var average_formatted = np.round(average, 2);
                var std_dev_formatted = np.round(std_dev, 2);


                //Add legend with average and standard deviation
                plt.legend(new string[] { $"Total count: {total_count}\n Average: {average_formatted} cm\nStd Dev: {std_dev_formatted} cm" });

                result = SavePlot(plt, theme: "ggplot", dpi: 200);
            }

            return Task.FromResult(result);
        }




        public Task<string> GeneratedCumulativeGraphFromValues(List<double> values)
        {
            string? result = null;
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {
                var (np, os, scipy, mpl, plt) = PythonHelper.ImportPythonModules();

                dynamic pythonValues = np.cumsum(np.array(values.ToArray()));

                // Ensure clearing the plot
                plt.clf();

                // Create a figure with increased size
                dynamic fig = plt.figure(figsize: new PyTuple(new PyObject[] { new PyFloat(6), new PyFloat(4) }));

                // Plot data
                plt.plot(values, color: "green");

                string cwd = os.getcwd();

                result = SavePlot(plt, theme: "ggplot", dpi: 200);

            }

            return Task.FromResult(result);
        }

        public Task<string> GenerateRandomizedCumulativeGraph()
        {
            string? result = null;          
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {

                dynamic np = Py.Import("numpy");

                //TODO : Remove imports of pandas and scipy and datetime if they are not needed

                Py.Import("pandas");
                Py.Import("scipy");
                Py.Import("datetime");
                dynamic os = Py.Import("os");

                dynamic mpl = Py.Import("matplotlib");
                dynamic plt = Py.Import("matplotlib.pyplot");

                // Set dark theme
                plt.style.use("ggplot");

                mpl.use("Agg");


                // Generate data
                //dynamic x = np.arange(0, 10, 0.1);
                //dynamic y = np.multiply(2, x); // Use NumPy's multiply function

                dynamic values = np.cumsum(np.random.randn(1000, 1));


                // Ensure clearing the plot
                plt.clf();

                // Create a figure with increased size
                dynamic fig = plt.figure(figsize: new PyTuple(new PyObject[] { new PyFloat(6), new PyFloat(4) }));

                // Plot data
                plt.plot(values, color: "blue");

                string cwd = os.getcwd();

                result = SavePlot(plt, theme: "ggplot", dpi: 200);

            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Saves the plot to a PNG file with a unique name based on the current date and time
        /// </summary>
        /// <param name="plot">Plot, must be a PyPlot plot use Python.net Py.Import("matplotlib.pyplot")</param>
        /// <param name="theme"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public string? SavePlot(dynamic plt, string theme = "ggplot", int dpi = 200)
        {
            string? plotSavedImagePath = null;
            //using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            //{
                dynamic os = Py.Import("os");
                dynamic mpl = Py.Import("matplotlib");
                // Set dark theme
                plt.style.use(theme);
                mpl.use("Agg"); //set up rendering of plot to back-buffer ('headless' mode)
             
                string cwd = os.getcwd();
                // Save plot to PNG file
                string imageToCreatePath = $@"GeneratedImages\{DateTime.Now.ToString("yyyyMMddHHmmss")}{Guid.NewGuid().ToString("N")}_plotimg.png";
                string imageToCreateWithFolderPath = $@"{cwd}\wwwroot\{imageToCreatePath}";
                plt.savefig(imageToCreateWithFolderPath, dpi: dpi); //save the plot to a file (use full path)
                plotSavedImagePath = imageToCreatePath;

                CleanupOldGeneratedImages(cwd);
            //}
            return plotSavedImagePath;
        }

        private static void CleanupOldGeneratedImages(string cwd)
        {
            lock (_lock)
            {

                Directory.GetFiles(cwd + @"\wwwroot\GeneratedImages", "*.png")
                 .OrderByDescending(File.GetLastWriteTime)
                 .Skip(10)
                 .ToList()
                 .ForEach(File.Delete);
            }
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
