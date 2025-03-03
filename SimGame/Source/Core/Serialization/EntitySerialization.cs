using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using MPirical.Core.ECS;

namespace MPirical.Core.Serialization
{
    /// <summary>
    /// Helper class for serializing and deserializing ECS entities and their components
    /// </summary>
    public class EntitySerializer
    {
        private World _world;
        
        /// <summary>
        /// Create a new entity serializer
        /// </summary>
        /// <param name="world">ECS world to work with</param>
        public EntitySerializer(World world)
        {
            _world = world;
        }
        
        /// <summary>
        /// Serialize an entity to a JSON string
        /// </summary>
        /// <param name="entity">Entity to serialize</param>
        /// <returns>JSON representation of the entity</returns>
        public string SerializeEntity(Entity entity)
        {
            var entityData = new EntityData
            {
                Id = entity.Id,
                Components = new List<ComponentData>()
            };
            
            // Get all component types registered in the world
            // For a real implementation, we'd need to track all component types
            // For now, here's a placeholder approach:
            var componentTypes = GetAllComponentTypes();
            
            // Check each component type to see if the entity has it
            foreach (var componentType in componentTypes)
            {
                // Use reflection to call HasComponent<T>
                MethodInfo hasComponentMethod = typeof(World).GetMethod("HasComponent")
                    .MakeGenericMethod(componentType);
                
                bool hasComponent = (bool)hasComponentMethod.Invoke(_world, new object[] { entity });
                
                if (hasComponent)
                {
                    // Use reflection to call GetComponent<T>
                    MethodInfo getComponentMethod = typeof(World).GetMethod("GetComponent")
                        .MakeGenericMethod(componentType);
                    
                    object component = getComponentMethod.Invoke(_world, new object[] { entity });
                    
                    // Serialize the component
                    string componentJson = JsonSerializer.Serialize(component, componentType);
                    
                    // Add to component data list
                    entityData.Components.Add(new ComponentData
                    {
                        Type = componentType.FullName,
                        Data = componentJson
                    });
                }
            }
            
            // Serialize the entity data
            return JsonSerializer.Serialize(entityData);
        }
        
        /// <summary>
        /// Deserialize an entity from a JSON string
        /// </summary>
        /// <param name="json">JSON representation of the entity</param>
        /// <returns>The deserialized entity</returns>
        public Entity DeserializeEntity(string json)
        {
            // Deserialize entity data
            var entityData = JsonSerializer.Deserialize<EntityData>(json);
            
            // Create entity with the same ID
            // Note: In a real implementation, we'd need to handle ID conflicts
            Entity entity = new Entity(entityData.Id);
            
            // Add all components
            foreach (var componentData in entityData.Components)
            {
                // Get component type
                Type componentType = Type.GetType(componentData.Type);
                if (componentType == null)
                {
                    Console.WriteLine($"Warning: Unknown component type {componentData.Type}");
                    continue;
                }
                
                // Deserialize component
                object component = JsonSerializer.Deserialize(componentData.Data, componentType);
                
                // Use reflection to call AddComponent<T>
                MethodInfo addComponentMethod = typeof(World).GetMethod("AddComponent")
                    .MakeGenericMethod(componentType);
                
                addComponentMethod.Invoke(_world, new object[] { entity, component });
            }
            
            return entity;
        }
        
        /// <summary>
        /// Serialize multiple entities to a JSON string
        /// </summary>
        /// <param name="entities">Entities to serialize</param>
        /// <returns>JSON representation of the entities</returns>
        public string SerializeEntities(IEnumerable<Entity> entities)
        {
            var entitiesData = new List<string>();
            
            foreach (var entity in entities)
            {
                entitiesData.Add(SerializeEntity(entity));
            }
            
            return JsonSerializer.Serialize(entitiesData);
        }
        
        /// <summary>
        /// Deserialize multiple entities from a JSON string
        /// </summary>
        /// <param name="json">JSON representation of the entities</param>
        /// <returns>The deserialized entities</returns>
        public List<Entity> DeserializeEntities(string json)
        {
            var entitiesData = JsonSerializer.Deserialize<List<string>>(json);
            var entities = new List<Entity>();
            
            foreach (var entityJson in entitiesData)
            {
                entities.Add(DeserializeEntity(entityJson));
            }
            
            return entities;
        }
        
        /// <summary>
        /// Save entities to a file
        /// </summary>
        /// <param name="entities">Entities to save</param>
        /// <param name="filePath">Path to save to</param>
        public void SaveEntitiesToFile(IEnumerable<Entity> entities, string filePath)
        {
            string json = SerializeEntities(entities);
            File.WriteAllText(filePath, json);
        }
        
        /// <summary>
        /// Load entities from a file
        /// </summary>
        /// <param name="filePath">Path to load from</param>
        /// <returns>The loaded entities</returns>
        public List<Entity> LoadEntitiesFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return DeserializeEntities(json);
        }
        
        /// <summary>
        /// Get all component types in the assembly
        /// </summary>
        /// <returns>List of component types</returns>
        private List<Type> GetAllComponentTypes()
        {
            var componentTypes = new List<Type>();
            
            // Get the assembly containing our components
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            // Find all types that implement IComponent
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IComponent).IsAssignableFrom(type) && 
                    type.IsValueType &&  // Ensure it's a struct
                    !type.IsInterface)   // Exclude the interface itself
                {
                    componentTypes.Add(type);
                }
            }
            
            return componentTypes;
        }
    }
    
    /// <summary>
    /// Data structure for serialized entities
    /// </summary>
    [Serializable]
    public class EntityData
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Components attached to the entity
        /// </summary>
        public List<ComponentData> Components { get; set; }
    }
    
    /// <summary>
    /// Data structure for serialized components
    /// </summary>
    [Serializable]
    public class ComponentData
    {
        /// <summary>
        /// Component type name
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Serialized component data
        /// </summary>
        public string Data { get; set; }
    }
}