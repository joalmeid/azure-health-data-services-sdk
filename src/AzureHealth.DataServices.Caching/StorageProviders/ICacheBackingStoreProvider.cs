﻿namespace AzureHealth.DataServices.Caching.StorageProviders
{
    /// <summary>
    /// Interface implemented by cache provider.
    /// </summary>
    public interface ICacheBackingStoreProvider
    {
        /// <summary>
        /// Adds an object to the cache.
        /// </summary>
        /// <typeparam name="T">Type of object to cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Object to cache.</param>
        /// <returns>Task</returns>
        Task AddAsync<T>(string key, T value);

        /// <summary>
        /// Adds a object to the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Object to cache.</param>
        /// <returns>Task</returns>
        Task AddAsync(string key, object value);

        /// <summary>
        /// Gets an object from the cache.
        /// </summary>
        /// <typeparam name="T">Type of object to cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>Cached object.</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Get a string from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>Cached object as string.</returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// Removes and object from the cache.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <returns>True is removed otherwise false.</returns>
        Task<bool> RemoveAsync(string key);

    }
}
