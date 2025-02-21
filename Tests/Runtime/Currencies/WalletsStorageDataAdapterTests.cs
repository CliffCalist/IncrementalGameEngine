using System.Collections.Generic;
using NUnit.Framework;
using R3;
using UnityEngine;

namespace WhiteArrow.Incremental.Tests
{
    public class WalletsStorageDataAdapterTests
    {
        private WalletsStorageDataAdapter _adapter;
        private WalletsStorageData _testData;
        private TestWalletSettingsStorage _testSettings;
        private int _onChangedCount;



        [SetUp]
        public void SetUp()
        {
            _testData = new();
            _testData.Wallets.Add(new WalletData(ResourceType.Cash, 100));
            _testData.Wallets.Add(new WalletData(ResourceType.SilverCoin, 50));

            _testSettings = ScriptableObject.CreateInstance<TestWalletSettingsStorage>();
            _testSettings.Init(new List<WalletInstanceSettings>
            {
                new WalletInstanceSettings(ResourceType.Cash, 100),
                new WalletInstanceSettings(ResourceType.SilverCoin, 50),
                new WalletInstanceSettings(ResourceType.Ruby, 10)
            });

            _adapter = new WalletsStorageDataAdapter(_testData, _testSettings);
            _onChangedCount = 0;
            _adapter.OnChanged.Subscribe(_ => _onChangedCount++);
        }



        [Test]
        public void Constructor_WhenInitialized_ShouldContainCorrectWallets()
        {
            Assert.AreEqual(3, _adapter.WalletAdapterMap.Count);
            Assert.IsTrue(_adapter.WalletAdapterMap.ContainsKey(ResourceType.Cash));
            Assert.IsTrue(_adapter.WalletAdapterMap.ContainsKey(ResourceType.SilverCoin));
            Assert.IsTrue(_adapter.WalletAdapterMap.ContainsKey(ResourceType.Ruby));
        }

        [Test]
        public void Add_WhenNewWalletAdded_ShouldIncreaseCountAndEmitOnChanged()
        {
            var newWallet = new WalletData(ResourceType.Saphire, 500);
            _adapter.Add(newWallet);

            Assert.AreEqual(4, _adapter.WalletAdapterMap.Count);
            Assert.IsTrue(_adapter.WalletAdapterMap.ContainsKey(ResourceType.Saphire));
            Assert.AreEqual(1, _onChangedCount);
        }

        [Test]
        public void Remove_WhenWalletRemoved_ShouldDecreaseCountAndEmitOnChanged()
        {
            var walletToRemove = _adapter.WalletAdapterMap[ResourceType.SilverCoin];
            _adapter.Remove(walletToRemove);

            Assert.AreEqual(2, _adapter.WalletAdapterMap.Count);
            Assert.IsFalse(_adapter.WalletAdapterMap.ContainsKey(ResourceType.SilverCoin));
            Assert.AreEqual(1, _onChangedCount);
        }
    }
}