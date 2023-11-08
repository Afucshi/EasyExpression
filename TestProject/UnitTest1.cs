using EasyExpression;

namespace TestProject
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
            Assert.AreEqual(11d,value);
        }
    }
}