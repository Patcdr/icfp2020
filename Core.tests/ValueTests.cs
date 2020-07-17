using NUnit.Framework;
using System;

namespace Core.tests
{
    public class ValueTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void NumberTests()
        {
            var n = new Number(0);

            Assert.AreEqual(0, n.AsNumber());
            Assert.AreEqual(n, n.Evaluate());
            Assert.Catch<NotImplementedException>(() => n.Invoke(null));

            n = new Number(long.MinValue);

            Assert.AreEqual(long.MinValue, n.AsNumber());
            Assert.AreEqual(n, n.Evaluate());
            
            n = new Number(long.MaxValue);

            Assert.AreEqual(long.MaxValue, n.AsNumber());
            Assert.AreEqual(n, n.Evaluate());
        }

        [Test]
        public void ConstantFunctionTests()
        {
            Value inner = null;
            var cf = new ConstantFunction(inner);

            Assert.Catch<NotImplementedException>(() => cf.AsNumber());
            Assert.AreEqual(cf, cf.Evaluate());
            Assert.AreEqual(inner, cf.Invoke(null));

            inner = new Number(123);
            cf = new ConstantFunction(inner);

            Assert.AreEqual(cf, cf.Evaluate());
            Assert.AreEqual(inner, cf.Invoke(null));
        }
    }
}