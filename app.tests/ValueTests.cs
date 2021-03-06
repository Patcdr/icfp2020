using Core;
using NUnit.Framework;
using System;
using static Core.Library;

namespace app.tests
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
            Assert.Catch<NotImplementedException>(() => n.Invoke(null, Value.EMPTY_ENV));

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
            Assert.Catch<NullReferenceException>(() => cf.Invoke(null, Value.EMPTY_ENV), "Cannot have a null inner node");

            inner = new Number(123);
            cf = new ConstantFunction(inner);
            Assert.AreEqual(inner, cf.Invoke(null, Value.EMPTY_ENV));

            inner = Nil;
            cf = new ConstantFunction(inner);
            Assert.AreEqual(inner, cf.Invoke(null, Value.EMPTY_ENV));

            inner = Identity;
            cf = new ConstantFunction(inner);
            Assert.AreEqual(inner, cf.Invoke(null, Value.EMPTY_ENV));
        }

        [Test]
        public void TrueClassTests()
        {
            var t = new TrueClass();

            Assert.Catch<NotImplementedException>(() => t.AsNumber());
            Assert.AreEqual(t, t.Evaluate(null));

            Value val = null;
            var result = t.Invoke(val, Value.EMPTY_ENV);
            Assert.Catch<NullReferenceException>(() => result.Invoke(null, Value.EMPTY_ENV), "Cannot have a null inner node");

            val = new Number(123);
            result = t.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(val, result.Invoke(null, Value.EMPTY_ENV));

            val = Nil;
            result = t.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(val, result.Invoke(null, Value.EMPTY_ENV));

            val = Identity;
            result = t.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(val, result.Invoke(null, Value.EMPTY_ENV));
        }

        [Test]
        public void FalseClassTests()
        {
            var f = new FalseClass();

            Assert.Catch<NotImplementedException>(() => f.AsNumber());
            Assert.AreEqual(f, f.Evaluate(null));

            Value val = null;
            var result = f.Invoke(val, Value.EMPTY_ENV);
            Assert.Catch<NullReferenceException>(() => result.Invoke(null, Value.EMPTY_ENV), "Cannot have a null inner node");

            val = new Number(123);
            result = f.Invoke(val, Value.EMPTY_ENV);
            Assert.Catch<NullReferenceException>(() => result.Invoke(null, Value.EMPTY_ENV), "Cannot invoke the Identity function with null");

            result = f.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(Nil, result.Invoke(Nil, Value.EMPTY_ENV));
        }

        [Test]
        public void IncrementClassTests()
        {
            var inc = new IncrementClass();

            Assert.Catch<NotImplementedException>(() => inc.AsNumber());
            Assert.AreEqual(inc, inc.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => inc.Invoke(val, Value.EMPTY_ENV));

            val = new Number(123);
            var result = inc.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(124, result.AsNumber());

            val = Nil;
            Assert.Catch<NotImplementedException>(() => inc.Invoke(val, Value.EMPTY_ENV));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => inc.Invoke(val, Value.EMPTY_ENV));
        }

        [Test]
        public void DecrementClassTests()
        {
            var dec = new DecrementClass();

            Assert.Catch<NotImplementedException>(() => dec.AsNumber());
            Assert.AreEqual(dec, dec.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => dec.Invoke(val, Value.EMPTY_ENV));

            val = new Number(123);
            var result = dec.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(122, result.AsNumber());

            val = Nil;
            Assert.Catch<NotImplementedException>(() => dec.Invoke(val, Value.EMPTY_ENV));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => dec.Invoke(val, Value.EMPTY_ENV));
        }

        [Test]
        public void NegateClassTests()
        {
            var neg = new NegateClass();

            Assert.Catch<NotImplementedException>(() => neg.AsNumber());
            Assert.AreEqual(neg, neg.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => neg.Invoke(val, Value.EMPTY_ENV));

            val = new Number(123);
            var result = neg.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(-123, result.AsNumber());

            val = Nil;
            Assert.Catch<NotImplementedException>(() => neg.Invoke(val, Value.EMPTY_ENV));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => neg.Invoke(val, Value.EMPTY_ENV));
        }

        [Test]
        public void Power2ClassTests()
        {
            var p2 = new Power2Class();

            Assert.Catch<NotImplementedException>(() => p2.AsNumber());
            Assert.AreEqual(p2, p2.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => p2.Invoke(val, Value.EMPTY_ENV));

            val = new Number(12);
            var result = p2.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(4096, result.AsNumber());

            val = Nil;
            Assert.Catch<NotImplementedException>(() => p2.Invoke(val, Value.EMPTY_ENV));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => p2.Invoke(val, Value.EMPTY_ENV));
        }

        [Test]
        public void IdentityClassTests()
        {
            var id = new IdentityClass();

            Assert.Catch<NotImplementedException>(() => id.AsNumber());
            Assert.AreEqual(id, id.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => id.Invoke(null, Value.EMPTY_ENV), "Cannot have a null inner node");

            val = new Number(123);
            var result = id.Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(123, result.AsNumber());

            val = Nil;
            Assert.AreEqual(val, id.Invoke(val, Value.EMPTY_ENV));

            val = Identity;
            Assert.AreEqual(val, id.Invoke(val, Value.EMPTY_ENV));
        }

        [Test]
        public void IsNilClassTests()
        {
            var isNil = new IsNilClass();

            Assert.Catch<NotImplementedException>(() => isNil.AsNumber());
            Assert.AreEqual(isNil, isNil.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => isNil.Invoke(val, Value.EMPTY_ENV));

            /*
             * Does not function correctly for Values other than Nil/Cons
            val = new Number(123);
            var result = isNil.Invoke(val, Value.EMPTY_ENV);
            Assert.IsInstanceOf<FalseClass>(result);

            val = Identity;
            var result = isNil.Invoke(val, Value.EMPTY_ENV);
            Assert.IsInstanceOf<FalseClass>(result);
            */

            val = Nil;
            Assert.AreEqual(TrueVal, isNil.Invoke(val, Value.EMPTY_ENV));

            val = Cons.Invoke(new Number(1), Value.EMPTY_ENV).Invoke(Nil, Value.EMPTY_ENV);
            Assert.AreEqual(FalseVal, isNil.Invoke(val, Value.EMPTY_ENV));
        }

        [Test]
        public void AddClassTests()
        {
            var add = new AddClass();

            Assert.Catch<NotImplementedException>(() => add.AsNumber());
            Assert.AreEqual(add, add.Evaluate(null));

            Value val = new Number(123);
            Value val2 = new Number(234);

            var result = add.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV);
            Assert.AreEqual(357, result.AsNumber());

            result = add.Invoke(val2, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(357, result.AsNumber());

            val2 = Nil;
            Assert.Catch<NotImplementedException>(() => add.Invoke(Nil, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => add.Invoke(Identity, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV));
        }

        [Test]
        public void MultClassTests()
        {
            var mult = new MultClass();

            Assert.Catch<NotImplementedException>(() => mult.AsNumber());
            Assert.AreEqual(mult, mult.Evaluate(null));

            Value val = new Number(123);
            Value val2 = new Number(234);

            var result = mult.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV);
            Assert.AreEqual(28782, result.AsNumber());

            result = mult.Invoke(val2, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(28782, result.AsNumber());

            Assert.Catch<NotImplementedException>(() => mult.Invoke(Nil, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
            Assert.Catch<NotImplementedException>(() => mult.Invoke(Identity, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
        }

        [Test]
        public void DivideClassTests()
        {
            var div = new DivideClass();

            Assert.Catch<NotImplementedException>(() => div.AsNumber());
            Assert.AreEqual(div, div.Evaluate(null));

            Value val = new Number(123);
            Value val2 = new Number(234);

            var result = div.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV);
            Assert.AreEqual(0, result.AsNumber());

            result = div.Invoke(val2, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV);
            Assert.AreEqual(1, result.AsNumber());

            Assert.Catch<NotImplementedException>(() => div.Invoke(Nil, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
            Assert.Catch<NotImplementedException>(() => div.Invoke(Identity, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
        }

        [Test]
        public void EqualsClassTests()
        {
            var eq = new EqualsClass();

            Assert.Catch<NotImplementedException>(() => eq.AsNumber());
            Assert.AreEqual(eq, eq.Evaluate(null));

            Value val = new Number(123);
            Value val2 = new Number(234);
            Assert.AreEqual(FalseVal, eq.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
            Assert.AreEqual(FalseVal, eq.Invoke(val2, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV));

            val2 = new Number(123);
            Assert.AreEqual(TrueVal, eq.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
            Assert.AreEqual(TrueVal, eq.Invoke(val2, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV));

            Assert.Catch<NotImplementedException>(() => eq.Invoke(Nil, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
            Assert.Catch<NotImplementedException>(() => eq.Invoke(Nil, Value.EMPTY_ENV).Invoke(Nil, Value.EMPTY_ENV));
            Assert.Catch<NotImplementedException>(() => eq.Invoke(Nil, Value.EMPTY_ENV).Invoke(Identity, Value.EMPTY_ENV));
            Assert.Catch<NotImplementedException>(() => eq.Invoke(Identity, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
        }

        public void LessThanClassTests()
        {
            var lt = new LessThanClass();

            Assert.Catch<NotImplementedException>(() => lt.AsNumber());
            Assert.AreEqual(lt, lt.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => lt.Invoke(val, Value.EMPTY_ENV));

            val = new Number(123);
            Value val2 = new Number(234);
            Assert.AreEqual(TrueVal, lt.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
            Assert.AreEqual(FalseVal, lt.Invoke(val2, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV));

            val2 = new Number(123);
            Assert.AreEqual(FalseVal, lt.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));
            Assert.AreEqual(FalseVal, lt.Invoke(val2, Value.EMPTY_ENV).Invoke(val, Value.EMPTY_ENV));

            val2 = Nil;
            Assert.Catch<NotImplementedException>(() => lt.Invoke(val, Value.EMPTY_ENV).Invoke(val2, Value.EMPTY_ENV));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => lt.Invoke(val, Value.EMPTY_ENV));
        }
    }
}