using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class ActivationLevel
    {
        public List<GameObject> ActivateObjects = new();
        public List<GameObject> DeactivateObjects = new();
    }
}
