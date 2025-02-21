using System;
using NUnit.Framework;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental.Tests
{
    public class PurchasingTests
    {
        private PurchasingDataAdapter _dataAdapter;
        private WalletsStorage _wallet;
        private Purchasing _purchasing;


        private const long START_PRICE = 100;
        private const int ITERATIONS_COUNT = 10;



        [SetUp]
        public void SetUp()
        {
            var walletData = new WalletsStorageData();
            walletData.Wallets.Add(new(ResourceType.Cash, 200));

            var walletSettings = ScriptableObject.CreateInstance<TestWalletSettingsStorage>();
            walletSettings.Init(new());

            var walletAdapter = new WalletsStorageDataAdapter(walletData, walletSettings);
            _wallet = new WalletsStorage(walletAdapter);


            var purchasingData = new PurchasingData(ResourceType.Cash);
            _dataAdapter = new PurchasingDataAdapter(purchasingData);
            _purchasing = new Purchasing(_dataAdapter, START_PRICE, ITERATIONS_COUNT, _wallet);
        }

        [TearDown]
        public void TearDown()
        {
            _wallet?.Dispose();
            _purchasing?.Dispose();
        }



        #region Constructor
        [Test]
        public void Constructor_ShouldInitializeCorrectly()
        {
            Assert.IsNotNull(_purchasing);
            Assert.AreEqual(START_PRICE, _purchasing.Price.CurrentValue);
            Assert.AreEqual(ITERATIONS_COUNT, _purchasing.IterationsCount);
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenInvalidParameters()
        {
            Assert.Throws<ArgumentNullException>(() => new Purchasing(null, START_PRICE, ITERATIONS_COUNT, _wallet));
            Assert.Throws<ArgumentNullException>(() => new Purchasing(_dataAdapter, START_PRICE, ITERATIONS_COUNT, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Purchasing(_dataAdapter, 0, ITERATIONS_COUNT, _wallet));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Purchasing(_dataAdapter, -10, ITERATIONS_COUNT, _wallet));
        }
        #endregion



        #region SetPrice
        [Test]
        public void SetPrice_ShouldUpdatePrice_WhenValid()
        {
            _purchasing.SetPice(200);
            Assert.AreEqual(200, _purchasing.Price.CurrentValue);
        }

        [Test]
        public void SetPrice_ShouldThrowException_WhenInvalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _purchasing.SetPice(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _purchasing.SetPice(-50));
        }
        #endregion



        #region RequiredResources
        [Test]
        public void RequiredResources_ShouldBeCalculatedCorrectly()
        {
            Assert.AreEqual(START_PRICE, _purchasing.RequiredResources.CurrentValue);

            _dataAdapter.InsertedResources.Value = 50;
            Assert.AreEqual(START_PRICE - 50, _purchasing.RequiredResources.CurrentValue);
        }
        #endregion



        #region NextIteration
        [Test]
        public void NextIteration_ShouldProcessCorrectly_WhenEnoughResources()
        {
            bool iterationProcessed = false;
            bool purchased = false;

            _purchasing.OnIterrationProcessed.Subscribe(_ => iterationProcessed = true);
            _purchasing.OnPurchased.Subscribe(_ => purchased = true);

            _purchasing.NextIteration();

            Assert.IsTrue(iterationProcessed, "OnIterationProcessed should be triggered.");
            Assert.IsFalse(purchased, "OnPurchased should not be triggered yet.");
            Assert.Greater(_dataAdapter.InsertedResources.Value, 0, "Inserted resources should increase.");
        }

        [Test]
        public void NextIteration_ShouldTriggerOnPurchased_WhenFullAmountInserted()
        {
            _dataAdapter.InsertedResources.Value = START_PRICE - 10;

            bool purchasedTriggered = false;
            _purchasing.OnPurchased.Subscribe(_ => purchasedTriggered = true);

            _purchasing.NextIteration();

            Assert.IsTrue(purchasedTriggered, "OnPurchased should be triggered when required resources are fully inserted.");
        }

        [Test]
        public void NextIteration_ShouldNotProcess_WhenNotEnoughResources()
        {
            _wallet.Reset(ResourceType.Cash);

            bool iterationProcessed = false;
            _purchasing.OnIterrationProcessed.Subscribe(_ => iterationProcessed = true);

            _purchasing.NextIteration();

            Assert.IsFalse(iterationProcessed, "NextIteration should not process if resources are insufficient.");
        }

        [Test]
        public void NextIteration_ShouldNotProcess_WhenDisabled()
        {
            _purchasing.IsEnabled.Value = false;

            bool iterationProcessed = false;
            _purchasing.OnIterrationProcessed.Subscribe(_ => iterationProcessed = true);

            _purchasing.NextIteration();

            Assert.IsFalse(iterationProcessed, "NextIteration should not be processed when purchasing is disabled.");
        }
        #endregion



        #region Progress
        [Test]
        public void Progress_ShouldBeCalculatedCorrectly()
        {
            Assert.AreEqual(0f, _purchasing.Progress.CurrentValue, "Initial progress should be 0.");

            _dataAdapter.InsertedResources.Value = 50;
            Assert.AreEqual(0.5f, _purchasing.Progress.CurrentValue, "Progress should be 50%.");

            _dataAdapter.InsertedResources.Value = 100;
            Assert.AreEqual(1f, _purchasing.Progress.CurrentValue, "Progress should be 100% when fully purchased.");
        }
        #endregion
    }
}
