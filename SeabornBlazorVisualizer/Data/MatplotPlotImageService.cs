using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Options;
using Python.Runtime;

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

        public Task<string> GenerateDefiniteIntegral(string functionExpression, int lowerBound, int upperBound)
        {

            string? result = null;

            using (Py.GIL()) // Ensure thread safety for Python calls
            {
                dynamic np = Py.Import("numpy");
                dynamic plt = Py.Import("matplotlib.pyplot");

                dynamic patches = Py.Import("matplotlib.patches"); // Import patches module

                // Create a Python execution scope
                using (var scope = Py.CreateScope())
                {
                    // Define the function inside the scope
                    scope.Exec($@"
import numpy as np
def func(x):
    return {functionExpression}
");

                    // Retrieve function reference from scope
                    dynamic func = scope.Get("func");

                    // Define integration limits
                    double a = lowerBound, b = upperBound;

                    // Generate x-values
                    dynamic x = np.linspace(0, 10, 100); //generate evenly spaced values in range [0, 20], 100 values (per 0.1)
                    dynamic y = func.Invoke(x);

                    // Create plot figure
                    var fig = plt.figure();
                    var ax = fig.add_subplot(111);

                    // set title to function expression
                    plt.title(functionExpression);

                    ax.plot(x, y, "r", linewidth: 2);
                    ax.set_ylim(0, null);

                    // Select range for integral shading
                    dynamic ix = np.linspace(a, b, 100);
                    dynamic iy = func.Invoke(ix);

                    // **Fix: Separate x and y coordinates properly**
                    List<double> xCoords = new List<double> { a }; // Start at (a, 0)
                    List<double> yCoords = new List<double> { 0 };

                    int length = (int)np.size(ix);
                    for (int i = 0; i < length; i++)
                    {
                        xCoords.Add((double)ix[i]);
                        yCoords.Add((double)iy[i]);
                    }

                    xCoords.Add(b); // End at (b, 0)
                    yCoords.Add(0);

                    // Convert x and y lists to NumPy arrays
                    dynamic npVerts = np.column_stack(new object[] { np.array(xCoords), np.array(yCoords) });

                    // **Correctly Instantiate Polygon Using NumPy Array**
                    dynamic poly = patches.Polygon(npVerts, facecolor: "0.6", edgecolor: "0.2");
                    ax.add_patch(poly);

                    // Compute integral area
                    double area = np.trapz(iy, ix);
                    ax.text(0.5 * (a + b), 30, "$\\int_a^b f(x)\\mathrm{d}x$", ha: "center", fontsize: 20);
                    ax.text(0.5 * (a + b), 10, $"Area = {area:F2}", ha: "center", fontsize: 12);

                    plt.show();


                    result = SavePlot(plt, dpi: 150);
                }
            }
            return Task.FromResult(result);
        }

        public Task<string> GenerateSeabornHistogram()
        {
            string? result = null;
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {
                var (np, os, scipy, mpl, plt, sns) = PythonHelper.ImportPythonModules();
   
                string cwd = os.getcwd();

                plt.clf();

                var penguins = sns.load_dataset("penguins");

                //sns.set_style("darkgrid");

                sns.histplot(data: penguins, x: "flipper_length_mm", kde: true);

                // tight layout to prevent overlap 
                plt.tight_layout();

                // Show the plot with the two subplots at last (render to back buffer 'Agg', see method SavePlot for details)
                plt.show();

                result = SavePlot(plt, theme: "bmh", dpi: 150);
            }

            return Task.FromResult(result);
        }


        public Task<string> GenerateHistogram(List<double> values, string title = "Provide Plot title", string xlabel = "Provide xlabel title", string ylabel = "Provide ylabel title")
        {
            string? result = null;
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {
                var (np, os, scipy, mpl, plt, sns) = PythonHelper.ImportPythonModules();

                var distribution = np.array(values.ToArray());

                //// Ensure clearing the plot
                //plt.clf();

                var fig = plt.figure(); //create a new figure
                var ax1 = fig.add_subplot(1, 2, 1);
                var ax2 = fig.add_subplot(1, 2, 2);

                // Add style
                plt.style.use("ggplot");

                var counts_bins_patches = ax1.hist(distribution, edgecolor: "black");

                // Normalize counts to get colors 
                var counts = counts_bins_patches[0];
                var patches = counts_bins_patches[2];

                var norm_counts = counts / np.max(counts);

                int norm_counts_size = Convert.ToInt32(norm_counts.size.ToString());

                // Apply colors to patches based on frequency
                for (int i = 0; i < norm_counts_size; i++)
                {
                    plt.setp(patches[i], "facecolor", plt.cm.viridis(norm_counts[i])); //plt.cm is the colormap module in MatPlotlib. viridis creates color maps from normalized value 0 to 1 that is optimized for color-blind people.
                }

                // **** AX1 Histogram first - frequency counts ***** 

                ax1.set_title(title);
                ax1.set_xlabel(xlabel);
                ax1.set_ylabel(ylabel);

                string cwd = os.getcwd();

                // Calculate average and standard deviation
                var average = np.mean(distribution);
                var std_dev = np.std(distribution);
                var total_count = np.size(distribution);

                // Format average and standard deviation to two decimal places
                var average_formatted = np.round(average, 2);
                var std_dev_formatted = np.round(std_dev, 2);

                //Add legend with average and standard deviation
                ax1.legend(new string[] { $"Total count: {total_count}\n Average: {average_formatted} cm\nStd Dev: {std_dev_formatted} cm" }, framealpha: 0.5, fancybox: true);



                //***** AX2 : Set up ax2 = Percentage histogram next *******

                ax2.set_title("Percentage distribution");
                ax2.set_xlabel(xlabel);
                ax2.set_ylabel(ylabel);
                // Fix for CS1977: Cast the lambda expression to a delegate type
                ax2.yaxis.set_major_formatter((PyObject)plt.FuncFormatter(new Func<double, int, string>((y, _) => $"{y:P0}")));

                ax2.hist(distribution, edgecolor: "black", weights: np.ones(distribution.size) / distribution.size);

                // Format y-axis to show percentages
                ax2.yaxis.set_major_formatter(plt.FuncFormatter(new Func<double, int, string>((y, _) => $"{y:P0}")));

                // tight layout to prevent overlap 
                plt.tight_layout();

                // Show the plot with the two subplots at last (render to back buffer 'Agg', see method SavePlot for details)
                plt.show();

                result = SavePlot(plt, theme: "bmh", dpi: 150);
            }

            return Task.FromResult(result);
        }

        public Task<string> GeneratedCumulativeGraphFromValues(List<double> values)
        {
            string? result = null;
            using (Py.GIL()) //Python Global Interpreter Lock (GIL)
            {
                var (np, os, scipy, mpl, plt, sns) = PythonHelper.ImportPythonModules();

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

                var (np, os, scipy, mpl, plt, sns) = PythonHelper.ImportPythonModules();
               
                // Set dark theme
                plt.style.use("ggplot");
       
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
