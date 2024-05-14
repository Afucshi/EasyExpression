using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyExpression
{
    public class Expression
    {
        public Expression(string expression)
        {
            Init(expression);
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

        private void Init(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ExpressionException("表达式不能为空");
            }
            SourceExpressionString = expression.Trim().Replace("||", "|").Replace("&&", "&").Replace("==", "=");
            ExpressionChildren = new List<Expression>();
            Operators = new List<Operator>();
            DataString = string.Empty;
            RealityString = null;
            ExpressionType = ExpressionType.Unknown;
            if (!TryParse())
                throw new ExpressionException($"表达式: {SourceExpressionString} 解析错误!");
        }

        public string ErrorMessgage { get; set; }

        /// <summary>
        /// 表达式类型
        /// </summary>
        public ExpressionType ExpressionType { get; set; }

        #region private properties
        /// <summary>
        /// 状态(标识表达式是否可以解析)
        /// </summary>
        public bool Status { get; set; } = true;
        /// <summary>
        /// 元素类型
        /// </summary>
        public ElementType ElementType { get; set; }
        /// <summary>
        /// 包含关键字的完整表达式
        /// </summary>
        public string SourceExpressionString { get; set; }
        /// <summary>
        /// 数据值
        /// </summary>
        public string DataString { get; set; }
        public string RealityString { get; set; }
        /// <summary>
        /// 运算符
        /// </summary>
        public List<Operator> Operators { get; set; }
        /// <summary>
        /// 函数类型
        /// </summary>
        public ExecuteType ExecuteType { get; set; }
        /// <summary>
        /// 若表达式为函数类型,则可以调用此委托来计算函数输出值
        /// </summary>    
        private Function Function { get; set; }
        public string FunctionName { get; set; }
        /// <summary>
        /// 子表达式
        /// </summary>
        public List<Expression> ExpressionChildren { get; set; }
        #endregion

        #region public method

        public List<KeyValuePair<string, string>> LoadArgument(Dictionary<string, string> keyValues)
        {
            var result = new List<KeyValuePair<string, string>>();
            LoadArgument(keyValues, result);
            return result;
        }

        private void LoadArgument(Dictionary<string, string> keyValues, List<KeyValuePair<string, string>> result)
        {
            if (!string.IsNullOrEmpty(DataString))
            {
                if (ElementType == ElementType.Function)
                {
                    var array = DataString.Split(',');
                    var pList = new List<string>();
                    foreach (var text in array)
                    {
                        if (keyValues.TryGetValue(text, out var v))
                        {
                            pList.Add(v);
                        }
                        else
                        {
                            pList.Add(text);
                        }
                    }
                    RealityString = pList.Aggregate((a, b) => { return $"{a},{b}"; });
                }
                else
                {
                    if (keyValues.TryGetValue(DataString, out var v))
                    {
                        RealityString = v;
                        result.Add(new KeyValuePair<string, string>(DataString, v));
                    }
                    else
                    {
                        RealityString = DataString;
                    }
                }
            }
            else
            {
                RealityString = DataString;
            }

            foreach (var childExp in ExpressionChildren)
            {
                childExp.LoadArgument(keyValues, result);
            }
        }

        public void LoadArgument()
        {
            if (ElementType == ElementType.Data || ElementType == ElementType.Function && !string.IsNullOrEmpty(DataString) && double.TryParse(DataString, out var _))
            {
                RealityString = DataString;
            }
            foreach (var childExp in ExpressionChildren)
            {
                childExp.LoadArgument();
            }
        }

        /// <summary>
        /// 判断表达式完备性
        /// </summary>
        public void Check()
        {
            CheckExpression(this);
        }

        private void CheckExpression(Expression expression)
        {
            /*
             * 1. 除了非运算只需要一个数据，其他的运算符至少需要2个数据
             */
            if (expression.Operators.Any())
            {
                var notOperatorCount = expression.Operators.Count(x => x == Operator.Not);
                if (expression.ExpressionChildren.Count - (expression.Operators.Count - notOperatorCount) != 1)
                {
                    throw new ExpressionException("expression check error: data not match operator");
                }
            }

            foreach (var child in expression.ExpressionChildren)
            {
                CheckExpression(child);
            }
        }

        public List<KeyValuePair<string, ElementType>> GetAllParams()
        {
            var results = new List<KeyValuePair<string, ElementType>>();
            if (ElementType == ElementType.Data && ExpressionChildren.Count == 0)
            {
                results.Add(new KeyValuePair<string, ElementType>(DataString.Replace("\\", ""), ElementType));
            }
            else
            {
                results.AddRange(GetChildrenAllParams(this));
            }
            return results;
        }

        private List<KeyValuePair<string, ElementType>> GetChildrenAllParams(Expression parent = null)
        {
            var childrenResults = new List<KeyValuePair<string, ElementType>>();
            foreach (var childExp in ExpressionChildren)
            {
                switch (childExp.ElementType)
                {
                    case ElementType.Expression:
                        childrenResults.AddRange(childExp.GetChildrenAllParams(childExp));
                        break;
                    case ElementType.Function:
                        if (childExp.ExpressionChildren.Any())
                        {
                            childrenResults.AddRange(childExp.GetChildrenAllParams(childExp));
                        }
                        else
                        {
                            var paramList = childExp.DataString.Split(',').ToList();
                            paramList.ForEach(x => { childrenResults.Add(new KeyValuePair<string, ElementType>(x.Replace("\\", ""), ElementType.Function)); });
                        }
                        break;
                    case ElementType.Data:
                    case ElementType.Reference:
                        var type = ElementType.Data;
                        if (parent != null && parent.ElementType != ElementType.Expression)
                        {
                            type = parent.ElementType = parent.ElementType;
                        }
                        childrenResults.Add(new KeyValuePair<string, ElementType>(childExp.DataString.Replace("\\", ""), type));
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
        public double Execute()
        {
            var result = ExecuteChildren();
            return result.First();
        }

        private List<double> ExecuteChildren()
        {
            var childrenResults = new List<double>();
            if (ExpressionChildren.Count == 0)
            {
                childrenResults.Add(ExecuteNode(this));
                return childrenResults;
            }
            foreach (var childExp in ExpressionChildren)
            {
                childrenResults.Add(ExecuteNode(childExp));
            }

            /*
             * 优先级
             * 1. 算术运算
             * 2. 关系运算
             * 3. 逻辑运算
             * 
             * 【注】：因为针对优先级进行了表达式树的重构，所以每一层级的所有运算符都是同一优先级，因此，这里按照顺序执行即可
             */
            if (Operators.Count == 0)
            {
                return childrenResults;
            }
            var result = childrenResults.First();
            //计算逻辑与和逻辑或,顺序执行
            for (int i = 0; i < Operators.Count; i++)
            {
                //非运算和负数特殊，它只需要一个操作数就可完成计算，其他运算符至少需要两个
                var value = Operators[i] == Operator.Not || Operators[i] == Operator.Negative ? childrenResults[i] : childrenResults[i + 1];
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
                        result = result - value < double.MinValue ? 1d : 0d;
                        break;
                    case Operator.UnEquals:
                        result = result - value != 0 ? 1d : 0d;
                        break;
                    case Operator.GreaterThanOrEquals:
                        result = result >= value ? 1d : 0d;
                        break;
                    case Operator.LessThanOrEquals:
                        result = result <= value ? 1d : 0d;
                        break;
                    case Operator.Negative:
                        result = value * -1;
                        break;
                    default:
                        break;
                }
            }
            childrenResults.Clear();
            childrenResults.Add(result);
            return childrenResults;
        }

        private double ExecuteNode(Expression childExp)
        {
            switch (childExp.ElementType)
            {
                case ElementType.Expression:
                    return childExp.Execute();
                case ElementType.Data:
                    var value = Convert2DoubleValue(childExp.RealityString);
                    return value;
                case ElementType.Function:
                    if (childExp.Function == null)
                    {
                        throw new ExpressionException($"at {SourceExpressionString}: 不存在函数实例{childExp.ExecuteType}");
                    }
                    double v = 0d;
                    switch (childExp.ExecuteType)
                    {
                        case ExecuteType.None:
                            v = childExp.Function.Invoke();
                            break;
                        case ExecuteType.Sum:
                        case ExecuteType.Avg:
                            var dataArray = childExp.RealityString?.Split(',').ToArray();
                            if (dataArray == null)
                            {
                                if (childExp.ExpressionChildren.Count == 0)
                                {
                                    throw new ExpressionException($"at {SourceExpressionString}: 函数 {childExp.ExecuteType} 形参 {childExp.DataString} 映射到实参 {childExp.RealityString} 错误");
                                }
                                var paramList = new List<double>();
                                foreach (var child in childExp.ExpressionChildren)
                                {
                                    child.ExecuteChildren().ForEach(x =>
                                    {
                                        paramList.Add(x);
                                    });
                                }
                                v = childExp.Function.Invoke(paramList.ToArray());
                            }
                            else
                            {
                                var data = dataArray.Select(x => Convert.ToDouble(x)).ToArray();
                                v = childExp.Function.Invoke(data);
                            }
                            break;
                        case ExecuteType.Contains:
                        case ExecuteType.ContainsExcept:
                        case ExecuteType.Equals:
                        case ExecuteType.StartWith:
                        case ExecuteType.EndWith:
                        case ExecuteType.Different:
                            var paramArray = childExp.RealityString.Split(',').ToList();
                            if (paramArray.Count != 2)
                            {
                                throw new ExpressionException($"at {SourceExpressionString}: 函数 {childExp.ExecuteType} 形参 {childExp.DataString} 映射到实参 {childExp.RealityString} 错误");
                            }
                            v = childExp.Function.Invoke(paramArray[0], paramArray[1]);
                            break;
                        case ExecuteType.Customer:
                            break;
                    }
                    return v;
                default:
                    throw new ExpressionException($"at {SourceExpressionString}: 未知表达式节点");
            }
        }

        #endregion

        #region private for parse
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
            catch (Exception ex)
            {
                result = false;
                ErrorMessgage = ex.Message;
            }
            return result;
        }

        private void Parse(Expression expression)
        {
            var lastBlock = MatchMode.None;
            for (int index = 0; index < expression.SourceExpressionString.Length; index++)
            {
                (bool Status, int EndIndex, string ChildrenExpressionString) matchScope = default;
                var currentChar = expression.SourceExpressionString[index];
                var (mode, endTag) = SetMatchMode(currentChar, lastBlock);
                switch (mode)
                {
                    case MatchMode.Scope:
                        matchScope = FindEnd('(', endTag, expression.SourceExpressionString, index);
                        expression.Status = matchScope.Status;
                        break;
                    case MatchMode.RelationSymbol:
                        var relationSymbolStr = GetFullSymbol(expression.SourceExpressionString, index, mode);
                        ExpressionType = ExpressionType.Relation;
                        //去除可能存在的空字符
                        var relationSymbol = ConvertOperator(relationSymbolStr.Replace(" ", ""));
                        expression.Operators.Add(relationSymbol);
                        expression.ElementType = ElementType.Expression;
                        //如果关系运算符为单字符，则索引+0，如果为多字符（<和=中间有空格，需要忽略掉），则跳过这段。eg: <；<=；<  =；
                        index += relationSymbolStr.Length - 1;
                        lastBlock = mode;
                        continue;
                    case MatchMode.LogicSymbol:
                        var logicSymbolStr = GetFullSymbol(expression.SourceExpressionString, index, mode);
                        var logicSymbol = ConvertOperator(logicSymbolStr.Replace(" ", ""));
                        //因为! 既可以单独修饰一个数据，当作逻辑非，也可以与=联合修饰两个数据，当作不等于，所以此处需要进行二次判定。如果是!=，则此符号为关系运算符
                        ExpressionType = logicSymbol == Operator.UnEquals ? ExpressionType.Relation : ExpressionType.Logic;
                        expression.Operators.Add(logicSymbol);
                        expression.ElementType = ElementType.Expression;
                        index += logicSymbolStr.Length - 1;
                        lastBlock = mode;
                        continue;
                    case MatchMode.ArithmeticSymbol:
                        ExpressionType = ExpressionType.Arithmetic;
                        var operatorSymbol = ConvertOperator(currentChar.ToString());
                        expression.Operators.Add(operatorSymbol);
                        expression.ElementType = ElementType.Expression;
                        lastBlock = mode;
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
                            FunctionName = executeType.ToString(),
                            SourceExpressionString = functionStr,
                            DataString = matchScope.ChildrenExpressionString
                        };
                        expression.ExpressionChildren.Add(functionExp);
                        var paramList = SplitParamObject(matchScope.ChildrenExpressionString);
                        paramList.ForEach(x =>
                        {
                            var paramExp = new Expression(x);
                            //functionExp.ExpressionChildren.AddRange(paramExp.ExpressionChildren);
                            functionExp.ExpressionChildren.Add(paramExp);
                        });
                        //函数解析完毕后直接从函数后面位置继续
                        index = matchScope.EndIndex;
                        lastBlock = mode;
                        continue;
                    case MatchMode.Data:
                        if (string.IsNullOrWhiteSpace(currentChar.ToString())) continue;
                        lastBlock = mode;
                        var (str, dataMtachMode) = GetFullData(expression.SourceExpressionString, index, lastBlock);
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
                            if (dataMtachMode == MatchMode.Scope && currentChar == '-')
                            {
                                //如果在Data分支下获取完整数据包含范围描述符号，即小括号，则认为这个负号修饰的是表达式，增加一个负号运算符
                                expression.Operators.Add(Operator.Negative);
                                continue;
                            }
                            expression.ExpressionChildren.Add(dataExp);
                        }
                        index += str.Length - 1;
                        continue;
                    case MatchMode.EscapeCharacter:
                        //跳过转义符号
                        index++;
                        lastBlock = mode;
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
                lastBlock = mode;
            }
        }

        private string GetFullSymbol(string exp, int startIndex, MatchMode matchMode)
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
                var (mode, _) = SetMatchMode(exp[i], matchMode);
                if (mode == MatchMode.RelationSymbol && matchMode == MatchMode.RelationSymbol)
                {
                    result += exp[i];
                    break;
                }
                else if (mode == MatchMode.LogicSymbol && exp[startIndex] == '!' && matchMode == MatchMode.LogicSymbol)
                {
                    result += exp[i];
                    break;
                }
                if (mode == MatchMode.Data) break;
                matchMode = mode;
            }
            return result;
        }

        private (string value, MatchMode mode) GetFullData(string exp, int startIndex, MatchMode matchMode)
        {
            if (startIndex == exp.Length) return (exp.Last() + "", MatchMode.Data);
            var result = "" + exp[startIndex];
            for (int i = startIndex + 1; i < exp.Length; i++)
            {
                var (mode, _) = SetMatchMode(exp[i], matchMode);
                switch (mode)
                {
                    case MatchMode.Data:
                        result += exp[i];
                        matchMode = mode;
                        continue;
                    case MatchMode.LogicSymbol:
                        return (result, MatchMode.LogicSymbol);
                    case MatchMode.ArithmeticSymbol:
                        return (result, MatchMode.ArithmeticSymbol);
                    case MatchMode.RelationSymbol:
                        return (result, MatchMode.RelationSymbol);
                    case MatchMode.Scope:
                        var matchScope = FindEnd('(', ')', exp, i);
                        return (matchScope.ChildrenExpressionString, MatchMode.Scope);
                    case MatchMode.EscapeCharacter:
                        //跳过转义符及后面一个字符
                        result += exp[i];
                        result += exp[i + 1];
                        i++;
                        matchMode = mode;
                        continue;
                    default:
                        return (result, MatchMode.Data);
                }
            }
            return (result, MatchMode.Data);
        }
        private (ExecuteType executeType, Function function) GetFunctionType(string key)
        {
            switch (key.ToLower())
            {
                case "sum":
                    return (ExecuteType.Sum, FormulaAction.Sum);
                case "avg":
                    return (ExecuteType.Avg, FormulaAction.Avg);
                case "contains":
                    return (ExecuteType.Contains, FormulaAction.Contains);
                case "excluding":
                    return (ExecuteType.ContainsExcept, FormulaAction.Excluding);
                case "equals":
                    return (ExecuteType.Equals, FormulaAction.Equals);
                case "startwith":
                    return (ExecuteType.StartWith, FormulaAction.StartWith);
                case "endwith":
                    return (ExecuteType.EndWith, FormulaAction.EndWith);
                case "different":
                    return (ExecuteType.Different, FormulaAction.Different);
                // 自定义函数实现
                default:
                    throw new ExpressionException($"at {SourceExpressionString}: {key} 函数未定义");
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
                return !(Contains(expressionString, '(')
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

        /// <summary>
        /// 匹配标记范围（配对标记，比如( 和 ），[ 和 ]；start和end不能相同）
        /// </summary>
        /// <param name="startTag"></param>
        /// <param name="endTag"></param>
        /// <param name="formula"></param>
        /// <param name="index"></param>
        /// <param name="containsStartTag"></param>
        /// <returns></returns>
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

        private List<string> SplitParamObject(string srcString)
        {
            var result = new List<string>();
            var paramString = string.Empty;
            var areaLevel = 0;
            for (int i = 0; i < srcString.Length; i++)
            {
                var currentChar = srcString[i];
                //()或[]封闭空间内的参数分隔符 , 需要忽略，因为它属于子表达式范围，不用在本层级分析，只把它当作普通字符即可
                switch (currentChar)
                {
                    case ',':
                        if (!string.IsNullOrEmpty(paramString) && areaLevel == 0)
                        {
                            result.Add(paramString);
                            paramString = string.Empty;
                            continue;
                        }
                        break;
                    case '(':
                    case '[':
                        //封闭空间开始,提升层级
                        areaLevel++;
                        break;
                    case ')':
                    case ']':
                        //封闭空间结束,降低层级
                        areaLevel--;
                        break;
                    default:
                        break;
                }
                paramString += currentChar;
            }
            if (!string.IsNullOrEmpty(paramString))
            {
                result.Add(paramString);
            }
            return result;
        }

        private (MatchMode Mode, char? EndTag) SetMatchMode(char currentChar, MatchMode lastMode)
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
                    //有可能是负号，也有可能是减号;上一个block是符号或者none，这此处应该当作负号处理
                    if (lastMode == MatchMode.None || lastMode == MatchMode.ArithmeticSymbol || lastMode == MatchMode.LogicSymbol || lastMode == MatchMode.RelationSymbol)
                    {
                        return (MatchMode.Data, null);
                    }
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
                    /*=继承上一个相邻符号的类型，比如<=,>=，此时=号为关系运算符；上一个为逻辑运算符的话，此处=为逻辑运算符，比如 !=；如果上一个block不为符号，那么此时=为等于（关系运算符）
                     因此，只有上一个block为逻辑运算符时，才返回logicSymbol，其他情况返回relationSymbol
                     */
                    if (lastMode == MatchMode.LogicSymbol)
                    {
                        return (MatchMode.LogicSymbol, null);
                    }
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
                    //负号特殊,此处算作减号
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
                case "!=":
                    return Operator.UnEquals;
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
            if (expressions[0].ElementType == ElementType.Function)
            {
                dataString = $"{expressions[0].SourceExpressionString}";
            }
            for (int i = 1; i < expressions.Count; i++)
            {
                var childStr = expressions[i].DataString;
                if (expressions[i].ElementType == ElementType.Expression)
                {
                    childStr = $"{expressions[i].SourceExpressionString}";
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
            if (operators.Count != 0)
            {
                result.Add(operators);
            }
            return result;
        }

        #endregion

        #endregion

        #region private for Execute

        private double Convert2DoubleValue(string tag)
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
                    return double.TryParse(tag, out double result) ? result : throw new ExpressionException($"at {SourceExpressionString}: {tag} 不是数值或布尔类型");
            }
        }

        #endregion
    }
}
