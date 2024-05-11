using System;

namespace EasyExpression
{
    public class ExpressionException : Exception
    {
        public override string Message { get; }
        public ExpressionException(string message)
        {
            Message = message;
        }
    }
}
