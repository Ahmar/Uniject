using System;
using System.Collections.Generic;
using Testable;
using NUnit.Framework;
using Ninject;

namespace Tests {
    public class TestSpriteAnimationPlayer : BaseInjectedTest {

        private List<int> calls = new List<int>();
        private void onFrame(int frame) {
            calls.Add(frame);
        }

        [Test]
        public void testHighFramerateDoesNotMultiTriggerEvents() {
            SpriteAnimationPlayer player = kernel.Get<SpriteAnimationPlayer>();
            player.Initialise(kernel.Get<FakePackedSprite>());
            player.registerCallbackForFrame(onFrame, 0, 1);

            player.play(0);

            ((FakeTimer) kernel.Get<ITime>()).DeltaTime = 0.0001f;

            TestUpdatableManager world = kernel.Get<TestUpdatableManager>();
            world.step(10);

            Assert.AreEqual(1, calls.Count);
        }
    }
}

