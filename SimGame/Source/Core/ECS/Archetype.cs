using System;
using System.Collections.Generic;
namespace MPirical.Core.ECS;


/// <summary>
/// Represents a group of entities that share a common set of component types
/// </summary>
public class Archetype
{
    /// <summary>
    /// Name of this archetype
    /// </summary>
    public Archetypes Type { get; }

    /// <summary>
    /// Set of component types that define this archetype
    /// </summary>
    private readonly HashSet<Type> _componentTypes = new HashSet<Type>();

    /// <summary>
    /// List of entities that belong to this archetype
    /// </summary>
    private readonly List<Entity> _entities = new List<Entity>();

    /// <summary>
    /// Creates a new archetype with the specified name and component types
    /// </summary>
    public Archetype(Archetypes type, params Type[] componentTypes)
    {
        Type = type;
        foreach (var component in componentTypes)
        {
            if (typeof(IComponent).IsAssignableFrom(component))
            {
                _componentTypes.Add(component);
            }
            else
            {
                throw new ArgumentException($"Type {component.Name} does not implement IComponent");
            }
        }
    }

    /// <summary>
    /// Checks if an entity matches this archetype based on its component types
    /// </summary>
    public bool Matches(HashSet<Type> entityComponentTypes)
    {
        // An entity matches if it has ALL the component types of this archetype
        return _componentTypes.IsSubsetOf(entityComponentTypes);
    }

    /// <summary>
    /// Adds an entity to this archetype
    /// </summary>
    public void AddEntity(Entity entity)
    {
        if (!_entities.Contains(entity))
        {
            _entities.Add(entity);
        }
    }

    /// <summary>
    /// Removes an entity from this archetype
    /// </summary>
    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    /// <summary>
    /// Gets all entities in this archetype
    /// </summary>
    public IReadOnlyList<Entity> GetEntities()
    {
        return _entities;
    }

    /// <summary>
    /// Gets the component types that define this archetype
    /// </summary>
    public IReadOnlySet<Type> GetComponentTypes()
    {
        return _componentTypes;
    }
}