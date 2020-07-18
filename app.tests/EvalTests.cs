using Core;
using NUnit.Framework;
using System.Collections.Generic;

namespace app.tests
{
    [TestFixture]
    public class EvalTests
    {
        [SetUp]
        public void SetUp()
        {
        }

// :a = ap ap ap s isnil :a nil
// :a = ap ap isnil nil ap :a nil
// :a = ap t ap :a nil

        [TestCase(":a = ap ap ap s isnil :a ap ap cons 0 1", "i")]
        public void OneLinerTest(string problem, string evaluated)
        {
            var lines = new List<string> { problem };
            var env = Parser.Parse(lines);
            var eval = env[":a"].Evaluate(env);
            Assert.AreEqual(evaluated, eval.ToString());
        }
    }
}