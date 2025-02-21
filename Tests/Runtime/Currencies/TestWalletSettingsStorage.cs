using System.Collections.Generic;

namespace WhiteArrow.Incremental.Tests
{
    internal class TestWalletSettingsStorage : WalletSettingsStorage
    {
        private List<WalletInstanceSettings> _baseInstancies;
        public override IReadOnlyList<WalletInstanceSettings> BaseInstancies => _baseInstancies;

        public void Init(List<WalletInstanceSettings> baseInstancies)
        {
            _baseInstancies = baseInstancies;
        }
    }
}