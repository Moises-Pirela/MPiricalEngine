using System.Collections.Generic;

namespace MPirical.Core.ECS;

/// <summary>
/// Stores components of a specific type
/// </summary>
/// <typeparam name="T">The component type</typeparam>
internal class ComponentStore<T> : IComponentStore where T : struct, IComponent
{
    private readonly Dictionary<int, T> _components = new Dictionary<int, T>();

    public void Set(int entityId, T component)
    {
        _components[entityId] = component;
    }

    public T Get(int entityId)
    {
        if (!_components.TryGetValue(entityId, out var component))
            throw new KeyNotFoundException($"Entity {entityId} does not have component of type {typeof(T).Name}");

        return component;
    }

    public bool Has(int entityId)
    {
        return _components.ContainsKey(entityId);
    }

    public void Remove(int entityId)
    {
        _components.Remove(entityId);
    }
}