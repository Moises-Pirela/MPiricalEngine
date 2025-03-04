using System;
using MPirical.Components;
using MPirical.Components.AI;
using MPirical.Core.ECS;

namespace MPirical.Core
{
    
    public enum Archetypes
    {
        PLAYER,
        ENEMY,
        CAMERA,
        INTERACTABLE,
        ITEM,
        LIGHT,
        DOOR,
        LOOTABLE,
        PHYSICS
    }
    
    /// <summary>
    /// Static class to define common entity archetypes
    /// </summary>
    public static class CommonArchetypes
    {
        /// <summary>
        /// Registers common archetypes to a world
        /// </summary>
        /// <param name="world">The world to register archetypes to</param>
        public static void RegisterCommonArchetypes(World world)
        {
            // Player archetype
            world.RegisterArchetype(new Archetype(Archetypes.PLAYER, 
                typeof(PlayerComponent), 
                typeof(TransformComponent), 
                typeof(RigidBodyComponent),
                typeof(ColliderComponent),
                typeof(StealthComponent),
                typeof(InventoryComponent)));
                
            // Enemy archetype
            world.RegisterArchetype(new Archetype(Archetypes.ENEMY,
                typeof(TransformComponent),
                typeof(RigidBodyComponent),
                typeof(ColliderComponent),
                typeof(VisionPerceptionComponent),
                typeof(HearingPerceptionComponent),
                typeof(MemoryComponent)));
                
            // Camera archetype
            world.RegisterArchetype(new Archetype(Archetypes.CAMERA,
                typeof(TransformComponent)));
                
            // Interactable archetype
            world.RegisterArchetype(new Archetype(Archetypes.INTERACTABLE,
                typeof(TransformComponent),
                typeof(InteractableComponent)));
                
            // Item archetype
            world.RegisterArchetype(new Archetype(Archetypes.ITEM,
                typeof(TransformComponent),
                typeof(ItemComponent)));
                
            // Light source archetype
            world.RegisterArchetype(new Archetype(Archetypes.LIGHT,
                typeof(TransformComponent),
                typeof(LightSourceComponent)));
                
            // Door archetype
            world.RegisterArchetype(new Archetype(Archetypes.DOOR,
                typeof(TransformComponent),
                typeof(InteractableComponent),
                typeof(ColliderComponent)));
                
            // Container archetype (for lootable containers)
            world.RegisterArchetype(new Archetype(Archetypes.LOOTABLE,
                typeof(TransformComponent),
                typeof(InteractableComponent),
                typeof(InventoryComponent)));
            
            world.RegisterArchetype(new Archetype( Archetypes.PHYSICS,
                typeof(TransformComponent),
                typeof(RigidBodyComponent)));
        }
    }
}