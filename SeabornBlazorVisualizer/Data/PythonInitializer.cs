using Python.Runtime;

namespace SeabornBlazorVisualizer.Data
{
    public static class PythonInitializer
    {

        public static void Initialize()
        {
            using (Py.GIL())
            {
                dynamic mpl = Py.Import("matplotlib");
                dynamic plt = Py.Import("matplotlib.pyplot");

                plt.plot(new float[] { 3, 4, 5 }, new float[] { 4, 5, 6 });
                plt.figure.savefig("imagematplot.png");
            }
        }
    }
}
