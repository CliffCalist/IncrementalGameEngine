using System;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class LevelData : IReadOnlyLevelData
    {
        public int Lvl = 1;

        int IReadOnlyLevelData.Lvl => Lvl;


        public LevelData() { }

        public LevelData(IReadOnlyLevelData template)
        {
            Lvl = template.Lvl;
        }
    }
}