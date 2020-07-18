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

        [TestCase(":a = ap inc 5", "6")]
        [TestCase(":a = ap dec 5", "4")]
        [TestCase(":a = ap neg 5", "-5")]
        [TestCase(":a = ap pwr2 5", "32")]
        [TestCase(":a = ap i 5", "5")]
        [TestCase(":a = ap ap ap nil 5 1 0", "1")]
        [TestCase(":a = ap ap ap isnil nil 1 0", "1")]
        [TestCase(":a = ap ap ap isnil ap ap cons 2 3 1 0", "0")]
        [TestCase(":a = ap ap add 2 3", "5")]
        [TestCase(":a = ap ap add 3 2", "5")]
        [TestCase(":a = ap ap mul 2 3", "6")]
        [TestCase(":a = ap ap mul 3 2", "6")]
        [TestCase(":a = ap ap div 2 3", "0")]
        [TestCase(":a = ap ap div 3 2", "1")]
        [TestCase(":a = ap ap ap ap eq 2 2 1 0", "1")]
        [TestCase(":a = ap ap ap ap eq 2 3 1 0", "0")]
        [TestCase(":a = ap ap ap ap lt 2 2 1 0", "0")]
        [TestCase(":a = ap ap ap ap lt 2 3 1 0", "1")]
        [TestCase(":a = ap ap ap if0 0 1 2", "1")]
        [TestCase(":a = ap ap ap if0 1 1 2", "2")]
        [TestCase(":a = ap ap ap if0 1 1 2", "2")]
        [TestCase(":a = ap ap ap s t inc 5", "5")]
        [TestCase(":a = ap ap ap s f inc 5", "6")]
        [TestCase(":a = ap ap ap c t 4 5", "5")]
        [TestCase(":a = ap ap ap c f 4 5", "4")]
        [TestCase(":a = ap ap ap b inc inc 5", "7")]
        [TestCase(":a = ap ap cons 1 2", " (1 , 2) ")]
        [TestCase(":a = ap ap ap cons 1 2 t", "1")]
        [TestCase(":a = ap ap ap cons 1 2 f", "2")]
        [TestCase(":a = ap ap vec 1 2", " (1 , 2) ")]
        [TestCase(":a = ap ap ap vec 1 2 t", "1")]
        [TestCase(":a = ap ap ap vec 1 2 f", "2")]
        [TestCase(":a = ap car ap ap cons 1 2", "1")]
        [TestCase(":a = ap cdr ap ap cons 1 2", "2")]
        public void ManyEvalTests(string problem, string solution)
        {
            var lines = new List<string> { problem };
            var env = Parser.Parse(lines);
            var eval = env[":a"].Evaluate(env);
            
            var answer = UtilityFunctions.EvaluateFully(eval, env);
            
            Assert.AreEqual(answer.ToString(), solution);
        }
    }
}