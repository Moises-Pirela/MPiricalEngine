using System;
using System.Collections.Generic;
using System.Numerics;
using MPirical.Core.ECS;
using MPirical.Components;
using MPirical.Components.AI;

namespace MPirical.Systems
{
    /// <summary>
    /// System that updates transform hierarchies
    /// </summary>
    public class TransformHierarchySystem : ISystem
    {
        private World _world;
        private Dictionary<int, Entity> _entityLookup = new Dictionary<int, Entity>();
        
        /// <summary>
        /// Name of this system
        /// </summary>
        public string Name => "TransformHierarchySystem";
        
        /// <summary>
        /// Priority of this system (runs early in the update loop)
        /// </summary>
        public int Priority => 100;

        /// <summary>
        /// Initialize the system with the world
        /// </summary>
        /// <param name="world">Reference to the world</param>
        public void Initialize(World world)
        {
            _world = world;
        }

        /// <summary>
        /// Update transform hierarchies
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Update our entity lookup table first
            // In a real implementation, we would listen for entity creation/destruction events
            UpdateEntityLookup();

            // Find all entities with hierarchy components but no parents (roots)
            // We'll update hierarchies starting from the roots
            foreach (var entity in _entityLookup.Values)
            {
                // Skip if entity doesn't have both transform and hierarchy components
                if (!_world.HasComponent<TransformComponent>(entity) || 
                    !_world.HasComponent<HierarchyComponent>(entity))
                    continue;

                var hierarchy = _world.GetComponent<HierarchyComponent>(entity);
                
                // Only process root entities (no parent)
                if (hierarchy.ParentId != -1)
                    continue;
                
                // Update this hierarchy tree starting from the root
                UpdateHierarchyRecursive(entity, Matrix4x4.Identity);
            }
        }

        /// <summary>
        /// Update our entity lookup table
        /// </summary>
        private void UpdateEntityLookup()
        {
            // In a real implementation, this would be more efficient by tracking entity creation/destruction
            // For simplicity, we're doing a full refresh
            _entityLookup.Clear();
            
            // We would need to iterate all entities in the world here
            // For now, this is a placeholder implementation
        }
        
        /// <summary>
        /// Recursively update a hierarchy tree
        /// </summary>
        /// <param name="entity">Current entity to update</param>
        /// <param name="parentWorldMatrix">World matrix of the parent</param>
        private void UpdateHierarchyRecursive(Entity entity, Matrix4x4 parentWorldMatrix)
        {
            var transform = _world.GetComponent<TransformComponent>(entity);
            var hierarchy = _world.GetComponent<HierarchyComponent>(entity);
            
            // Calculate local transform matrix
            Matrix4x4 localMatrix = CalculateLocalMatrix(hierarchy.LocalPosition, 
                                                        hierarchy.LocalRotation, 
                                                        hierarchy.LocalScale);
            
            // Calculate world matrix by combining with parent
            Matrix4x4 worldMatrix = Matrix4x4.Multiply(localMatrix, parentWorldMatrix);
            
            // Extract world transform properties and update transform component
            DecomposeMatrix(worldMatrix, out Vector3 position, out Quaternion rotation, out Vector3 scale);
            
            // Update the transform component with the new world values
            transform.Position = position;
            transform.Rotation = rotation;
            transform.Scale = scale;
            
            // Update the component in the world
            _world.AddComponent(entity, transform);
            
            // Recursively update children
            foreach (int childId in hierarchy.ChildIds)
            {
                if (_entityLookup.TryGetValue(childId, out Entity childEntity))
                {
                    UpdateHierarchyRecursive(childEntity, worldMatrix);
                }
            }
        }
        
        /// <summary>
        /// Calculate a local transformation matrix
        /// </summary>
        private Matrix4x4 CalculateLocalMatrix(Vector3 position, Quaternion rotation, Vector3 scale)
        {   
            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(position);
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation);
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scale);
            
            // Combine: Scale -> Rotate -> Translate
            return Matrix4x4.Multiply(Matrix4x4.Multiply(scaleMatrix, rotationMatrix), translationMatrix);
        }
        
        /// <summary>
        /// Decompose a transformation matrix into position, rotation, and scale
        /// </summary>
        private void DecomposeMatrix(Matrix4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            Matrix4x4.Decompose(matrix, out scale, out rotation, out position);
        }
    }

    /// <summary>
    /// System that handles AI vision perception
    /// </summary>
    public class VisionPerceptionSystem : ISystem
    {
        private World _world;
        private List<Entity> _visionEntities = new List<Entity>();
        private List<Entity> _potentialTargets = new List<Entity>();
        
        /// <summary>
        /// Name of this system
        /// </summary>
        public string Name => "VisionPerceptionSystem";
        
        /// <summary>
        /// Priority of this system
        /// </summary>
        public int Priority => 300;

        /// <summary>
        /// Initialize the system with the world
        /// </summary>
        /// <param name="world">Reference to the world</param>
        public void Initialize(World world)
        {
            _world = world;
        }

        /// <summary>
        /// Update vision perception for AI entities
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Update our entity lists
            UpdateEntityLists();
            
            // Process vision for each entity with vision perception
            foreach (var entity in _visionEntities)
            {
                if (!_world.HasComponent<TransformComponent>(entity))
                    continue;
                
                var vision = _world.GetComponent<VisionPerceptionComponent>(entity);
                var transform = _world.GetComponent<TransformComponent>(entity);
                
                // Get the forward vector of the entity
                Vector3 forward = transform.Forward;
                
                // Calculate FOV in radians
                float halfFovRadians = MathF.PI * vision.FieldOfViewDegrees / 360.0f;
                float cosHalfFov = MathF.Cos(halfFovRadians);
                
                // Create a set of newly visible entities this frame
                HashSet<int> visibleThisFrame = new HashSet<int>();
                
                // Check each potential target
                foreach (var targetEntity in _potentialTargets)
                {
                    // Skip self
                    if (targetEntity.Id == entity.Id)
                        continue;
                    
                    if (!_world.HasComponent<TransformComponent>(targetEntity))
                        continue;
                    
                    var targetTransform = _world.GetComponent<TransformComponent>(targetEntity);
                    
                    // Calculate vector to target
                    Vector3 directionToTarget = Vector3.Normalize(targetTransform.Position - transform.Position);
                    
                    // Calculate distance to target
                    float distanceToTarget = Vector3.Distance(transform.Position, targetTransform.Position);
                    
                    // Check if target is within view distance
                    if (distanceToTarget > vision.ViewDistance)
                        continue;
                    
                    // Check if target is within FOV
                    float dotProduct = Vector3.Dot(forward, directionToTarget);
                    if (dotProduct < cosHalfFov)
                        continue;
                    
                    // Check for line of sight (would use raycasting in a real implementation)
                    bool hasLineOfSight = CheckLineOfSight(transform.Position, targetTransform.Position);
                    if (!hasLineOfSight)
                        continue;
                    
                    // Target is visible, calculate awareness level
                    
                    // Base awareness based on distance (closer = more aware)
                    float distanceFactor = 1.0f - (distanceToTarget / vision.ViewDistance);
                    
                    // Angle factor (centered in FOV = more aware)
                    float angleFactor = (dotProduct - cosHalfFov) / (1.0f - cosHalfFov);
                    
                    // Light factor (would be calculated based on lighting at the target's position)
                    float lightFactor = CalculateLightFactor(targetTransform.Position);
                    
                    // Calculate target visibility
                    float visibility = distanceFactor * angleFactor * lightFactor;
                    
                    // Get current awareness or start at 0
                    float currentAwareness = 0.0f;
                    vision.VisibleEntities.TryGetValue(targetEntity.Id, out currentAwareness);
                    
                    // Update awareness based on visibility and awareness speed
                    float newAwareness = MathF.Min(1.0f, currentAwareness + visibility * vision.AwarenessSpeed * deltaTime);
                    
                    // Update visible entities and tracking data
                    vision.VisibleEntities[targetEntity.Id] = newAwareness;
                    vision.LastKnownPositions[targetEntity.Id] = targetTransform.Position;
                    vision.TimeSinceLastSeen[targetEntity.Id] = 0.0f;
                    
                    // Mark as visible this frame
                    visibleThisFrame.Add(targetEntity.Id);
                }
                
                // Update time since last seen for entities not visible this frame
                List<int> entitiesToRemove = new List<int>();
                foreach (var kvp in vision.TimeSinceLastSeen)
                {
                    int targetId = kvp.Key;
                    if (!visibleThisFrame.Contains(targetId))
                    {
                        // Increase time since last seen
                        vision.TimeSinceLastSeen[targetId] += deltaTime;
                        
                        // Decrease awareness over time
                        if (vision.VisibleEntities.ContainsKey(targetId))
                        {
                            float currentAwareness = vision.VisibleEntities[targetId];
                            vision.VisibleEntities[targetId] = MathF.Max(0.0f, currentAwareness - deltaTime * 0.2f);
                            
                            // Remove if awareness drops to zero
                            if (vision.VisibleEntities[targetId] <= 0.0f)
                            {
                                entitiesToRemove.Add(targetId);
                            }
                        }
                    }
                }
                
                // Remove entities that are no longer tracked
                foreach (int targetId in entitiesToRemove)
                {
                    vision.VisibleEntities.Remove(targetId);
                    vision.TimeSinceLastSeen.Remove(targetId);
                    // Keep last known position for memory
                }
                
                // Update the vision component in the world
                _world.AddComponent(entity, vision);
            }
        }
        
        /// <summary>
        /// Check if there is a clear line of sight between two points
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <returns>True if there is a clear line of sight</returns>
        private bool CheckLineOfSight(Vector3 start, Vector3 end)
        {
            // In a real implementation, this would use raycasting against the physics system
            // For now, we'll just return true as a placeholder
            return true;
        }
        
        /// <summary>
        /// Calculate how visible a point is based on lighting
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Light factor between 0 and 1</returns>
        private float CalculateLightFactor(Vector3 position)
        {
            // In a real implementation, this would check the lighting system
            // For now, return a placeholder value
            return 0.8f;
        }
        
        /// <summary>
        /// Update the lists of entities we're tracking
        /// </summary>
        private void UpdateEntityLists()
        {
            // In a real implementation, we would have a more efficient way of tracking
            // entities with VisionPerceptionComponents and potential targets
            _visionEntities.Clear();
            _potentialTargets.Clear();
            
            // We would need to iterate all entities in the world
            // For now, this is a placeholder implementation
        }
    }

    /// <summary>
    /// System that handles AI hearing perception
    /// </summary>
    public class HearingPerceptionSystem : ISystem
    {
        private World _world;
        private List<Entity> _hearingEntities = new List<Entity>();
        
        /// <summary>
        /// Name of this system
        /// </summary>
        public string Name => "HearingPerceptionSystem";
        
        /// <summary>
        /// Priority of this system
        /// </summary>
        public int Priority => 350;

        /// <summary>
        /// Initialize the system with the world
        /// </summary>
        /// <param name="world">Reference to the world</param>
        public void Initialize(World world)
        {
            _world = world;
        }

        /// <summary>
        /// Update hearing perception for AI entities
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            // Update our entity lists
            UpdateEntityLists();
            
            // Process hearing for each entity with hearing perception
            foreach (var entity in _hearingEntities)
            {
                if (!_world.HasComponent<TransformComponent>(entity))
                    continue;
                
                var hearing = _world.GetComponent<HearingPerceptionComponent>(entity);
                var transform = _world.GetComponent<TransformComponent>(entity);
                
                // Process all current sounds and update memory
                List<SoundPerception> soundsToRemove = new List<SoundPerception>();
                
                foreach (var sound in hearing.PerceivedSounds)
                {
                    // Check if sound has expired
                    float timeSinceHeard = GetCurrentTime() - sound.TimeHeard;
                    if (timeSinceHeard > hearing.MemoryDuration)
                    {
                        soundsToRemove.Add(sound);
                        continue;
                    }
                    
                    // In a real implementation, we might update AI memory or behaviors here
                    // based on sounds they've heard
                }
                
                // Remove expired sounds
                foreach (var sound in soundsToRemove)
                {
                    hearing.PerceivedSounds.Remove(sound);
                }
                
                // Update the hearing component in the world
                _world.AddComponent(entity, hearing);
            }
        }
        
        /// <summary>
        /// Called when a sound is emitted in the world
        /// </summary>
        /// <param name="sourceEntityId">Entity that made the sound</param>
        /// <param name="position">Position where the sound originated</param>
        /// <param name="volume">Volume of the sound (0-1)</param>
        /// <param name="type">Type of sound</param>
        /// <param name="priority">Priority of the sound (0-1)</param>
        public void EmitSound(int sourceEntityId, Vector3 position, float volume, SoundType type, float priority)
        {
            // Process each entity with hearing perception
            foreach (var entity in _hearingEntities)
            {
                if (!_world.HasComponent<TransformComponent>(entity) || 
                    !_world.HasComponent<HearingPerceptionComponent>(entity))
                    continue;
                
                var hearing = _world.GetComponent<HearingPerceptionComponent>(entity);
                var transform = _world.GetComponent<TransformComponent>(entity);
                
                // Calculate distance to sound
                float distance = Vector3.Distance(transform.Position, position);
                
                // Check if sound is within hearing range (adjusted by volume)
                float adjustedRange = hearing.HearingRange * volume;
                if (distance > adjustedRange)
                    continue;
                
                // Calculate perceived volume based on distance and hearing sensitivity
                float distanceFactor = 1.0f - (distance / adjustedRange);
                float perceivedVolume = volume * distanceFactor * hearing.HearingSensitivity;
                
                // Create sound perception
                SoundPerception soundPerception = new SoundPerception
                {
                    SourceEntityId = sourceEntityId,
                    Position = position,
                    Volume = perceivedVolume,
                    Type = type,
                    TimeHeard = GetCurrentTime(),
                    Priority = priority * perceivedVolume // Adjust priority by perceived volume
                };
                
                // Add to perceived sounds
                hearing.PerceivedSounds.Add(soundPerception);
                
                // Update the hearing component in the world
                _world.AddComponent(entity, hearing);
            }
        }
        
        /// <summary>
        /// Get current time in seconds
        /// </summary>
        /// <returns>Current time</returns>
        private float GetCurrentTime()
        {
            // In a real implementation, this would use the game's time system
            return (float)DateTime.Now.TimeOfDay.TotalSeconds;
        }
        
        /// <summary>
        /// Update the list of entities with hearing perception
        /// </summary>
        private void UpdateEntityLists()
        {
            // In a real implementation, we would have a more efficient way of tracking
            // entities with HearingPerceptionComponents
            _hearingEntities.Clear();
            
            // We would need to iterate all entities in the world
            // For now, this is a placeholder implementation
        }
    }
}