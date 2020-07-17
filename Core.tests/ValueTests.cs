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
            Assert.AreEqual(n, n.Evaluate(null));
            Assert.Catch<NotImplementedException>(() => n.Invoke(null));

            n = new Number(long.MinValue);

            Assert.AreEqual(long.MinValue, n.AsNumber());
            
            n = new Number(long.MaxValue);

            Assert.AreEqual(long.MaxValue, n.AsNumber());
        }

        [Test]
        public void ConstantFunctionTests()
        {
            Value inner = null;
            var cf = new ConstantFunction(inner);

            Assert.Catch<NotImplementedException>(() => cf.AsNumber());
            Assert.AreEqual(cf, cf.Evaluate(null));
            Assert.AreEqual(inner, cf.Invoke(null));

            inner = new Number(123);
            cf = new ConstantFunction(inner);

            Assert.AreEqual(inner, cf.Invoke(null));

            inner = new IdentityClass();
            cf = new ConstantFunction(inner);

            Assert.AreEqual(inner, cf.Invoke(null));
        }

        [Test]
        public void TrueClassTests()
        {
            var t = new TrueClass();

            Assert.Catch<NotImplementedException>(() => t.AsNumber());
            Assert.AreEqual(t, t.Evaluate(null));

            Value val = null;
            var result = t.Invoke(val);
            Assert.AreEqual(val, result.Invoke(null));

            val = new Number(123);
            result = t.Invoke(val);
            Assert.AreEqual(val, result.Invoke(null));

            val = new IdentityClass();
            result = t.Invoke(val);
            Assert.AreEqual(val, result.Invoke(null));
        }

        [Test]
        public void FalseClassTests()
        {
            var f = new FalseClass();

            Assert.Catch<NotImplementedException>(() => f.AsNumber());
            Assert.AreEqual(f, f.Evaluate(null));

            Value val = null;
            var result = f.Invoke(val);
            Assert.AreEqual(null, result.Invoke(null));

            val = new Number(123);
            result = f.Invoke(val);
            Assert.AreEqual(null, result.Invoke(null));

            val = new IdentityClass();
            result = f.Invoke(val);
            Assert.AreEqual(null, result.Invoke(null));
        }

        [Test]
        public void IncrementClassTests()
        {
            var inc = new IncrementClass();

            Assert.Catch<NotImplementedException>(() => inc.AsNumber());
            Assert.AreEqual(inc, inc.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => inc.Invoke(val));

            val = new Number(123);
            var result = inc.Invoke(val);
            Assert.AreEqual(124, result.AsNumber());

            val = new IdentityClass();
            Assert.Catch<NotImplementedException>(() => inc.Invoke(val));
        }

        [Test]
        public void DecrementClassTests()
        {
            var dec = new DecrementClass();

            Assert.Catch<NotImplementedException>(() => dec.AsNumber());
            Assert.AreEqual(dec, dec.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => dec.Invoke(val));

            val = new Number(123);
            var result = dec.Invoke(val);
            Assert.AreEqual(122, result.AsNumber());

            val = new IdentityClass();
            Assert.Catch<NotImplementedException>(() => dec.Invoke(val));
        }

        [Test]
        public void NegateClassTests()
        {
            var neg = new NegateClass();

            Assert.Catch<NotImplementedException>(() => neg.AsNumber());
            Assert.AreEqual(neg, neg.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => neg.Invoke(val));

            val = new Number(123);
            var result = neg.Invoke(val);
            Assert.AreEqual(-123, result.AsNumber());

            val = new IdentityClass();
            Assert.Catch<NotImplementedException>(() => neg.Invoke(val));
        }
    }
}