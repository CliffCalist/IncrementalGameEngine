using System;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class DelayData
    {
        public float Delay;
        public float ElapsedTime;



        public DelayData(float delay, float elapsedTime = 0)
        {
            if (delay <= 0)
                throw new ArgumentOutOfRangeException(nameof(delay));
            if (elapsedTime < 0)
                throw new ArgumentOutOfRangeException(nameof(elapsedTime));

            Delay = delay;
            ElapsedTime = elapsedTime;
        }
    }
}