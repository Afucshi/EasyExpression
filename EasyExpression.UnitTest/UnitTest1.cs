using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EasyExpression.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LogicTest()
        {
            var expStr = "3 * (1 + 2) < = 5 || !(8 / (4 - 2) > [SUM](1,2,3))";
            var exp = new Expression(expStr);
            exp.LoadArgument();
            var value = exp.Excute();
            Assert.AreEqual(1d, value);
        }

        [TestMethod]
        public void ArithmeticTest()
        {
            var expStr = "3 * (1 + 2) + 5 - (30 / (4 - 2) % [SUM](1,2,3))";
            var exp = new Expression(expStr);
            exp.LoadArgument();
            var value = exp.Excute();
            Assert.AreEqual(11d, value);
        }

        [TestMethod]
        public void StringTest()
        {
            var expStr = "a * (b + c) > d & [Contains](srcText,text)";
            var dic = new Dictionary<string, string>
            {
                { "a","3"},
                { "b","1"},
                { "c","2"},
                { "d","4"},
                { "srcText","abc"},
                { "text","bc"},
            };
            var exp = new Expression(expStr);
            exp.LoadArgument(dic);
            var value = exp.Excute();
            Assert.AreEqual(1d, value);
        }

        [TestMethod]
        public void ParamsTest()
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
            Assert.AreEqual(11d, value);
        }
    }
}
