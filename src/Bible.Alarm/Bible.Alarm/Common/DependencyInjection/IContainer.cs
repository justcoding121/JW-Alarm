﻿namespace Bible.Alarm
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A container for managing dependencies between objects.
    /// </summary>
    public interface IContainer
    {
        Dictionary<string, object> Context { get; }

        /// <summary>
        /// Gets all the registered types.
        /// </summary>
        /// <value>The registered types.</value>
        IEnumerable<Type> RegisteredTypes { get; }

        /// <summary>
        /// Register a factory for the given type.
        /// </summary>
        /// <returns>The register.</returns>
        /// <param name="factory">Factory.</param>
        /// <typeparam name="T">The registered type.</typeparam>
        void Register<T>(Func<IContainer, T> factory);


        /// <summary>
        /// Register a factory for the given type.
        /// </summary>
        /// <returns>The register.</returns>
        /// <param name="factory">Factory.</param>
        /// <typeparam name="T">The registered type.</typeparam>
        void RegisterSingleton<T>(Func<IContainer, T> factory);

        /// <summary>
        /// Get an instance of the given type.
        /// </summary>
        /// <remarks>
        /// The type should be registered before use.
        /// </remarks>
        /// <returns>The of.</returns>
        /// <typeparam name="T">The requested type.</typeparam>
        T Resolve<T>();

        /// <summary>
        /// Get an instance of the given type.
        /// </summary>
        /// <remarks>
        /// The type should be registered before use.
        /// </remarks>
        /// <returns>The of.</returns>
        /// <typeparam name="T">The requested type.</typeparam>
        object Resolve(Type type);


        /// <summary>
        /// Returns true if the type has been registered.
        /// </summary>
        /// <returns><c>true</c>, if type has been registered, <c>false</c> otherwise.</returns>
        /// <typeparam name="T">The type parameter.</typeparam>
        bool IsRegistered<T>();

        /// <summary>
        /// Create a new instance (even if the type has been registered as an singleton).
        /// </summary>
        /// <returns>The new instance.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T New<T>();

        /// <summary>
        /// Create a new instance (even if the type has been registered as an singleton).
        /// </summary>
        /// <returns>The new instance.</returns>
        /// <param name="type">Type.</param>
        object New(Type type);

        /// <summary>
        /// Unregister the factory for a given instance type, and deletes any existing instance of this type.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void Unregister<T>();

        /// <summary>
        /// Wipes the container of all factories and instances.
        /// </summary>
        void WipeContainer();
    }
}
