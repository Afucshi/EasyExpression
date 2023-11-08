
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyExpression
{
    public class Expression
    {
        public Expression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new Exception("表达式不能为空");
            }
            SourceExpressionString = expression.Trim().Replace("||","|").Replace("&&","&").Replace("==","=").ToLower();
            ExpressionChildren = new List<Expression>();
            Operators = new List<Operator>();
            DataString = string.Empty;
            RealityString = null;
            ExpressionType = ExpressionType.Unknown;
            if (!TryParse()) 
                throw new Exception("表达式解析错误!");
        }

        private Expression()
        {
            ExpressionChildren = new List<Expression>();
            Operators = new List<Operator>();
            DataString = string.Empty;
            SourceExpressionString = string.Empty;
            RealityString = null;
            ExpressionType = ExpressionType.Unknown;
        }

        /// <summary>
        /// 表达式类型
        /// </summary>
        public ExpressionType ExpressionType { get; set; }

        #region private properties
        /// <summary>
        /// 状态(标识表达式是否可以解析)
        /// </summary>
        private bool Status { get; set; } = true;
        /// <summary>
        /// 元素类型
        /// </summary>
        private ElementType ElementType { get; set; }
        /// <summary>
        /// 包含关键字的完整表达式
        /// </summary>
        private string SourceExpressionString { get; set; }
        /// <summary>
        /// 数据值
        /// </summary>
        private string DataString { get; set; }
        private string RealityString { get; set; }
        /// <summary>
        /// 运算符
        /// </summary>
        private List<Operator> Operators { get; set; }
        /// <summary>
        /// 函数类型
        /// </summary>
        private ExecuteType ExecuteType { get; set; }
        /// <summary>
        /// 若表达式为函数类型,则可以调用此委托来计算函数输出值
        /// </summary>
        internal Function Function { get; set; }
        /// <summary>
        /// 子表达式
        /// </summary>
        private List<Expression> ExpressionChildren { get; set; }
        #endregion

        #region public method

        public void LoadArgument(Dictionary<string, string> keyValues)
        {
            if (keyValues.TryGetValue(DataString.Replace("\\", ""), out var v))
            {
                RealityString = v;
            }
            else
            {
                RealityString = DataString;
            }
            foreach (var childExp in ExpressionChildren)
            {
                childExp.LoadArgument(keyValues);
            }
        }

        public void LoadArgument()
        {
            if (ElementType == ElementType.Data || ElementType == ElementType.Function && !string.IsNullOrEmpty(DataString) && double.TryParse(DataString,out var _))
            {
                RealityString = DataString;
            }
            foreach (var childExp in ExpressionChildren)
            {
                childExp.LoadArgument();
            }
        }

        public List<KeyValuePair<string, ElementType>> GetAllParams()
        {
            var childrenResults = new List<KeyValuePair<string, ElementType>>();

            foreach (var childExp in ExpressionChildren)
            {
                switch (childExp.ElementType)
                {
                    case ElementType.Expression:
                        childrenResults.AddRange(childExp.GetAllParams());
                        break;
                    case ElementType.Function:
                    case ElementType.Data:
                    case ElementType.Reference:
                        childrenResults.Add(new KeyValuePair<string, ElementType>(childExp.DataString.Replace("\\", ""), childExp.ElementType));
                        break;
                }
            }
            return childrenResults;
        }

        /*
         * 运算优先级从高到低为：
         * 小括号：()
         * 非：!
         * 乘除：* /
         * 加减：+ -
         * 关系运算符：< > =
         * 逻辑运算符：& ||
         * 
         * 如果是逻辑表达式，则返回值只有0或1，分别代表false和true
         */
        public double Excute()
        {
            var childrenResults = new List<double>();
            foreach (var childExp in ExpressionChildren)
            {
                switch (childExp.ElementType)
                {
                    case ElementType.Expression:
                        childrenResults.Add(childExp.Excute());
                        break;
                    case ElementType.Data:
                        var value = Convert2DoubleValue(childExp.RealityString);
                        childrenResults.Add(value);
                        break;
                    case ElementType.Function:
                        if (childExp.Function == null)
                        {
                            throw new Exception($"不存在函数实例{childExp.ExecuteType}");
                        }
                        var dataArray = childExp.RealityString.Split(',').ToArray() ?? throw new Exception($"函数 {childExp.ExecuteType} 形参 {childExp.DataString} 映射到实参 {childExp.RealityString} 错误");
                        var data = dataArray.Select(x => Convert.ToDouble(x)).ToArray();
                        var v = childExp.Function.Invoke(data);
                        childrenResults.Add(v);
                        break;
                    default:
                        break;
                }
            }

            /*
             * 优先级
             * 1. 算术运算
             * 2. 关系运算
             * 3. 逻辑运算
             * 
             * 【注】：因为针对优先级进行了表达式树的重构，所以每一层级的所有运算符都是同一优先级，因此，这里按照顺序执行即可
             */

            var result = childrenResults.First();
            //计算逻辑与和逻辑或,顺序执行
            for (int i = 0; i < Operators.Count; i++)
            {
                //非运算特殊，它只需要一个操作数就可完成计算，其他运算符至少需要两个
                var value = Operators[i] == Operator.Not ? childrenResults[i] : childrenResults[i + 1];
                switch (Operators[i])
                {
                    case Operator.None:
                        break;
                    case Operator.And:
                        result = result != 0d && value != 0d ? 1d : 0d;
                        break;
                    case Operator.Or:
                        result = result != 0d || value != 0d ? 1d : 0d;
                        break;
                    case Operator.Not:
                        result = value != 0d ? 0d : 1d;
                        break;
                    case Operator.Plus:
                        result += value;
                        break;
                    case Operator.Subtract:
                        result -= value;
                        break;
                    case Operator.Multiply:
                        result *= value;
                        break;
                    case Operator.Divide:
                        result /= value;
                        break;
                    case Operator.Mod:
                        result %= value;
                        break;
                    case Operator.GreaterThan:
                        result = result > value ? 1d : 0d;
                        break;
                    case Operator.LessThan:
                        result = result < value ? 1d : 0d;
                        break;
                    case Operator.Equals:
                        result = (result - value) < 0.001d ? 1d : 0d;
                        break;
                    case Operator.GreaterThanOrEquals:
                        result = result >= value ? 1d : 0d;
                        break;
                    case Operator.LessThanOrEquals:
                        result = result <= value ? 1d : 0d;
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        #endregion

        #region private for parse

        public List<KeyValuePair<string, ElementType>> ReplaceParams(Dictionary<string, object> keyValues)
        {
            var childrenResults = new List<KeyValuePair<string, ElementType>>();

            foreach (var childExp in ExpressionChildren)
            {
                switch (childExp.ElementType)
                {
                    case ElementType.Expression:
                        childrenResults.AddRange(childExp.GetAllParams());
                        break;
                    case ElementType.Function:
                    case ElementType.Data:
                    case ElementType.Reference:
                        childrenResults.Add(new KeyValuePair<string, ElementType>(childExp.DataString.Replace("\\", ""), childExp.ElementType));
                        break;
                }
            }
            return childrenResults;
        }

        private bool TryParse()
        {
            bool result;
            try
            {
                Parse(this);
                result = ParseStatus(this);
                if (!result)
                {
                    result = false;
                }
                //根据运算优先级，重组表达式树
                RebuildExpression();
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private void Parse(Expression expression)
        {
            for (int index = 0; index < expression.SourceExpressionString.Length; index++)
            {
                (bool Status, int EndIndex, string ChildrenExpressionString) matchScope = default;
                var currentChar = expression.SourceExpressionString[index];
                var (mode, endTag) = SetMatchMode(currentChar);
                switch (mode)
                {
                    case MatchMode.Scope:
                        matchScope = FindEnd('(', endTag, expression.SourceExpressionString, index);
                        expression.Status = matchScope.Status;
                        break;
                    case MatchMode.RelationSymbol:
                        var relationSymbolStr = GetFullSymbol(expression.SourceExpressionString, index);
                        ExpressionType = ExpressionType.Relation;
                        //去除可能存在的空字符
                        var relationSymbol = ConvertOperator(relationSymbolStr.Replace(" ",""));
                        expression.Operators.Add(relationSymbol);
                        expression.ElementType = ElementType.Expression;
                        //如果关系运算符为单字符，则索引+0，如果为多字符（<和=中间有空格，需要忽略掉），则跳过这段。eg: <；<=；<  =；
                        index += relationSymbolStr.Length - 1;
                        continue;
                    case MatchMode.LogicSymbol:
                        ExpressionType = ExpressionType.Logic;
                        var logicSymbol = ConvertOperator(currentChar.ToString());
                        expression.Operators.Add(logicSymbol);
                        expression.ElementType = ElementType.Expression;
                        continue;
                    case MatchMode.ArithmeticSymbol:
                        ExpressionType = ExpressionType.Arithmetic;
                        var operatorSymbol = ConvertOperator(currentChar.ToString());
                        expression.Operators.Add(operatorSymbol);
                        expression.ElementType = ElementType.Expression;
                        continue;
                    case MatchMode.Function:
                        matchScope = FindEnd('[', endTag, expression.SourceExpressionString, index);
                        //确定函数类型
                        var (executeType, function) = GetFunctionType(matchScope.ChildrenExpressionString);
                        var functionStr = $"[{matchScope.ChildrenExpressionString}]";
                        //如果是函数，则匹配函数内的表达式,eg: [sum](****)
                        matchScope = FindEnd('(', ')', expression.SourceExpressionString, matchScope.EndIndex + 1);
                        functionStr += $"({matchScope.ChildrenExpressionString})";
                        var functionExp = new Expression
                        {
                            ElementType = ElementType.Function,
                            ExecuteType = executeType,
                            Function = function,
                            SourceExpressionString = functionStr,
                            DataString = matchScope.ChildrenExpressionString
                        };
                        expression.ExpressionChildren.Add(functionExp);
                        break;
                    case MatchMode.Data:
                        var str = GetFullData(expression.SourceExpressionString, index);
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            if (str.Equals(expression.SourceExpressionString))
                            {
                                expression.ElementType = ElementType.Data;
                                expression.DataString = str;
                                return;
                            }
                            var dataExp = new Expression(str);
                            if (dataExp.ExpressionType == ExpressionType.Logic)
                            {
                                ExpressionType = ExpressionType.Logic;
                            }
                            expression.ExpressionChildren.Add(dataExp);
                        }
                        index += str.Length - 1;
                        continue;
                    case MatchMode.EscapeCharacter:
                        //跳过转义符号
                        index++;
                        continue;
                    default:
                        break;
                }
                if (!expression.Status)
                {
                    break;
                }
                // 递归解析子表达式
                var isOver = ElementType == ElementType.Data || IsOver(matchScope.ChildrenExpressionString);
                if (!isOver)
                {
                    var expressionChildren = new Expression(matchScope.ChildrenExpressionString);
                    expression.ExpressionChildren.Add(expressionChildren);
                }
                // 跳过已解析的块
                index = matchScope.EndIndex;
            }
        }

        private static string GetFullSymbol(string exp, int startIndex)
        {
            if (startIndex == exp.Length) return exp.Last() + "";
            var result = "" + exp[startIndex];
            for (int i = startIndex + 1; i < exp.Length; i++)
            {
                if (exp[i] == ' ' && i - startIndex == result.Length)
                {
                    result += exp[i];
                    continue;
                }
                var (mode, endTag) = SetMatchMode(exp[i]);
                if (mode == MatchMode.RelationSymbol)
                {
                    result += exp[i];
                    break;
                }
            }
            return result;
        }

        private static string GetFullData(string exp,int startIndex)
        {
            if (startIndex == exp.Length) return exp.Last() + "";
            var result = "" + exp[startIndex];
            for (int i = startIndex + 1; i < exp.Length; i++)
            {
                var (mode, endTag) = SetMatchMode(exp[i]);
                switch (mode)
                {
                    case MatchMode.Data:
                        result += exp[i];
                        continue;
                    case MatchMode.LogicSymbol:
                        return result;
                    case MatchMode.ArithmeticSymbol:
                        return result;
                    case MatchMode.RelationSymbol:
                        return result;
                    case MatchMode.Scope:
                        return result;
                    case MatchMode.EscapeCharacter:
                        //跳过转义符及后面一个字符
                        result += exp[i];
                        result += exp[i+1];
                        i++;
                        continue;
                    default:
                        return result;
                }
            }
            return result;
        }
        private static (ExecuteType executeType, Function function) GetFunctionType(string key)
        {
            switch (key)
            {
                case "sum":
                    return (ExecuteType.Sum, FormulaAction.Sum);
                case "avg":
                    return (ExecuteType.Avg, FormulaAction.Avg);
                default:
                    throw new Exception($"{key} 函数未定义");
            }
        }
        private static bool IsOver(string expressionString)
        {
            if (string.IsNullOrEmpty(expressionString))
            {
                return true;
            }
            else
            {
                return !(Contains(expressionString,'(') 
                    || Contains(expressionString, '[')
                    || Contains(expressionString, '&')
                    || Contains(expressionString, '|')
                    || Contains(expressionString, '!')
                    || Contains(expressionString, '>')
                    || Contains(expressionString, '<')
                    || Contains(expressionString, '=')
                    || Contains(expressionString, '+')
                    || Contains(expressionString, '-')
                    || Contains(expressionString, '*')
                    || Contains(expressionString, '/')
                    || Contains(expressionString, '%'));
            }
        }

        /// <summary>
        /// 判断是否包含指定字符，且未被转义
        /// </summary>
        /// <param name="text"></param>
        /// <param name="contains"></param>
        /// <returns></returns>
        private static bool Contains(string text, char contains)
        {
            var lastChar = char.MinValue;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == contains)
                {
                    if (lastChar != '\\') return true;
                }
                lastChar = text[i];
            }
            return false;
        }

        private (bool Status, int EndIndex, string ChildrenExpressionString) FindEnd(char startTag, char? endTag, string formula, int index)
        {
            var result = (Status: true, EndIndex: 0, ChildrenExpressionString: "");
            try
            {
                int currentLevel = 0;
                int? endIndex = null;
                for (; index < formula.Length; index++)
                {
                    var currentChar = formula[index];
                    // 第一次匹配到startTag不加层级，因为它的层级就是0
                    if (currentChar == startTag) //
                    {
                        currentLevel++;
                        if (currentLevel == 1)
                        {
                            continue;
                        }
                    }
                    else if (currentChar == endTag)
                    {
                        currentLevel--;
                    }
                    // 层级相同且与结束标志一致，则返回结束标志索引
                    if (currentLevel == 0 && currentChar == endTag)
                    {
                        endIndex = index;
                        break;
                    }
                    result.ChildrenExpressionString += currentChar;
                }
                if (endIndex.HasValue)
                {
                    result.EndIndex = endIndex.Value;
                }
                else
                {
                    result.Status = false;
                }
            }
            catch
            {
                result.Status = false;
            }
            return result;
        }

        private static (MatchMode Mode, char? EndTag) SetMatchMode(char currentChar)
        {
            switch (currentChar)
            {
                case '(':
                    return (MatchMode.Scope, ')');
                case '[':
                    return (MatchMode.Function, ']');
                case '&':
                    return (MatchMode.LogicSymbol, null);
                case '|':
                    return (MatchMode.LogicSymbol, null);
                case '!':
                    return (MatchMode.LogicSymbol, null);
                case '+':
                    return (MatchMode.ArithmeticSymbol, null);
                case '-':
                    return (MatchMode.ArithmeticSymbol, null);
                case '*':
                    return (MatchMode.ArithmeticSymbol, null);
                case '/':
                    return (MatchMode.ArithmeticSymbol, null);
                case '%':
                    return (MatchMode.ArithmeticSymbol, null);
                case '<':
                    return (MatchMode.RelationSymbol, null);
                case '>':
                    return (MatchMode.RelationSymbol, null);
                case '=':
                    return (MatchMode.RelationSymbol, null);
                case '\\':
                    return (MatchMode.EscapeCharacter, null);
                default:
                    return (MatchMode.Data, null);
            }
        }

        private static Operator ConvertOperator(string currentChar)
        {
            switch (currentChar)
            {
                case "&":
                    return Operator.And;
                case "|":
                    return Operator.Or;
                case "!":
                    return Operator.Not;
                case "+":
                    return Operator.Plus;
                case "-":
                    return Operator.Subtract;
                case "*":
                    return Operator.Multiply;
                case "/":
                    return Operator.Divide;
                case "%":
                    return Operator.Mod;
                case ">":
                    return Operator.GreaterThan;
                case "<":
                    return Operator.LessThan;
                case "=":
                    return Operator.Equals;
                case "<=":
                case "=<":
                    return Operator.LessThanOrEquals;
                case ">=":
                case "=>":
                    return Operator.GreaterThanOrEquals;
                default:
                    return Operator.None;
            }
        }

        private bool ParseStatus(Expression expression)
        {
            var result = true;
            try
            {
                if (expression == null || !expression.Status)
                {
                    result = false;
                    return result;
                }
                var status = new List<bool>();
                GetExpressionStatus(expression, ref status);
                if (status.Contains(false))
                {
                    result = false;
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private void GetExpressionStatus(Expression expression, ref List<bool> status)
        {
            if (!status.Contains(expression.Status))
            {
                status.Add(expression.Status);
            }
            if (!expression.Status)
            {
                return;
            }
            if (expression.ExpressionChildren.Any())
            {
                foreach (var item in expression.ExpressionChildren)
                {
                    if (!status.Contains(item.Status))
                    {
                        status.Add(item.Status);
                    }
                    if (item.ExpressionChildren.Any())
                    {
                        GetExpressionStatus(item, ref status);
                    }
                }
            }
        }

        #region 根据运算优先级重组表达式树

        private void RebuildExpression()
        {
            if (!Operators.Any()) return;
            while (true)
            {
                var count = Operators.Select(x => x.GetOperatorObj().Level).Distinct();
                //如果只有同一种优先级的运算符，则说明当前层级可以顺序执行，不需要重构了，直接返回
                if (count.Count() == 1)
                {
                    ExpressionType = GetExpressionType(Operators[0].GetOperatorObj().Level);
                    return;
                }
                //获取最高优先级，并重组
                var level = count.Max();
                //获取需要合并的优先级所有子表达式的操作符
                var operators = GetTargetLevelOperators(Operators, level);
                var type = GetExpressionType(level);
                foreach (var list in operators)
                {
                    //因为是倒序，所以起始位置是反的
                    var startIndex = list.Last();
                    var endIndex = list.First();
                    //获取需要合并为子表达式的exp，+2是因为索引本身比数量小1，且操作符数量始终比操作数少1
                    var children = ExpressionChildren.Skip(startIndex).Take(endIndex - startIndex + 2).ToList();
                    var childrenOperators = new List<Operator>();
                    list.ForEach(x => childrenOperators.Add(Operators[x]));
                    //合并为一个新的表达式
                    var newExp = BuildChildren(children, childrenOperators);
                    newExp.ExpressionType = type;
                    //在表达式及操作符集合中删除合并的部分
                    ExpressionChildren.Insert(startIndex, newExp);
                    children.ForEach(x => ExpressionChildren.Remove(x));
                    list.ForEach(Operators.RemoveAt);
                }
            }
        }

        private static ExpressionType GetExpressionType(int level)
        {
            switch (level)
            {
                case 1:
                case 2:
                    return ExpressionType.Logic;
                case 3:
                    return ExpressionType.Relation;
                case 4:
                case 5:
                    return ExpressionType.Arithmetic;
                default:
                    return ExpressionType.Unknown;
            }
        }

        private static Expression BuildChildren(List<Expression> expressions, List<Operator> operators)
        {
            var dataString = expressions[0].DataString;
            for (int i = 1; i < expressions.Count; i++)
            {
                var childStr = expressions[i].DataString;
                if (expressions[i].ElementType == ElementType.Expression)
                {
                    childStr = $"({expressions[i].SourceExpressionString})";
                }
                dataString += $"{operators[i - 1].GetOperatorObj().Value}{childStr}";
            }
            var exp = new Expression()
            {
                ExpressionChildren = expressions,
                Operators = operators,
                DataString = dataString,
                ElementType = ElementType.Expression,
                SourceExpressionString = dataString,
                Status = true,
                ExecuteType = ExecuteType.None
            };
            return exp;
        }

        /// <summary>
        /// 获取相同级别且连续的子表达式运算符
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private List<List<int>> GetTargetLevelOperators(List<Operator> oldOperators, int level)
        {
            /*
             * eg:
             * 序列为{2,3,1,3,3,2,3,3}, 输入为3，最终输出为各元素的索引集合，{1},{3,4},{6,7}
             */
            var result = new List<List<int>>();
            var operators = new List<int>();
            //此处倒序循环是为了方便后续做删除操作，否则删除后索引的变化会导致数组越界
            for (int i = oldOperators.Count - 1; i >= 0; i--)
            {
                if (Operators[i].GetOperatorObj().Level == level)
                {
                    operators.Add(i);
                }
                else
                {
                    if (operators.Any())
                    {
                        result.Add(operators.DeepCopy());
                        operators.Clear();
                    }
                }
            }
            if (operators.Any())
            {
                result.Add(operators);
            }
            return result;
        }

        #endregion

        #endregion

        #region private for excute

        private static double Convert2DoubleValue(string tag)
        {
            switch (tag)
            {
                case "true":
                    return 1d;
                case "false":
                    return 0d;
                case "1":
                    return 1d;
                case "0":
                    return 0d;
                default:
                    return double.TryParse(tag, out double result) ? result : throw new Exception($"{tag} 不是数值类型");
            }
        }

        #endregion
    }
}
