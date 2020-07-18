using app;
using Core;
using static Core.Library;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace app.tests
{
    [TestFixture]
    public class InteractorTests
    {
        [SetUp]
        public void SetUp()
        {
        }


        [TestCase(1, 0)]
        [TestCase(2, 3)]
        [TestCase(4, 1)]
        public void StatelessDrawTest(int x, int y)
        {
            var point = new ConsIntermediate2(new Number(x), new Number(y));

            var protocol = new StatelessDrawProtocol();
            var response = protocol.call(Nil, point);

            Assert.AreEqual(0, response.Flag);
            Assert.AreEqual(Nil, response.NewState);
            Assert.AreEqual(point.ToString(), response.Data.Invoke(TrueVal, Value.EMPTY_ENV).Invoke(TrueVal, Value.EMPTY_ENV).ToString());
        }

        [Test]
        public void StatefulDrawTest()
        {
            var protocol = new StatefulDrawProtocol();

            var point = new ConsIntermediate2(new Number(0), new Number(0));
            var response = protocol.call(Nil, point);

            Assert.AreEqual(0, response.Flag);
            Assert.AreEqual(" ( (0 , 0)  , nil) ", response.NewState.ToString());
            Assert.AreEqual(point.ToString(), response.Data.Invoke(TrueVal, Value.EMPTY_ENV).Invoke(TrueVal, Value.EMPTY_ENV).ToString());

            point = new ConsIntermediate2(new Number(2), new Number(3));
            response = protocol.call(response.NewState, point);

            Assert.AreEqual(0, response.Flag);
            Assert.AreEqual(" ( (2 , 3)  ,  ( (0 , 0)  , nil) ) ", response.NewState.ToString());
            Assert.AreEqual(point.ToString(), response.Data.Invoke(TrueVal, Value.EMPTY_ENV).Invoke(TrueVal, Value.EMPTY_ENV).ToString());

            point = new ConsIntermediate2(new Number(1), new Number(2));
            response = protocol.call(response.NewState, point);

            Assert.AreEqual(0, response.Flag);
            Assert.AreEqual(" ( (1 , 2)  ,  ( (2 , 3)  ,  ( (0 , 0)  , nil) ) ) ", response.NewState.ToString());
            Assert.AreEqual(point.ToString(), response.Data.Invoke(TrueVal, Value.EMPTY_ENV).Invoke(TrueVal, Value.EMPTY_ENV).ToString());

        }

        [Test]
        public async Task StatefulInteractorDrawTest()
        {
            var points = new ConsIntermediate2 [] {
                new ConsIntermediate2(new Number(0), new Number(0)),
                new ConsIntermediate2(new Number(2), new Number(3)),
                new ConsIntermediate2(new Number(1), new Number(2)),
                new ConsIntermediate2(new Number(3), new Number(2)),
                new ConsIntermediate2(new Number(4), new Number(0))
            };

            var protocol = new StatefulDrawProtocol();
            var point = new ConsIntermediate2(new Number(0), new Number(0));

            var result = await Interactor.Interact(protocol);
            Assert.AreEqual(1, result.MultiDrawResult.Count);

            for (int i = 1; i < points.Length; i++)
            {
                result = await Interactor.Interact(protocol, result.NewState, points[i]);
                Assert.AreEqual(1, result.MultiDrawResult.Count);
                Assert.AreEqual(points[i].ToString(), result.NewState.ToString().Substring(2, 9));
            }
        }
    }
}