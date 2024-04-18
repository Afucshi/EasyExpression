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
        public void MutipleFunctionTest()
        {
            var expStr = "[SUM]([SUM](6,7,8),2,3)";

            var expStr1 = "[SUM](1,[SUM](6,7,8),3)";

            var expStr2 = "[SUM](1,2,[SUM](6,7,8))";

            var expStr3 = "[SUM]([SUM](6,7,8),2,[SUM](6,7,8))";

            var expStr4 = "[SUM]([SUM](6,7,8),[SUM](6,7,8),[SUM](6,7,8))";

            var exp = new Expression(expStr);
            var exp1 = new Expression(expStr1);
            var exp2 = new Expression(expStr2);
            var exp3 = new Expression(expStr3);
            var exp4 = new Expression(expStr4);
            exp.LoadArgument();
            exp1.LoadArgument();
            exp2.LoadArgument();
            exp3.LoadArgument();
            exp4.LoadArgument();
            var value = exp.Excute();
            Assert.AreEqual(11d, value);
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
