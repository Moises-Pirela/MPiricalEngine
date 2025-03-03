using System;

namespace MPirical.Core.ECS;

/// <summary>
/// Interface for component configuration.
/// Provides a standardized way to create and initialize components.
/// </summary>
/// <typeparam name="T">The component type this config creates</typeparam>
public interface IComponentConfig<T> where T : struct, IComponent
{
    /// <summary>
    /// Creates a new instance of the component with default values.
    /// </summary>
    /// <returns>A new component instance</returns>
    T CreateDefault();
        
    /// <summary>
    /// Creates a component with custom configuration.
    /// </summary>
    /// <param name="configureAction">Action to configure the component</param>
    /// <returns>A configured component instance</returns>
    T Create(Action<T> configureAction);
}