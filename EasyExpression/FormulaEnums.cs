﻿namespace EasyExpression
{
    public enum ElementType
    {
        /// <summary>
        /// 表达式
        /// </summary>
        Expression = 0,
        /// <summary>
        /// 数据
        /// </summary>
        Data = 1,
        /// <summary>
        /// 函数
        /// </summary>
        Function = 2,
        /// <summary>
        /// 引用
        /// </summary>
        Reference = 3,
    }

    public enum ExpressionType
    {
        Unknown = 0,
        /// <summary>
        /// 逻辑表达式
        /// </summary>
        Logic = 1,
        /// <summary>
        /// 算术表达式
        /// </summary>
        Arithmetic = 2,
        /// <summary>
        /// 关系表达式
        /// </summary>
        Relation = 3
    }

    public enum ExecuteType
    {
        /// <summary>
        /// 非函数
        /// </summary>
        None = 0,
        /// <summary>
        /// 求和
        /// </summary>
        Sum = 1,
        /// <summary>
        /// 求平均
        /// </summary>
        Avg = 2
    }
    public enum MatchMode
    {
        /// <summary>
        /// 数据
        /// </summary>
        Data = 1,
        /// <summary>
        /// 逻辑运算符
        /// </summary>
        LogicSymbol = 2,
        /// <summary>
        /// 算术运算符
        /// </summary>
        ArithmeticSymbol = 3,
        /// <summary>
        /// 运算范围(小括号)
        /// </summary>
        Scope = 4,
        /// <summary>
        /// 函数
        /// </summary>
        Function = 5,
        /// <summary>
        /// 关系运算符
        /// </summary>
        RelationSymbol = 6,
        /// <summary>
        /// 转义符
        /// </summary>
        EscapeCharacter = 7,
    }

    public enum Operator
    {
        /// <summary>
        /// 未知
        /// </summary>
        None = 0,
        /// <summary>
        /// 与(&)
        /// </summary>
        [Operator("与",1,"&")]
        And = 1,
        /// <summary>
        /// 或(|)
        /// </summary>
        [Operator("或", 1,"|")]
        Or = 2,
        /// <summary>
        /// 非(!)
        /// </summary>
        [Operator("非", 2,"!")]
        Not = 3,
        ///<summary>
        ///加
        ///</summary>
        [Operator("加", 4,"+")]
        Plus = 4,
        ///<summary>
        ///减
        ///</summary>
        [Operator("减", 4,"-")]
        Subtract = 5,
        ///<summary>
        ///乘
        ///</summary>
        [Operator("乘", 5,"*")]
        Multiply = 6,
        ///<summary>
        ///除
        ///</summary>
        [Operator("除", 5,"/")]
        Divide = 7,
        ///<summary>
        ///模
        ///</summary>
        [Operator("模", 5,"%")]
        Mod = 8,
        /// <summary>
        /// 大于
        /// </summary>
        [Operator("大于", 3,">")]
        GreaterThan = 9,
        /// <summary>
        /// 小于
        /// </summary>
        [Operator("小于", 3,"<")]
        LessThan = 10,
        /// <summary>
        /// 等于
        /// </summary>
        [Operator("等于", 3,"=")]
        Equals = 11,
        /// <summary>
        /// 大于等于
        /// </summary>
        [Operator("大于等于", 3,">=")]
        GreaterThanOrEquals = 12,
        /// <summary>
        /// 小于等于
        /// </summary>
        [Operator("小于等于", 3,"<=")]
        LessThanOrEquals = 13,
    }

    public enum LogicalOperator
    {
        /// <summary>
        /// 未知
        /// </summary>
        None = 0,
        /// <summary>
        /// 与(&)
        /// </summary>
        And = 1,
        /// <summary>
        /// 或(|)
        /// </summary>
        Or = 2,
        /// <summary>
        /// 非(!)
        /// </summary>
        Not = 3
    }

    public enum FourOperator
    {
        /// <summary>
        /// 未知
        /// </summary>
        None = 0,
        ///<summary>
        ///加
        ///</summary>
        Plus = 1,
        ///<summary>
        ///减
        ///</summary>
        Subtract = 2,
        ///<summary>
        ///乘
        ///</summary>
        Multiply = 3,
        ///<summary>
        ///除
        ///</summary>
        Divide
    }


}
