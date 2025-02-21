using System;
using System.Collections.Generic;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class WalletsStorageData
    {
        public List<WalletData> Wallets = new();
    }
}