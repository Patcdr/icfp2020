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

        [TestCase(":a = ap ap ap s isnil :a cons 0 1", " ((ap i (ap i f)) ,  (ap :a cons) ) ", "t")]
        public void OneLinerTest(string problem, string mid, string solution)
        {
            var lines = new List<string> { problem };
            var env = Parser.Parse(lines);
            var eval = env[":a"].Evaluate(env);
            Assert.AreEqual(eval.ToString(), mid);
            var answer = UtilityFunctions.EvaluateFully(eval, env);
            Assert.AreEqual(answer.ToString(), solution);
        }
    }
}