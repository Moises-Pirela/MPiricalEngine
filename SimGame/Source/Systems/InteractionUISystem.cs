using System;
using MPirical.Components;
using MPirical.Core.ECS;

namespace MPirical.Systems;

/// <summary>
/// System that handles interaction highlighting and UI
/// </summary>
public class InteractionUISystem : ISystem
{
    private World _world;
    private Entity _playerEntity;
    private bool _playerFound = false;
    private Entity _highlightedEntity = new Entity(-1);

    /// <summary>
    /// Name of this system
    /// </summary>
    public string Name => "InteractionUISystem";

    /// <summary>
    /// Priority of this system (runs after input)
    /// </summary>
    public int Priority => 800;

    /// <summary>
    /// Initialize the system with the world
    /// </summary>
    /// <param name="world">Reference to the world</param>
    public void Initialize(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Update interaction UI
    /// </summary>
    /// <param name="deltaTime">Time since last update</param>
    public void Update(float deltaTime)
    {
        // Find player entity if not already found
        if (!_playerFound)
        {
            FindPlayerEntity();
        }

        // Clear previous highlight
        if (_highlightedEntity.Id != -1)
        {
            ClearHighlight(_highlightedEntity);
            _highlightedEntity = new Entity(-1);
        }

        // Check for entities to highlight
        if (_playerFound)
        {
            if (!_world.HasComponent<PlayerComponent>(_playerEntity) ||
                !_world.HasComponent<TransformComponent>(_playerEntity))
            {
                _playerFound = false;
                return;
            }

            // Get player components
            var player = _world.GetComponent<PlayerComponent>(_playerEntity);
            var transform = _world.GetComponent<TransformComponent>(_playerEntity);

            // Find interactable in view
            _highlightedEntity = FindInteractableInView(transform, player.InteractionRange);

            // Highlight the entity if found
            if (_highlightedEntity.Id != -1)
            {
                HighlightEntity(_highlightedEntity);

                // Show interaction prompt
                if (_world.HasComponent<InteractableComponent>(_highlightedEntity))
                {
                    var interactable = _world.GetComponent<InteractableComponent>(_highlightedEntity);
                    ShowInteractionPrompt(interactable);
                }
            }
        }
    }

    /// <summary>
    /// Find the player entity in the world
    /// </summary>
    private void FindPlayerEntity()
    {
        // In a real implementation, we would have a more efficient way to find the player
        // For now, this is a placeholder implementation

        // Similar to how the player input system finds the player

        _playerFound = false; // Placeholder
    }

    /// <summary>
    /// Find an interactable entity in the player's view
    /// </summary>
    private Entity FindInteractableInView(TransformComponent playerTransform, float interactionRange)
    {
        // In a real implementation, this would use the physics system
        // to perform a proper raycast against colliders

        // For now, return an invalid entity as a placeholder
        return new Entity(-1);
    }

    /// <summary>
    /// Highlight an entity to show it's interactable
    /// </summary>
    private void HighlightEntity(Entity entity)
    {
        // In a real implementation, this would apply a highlight shader effect
        // or change the material properties of the entity
    }

    /// <summary>
    /// Clear highlight from an entity
    /// </summary>
    private void ClearHighlight(Entity entity)
    {
        // In a real implementation, this would restore the original shader effect
        // or material properties of the entity
    }

    /// <summary>
    /// Show interaction prompt for an interactable
    /// </summary>
    private void ShowInteractionPrompt(InteractableComponent interactable)
    {
        // In a real implementation, this would update UI elements
        // to show the interaction prompt

        // For example: "Press F to [Use] Door"
        string actionText = GetActionTextForInteractionType(interactable.Type);
        string promptText = $"Press F to {actionText} {interactable.DisplayName}";

        // Update UI text element (placeholder)
        Console.WriteLine(promptText);
    }

    /// <summary>
    /// Get action text for an interaction type
    /// </summary>
    private string GetActionTextForInteractionType(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.Pickup: return "Pick Up";
            case InteractionType.Use: return "Use";
            case InteractionType.Toggle: return "Toggle";
            case InteractionType.Read: return "Read";
            case InteractionType.Open: return "Open";
            case InteractionType.Hack: return "Hack";
            case InteractionType.Talk: return "Talk to";
            case InteractionType.Push: return "Push";
            case InteractionType.Custom: return "Interact with";
            default: return "Use";
        }
    }
}