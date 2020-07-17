using NUnit.Framework;

namespace app
{
    [TestFixture]
    public class NumberFunctionsTests
    {

        [SetUp]
        public void SetUp()
        {
        }

        [TestCase("010", 0)]
        [TestCase("01100001", 1)]
        [TestCase("10100001", -1)]
        [TestCase("01100010", 2)]
        [TestCase("10100010", -2)]
        [TestCase("0111000010000", 16)]
        [TestCase("1011000010000", -16)]
        [TestCase("0111011111111", 255)]
        [TestCase("1011011111111", -255)]
        [TestCase("011110000100000000", 256)]
        [TestCase("101110000100000000", -256)]
        public void DemTest(string problem, int solution)
        {
            var answer = NumberFunctions.Dem(problem);
            Assert.AreEqual(solution, answer);
        }

        [TestCase(0, "010")]
        [TestCase(1, "01100001")]
        [TestCase(-1, "10100001")]
        [TestCase(2, "01100010")]
        [TestCase(-2, "10100010")]
        [TestCase(16, "0111000010000")]
        [TestCase(-16, "1011000010000")]
        [TestCase(255, "0111011111111")]
        [TestCase(-255, "1011011111111")]
        [TestCase(256, "011110000100000000")]
        [TestCase(-256, "101110000100000000")]
        public void ModTest(int problem, string solution)
        {
            var answer = NumberFunctions.Mod(problem);
            Assert.AreEqual(solution, answer);
        }
    }
}