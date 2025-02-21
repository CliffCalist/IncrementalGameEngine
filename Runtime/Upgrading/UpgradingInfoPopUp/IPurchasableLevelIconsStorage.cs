using UnityEngine;

namespace WhiteArrow.Incremental
{
    public interface IPurchasableLevelIconsStorage
    {
        Sprite GetLvlIcon(int tier, int lvl);
    }
}