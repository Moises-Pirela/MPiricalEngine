using System;
using Microsoft.Xna.Framework;
using MPirical.Core.ECS;
namespace MPirical.Components;

/// <summary>
/// Component that represents an interactive object
/// </summary>
public struct InteractableComponent : IComponent
{
    /// <summary>
    /// Display name for interaction prompt
    /// </summary>
    public string DisplayName;
        
    /// <summary>
    /// Type of interaction possible with this object
    /// </summary>
    public InteractionType Type;
        
    /// <summary>
    /// Whether this object is currently usable
    /// </summary>
    public bool IsEnabled;
        
    /// <summary>
    /// Custom data for this interactable (depends on interaction type)
    /// </summary>
    public string Data;
        
    /// <summary>
    /// Highlight color when focused
    /// </summary>
    public Vector3 HighlightColor;
}

/// <summary>
/// Types of interactions possible with objects
/// </summary>
public enum InteractionType
{
    Pickup,
    Use,
    Toggle,
    Read,
    Open,
    Hack,
    Talk,
    Push,
    Custom
}

/// <summary>
/// Configuration for interactable component
/// </summary>
public class InteractableComponentConfig : IComponentConfig<InteractableComponent>
{
    /// <summary>
    /// Creates an interactable component with default values
    /// </summary>
    /// <returns>A new interactable component</returns>
    public InteractableComponent CreateDefault()
    {
        return new InteractableComponent
        {
            DisplayName = "Object",
            Type = InteractionType.Use,
            IsEnabled = true,
            Data = "",
            HighlightColor = new Vector3(1.0f, 0.8f, 0.2f) // Yellow-ish highlight
        };
    }

    /// <summary>
    /// Creates an interactable component with custom configuration
    /// </summary>
    /// <param name="configureAction">Action to configure the component</param>
    /// <returns>A configured interactable component</returns>
    public InteractableComponent Create(Action<InteractableComponent> configureAction)
    {
        var component = CreateDefault();
        configureAction(component);
        return component;
    }
}