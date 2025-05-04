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
        /// Imports Python modules. Returned are the following modules:
        /// <para>np (numpy)</para>
        /// <para>os (OS module - standard library)</para>
        /// <para>scipy (scipy)</para>
        /// <para>mpl (matplotlib)</para>
        /// <para>plt (matplotlib.pyplot </para>
        /// </summary>
        /// <returns>Tuple of Python modules</returns>
        public static (dynamic np, dynamic os, dynamic scipy, dynamic mpl, dynamic plt) ImportPythonModules()
        {

            dynamic np = Py.Import("numpy");
            dynamic os = Py.Import("os");
            dynamic mpl = Py.Import("matplotlib");
            dynamic plt = Py.Import("matplotlib.pyplot");
            dynamic scipy = Py.Import("scipy");

            mpl.use("Agg");

            return (np, os, scipy, mpl, plt);
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


    }
}
