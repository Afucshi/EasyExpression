using System;

namespace EasyExpression
{
    public class FunctionRemarkAttribute : Attribute
    {
        public FunctionRemarkAttribute(string name, string value, string group, string @params, string @return, string description)
        {
            Name = name;
            Value = value;
            Group = group;
            Params = @params;
            Return = @return;
            Description = description;
        }
        /// <summary>
        /// 函数名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 函数标识
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 分组
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// 参数列表
        /// </summary>
        public string Params { get; set; }
        /// <summary>
        /// 返回值
        /// </summary>
        public string Return { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
