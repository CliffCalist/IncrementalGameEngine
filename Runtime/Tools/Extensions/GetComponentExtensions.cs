using System;
using UnityEngine;

namespace WhiteArrow.Incremental
{
    public static class GetComponentExtensions
    {
        #region Conditional
        public static T GetComponentConditional<T>(this Transform transform, Func<T, bool> condition)
        {
            var comopentns = transform.GetComponents<T>();
            foreach (var component in comopentns)
            {
                if (condition(component))
                    return component;
            }

            return default;
        }

        public static T GetComponentConditional<T>(this GameObject gameObject, Func<T, bool> condition)
        {
            return gameObject.transform.GetComponentConditional(condition);
        }

        public static T GetComponentConditional<T>(this MonoBehaviour monoBehaviour, Func<T, bool> condition)
        {
            return monoBehaviour.transform.GetComponentConditional(condition);
        }
        #endregion



        #region Requarement
        public static T GetComponentRequarement<T>(this Transform transform)
        {
            var component = transform.GetComponent<T>();
            return component ?? throw new Exception($"Requaired component {typeof(T).FullName} isn't finded on {transform.name}.");
        }

        public static T GetComponentRequarement<T>(this GameObject gameObject)
        {
            return gameObject.transform.GetComponentRequarement<T>();
        }

        public static T GetComponentRequarement<T>(this MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.transform.GetComponentRequarement<T>();
        }



        public static T GetComponentInChildrensRequarement<T>(this Transform transform)
        {
            var component = transform.GetComponentInChildren<T>();
            return component ?? throw new Exception($"Requaired component {typeof(T).FullName} isn't finded on {transform.name}.");
        }

        public static T GetComponentInChildrensRequarement<T>(this GameObject gameObject)
        {
            var component = gameObject.GetComponentInChildren<T>();
            return component ?? throw new Exception($"Requaired component {typeof(T).FullName} isn't finded on {gameObject.name}.");
        }

        public static T GetComponentInChildrensRequarement<T>(this MonoBehaviour monoBehaviour)
        {
            var component = monoBehaviour.GetComponentInChildren<T>();
            return component ?? throw new Exception($"Requaired component {typeof(T).FullName} isn't finded on {monoBehaviour.name}.");
        }
        #endregion
    }
}