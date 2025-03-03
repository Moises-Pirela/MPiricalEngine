using System;
using Microsoft.Xna.Framework;
using MPirical.Core.ECS;

namespace MPirical.Components;

/// <summary>
/// Component that marks an entity as player-controlled
/// </summary>
public struct PlayerComponent : IComponent
{
    /// <summary>
    /// Movement speed in units per second
    /// </summary>
    public float MovementSpeed;

    /// <summary>
    /// Mouse sensitivity for camera rotation
    /// </summary>
    public float MouseSensitivity;

    /// <summary>
    /// Maximum interaction distance
    /// </summary>
    public float InteractionRange;

    /// <summary>
    /// Current health of the player
    /// </summary>
    public float Health;

    /// <summary>
    /// Maximum health of the player
    /// </summary>
    public float MaxHealth;

    /// <summary>
    /// Whether the player is currently crouching
    /// </summary>
    public bool IsCrouching;

    /// <summary>
    /// Whether the player is currently leaning
    /// </summary>
    public float LeanAmount; // -1 (left) to 1 (right)
}

/// <summary>
/// Configuration for player component
/// </summary>
public class PlayerComponentConfig : IComponentConfig<PlayerComponent>
{
    /// <summary>
    /// Creates a player component with default values
    /// </summary>
    /// <returns>A new player component</returns>
    public PlayerComponent CreateDefault()
    {
        return new PlayerComponent
        {
            MovementSpeed = 5.0f,
            MouseSensitivity = 0.002f,
            InteractionRange = 2.5f,
            Health = 100.0f,
            MaxHealth = 100.0f,
            IsCrouching = false,
            LeanAmount = 0.0f
        };
    }

    /// <summary>
    /// Creates a player component with custom configuration
    /// </summary>
    /// <param name="configureAction">Action to configure the component</param>
    /// <returns>A configured player component</returns>
    public PlayerComponent Create(Action<PlayerComponent> configureAction)
    {
        var component = CreateDefault();
        configureAction(component);
        return component;
    }
}