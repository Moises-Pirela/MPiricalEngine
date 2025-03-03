 using System;
 using System.Collections.Generic;
 using System.Numerics;
 using MPirical.Components;
 using MPirical.Components.AI;
 using MPirical.Core.ECS;
 namespace MPirical.Systems;

 /// <summary>
    /// System that manages stealth mechanics
    /// </summary>
    public class StealthSystem : ISystem
    {
        private World _world;
        private List<Entity> _stealthEntities = new List<Entity>();
        private List<Entity> _lightSources = new List<Entity>();
        private HearingPerceptionSystem _hearingSystem;
        
        /// <summary>
        /// Name of this system
        /// </summary>
        public string Name => "StealthSystem";
        
        /// <summary>
        /// Priority of this system
        /// </summary>
        public int Priority => 400;

        /// <summary>
        /// Initialize the system with the world
        /// </summary>
        /// <param name="world">Reference to the world</param>
        public void Initialize(World world)
        {
            _world = world;
            
            // Find the hearing system for sound emission
            foreach (var system in GetSystems())
            {
                if (system is HearingPerceptionSystem hearingSystem)
                {
                    _hearingSystem = hearingSystem;
                    break;
                }
            }
        }

        /// <summary>
        /// Update stealth mechanics
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Update entity lists
            UpdateEntityLists();
            
            // Process each entity with stealth component
            foreach (var entity in _stealthEntities)
            {
                if (!_world.HasComponent<StealthComponent>(entity) || 
                    !_world.HasComponent<TransformComponent>(entity))
                    continue;
                
                var stealth = _world.GetComponent<StealthComponent>(entity);
                var transform = _world.GetComponent<TransformComponent>(entity);
                
                // Check if entity is in shadow
                stealth.IsInShadow = IsInShadow(transform.Position);
                
                // Calculate base visibility (adjusted by shadow)
                float baseVisibility = stealth.IsInShadow ? 0.3f : 1.0f;
                
                // Adjust visibility based on crouch and movement
                float movementFactor = stealth.MovementSpeed;
                float stanceFactor = stealth.IsCrouched ? 0.5f : 1.0f;
                
                // Calculate final visibility
                stealth.Visibility = baseVisibility * movementFactor * stanceFactor;
                
                // Get surface-specific noise factor
                float surfaceFactor = GetSurfaceNoiseFactor(stealth.CurrentSurface);
                
                // Calculate noise level based on movement, stance, and surface
                stealth.NoiseLevel = surfaceFactor * movementFactor * (stealth.IsCrouched ? 0.3f : 1.0f);
                
                // Emit sound if moving and making noise
                if (stealth.MovementSpeed > 0.1f && stealth.NoiseLevel > 0.1f)
                {
                    EmitMovementSound(entity, transform.Position, stealth);
                }
                
                // Update the stealth component in the world
                _world.AddComponent(entity, stealth);
            }
        }
        
        /// <summary>
        /// Check if a position is in shadow
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is in shadow</returns>
        private bool IsInShadow(Vector3 position)
        {
            // In a real implementation, this would use the lighting system
            // to check if the position is lit by any light sources
            
            // For this demonstration, we'll do a simple distance-based check
            // against all light sources
            
            foreach (var lightEntity in _lightSources)
            {
                if (!_world.HasComponent<LightSourceComponent>(lightEntity) || 
                    !_world.HasComponent<TransformComponent>(lightEntity))
                    continue;
                
                var light = _world.GetComponent<LightSourceComponent>(lightEntity);
                var lightTransform = _world.GetComponent<TransformComponent>(lightEntity);
                
                // Skip lights that are off
                if (!light.IsOn)
                    continue;
                
                // For point and spot lights, check distance
                if (light.Type == LightType.Point || light.Type == LightType.Spot)
                {
                    float distance = Vector3.Distance(position, lightTransform.Position);
                    
                    if (distance < light.Range)
                    {
                        // For spot lights, check if position is within the cone
                        if (light.Type == LightType.Spot)
                        {
                            Vector3 directionToPosition = Vector3.Normalize(position - lightTransform.Position);
                            float dot = Vector3.Dot(lightTransform.Forward, directionToPosition);
                            float halfAngleRadians = MathF.PI * light.SpotAngle / 360.0f;
                            float cosHalfAngle = MathF.Cos(halfAngleRadians);
                            
                            if (dot > cosHalfAngle)
                            {
                                // Position is within spot light cone, check for shadows
                                if (light.CastsShadows)
                                {
                                    // Check if there's a clear line of sight
                                    if (HasLineOfSight(lightTransform.Position, position))
                                    {
                                        return false; // Not in shadow
                                    }
                                }
                                else
                                {
                                    return false; // Not in shadow (light doesn't cast shadows)
                                }
                            }
                        }
                        else // Point light
                        {
                            // Check for shadows
                            if (light.CastsShadows)
                            {
                                // Check if there's a clear line of sight
                                if (HasLineOfSight(lightTransform.Position, position))
                                {
                                    return false; // Not in shadow
                                }
                            }
                            else
                            {
                                return false; // Not in shadow (light doesn't cast shadows)
                            }
                        }
                    }
                }
                else if (light.Type == LightType.Directional)
                {
                    // For directional lights, check if the position is in the shadow
                    // of any objects in the light's direction
                    Vector3 lightDirection = -lightTransform.Forward; // Directional lights point in the negative forward direction
                    
                    if (light.CastsShadows)
                    {
                        // Check if there's a clear line of sight in the light's direction
                        // This is a simplified approach - real implementations would use shadow mapping
                        if (HasLineOfSight(position - lightDirection * 100.0f, position))
                        {
                            return false; // Not in shadow
                        }
                    }
                    else
                    {
                        return false; // Not in shadow (light doesn't cast shadows)
                    }
                }
            }
            
            // If we get here, the position is not directly lit by any light source
            return true;
        }
        
        /// <summary>
        /// Check if there is a clear line of sight between two points
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <returns>True if there is a clear line of sight</returns>
        private bool HasLineOfSight(Vector3 start, Vector3 end)
        {
            // In a real implementation, this would use raycasting against the physics system
            // For now, we'll just return true as a placeholder
            return true;
        }
        
        /// <summary>
        /// Get noise factor for a surface type
        /// </summary>
        /// <param name="surfaceType">Surface type</param>
        /// <returns>Noise factor (0-1)</returns>
        private float GetSurfaceNoiseFactor(SurfaceType surfaceType)
        {
            switch (surfaceType)
            {
                case SurfaceType.Carpet: return 0.2f;
                case SurfaceType.Wood: return 0.6f;
                case SurfaceType.Tile: return 0.8f;
                case SurfaceType.Metal: return 1.0f;
                case SurfaceType.Grass: return 0.3f;
                case SurfaceType.Water: return 0.7f;
                case SurfaceType.Gravel: return 1.0f;
                default: return 0.5f;
            }
        }
        
        /// <summary>
        /// Emit movement sound for an entity
        /// </summary>
        /// <param name="entity">Entity making the sound</param>
        /// <param name="position">Position of the sound</param>
        /// <param name="stealth">Stealth component of the entity</param>
        private void EmitMovementSound(Entity entity, Vector3 position, StealthComponent stealth)
        {
            if (_hearingSystem == null)
                return;
            
            // Calculate sound volume based on noise level and movement speed
            float volume = stealth.NoiseLevel * stealth.MovementSpeed;
            
            // Determine sound type based on surface
            SoundType soundType = SoundType.Footstep;
            
            // Emit the sound
            _hearingSystem.EmitSound(entity.Id, position, volume, soundType, 0.5f);
        }
        
        /// <summary>
        /// Update the lists of entities we're tracking
        /// </summary>
        private void UpdateEntityLists()
        {
            // In a real implementation, we would have a more efficient way of tracking
            // entities with StealthComponents and LightSourceComponents
            _stealthEntities.Clear();
            _lightSources.Clear();
            
            // We would need to iterate all entities in the world
            // For now, this is a placeholder implementation
        }
        
        /// <summary>
        /// Get all systems in the world
        /// This would be provided by the World class in a real implementation
        /// </summary>
        private IEnumerable<ISystem> GetSystems()
        {
            // Placeholder - would return systems from the world
            yield break;
        }
    }