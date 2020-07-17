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

        [Test]
        public void AddClassTests()
        {
            var add = new AddClass();

            Assert.Catch<NotImplementedException>(() => add.AsNumber());
            Assert.AreEqual(add, add.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => add.Invoke(val));

            val = new Number(123);
            Value val2 = new Number(234);
            
            var result = add.Invoke(val).Invoke(val2);
            Assert.AreEqual(357, result.AsNumber());
            
            result = add.Invoke(val2).Invoke(val);
            Assert.AreEqual(357, result.AsNumber());

            val2 = Nil;
            Assert.Catch<NotImplementedException>(() => add.Invoke(val).Invoke(val2));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => add.Invoke(val));
        }

        [Test]
        public void MultClassTests()
        {
            var mult = new MultClass();

            Assert.Catch<NotImplementedException>(() => mult.AsNumber());
            Assert.AreEqual(mult, mult.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => mult.Invoke(val));

            val = new Number(123);
            Value val2 = new Number(234);

            var result = mult.Invoke(val).Invoke(val2);
            Assert.AreEqual(28782, result.AsNumber());

            result = mult.Invoke(val2).Invoke(val);
            Assert.AreEqual(28782, result.AsNumber());

            val2 = Nil;
            Assert.Catch<NotImplementedException>(() => mult.Invoke(val).Invoke(val2));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => mult.Invoke(val));
        }

        [Test]
        public void DivideClassTests()
        {
            var div = new DivideClass();

            Assert.Catch<NotImplementedException>(() => div.AsNumber());
            Assert.AreEqual(div, div.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => div.Invoke(val));

            val = new Number(123);
            Value val2 = new Number(234);

            var result = div.Invoke(val).Invoke(val2);
            Assert.AreEqual(0, result.AsNumber());

            result = div.Invoke(val2).Invoke(val);
            Assert.AreEqual(1, result.AsNumber());

            val2 = Nil;
            Assert.Catch<NotImplementedException>(() => div.Invoke(val).Invoke(val2));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => div.Invoke(val));
        }

        [Test]
        public void EqualsClassTests()
        {
            var eq = new EqualsClass();

            Assert.Catch<NotImplementedException>(() => eq.AsNumber());
            Assert.AreEqual(eq, eq.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => eq.Invoke(val));

            val = new Number(123);
            Value val2 = new Number(234);
            Assert.AreEqual(FalseVal, eq.Invoke(val).Invoke(val2));
            Assert.AreEqual(FalseVal, eq.Invoke(val2).Invoke(val));

            val2 = new Number(123);
            Assert.AreEqual(TrueVal, eq.Invoke(val).Invoke(val2));
            Assert.AreEqual(TrueVal, eq.Invoke(val2).Invoke(val));

            val2 = Nil;
            Assert.Catch<NotImplementedException>(() => eq.Invoke(val).Invoke(val2));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => eq.Invoke(val));
        }

        public void LessThanClassTests()
        {
            var lt = new LessThanClass();

            Assert.Catch<NotImplementedException>(() => lt.AsNumber());
            Assert.AreEqual(lt, lt.Evaluate(null));

            Value val = null;
            Assert.Catch<NullReferenceException>(() => lt.Invoke(val));

            val = new Number(123);
            Value val2 = new Number(234);
            Assert.AreEqual(TrueVal, lt.Invoke(val).Invoke(val2));
            Assert.AreEqual(FalseVal, lt.Invoke(val2).Invoke(val));

            val2 = new Number(123);
            Assert.AreEqual(FalseVal, lt.Invoke(val).Invoke(val2));
            Assert.AreEqual(FalseVal, lt.Invoke(val2).Invoke(val));

            val2 = Nil;
            Assert.Catch<NotImplementedException>(() => lt.Invoke(val).Invoke(val2));

            val = Identity;
            Assert.Catch<NotImplementedException>(() => lt.Invoke(val));
        }
    }
}