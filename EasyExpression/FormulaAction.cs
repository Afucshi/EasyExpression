using System.Linq;

namespace EasyExpression
{
    public delegate double Function(double[] values);

    public class FormulaAction
    {
        public static double Sum(double[] values)
        {
            return values.Sum();
        }

        public static double Avg(double[] values)
        {
            return values.Average();
        }
    }
}
