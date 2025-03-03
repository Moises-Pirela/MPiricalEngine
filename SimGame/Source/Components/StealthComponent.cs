using System;
using System.Numerics;
using MPirical.Core.ECS;
namespace MPirical.Components;

/// <summary>
/// Component that defines an entity's visibility in the stealth system
/// </summary>
public struct StealthComponent : IComponent
{
    /// <summary>
    /// How visible the entity is (0-1, where 0 is invisible and 1 is fully visible)
    /// </summary>
    public float Visibility;

    /// <summary>
    /// How much noise the entity makes when moving (0-1)
    /// </summary>
    public float NoiseLevel;

    /// <summary>
    /// Whether the entity is currently in shadow (affects visibility)
    /// </summary>
    public bool IsInShadow;

    /// <summary>
    /// Current movement speed as a percentage of max speed (affects noise)
    /// </summary>
    public float MovementSpeed;

    /// <summary>
    /// Whether the entity is currently crouched (affects visibility and noise)
    /// </summary>
    public bool IsCrouched;

    /// <summary>
    /// Materials the entity is currently moving on (affects noise)
    /// </summary>
    public SurfaceType CurrentSurface;
}

/// <summary>
/// Types of surfaces that affect sound propagation
/// </summary>
public enum SurfaceType
{
    Default,
    Carpet,
    Wood,
    Tile,
    Metal,
    Grass,
    Water,
    Gravel
}

/// <summary>
/// Configuration for stealth component
/// </summary>
public class StealthComponentConfig : IComponentConfig<StealthComponent>
{
    /// <summary>
    /// Creates a stealth component with default values
    /// </summary>
    /// <returns>A new stealth component</returns>
    public StealthComponent CreateDefault()
    {
        return new StealthComponent
        {
            Visibility = 0.5f,
            NoiseLevel = 0.5f,
            IsInShadow = false,
            MovementSpeed = 0.0f,
            IsCrouched = false,
            CurrentSurface = SurfaceType.Default
        };
    }

    /// <summary>
    /// Creates a stealth component with custom configuration
    /// </summary>
    /// <param name="configureAction">Action to configure the component</param>
    /// <returns>A configured stealth component</returns>
    public StealthComponent Create(Action<StealthComponent> configureAction)
    {
        var component = CreateDefault();
        configureAction(component);
        return component;
    }
}