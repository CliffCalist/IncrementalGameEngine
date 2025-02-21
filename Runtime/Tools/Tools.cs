using System;

namespace WhiteArrow.Incremental
{
    public static class Tools
    {
        /// <summary>
        /// Tries to cast the instance to the specified type. Throws an InvalidCastException
        /// if the types are incompatible, providing detailed information about the expected and actual types.
        /// </summary>
        /// <typeparam name="T">The target type to cast to.</typeparam>
        /// <param name="instance">The object to cast.</param>
        /// <returns>The instance cast to the specified type.</returns>
        /// <exception cref="InvalidCastException">Thrown when the instance cannot be cast to the specified type.</exception>
        public static T SafeCast<T>(object instance)
        {
            if (instance is T castedInstance)
                return castedInstance;

            throw new InvalidCastException(
                $"Expected type '{typeof(T)}', but received type '{instance?.GetType() ?? typeof(object)}'.");
        }
    }
}