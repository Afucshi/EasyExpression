
using System;
using System.Collections.Generic;

namespace EasyExpression.TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("LogicTest");
            LogicTest();
            Console.WriteLine("ArithmeticTest");
            ArithmeticTest();
            Console.WriteLine("ParamsTest");
            ParamsTest();
            Console.ReadLine();
        }

        static void LogicTest()
        {
            var expStr = "3 * (1 + 2) < = 5 || !(8 / (4 - 2) > [SUM](1,2,3))";
            var exp = new Expression(expStr);
            exp.LoadArgument();
            var value = exp.Excute();
            Console.WriteLine(value == 1d);
        }

        static void ArithmeticTest()
        {
            var expStr = "3 * (1 + 2) + 5 - (30 / (4 - 2) % [SUM](1,2,3))";
            var exp = new Expression(expStr);
            exp.LoadArgument();
            var value = exp.Excute();
            Console.WriteLine(value == 11d);
        }

        static void ParamsTest()
        {
            var expStr = "a * (b + c) + 5 - (30 / (d - 2) % [SUM](1,2,3))";
            var dic = new Dictionary<string, string>
            {
                { "a","3"},
                { "b","1"},
                { "c","2"},
                { "d","4"},
            };
            var exp = new Expression(expStr);
            exp.LoadArgument(dic);
            var value = exp.Excute();
            Console.WriteLine(value == 11d);
        }
    }
}
