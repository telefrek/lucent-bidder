namespace Lucent.Common.Serialization
{
    /// <summary>
    /// Repository for type serializers
    /// </summary>
    public interface ISerializationRegistry
    {
        /// <summary>
        /// Checks if a type has a serializer available
        /// </summary>
        /// <typeparam name="T">The type to check for</typeparam>
        /// <returns>True if there is a serializer registerred for the type</returns>
        bool IsSerializerRegisterred<T>() where T : new();

        /// <summary>
        /// Registers a serializer for the given type
        /// </summary>
        /// <param name="serializer">The serializer to use for the given type</param>
        /// <typeparam name="T">The type of object the serializer works on</typeparam>
        void Register<T>(IEntitySerializer<T> serializer) where T : new();

        /// <summary>
        /// Gets the serializer registerred for the given type
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <returns>The serializer, if it exists, or null</returns>
        IEntitySerializer<T> GetSerializer<T>() where T : new();
    }

    /// <summary>
    /// Class to store extensions for the ISerializationRegistry type
    /// </summary>
    public static class ISerializationRegistryExtensions
    {
        /// <summary>
        /// Guard t ensure the serializer has the correct type registerred
        /// </summary>
        /// <param name="registry">The registry to check</param>
        /// <typeparam name="T">The type of object that must have a serializer registerredd</typeparam>
        public static void Guard<T>(this ISerializationRegistry registry) where T : new()
        {
            if (!registry.IsSerializerRegisterred<T>())
                throw new SerializationException("No serializer registerred for type {0}".FormatWith(typeof(T).FullName));
        }
    }
}