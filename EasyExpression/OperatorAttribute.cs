using System;

namespace EasyExpression
{
    public class OperatorAttribute : Attribute
    {
        public OperatorAttribute(string name, int level, string value)
        {
            Name = name;
            Level = level;
            Value = value;
        }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
