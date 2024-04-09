using System.Linq;

namespace EasyExpression
{
    public delegate double Function(params object[] values);

    public class FormulaAction
    {
        public static double Sum(params object[] values)
        {
            return ((double[])values[0]).Sum();
        }

        public static double Avg(params object[] values)
        {
            return ((double[])values[0]).Average();
        }
        public static double Contains(params object[] values)
        {
            return ((string)values[0]).Contains((string)values[1]) ? 1d : 0d;
        }
        public static double Excluding(params object[] values)
        {
            return ((string)values[0]).Contains((string)values[1]) ? 0d : 1d;
        }
        public static double StartWith(params object[] values)
        {
            return ((string)values[0]).StartsWith((string)values[1]) ? 1d : 0d;
        }
        public static double EndWith(params object[] values)
        {
            return ((string)values[0]).EndsWith((string)values[1]) ? 1d : 0d;
        }
        public static double Equals(params object[] values)
        {
            return ((string)values[0]).Equals((string)values[1]) ? 1d : 0d;
        }

        public static double Different(params object[] values)
        {
            return ((string)values[0]).Equals((string)values[1]) ? 0d : 1d;
        }
    }
}
