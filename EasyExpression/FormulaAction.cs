using System.Linq;

namespace EasyExpression
{
    public delegate double Function(params object[] values);

    public class FormulaAction
    {
        [FunctionRemark("SUM", "[SUM]", "数学", "数值、结果为数值的表达式、结果为数值的函数", "双精度浮点数", "求和\r\n参数以英文逗号分隔，无数量限制")]
        public static double Sum(params object[] values)
        {
            return ((double[])values[0]).Sum();
        }
        [FunctionRemark("AVG", "[AVG]", "数学", "数值、结果为数值的表达式、结果为数值的函数", "双精度浮点数", "求平均值\r\n参数以英文逗号分隔，无数量限制")]
        public static double Avg(params object[] values)
        {
            return ((double[])values[0]).Average();
        }
        [FunctionRemark("CONTAINS", "[CONTAINS]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "包含\r\nsource.contains(target)")]
        public static double Contains(params object[] values)
        {
            return ((string)values[0]).Contains((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("EXCLUDING", "[EXCLUDING]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "不包含\r\nsource.excluding(target)")]
        public static double Excluding(params object[] values)
        {
            return ((string)values[0]).Contains((string)values[1]) ? 0d : 1d;
        }
        [FunctionRemark("STARTWITH", "[STARTWITH]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "开头是\r\nsource.startwith(target)")]
        public static double StartWith(params object[] values)
        {
            return ((string)values[0]).StartsWith((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("ENDWITH", "[ENDWITH]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "结尾是\r\nsource.endwith(target)")]
        public static double EndWith(params object[] values)
        {
            return ((string)values[0]).EndsWith((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("EQUALS", "[EQUALS]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "完全匹配\r\nsource.equals(target)")]
        public static double Equals(params object[] values)
        {
            return ((string)values[0]).Equals((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("DIFFERENT", "[DIFFERENT]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "不匹配\r\nsource.different(target)")]
        public static double Different(params object[] values)
        {
            return ((string)values[0]).Equals((string)values[1]) ? 0d : 1d;
        }
    }
}
