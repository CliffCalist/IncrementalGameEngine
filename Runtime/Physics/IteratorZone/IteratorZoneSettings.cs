using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class IteratorZoneSettings
    {
        [SerializeField, Min(0.001F)] private float _launchDelay;
        [SerializeField, Min(0.001F)] private float _timeRate;



        public IteratorZoneSettings(float launchDelay, float timeRate)
        {
            if (launchDelay <= 0)
                throw new ArgumentOutOfRangeException(nameof(launchDelay));
            _launchDelay = launchDelay;

            if (timeRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeRate));
            _timeRate = timeRate;
        }


        public float LaunchDelay => _launchDelay;
        public float TimeRate => _timeRate;
    }
}