using Microsoft.Extensions.Options;
using Python.Runtime;

namespace SeabornBlazorVisualizer.Data
{

    /// <summary>
    /// Helper class to initialize the Python runtime
    /// </summary>
    public static class PythonHelper
    {

        private static bool runtime_initialized = false;

        /// <summary>
        /// Perform one-time initialization of Python runtime
        /// </summary>
        /// <param name="pythonConfig"></param>
        public static void InitializePythonRuntime(IOptions<PythonConfig> pythonConfig)
        {
            if (runtime_initialized)
                return;
            var config = pythonConfig.Value;

            // Set environment variables
            Environment.SetEnvironmentVariable("PYTHONHOME", config.PythonHome, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", config.PythonSitePackages, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", config.PythonDllPath);
            Environment.SetEnvironmentVariable("PYTHONNET_PYVER", config.PythonVersion);
            
            PythonEngine.Initialize();

            PythonEngine.PythonHome = config.PythonHome ?? Environment.GetEnvironmentVariable("PYTHONHOME", EnvironmentVariableTarget.Process)!;
            PythonEngine.PythonPath = config.PythonDllPath ?? Environment.GetEnvironmentVariable("PYTHONNET_PYDLL", EnvironmentVariableTarget.Process)!;

            PythonEngine.BeginAllowThreads();
            AddSitePackagesToPythonPath(pythonConfig);
            runtime_initialized = true;
        }

        /// <summary>
        /// Imports Python modules. Returned are the following modules :
        /// np : numpy
        /// os : OS module (standard library)
        /// mpl : matplotlib
        /// plt : matplotlib.pyplot (it is always cleared using 'clf' to ensure the plot is empty to avoid overlapping with previous plot renderings)
        /// </summary>
        /// <returns></returns>
        public static (dynamic np, dynamic os, dynamic mpl, dynamic plt) ImportPythonModules()
        {
            //using (Py.GIL())
            //{
                dynamic np = Py.Import("numpy");
                dynamic os = Py.Import("os");
                dynamic mpl = Py.Import("matplotlib");
                dynamic plt = Py.Import("matplotlib.pyplot");

                //plt.clf(); //always clear the plot
                return (np, os, mpl, plt);
            //}
        }

        private static void AddSitePackagesToPythonPath(IOptions<PythonConfig> pythonConfig)
        {
            if (!runtime_initialized)
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(pythonConfig.Value.PythonSitePackages);
                    Console.WriteLine(sys.path);

                    //add folders in solution this too with scripts
                    sys.path.append(@"Data/");
                }
            }
        }

        //public static void Initialize()
        //{


        //    using (Py.GIL())
        //    {
        //        dynamic mpl = Py.Import("matplotlib");
        //        dynamic plt = Py.Import("matplotlib.pyplot");

        //        plt.plot(new float[] { 3, 4, 5 }, new float[] { 4, 5, 6 });
        //        plt.figure.savefig("imagematplot.png");
        //    }
        //}
    }
}
