using NUnit.Framework;
using R3;

namespace WhiteArrow.Incremental.Tests
{
    public class WalletDataAdapterTests
    {
        private WalletDataAdapter _walletAdapter;
        private WalletData _walletData;
        private int _onChangedCount;


        [SetUp]
        public void SetUp()
        {
            _walletData = new(ResourceType.Cash, 100);
            _walletAdapter = new WalletDataAdapter(_walletData);
            _onChangedCount = 0;
            _walletAdapter.OnChanged.Subscribe(_ => _onChangedCount++);
        }


        [Test]
        public void Balance_WhenUpdated_ShouldReflectInWalletData()
        {
            _walletAdapter.Balance.Value = 200;
            Assert.AreEqual(200, _walletData.Balance);
        }

        [Test]
        public void Balance_WhenUpdated_ShouldTriggerOnChangedEvent()
        {
            _walletAdapter.Balance.Value = 200;
            Assert.AreEqual(1, _onChangedCount);
        }
    }
}
