using System;
using System.Numerics;
using MPirical.Core.ECS;
namespace MPirical.Components;

/// <summary>
/// Types of light sources
/// </summary>
public enum LightType
{
    Point,
    Spot,
    Directional,
    Area
}

/// <summary>
/// Component that defines a light source for stealth mechanics
/// </summary>
public struct LightSourceComponent : IComponent
{
    /// <summary>
    /// Intensity of the light (affects visibility)
    /// </summary>
    public float Intensity;

    /// <summary>
    /// Range of the light in world units
    /// </summary>
    public float Range;

    /// <summary>
    /// Color of the light
    /// </summary>
    public Vector3 Color;

    /// <summary>
    /// Whether the light casts shadows
    /// </summary>
    public bool CastsShadows;

    /// <summary>
    /// Whether the light is currently on
    /// </summary>
    public bool IsOn;

    /// <summary>
    /// Type of light source
    /// </summary>
    public LightType Type;

    /// <summary>
    /// Angle for spot lights (in degrees)
    /// </summary>
    public float SpotAngle;
}

/// <summary>
/// Configuration for light source component
/// </summary>
public class LightSourceComponentConfig : IComponentConfig<LightSourceComponent>
{
    /// <summary>
    /// Creates a light source component with default values
    /// </summary>
    /// <returns>A new light source component</returns>
    public LightSourceComponent CreateDefault()
    {
        return new LightSourceComponent
        {
            Intensity = 1.0f,
            Range = 10.0f,
            Color = new Vector3(1.0f, 1.0f, 1.0f), // White
            CastsShadows = true,
            IsOn = true,
            Type = LightType.Point,
            SpotAngle = 45.0f
        };
    }

    /// <summary>
    /// Creates a light source component with custom configuration
    /// </summary>
    /// <param name="configureAction">Action to configure the component</param>
    /// <returns>A configured light source component</returns>
    public LightSourceComponent Create(Action<LightSourceComponent> configureAction)
    {
        var component = CreateDefault();
        configureAction(component);
        return component;
    }
}