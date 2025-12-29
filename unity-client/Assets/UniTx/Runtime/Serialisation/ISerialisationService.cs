namespace UniTx.Runtime.Serialisation
{
    /// <summary>
    /// Saves and loads <see cref="ISavedData"/> instances to persistent storage.
    /// </summary>
    public interface ISerialisationService
    {
        /// <summary>
        /// Saves the specified data object to persistent storage.
        /// </summary>
        /// <param name="data">
        /// The <see cref="ISavedData"/> instance to serialize and store.
        /// </param>
        void Save(ISavedData data);

        /// <summary>
        /// Loads a previously saved data object of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The concrete type of <see cref="ISavedData"/> to load.  
        /// Must inherit from <see cref="ISavedData"/> and have a parameterless constructor.
        /// </typeparam>
        /// <param name="id">
        /// The unique identifier of the saved data entry.
        /// </param>
        /// <returns>
        /// An instance of <typeparamref name="T"/> loaded from storage,  
        /// or a new instance if no saved data with the given <paramref name="id"/> exists.
        /// </returns>
        T Load<T>(string id)
            where T : ISavedData, new();
    }
}