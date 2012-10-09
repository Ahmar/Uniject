using System;
using NUnit.Framework;
using Ninject;
using LS3Test;
using Testable;

namespace Tests {
    [TestFixture()]
    public class TestTestableFramework : BaseInjectedTest {

        [Game.GameObjectBoundary]
        public class Foo : Testable.TestableComponent {
            public Foo(TestableGameObject obj) : base(obj) {
            }
        }

        [Game.GameObjectBoundary]
        public class HasFoo : Testable.TestableComponent {
            public Foo nested { get; private set; }
            public HasFoo(TestableGameObject obj, Foo nested) : base(obj) {
                this.nested = nested;
            }
        }

        [Test()]
        public void TestTestableComponentIsUpdated() {
            FakeComponent component = kernel.Get<FakeComponent>();

            Assert.AreEqual(0, component.updateCount);
            TestableGameObject obj = (TestableGameObject)component.obj;
            obj.Update();

            Assert.AreEqual(1, component.updateCount);
        }

        [Test]
        public void testNestedGameObjects() {
            HasFoo foo = kernel.Get<HasFoo>();
            Assert.AreNotSame(foo.Obj, foo.nested.Obj);
        }
    }
}


