using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MPirical.Core.ECS;

namespace MPirical.Components.AI
{
    /// <summary>
    /// Component that handles vision perception for AI entities
    /// </summary>
    public struct VisionPerceptionComponent : IComponent
    {
        /// <summary>
        /// Maximum distance the AI can see
        /// </summary>
        public float ViewDistance;
        
        /// <summary>
        /// Field of view angle in degrees
        /// </summary>
        public float FieldOfViewDegrees;
        
        /// <summary>
        /// How quickly the AI notices entities that enter its FOV (0-1)
        /// </summary>
        public float AwarenessSpeed;
        
        /// <summary>
        /// How much light affects vision (multiplier)
        /// </summary>
        public float LightSensitivity;
        
        /// <summary>
        /// Entities currently visible to this AI
        /// Maps entity ID to awareness level (0-1)
        /// </summary>
        public Dictionary<int, float> VisibleEntities;
        
        /// <summary>
        /// Last known positions of entities
        /// Maps entity ID to last known position
        /// </summary>
        public Dictionary<int, Vector3> LastKnownPositions;
        
        /// <summary>
        /// Time since the entity was last seen
        /// Maps entity ID to elapsed time in seconds
        /// </summary>
        public Dictionary<int, float> TimeSinceLastSeen;
    }

    /// <summary>
    /// Configuration for vision perception component
    /// </summary>
    public class VisionPerceptionComponentConfig : IComponentConfig<VisionPerceptionComponent>
    {
        /// <summary>
        /// Creates a vision perception component with default values
        /// </summary>
        /// <returns>A new vision perception component</returns>
        public VisionPerceptionComponent CreateDefault()
        {
            return new VisionPerceptionComponent
            {
                ViewDistance = 15.0f,
                FieldOfViewDegrees = 110.0f,
                AwarenessSpeed = 0.5f,
                LightSensitivity = 1.0f,
                VisibleEntities = new Dictionary<int, float>(),
                LastKnownPositions = new Dictionary<int, Vector3>(),
                TimeSinceLastSeen = new Dictionary<int, float>()
            };
        }

        /// <summary>
        /// Creates a vision perception component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured vision perception component</returns>
        public VisionPerceptionComponent Create(Action<VisionPerceptionComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }

    /// <summary>
    /// Component that handles hearing perception for AI entities
    /// </summary>
    public struct HearingPerceptionComponent : IComponent
    {
        /// <summary>
        /// Maximum distance the AI can hear at normal volume
        /// </summary>
        public float HearingRange;
        
        /// <summary>
        /// How sensitive the AI is to sounds (multiplier)
        /// </summary>
        public float HearingSensitivity;
        
        /// <summary>
        /// Time it takes for the AI to forget a sound (in seconds)
        /// </summary>
        public float MemoryDuration;
        
        /// <summary>
        /// List of sounds currently perceived
        /// </summary>
        public List<SoundPerception> PerceivedSounds;
    }

    /// <summary>
    /// Represents a sound heard by an AI
    /// </summary>
    public struct SoundPerception
    {
        /// <summary>
        /// Source of the sound (entity ID)
        /// </summary>
        public int SourceEntityId;
        
        /// <summary>
        /// Where the sound originated
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// Volume of the sound (0-1)
        /// </summary>
        public float Volume;
        
        /// <summary>
        /// Type of sound for contextual awareness
        /// </summary>
        public SoundType Type;
        
        /// <summary>
        /// When the sound was heard (for memory decay)
        /// </summary>
        public float TimeHeard;
        
        /// <summary>
        /// Priority of the sound for AI decision making
        /// </summary>
        public float Priority;
    }

    /// <summary>
    /// Types of sounds that can be perceived
    /// </summary>
    public enum SoundType
    {
        Footstep,
        Door,
        Combat,
        Voice,
        Alarm,
        Explosion,
        Object,
        Environment,
        Unknown
    }

    /// <summary>
    /// Configuration for hearing perception component
    /// </summary>
    public class HearingPerceptionComponentConfig : IComponentConfig<HearingPerceptionComponent>
    {
        /// <summary>
        /// Creates a hearing perception component with default values
        /// </summary>
        /// <returns>A new hearing perception component</returns>
        public HearingPerceptionComponent CreateDefault()
        {
            return new HearingPerceptionComponent
            {
                HearingRange = 20.0f,
                HearingSensitivity = 1.0f,
                MemoryDuration = 10.0f,
                PerceivedSounds = new List<SoundPerception>()
            };
        }

        /// <summary>
        /// Creates a hearing perception component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured hearing perception component</returns>
        public HearingPerceptionComponent Create(Action<HearingPerceptionComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }

    /// <summary>
    /// Component that handles memory for AI entities
    /// </summary>
    public struct MemoryComponent : IComponent
    {
        /// <summary>
        /// Known entities and their last state
        /// </summary>
        public Dictionary<int, EntityMemory> KnownEntities;
        
        /// <summary>
        /// Interest points that require investigation
        /// </summary>
        public List<InterestPoint> InterestPoints;
        
        /// <summary>
        /// Current alert level of the AI (0-1)
        /// </summary>
        public float AlertLevel;
        
        /// <summary>
        /// How quickly alert level decays when no threats are present
        /// </summary>
        public float AlertDecayRate;
        
        /// <summary>
        /// Maximum time the AI will remember entities (in seconds)
        /// </summary>
        public float MemoryRetention;
    }

    /// <summary>
    /// Memory of a specific entity
    /// </summary>
    public struct EntityMemory
    {
        /// <summary>
        /// Entity ID this memory is about
        /// </summary>
        public int EntityId;
        
        /// <summary>
        /// Last known position
        /// </summary>
        public Vector3 LastPosition;
        
        /// <summary>
        /// Time since this entity was last perceived
        /// </summary>
        public float TimeSinceLastSeen;
        
        /// <summary>
        /// Perceived threat level (0-1)
        /// </summary>
        public float ThreatLevel;
        
        /// <summary>
        /// Whether this entity is considered hostile
        /// </summary>
        public bool IsHostile;
        
        /// <summary>
        /// Last known velocity
        /// </summary>
        public Vector3 LastVelocity;
    }

    /// <summary>
    /// A point of interest for AI investigation
    /// </summary>
    public struct InterestPoint
    {
        /// <summary>
        /// Position to investigate
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// Time this interest point was created
        /// </summary>
        public float CreationTime;
        
        /// <summary>
        /// Priority of this interest point (higher = more important)
        /// </summary>
        public float Priority;
        
        /// <summary>
        /// Type of interest point
        /// </summary>
        public InterestType Type;
        
        /// <summary>
        /// Whether this interest point has been investigated
        /// </summary>
        public bool Investigated;
    }

    /// <summary>
    /// Types of interest points for AI investigation
    /// </summary>
    public enum InterestType
    {
        Sound,
        VisualDisturbance,
        LightChange,
        SuspiciousActivity,
        LastKnownPosition,
        PatrolPoint
    }

    /// <summary>
    /// Configuration for memory component
    /// </summary>
    public class MemoryComponentConfig : IComponentConfig<MemoryComponent>
    {
        /// <summary>
        /// Creates a memory component with default values
        /// </summary>
        /// <returns>A new memory component</returns>
        public MemoryComponent CreateDefault()
        {
            return new MemoryComponent
            {
                KnownEntities = new Dictionary<int, EntityMemory>(),
                InterestPoints = new List<InterestPoint>(),
                AlertLevel = 0.0f,
                AlertDecayRate = 0.1f,
                MemoryRetention = 60.0f
            };
        }

        /// <summary>
        /// Creates a memory component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured memory component</returns>
        public MemoryComponent Create(Action<MemoryComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }
}