using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Game;
using Tests;
using Testable;

namespace Tests {
    class TestModule : Ninject.Modules.NinjectModule {

        private bool val;
        public TestModule(bool val) {
            this.val = val;
        }

        public override void Load () {
            Bind<Game.IPathfinder> ().To<FakePathfinder> ();
            Bind<Game.IFireable> ().To<FakeFireable> ();
            Bind<Game.IZombieSoundBoard> ().To<FakeZombieSoundBoard> ();

            Bind<ILayerMask> ().To<FakeLayerMask> ();
            Bind<Testable.ITime> ().To<FakeTimer> ().InSingletonScope();
            Bind<Game.IUserPrefs> ().To<FakeUserPrefs> ().InSingletonScope();

            FakeMath math = new FakeMath(val);
            Bind<Testable.IMaths> ().ToConstant(math);

            Bind<ILogger>().To<TestLogger>();

            Bind<IVectorLine>().To<FakeVectorLine>();
            Bind<IAudioListener>().To<FakeAudioListener>();

            Bind<Testable.IRigidBody> ().To<FakeRigidBody> ().InScope(GameModule.functor);
            Bind<Testable.INavmeshAgent> ().To<FakeNavmeshAgent> ();
            Bind<Testable.IPackedSprite> ().To<FakePackedSprite> ();
            Bind<ISphereCollider> ().To<FakeSphereCollider> ().InScope (GameModule.functor);
            Bind<IAudioSource>().To<FakeAudioSource>().InScope(GameModule.functor);
            Bind<Game.IPackedSpriteFactory> ().To<FakePackedSpriteFactory> ();

            Bind<Testable.IPhysics> ().To<MockPhysics> ().InSingletonScope ();
            Bind<Testable.IUtil>().To<MockUtil>().InSingletonScope();
            Bind<TestUpdatableManager>().ToSelf().InSingletonScope();

            Bind<IResourceLoader>().To<MockResourceLoader>().InSingletonScope();
            Bind<IDecalManager>().To<FakeDecalManager>().InSingletonScope();

            Bind<ITapJoyPlugin>().To<FakeTapjoyPlugin>().InSingletonScope();
            Bind<IBulletTracer>().To<FakeBulletTracer>().InSingletonScope();

            Bind<IReferralsAdapter>().To<MockReferralsAdapter>().InSingletonScope();
            Bind<IAnalytics>().To<FakeAnalytics>().InSingletonScope();
            Bind<TestableGameObject>().To<FakeGameObject>().InScope(GameModule.functor);
            Bind<ITransform>().To<FakeGameObject.FakeTransform>();
        }
    }
}
