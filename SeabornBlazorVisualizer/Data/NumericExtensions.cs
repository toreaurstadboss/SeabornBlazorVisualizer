using System.Runtime.CompilerServices;

namespace SeabornBlazorVisualizer.Data
{
    public static class NumericExtensions
    {
        
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
