using System;
using System.Linq;

namespace EasyExpression
{
    public delegate object Function(params object[] values);

    public class FormulaAction
    {
        #region Math

        [FunctionRemark("SUM", "[SUM]", "数学", "数值、结果为数值的表达式、结果为数值的函数", "双精度浮点数", "求和\r\n参数以英文逗号分隔，无数量限制")]
        public static object Sum(params object[] values)
        {
            return values.Select(x => Convert.ToDouble(x)).Sum();
        }
        [FunctionRemark("AVG", "[AVG]", "数学", "数值、结果为数值的表达式、结果为数值的函数", "双精度浮点数", "求平均值\r\n参数以英文逗号分隔，无数量限制")]
        public static object Avg(params object[] values)
        {
            return values.Select(x => Convert.ToDouble(x)).Average();
        }

        [FunctionRemark("ROUND", "[ROUND]", "数学", "ROUND(a,b)\r\na=需要进行四舍五入的数字\r\nb=指定的位数，按此为进行精度处理", "双精度浮点数", "求平均值\r\n参数以英文逗号分隔，无数量限制")]
        public static object Round(params object[] values)
        {
            if (double.TryParse((string)values[0], out var value))
            {
                var accuracy = Convert.ToInt32((string)values[1]);
                var mode = Convert.ToInt32(values[2]);
                var delta = 5 / Math.Pow(10, accuracy + 1);
                switch (mode)
                {
                    case -1:
                        return Math.Round(value - delta, accuracy);
                    case 0:
                        return Math.Round(value, accuracy);
                    case 1:
                        return Math.Round(value + delta, accuracy);
                }
            }
            throw new Exception("ROUND execute error");
        }
        #endregion

        #region String

        [FunctionRemark("CONTAINS", "[CONTAINS]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "包含\r\nsource.contains(target)")]
        public static object Contains(params object[] values)
        {
            return ((string)values[0]).Contains((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("EXCLUDING", "[EXCLUDING]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "不包含\r\nsource.excluding(target)")]
        public static object Excluding(params object[] values)
        {
            return ((string)values[0]).Contains((string)values[1]) ? 0d : 1d;
        }
        [FunctionRemark("STARTWITH", "[STARTWITH]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "开头是\r\nsource.startwith(target)")]
        public static object StartWith(params object[] values)
        {
            return ((string)values[0]).StartsWith((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("ENDWITH", "[ENDWITH]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "结尾是\r\nsource.endwith(target)")]
        public static object EndWith(params object[] values)
        {
            return ((string)values[0]).EndsWith((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("EQUALS", "[EQUALS]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "完全匹配\r\nsource.equals(target)")]
        public static object Equals(params object[] values)
        {
            return ((string)values[0]).Equals((string)values[1]) ? 1d : 0d;
        }
        [FunctionRemark("DIFFERENT", "[DIFFERENT]", "字符串", "1. source \r\n 2. target", "0（false）或者1（true）", "不匹配\r\nsource.different(target)")]
        public static object Different(params object[] values)
        {
            return ((string)values[0]).Equals((string)values[1]) ? 0d : 1d;
        }

        #endregion

        #region Time
        [FunctionRemark("EDATE", "[EDATE]", "时间", "EDATE(a,b,c)\r\na=\"StartDate\"\r\nb=Unit", "日期值", "样例:EDATE(\"2019-10-10\",10,\"D\")，返回值代表指定日期之前或之后n天/月/年的日期\r\n参数按照顺序以英文逗号分隔")]
        public static object EDate(params object[] values)
        {
            if (DateTime.TryParse((string)values[0], out var startTime))
            {
                if (int.TryParse((string)values[1], out var delta))
                {
                    var mode = (string)values[2];
                    switch (mode)
                    {
                        case "y":
                        case "Y":
                            startTime = startTime.AddYears(delta);
                            break;
                        case "M":
                            startTime = startTime.AddMonths(delta);
                            break;
                        case "d":
                        case "D":
                            startTime = startTime.AddDays(delta);
                            break;
                        case "h":
                        case "H":
                            startTime = startTime.AddHours(delta);
                            break;
                        case "m":
                            startTime = startTime.AddMinutes(delta);
                            break;
                        case "s":
                        case "S":
                            startTime = startTime.AddSeconds(delta);
                            break;
                        case "f":
                        case "F":
                            startTime = startTime.AddMilliseconds(delta);
                            break;
                    }
                    return startTime;
                }
            }
            throw new Exception("EDATE execute error");
        }

        [FunctionRemark("EODATE", "[EODATE]", "时间", "EODATE(a,b,c)\r\na=\"StartDate\"\r\nb=Months", "日期值", "样例:EODATE(\"2019-10-10\",10,\"S\")，返回StartDate前后n个月最后一天或者第一天的日期\r\n参数按照顺序以英文逗号分隔\r\nC={\"S\" \"E”}")]
        public static object EODate(params object[] values)
        {
            if (DateTime.TryParse((string)values[0], out var startTime))
            {
                if (int.TryParse((string)values[1], out var delta))
                {
                    var mode = (string)values[2];
                    startTime = startTime.AddMonths(delta);
                    switch (mode)
                    {
                        case "s":
                        case "S":
                            return new DateTime(startTime.Year, startTime.Month, 1);
                        case "e":
                        case "E":
                            return new DateTime(startTime.Year, startTime.Month + 1, 1).AddDays(-1);
                    }
                }
            }
            throw new Exception("EODate execute error");
        }

        [FunctionRemark("NOWTIME", "[NOWTIME]", "时间", "无参数", "日期值", "样例:NOWTIME()，获取当前时间，默认返回今天的日期")]
        public static object NowTime(params object[] values)
        {
            return DateTime.Now;
        }

        [FunctionRemark("TIMETOSTRING", "[TIMETOSTRING]", "时间", "TOSTRING(a,b)\r\na=日期、结果为日期的表达式、结果为日期的函数\r\nb=日期格式", "日期值", "样例:TOSTRING(\"2019-10-10\"，\"YYYY.DD.MM\")，返回特定格式")]
        public static object TimeToString(params object[] values)
        {
            if (DateTime.TryParse((string)values[0], out var time))
            {
                return time.ToString((string)values[1]);
            }
            throw new Exception("TIMETOSTRING execute error");
        }

        #endregion
    }
}
