using MathNet.Numerics.Distributions;
using System.Runtime.CompilerServices;

namespace SeabornBlazorVisualizer.Data
{
    public static class NumericExtensions
    {

        public static IEnumerable<double> GeneratedStandardNormalSamples(int count)
        {
            var normalDistribution = new Normal(0, 1); //MathNet.Numerics lib - Normal distribution https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Distributions/Normal.cs
            foreach (var n in Enumerable.Range(0, count))
            {
                yield return normalDistribution.Sample();
            }
        }

        public static IEnumerable<double> CumulativeSum(this IEnumerable<double> sequence)
        {
            double sum = 0;
            foreach (var item in sequence)
            {
                sum += item;
                yield return sum;
            }
        }





    }
}
