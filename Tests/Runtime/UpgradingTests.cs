using Moq;
using NUnit.Framework;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental.Tests
{
    public class UpgradingTests
    {
        private Mock<IPurchasableLevelsStorage<IPurchasableLevel>> _mockLevelsStorage;
        private Mock<IWalletsStorage> _mockWallet;
        private Mock<Collider> _mockCollider;
        private Upgrading<IPurchasableLevel> _upgrading;



        private const int INITIAL_TIER = 1;
        private const int MAX_LEVEL = 10;
        private const long INITIAL_PRICE = 100;
        private const int PURCHASING_ITERATIONS = 5;



        [SetUp]
        public void SetUp()
        {
            var upgradingData = new UpgradingData(ResourceType.Cash);
            var upgradingDataAdapter = new UpgradingDataAdapter(upgradingData);

            _mockLevelsStorage = new Mock<IPurchasableLevelsStorage<IPurchasableLevel>>();
            _mockLevelsStorage.Setup(ls => ls.TiersCount).Returns(3);
            _mockLevelsStorage.Setup(ls => ls.PurchasingIterationsCount).Returns(PURCHASING_ITERATIONS);
            _mockLevelsStorage.Setup(ls => ls.GetMaxLvl(It.IsAny<int>())).Returns(MAX_LEVEL);
            _mockLevelsStorage.Setup(ls => ls.GetNextLvlPrice(It.IsAny<int>(), It.IsAny<int>())).Returns(INITIAL_PRICE);
            _mockLevelsStorage.Setup(ls => ls.GetLvlSetting(It.IsAny<int>(), 1)).Returns(Mock.Of<IPurchasableLevel>());

            _mockWallet = new Mock<IWalletsStorage>();
            _mockWallet.Setup(w => w.GetBalance(It.IsAny<ResourceType>())).Returns(1000);
            _mockWallet.Setup(w => w.TryDebit(It.IsAny<ResourceType>(), It.IsAny<long>())).Returns(true);

            _mockCollider = new Mock<Collider>();

            var iteratorSettings = new IteratorZoneSettings(0.5f, 0.5f);
            _upgrading = new Upgrading<IPurchasableLevel>(
                upgradingDataAdapter,
                _mockLevelsStorage.Object,
                iteratorSettings,
                _mockCollider.Object,
                _mockWallet.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _upgrading.Dispose();
        }



        #region Constructor
        [Test]
        public void Constructor_ShouldInitializeCorrectly()
        {
            Assert.IsNotNull(_upgrading);
            Assert.IsNotNull(_upgrading.Purchasing);
            Assert.IsNotNull(_upgrading.Iterator);
            Assert.AreEqual(INITIAL_TIER, _upgrading.Tier.CurrentValue);
        }
        #endregion



        #region Purchase
        [Test]
        public void Purchase_ShouldIncreaseLevel()
        {
            Assert.AreEqual(1, _upgrading.Lvl.Value.CurrentValue);


            _mockWallet.Setup(w => w.GetBalance(It.IsAny<ResourceType>())).Returns(INITIAL_PRICE);
            _mockWallet.Setup(w => w.TryDebit(It.IsAny<ResourceType>(), It.IsAny<long>())).Returns(true);

            for (int i = 0; i < PURCHASING_ITERATIONS; i++)
                _upgrading.Purchasing.NextIteration();


            Assert.AreEqual(2, _upgrading.Lvl.Value.CurrentValue);
        }

        [Test]
        public void Purchase_ShouldUpdatePrice()
        {
            _mockWallet.Setup(w => w.GetBalance(It.IsAny<ResourceType>())).Returns(INITIAL_PRICE);
            _mockWallet.Setup(w => w.TryDebit(It.IsAny<ResourceType>(), It.IsAny<long>())).Returns(true);

            for (int i = 0; i < PURCHASING_ITERATIONS; i++)
                _upgrading.Purchasing.NextIteration();


            _mockLevelsStorage.Verify(ls => ls.GetNextLvlPrice(INITIAL_TIER, 2), Times.Once);
        }
        #endregion



        #region MaxLevel
        [Test]
        public void MaxLevel_ShouldDisablePurchasing()
        {
            _mockWallet.Setup(w => w.GetBalance(It.IsAny<ResourceType>())).Returns(INITIAL_PRICE);
            _mockWallet.Setup(w => w.TryDebit(It.IsAny<ResourceType>(), It.IsAny<long>())).Returns(true);

            for (int i = 1; i < MAX_LEVEL; i++)
            {
                for (int j = 0; j < PURCHASING_ITERATIONS; j++)
                    _upgrading.Purchasing.NextIteration();
            }

            Assert.IsFalse(_upgrading.Purchasing.IsEnabled.CurrentValue);
        }
        #endregion



        #region ResetLvl
        [Test]
        public void ResetLvl_ShouldResetLevel()
        {
            for (int level = 1; level < 5; level++)
            {
                _mockWallet.Setup(w => w.GetBalance(It.IsAny<ResourceType>())).Returns(INITIAL_PRICE);
                _mockWallet.Setup(w => w.TryDebit(It.IsAny<ResourceType>(), It.IsAny<long>())).Returns(true);

                for (int i = 0; i < PURCHASING_ITERATIONS; i++)
                    _upgrading.Purchasing.NextIteration();

                Assert.AreEqual(level + 1, _upgrading.Lvl.Value.CurrentValue, $"Expected level {level + 1} after purchase.");
            }

            _upgrading.ResetLvl();


            Assert.AreEqual(1, _upgrading.Lvl.Value.CurrentValue);
        }

        [Test]
        public void ResetLvl_WithNewTier_ShouldUpdateTierAndResetLevel()
        {
            Assert.AreEqual(INITIAL_TIER, _upgrading.Tier.CurrentValue);

            _upgrading.ResetLvl(2);

            Assert.AreEqual(2, _upgrading.Tier.CurrentValue);
            Assert.AreEqual(1, _upgrading.Lvl.Value.CurrentValue);
        }
        #endregion
    }
}