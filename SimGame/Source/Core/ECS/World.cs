using System;
using System.Collections.Generic;
using System.Linq;

namespace MPirical.Core.ECS
{
    /// <summary>
    /// Central class that manages all entities, components, and systems
    /// </summary>
    public class World
    {
        private int _nextEntityId = -1;
        private readonly Dictionary<Type, IComponentStore> _componentStores = new Dictionary<Type, IComponentStore>();
        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly Dictionary<int, HashSet<Type>> _entityComponents = new Dictionary<int, HashSet<Type>>();
        private readonly Dictionary<Archetypes, Archetype> _archetypes = new Dictionary<Archetypes, Archetype>();
        
        /// <summary>
        /// Registers a new archetype
        /// </summary>
        public void RegisterArchetype(Archetype archetype)
        {
            _archetypes[archetype.Type] = archetype;
            
            // Check all existing entities to see if they match this new archetype
            foreach (var entityEntry in _entityComponents)
            {
                int entityId = entityEntry.Key;
                HashSet<Type> componentTypes = entityEntry.Value;
                
                if (archetype.Matches(componentTypes))
                {
                    archetype.AddEntity(new Entity(entityId));
                }
            }
        }
        
        /// <summary>
        /// Gets an archetype by name
        /// </summary>
        public Archetype GetArchetype(Archetypes type)
        {
            if (_archetypes.TryGetValue(type, out var archetype))
            {
                return archetype;
            }
            return null;
        }
        
        /// <summary>
        /// Gets all registered archetypes
        /// </summary>
        public IReadOnlyCollection<Archetype> GetAllArchetypes()
        {
            return _archetypes.Values;
        }

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns>The newly created entity</returns>
        public Entity CreateEntity()
        {
            int id = _nextEntityId++;
            _entityComponents[id] = new HashSet<Type>();
            return new Entity(id);
        }

        /// <summary>
        /// Destroys an entity and all its components
        /// </summary>
        /// <param name="entity">The entity to destroy</param>
        public void DestroyEntity(Entity entity)
        {
            foreach (var archetype in _archetypes.Values)
            {
                archetype.RemoveEntity(entity);
            }
            
            if (!_entityComponents.TryGetValue(entity.Id, out var componentTypes))
                return;

            foreach (var componentType in componentTypes)
            {
                if (_componentStores.TryGetValue(componentType, out var store))
                {
                    store.Remove(entity.Id);
                }
            }

            _entityComponents.Remove(entity.Id);
        }

        /// <summary>
        /// Adds a component to an entity
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="entity">The entity</param>
        /// <param name="component">The component instance</param>
        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            Type componentType = typeof(T);
            
            if (!_componentStores.TryGetValue(componentType, out var store))
            {
                store = new ComponentStore<T>();
                _componentStores[componentType] = store;
            }

            ((ComponentStore<T>)store).Set(entity.Id, component);
            
            if (_entityComponents.TryGetValue(entity.Id, out var componentTypes))
            {
                componentTypes.Add(componentType);
                
                UpdateEntityArchetypes(entity, componentTypes);
                
            }
        }
        
        public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
        {
            Type componentType = typeof(T);
            
            if (_componentStores.TryGetValue(componentType, out var store))
            {
                store.Remove(entity.Id);
            }
            
            if (_entityComponents.TryGetValue(entity.Id, out var componentTypes))
            {
                componentTypes.Remove(componentType);
                
                UpdateEntityArchetypes(entity, componentTypes);
            }
        }

        /// <summary>
        /// Gets a component from an entity
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="entity">The entity</param>
        /// <returns>The component if it exists</returns>
        public T GetComponent<T>(Entity entity) where T : struct, IComponent
        {
            Type componentType = typeof(T);
            
            if (!_componentStores.TryGetValue(componentType, out var store))
                throw new KeyNotFoundException($"No component store found for type {componentType.Name}");

            return ((ComponentStore<T>)store).Get(entity.Id);
        }

        /// <summary>
        /// Checks if an entity has a specific component
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="entity">The entity</param>
        /// <returns>True if the entity has the component</returns>
        public bool HasComponent<T>(Entity entity) where T : struct, IComponent
        {
            Type componentType = typeof(T);
            
            if (!_entityComponents.TryGetValue(entity.Id, out var componentTypes))
                return false;

            return componentTypes.Contains(componentType);
        }

        /// <summary>
        /// Registers a system to this world
        /// </summary>
        /// <param name="system">The system to register</param>
        public void RegisterSystem(ISystem system)
        {
            _systems.Add(system);
            system.Initialize(this);
            
            // Sort systems by priority
            _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        /// <summary>
        /// Updates all registered systems
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        public void Update(float deltaTime)
        {
            foreach (var system in _systems)
            {
                system.Update(deltaTime);
            }
        }
        
        private void UpdateEntityArchetypes(Entity entity, HashSet<Type> componentTypes)
        {
            foreach (var archetype in _archetypes.Values)
            {
                bool matches = archetype.Matches(componentTypes);
                bool contained = archetype.GetEntities().Contains(entity);
                
                if (matches && !contained)
                {
                    archetype.AddEntity(entity);
                }
                else if (!matches && contained)
                {
                    archetype.RemoveEntity(entity);
                }
            }
        }
    }
}