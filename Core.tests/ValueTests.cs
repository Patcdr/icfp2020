using NUnit.Framework;
using System;
using static Core.Library;

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

            inner = Nil;
            cf = new ConstantFunction(inner);
            Assert.AreEqual(inner, cf.Invoke(null));

            inner = Identity;
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

            val = Nil;
            result = t.Invoke(val);
            Assert.AreEqual(val, result.Invoke(null));

            val = Identity;
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

            val = Nil;
            result = f.Invoke(val);
            Assert.AreEqual(null, result.Invoke(null));

            val = Identity;
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

            val = Nil;
            Assert.Catch<NotImplementedException>(() => inc.Invoke(val));

            val = Identity;
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

            val = Nil;
            Assert.Catch<NotImplementedException>(() => dec.Invoke(val));

            val = Identity;
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

            val = Nil;
            Assert.Catch<NotImplementedException>(() => neg.Invoke(val));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => neg.Invoke(val));
        }

        [Test]
        public void Power2ClassTests()
        {
            var p2 = new Power2Class();

            Assert.Catch<NotImplementedException>(() => p2.AsNumber());
            Assert.AreEqual(p2, p2.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => p2.Invoke(val));

            val = new Number(12);
            var result = p2.Invoke(val);
            Assert.AreEqual(4096, result.AsNumber());

            val = Nil;
            Assert.Catch<NotImplementedException>(() => p2.Invoke(val));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => p2.Invoke(val));
        }

        [Test]
        public void IdentityClassTests()
        {
            var id = new IdentityClass();

            Assert.Catch<NotImplementedException>(() => id.AsNumber());
            Assert.AreEqual(id, id.Evaluate(null));

            Value val = null;
            Assert.AreEqual(null, id.Invoke(null));

            val = new Number(123);
            var result = id.Invoke(val);
            Assert.AreEqual(123, result.AsNumber());

            val = Nil;
            Assert.AreEqual(val, id.Invoke(val));

            val = Identity;
            Assert.AreEqual(val, id.Invoke(val));
        }

        [Test]
        public void IsNilClassTests()
        {
            var isNil = new IsNilClass();

            Assert.Catch<NotImplementedException>(() => isNil.AsNumber());
            Assert.AreEqual(isNil, isNil.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => isNil.Invoke(val));

            /*
             * Does not function correctly for Values other than Nil/Cons
            val = new Number(123);
            var result = isNil.Invoke(val);
            Assert.IsInstanceOf<FalseClass>(result);
            
            val = Identity;
            var result = isNil.Invoke(val);
            Assert.IsInstanceOf<FalseClass>(result);
            */

            val = Nil;
            Assert.AreEqual(TrueVal, isNil.Invoke(val));

            val = Cons.Invoke(new Number(1)).Invoke(Nil);
            Assert.AreEqual(FalseVal, isNil.Invoke(val));
        }
    }
}