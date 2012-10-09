using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Game;

namespace Tests {

    public class TestTapjoyIntegration : BaseInjectedTest {

        [Test]
        public void testZBSynchronisedToTapjoyOnFirstBoot() {
            IUserPrefs prefs = kernel.Get<IUserPrefs>();
            ZombucksAccount.writeBalance(prefs, 100);

            ITapJoyPlugin tapjoy = kernel.Get<ITapJoyPlugin>();
            Assert.AreEqual(0, tapjoy.QueryTapPoints());

            kernel.Get<ZombucksAccount>();
            Assert.AreEqual(100, tapjoy.QueryTapPoints());
        }

        [Test]
        public void testZBNotMultiCreditedOnSecondBoot() {
            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            account.Credit(100);

            ITapJoyPlugin tapjoy = kernel.Get<ITapJoyPlugin>();
            Assert.AreEqual(100, tapjoy.QueryTapPoints());

            new ZombucksAccount(account.prefs, kernel.Get<TapjoyProxy>());

            Assert.AreEqual(100, tapjoy.QueryTapPoints());
        }

        [Test]
        public void testUserHasTapjoyCreditWipesPhoneAndReinstalls() {
            ITapJoyPlugin tapjoy = kernel.Get<ITapJoyPlugin>();
            tapjoy.AwardTapPoints(100);

            Assert.AreEqual(100, kernel.Get<ZombucksAccount>().Balance);
        }

        [Test]
        public void testZBEarnedFromTapjoy() {
            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            account.Credit(100);

            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.balance += 5;

            kernel.Get<TapjoyNotifier>().fireCurrencyEarned(string.Empty);

            Assert.AreEqual(105, account.Balance);
        }

        [Test]
        public void testZBEarnedFromTapjoyWhilstOffline() {
            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            account.Credit(100);

            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.balance += 50;

            ZombucksAccount newAccount = new ZombucksAccount(account.prefs, kernel.Get<TapjoyProxy>());
            Assert.AreEqual(150, newAccount.Balance);
            Assert.AreEqual(150, tapjoy.balance);
        }

        [Test]
        public void testZBEarnedWhilstOnline() {
            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            account.Credit(200);

            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.simulateEarnedAmount(30);
            Assert.AreEqual(230, account.Balance);
            Assert.AreEqual(230, tapjoy.balance);
        }

        [Test]
        public void testZBSpentWhilstOnline() {
            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            account.Credit(100);

            account.Debit(5);
            Assert.AreEqual(95, account.Balance);
            Assert.AreEqual(95, kernel.Get<ITapJoyPlugin>().QueryTapPoints());
        }

        [Test]
        public void testZBEarnedWhilstOffline() {
            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.balance = 100;

            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            Assert.AreEqual(100, account.Balance);

            tapjoy.offline = true;

            account.Credit(5);
            Assert.AreEqual(105, account.Balance);

            tapjoy.offline = false;

            Assert.AreEqual(100, tapjoy.balance);
            new ZombucksAccount(account.prefs, kernel.Get<TapjoyProxy>());
            Assert.AreEqual(105, tapjoy.balance);

            tapjoy.offline = true;
            account.Credit(5);
            tapjoy.offline = false;
            account.Credit(5);
            Assert.AreEqual(115, tapjoy.balance);
        }

        [Test]
        public void testZBSpentWhilstOffline() {
            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            account.Credit(100);
            Assert.AreEqual(100, tapjoy.balance);

            tapjoy.offline = true;

            Assert.True(account.Debit(5));
            Assert.True(account.Debit(10));

            Assert.AreEqual(100, tapjoy.balance);

            tapjoy.offline = false;

            new ZombucksAccount(account.prefs, kernel.Get<TapjoyProxy>());

            Assert.AreEqual(85, tapjoy.balance);
            Assert.AreEqual(85, account.Balance);
        }

        [Test]
        public void testLocalZBBalanceUsedWhilstOffline() {
            ZombucksAccount account = kernel.Get<ZombucksAccount>();
            account.Credit(100);

            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.offline = true;

            Assert.IsTrue(new ZombucksAccount(account.prefs, kernel.Get<TapjoyProxy>()).Debit(10));
        }

        [Test]
        public void testQueueSerialisation() {
            Queue<int> q = new Queue<int>();
            q.Enqueue(1);
            q.Enqueue(3);
            q.Enqueue(2);

            Queue<int> d = TapjoyProxy.deserialiseQueue(TapjoyProxy.serialiseQueue(q));
            Assert.AreEqual(q, d);
        }

        [Test]
        public void testTapjoyProxyOnline() {
            TapjoyProxy proxy = kernel.Get<TapjoyProxy>();
            proxy.awardPoints(5);
            proxy.awardPoints(10);
            proxy.spendPoints(5);
            proxy.awardPoints(15);

            Assert.AreEqual(25, kernel.Get<ITapJoyPlugin>().QueryTapPoints());
        }

        [Test]
        public void testTapjoyProxyOffline() {
            TapjoyProxy proxy = kernel.Get<TapjoyProxy>();
            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.offline = true;

            proxy.awardPoints(5);
            proxy.awardPoints(10);
            proxy.spendPoints(5);
            proxy.awardPoints(15);

            tapjoy.offline = false;

            // Restarted phone.
            proxy = kernel.Get<TapjoyProxy>();
            proxy.awardPoints(1);

            Assert.AreEqual(26, kernel.Get<ITapJoyPlugin>().QueryTapPoints());
        }

        [Test]
        public void testTapjoyProxyRecoversIfTapjoyDoesNotRespond() {
            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.broken = true;

            TapjoyProxy proxy = kernel.Get<TapjoyProxy>();
            proxy.awardPoints(5);
            proxy.spendPoints(1);

            tapjoy.broken = false;
            ((FakeTimer) kernel.Get<Testable.ITime>()).realtimeSinceStartup = 30.0f;
            proxy.spendPoints(1);

            Assert.AreEqual(3, kernel.Get<ITapJoyPlugin>().QueryTapPoints());
        }

        [Test]
        public void testTapjoyProxyClearsCompletedTransactions() {
            TapjoyProxy proxy = kernel.Get<TapjoyProxy>();

            FakeTapjoyPlugin tapjoy = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            tapjoy.offline = true;

            proxy.awardPoints(5);
            proxy.spendPoints(1);
            tapjoy.offline = false;
            Assert.AreEqual(1, proxy.pendingCreditCount);
            Assert.AreEqual(1, proxy.pendingDebitCount);
            proxy.awardPoints(1);

            kernel = createNewKernel();
            TapjoyProxy proxy2 = new TapjoyProxy(kernel.Get<ITapJoyPlugin>(), kernel.Get<TapjoyNotifier>(), proxy.prefs, kernel.Get<Testable.ITime>(), kernel.Get<ILogger>());

            Assert.AreEqual(0, proxy2.pendingCreditCount);
            Assert.AreEqual(0, proxy2.pendingDebitCount);

            kernel = createNewKernel();

            ((FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>()).offline = true;
            TapjoyProxy proxy3 = new TapjoyProxy(kernel.Get<ITapJoyPlugin>(), kernel.Get<TapjoyNotifier>(), proxy.prefs, kernel.Get<Testable.ITime>(), kernel.Get<ILogger>());
            Assert.AreEqual(0, proxy3.pendingCreditCount);
            Assert.AreEqual(0, proxy3.pendingDebitCount);
        }

        int callcount;
        private void onChanged(int old, int newBalance) {
            callcount++;
        }

        [Test]
        public void testEventRaising() {
            EventManager.OnZombucksChanged += onChanged;
            kernel.Get<ZombucksAccount>().Credit(100);
            Assert.AreEqual(1, callcount);
        }

        [Test]
        public void testTapjoyNotReady() {

            FakeTapjoyPlugin fake = (FakeTapjoyPlugin) kernel.Get<ITapJoyPlugin>();
            fake.ready = false;

            ZombucksAccount.writeBalance(kernel.Get<IUserPrefs>(), 100);

            ZombucksAccount account = kernel.Get<ZombucksAccount>();

            Assert.AreEqual(0, fake.balance);

            fake.ready = true;
            account.Credit(1);

            Assert.AreEqual(101, fake.balance);
        }
    }
}

