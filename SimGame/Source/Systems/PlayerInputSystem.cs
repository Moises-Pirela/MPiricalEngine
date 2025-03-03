using System;
using Microsoft.Xna.Framework.Input;
using MPirical.Components;
using MPirical.Core.ECS;
using Microsoft.Xna.Framework;
using MPirical.Core;
using MPirical.Core.Math;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace MPirical.Systems;

/// <summary>
/// System that handles player input and movement
/// </summary>
public class PlayerInputSystem : ISystem
{
    private World _world;
    private Entity _playerEntity;
    private bool _playerFound = false;

    private KeyboardState _currentKeyboardState;
    private KeyboardState _previousKeyboardState;
    private MouseState _currentMouseState;
    private MouseState _previousMouseState;

    private bool _isMouseLocked = false;
    private Vector2 _mouseDelta = Vector2.Zero;

    /// <summary>
    /// Name of this system
    /// </summary>
    public string Name => "PlayerInputSystem";

    /// <summary>
    /// Priority of this system (runs early)
    /// </summary>
    public int Priority => 50;

    /// <summary>
    /// Initialize the system with the world
    /// </summary>
    /// <param name="world">Reference to the world</param>
    public void Initialize(World world)
    {
        _world = world;

        // Initial input state
        _currentKeyboardState = Keyboard.GetState();
        _previousKeyboardState = _currentKeyboardState;
        _currentMouseState = Mouse.GetState();
        _previousMouseState = _currentMouseState;
    }

    /// <summary>
    /// Update player input and movement
    /// </summary>
    /// <param name="deltaTime">Time since last update</param>
    public void Update(float deltaTime)
    {
        var playerEntities = _world.GetArchetype(Archetypes.PLAYER).GetEntities();
        UpdateInputState();
            
        foreach (var entity in playerEntities)
        {
            // No need to check if components exist - guaranteed by archetype
            var player = _world.GetComponent<PlayerComponent>(entity);
            var transform = _world.GetComponent<TransformComponent>(entity);
            var rigidBody = _world.GetComponent<RigidBodyComponent>(entity);
                
            // Handle player input
            HandleMouseLook(ref player, ref transform, deltaTime);
            HandleMovement(ref player, ref transform, ref rigidBody, deltaTime);
            HandleInteraction(ref player, transform);
                
            // Update components
            _world.AddComponent(entity, player);
            _world.AddComponent(entity, transform);
            _world.AddComponent(entity, rigidBody);
        }
    }

    /// <summary>
    /// Updates the input state for keyboard and mouse
    /// </summary>
    private void UpdateInputState()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        // Calculate mouse delta if mouse is locked
        if (_isMouseLocked)
        {
            _mouseDelta = new Vector2(
                _currentMouseState.X - _previousMouseState.X,
                _currentMouseState.Y - _previousMouseState.Y
            );
        }
        else
        {
            _mouseDelta = Vector2.Zero;
        }
    }

    /// <summary>
    /// Handle mouse input for camera rotation
    /// </summary>
    private void HandleMouseLook(ref PlayerComponent player, ref TransformComponent transform, float deltaTime)
    {
        if (_mouseDelta == Vector2.Zero)
            return;

        // Calculate rotation based on mouse movement
        float yaw = -_mouseDelta.X * player.MouseSensitivity;
        float pitch = -_mouseDelta.Y * player.MouseSensitivity;

        // Create rotation quaternions for yaw (around Y axis)
        Quaternion yawRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);

        // Apply yaw rotation to the transform
        transform.Rotation = Quaternion.Multiply(yawRotation, transform.Rotation);
        transform.Rotation = Quaternion.Normalize(transform.Rotation);

        // Get current pitch from transform (extract rotation around X axis)
        Vector3 right = transform.Right;

        // Extract current forward direction in world space
        Vector3 forward = transform.Forward;

        // Project forward onto XZ plane and normalize to get forward direction without pitch
        Vector3 forwardNoY = new Vector3(forward.X, 0, forward.Z);
        if (forwardNoY != Vector3.Zero) // Avoid normalizing zero vector
        {
            forwardNoY = Vector3.Normalize(forwardNoY);
        }

        // Calculate current pitch angle using dot product between forward and forwardNoY
        float currentPitch = MathF.Asin(Vector3.Dot(Vector3.UnitY, forward));

        // Calculate new pitch angle, clamping to avoid gimbal lock (-89 to 89 degrees in radians)
        float newPitch = MathUtil.Clamp(currentPitch + pitch, -MathF.PI * 0.49f, MathF.PI * 0.49f);

        // Calculate pitch difference
        float pitchDiff = newPitch - currentPitch;

        // Create and apply pitch rotation around local X axis
        Quaternion pitchRotation = Quaternion.CreateFromAxisAngle(right, pitchDiff);
        transform.Rotation = Quaternion.Multiply(transform.Rotation, pitchRotation);
        transform.Rotation = Quaternion.Normalize(transform.Rotation);
    }

    /// <summary>
    /// Handle keyboard input for player movement
    /// </summary>
    private void HandleMovement(ref PlayerComponent player, ref TransformComponent transform,
        ref RigidBodyComponent rigidBody, float deltaTime)
    {
        // Get movement input
        Vector3 movementInput = Vector3.Zero;

        // Forward/backward
        if (_currentKeyboardState.IsKeyDown(Keys.W))
            movementInput.Z += 1;
        if (_currentKeyboardState.IsKeyDown(Keys.S))
            movementInput.Z -= 1;

        // Left/right
        if (_currentKeyboardState.IsKeyDown(Keys.A))
            movementInput.X -= 1;
        if (_currentKeyboardState.IsKeyDown(Keys.D))
            movementInput.X += 1;

        // Normalize input if not zero
        if (movementInput != Vector3.Zero)
            movementInput = Vector3.Normalize(movementInput);

        // Handle crouch toggle
        if (IsKeyPressed(Keys.LeftControl) || IsKeyPressed(Keys.C))
        {
            player.IsCrouching = !player.IsCrouching;

            // In a real implementation, we would adjust the player's height and collision shape
        }

        // Handle leaning (a staple for immersive sims!)
        if (_currentKeyboardState.IsKeyDown(Keys.Q))
            player.LeanAmount = MathUtil.Clamp(player.LeanAmount - deltaTime * 3.0f, -1.0f, 0.0f);
        else if (_currentKeyboardState.IsKeyDown(Keys.E))
            player.LeanAmount = MathUtil.Clamp(player.LeanAmount + deltaTime * 3.0f, 0.0f, 1.0f);
        else
            player.LeanAmount = MathUtil.Lerp(player.LeanAmount, 0.0f, deltaTime * 10.0f);

        // Calculate move speed based on stance
        float moveSpeed = player.MovementSpeed;
        if (player.IsCrouching)
            moveSpeed *= 0.5f;

        // Transform movement from local to world space
        Vector3 forward = new Vector3(transform.Forward.X, 0, transform.Forward.Z);
        if (forward != Vector3.Zero)
            forward = Vector3.Normalize(forward);

        Vector3 right = new Vector3(transform.Right.X, 0, transform.Right.Z);
        if (right != Vector3.Zero)
            right = Vector3.Normalize(right);

        // Calculate movement vector in world space
        Vector3 movement = (forward * movementInput.Z) + (right * movementInput.X);
        if (movement != Vector3.Zero)
            movement = Vector3.Normalize(movement) * moveSpeed;

        // Set velocity directly for direct control
        // In a real implementation, we might apply forces instead for more physical behavior
        rigidBody.Velocity = new Vector3(movement.X, rigidBody.Velocity.Y, movement.Z);

        // Apply lean offset to transform
        // In a real implementation, we would use a camera offset component
        // This is a simplified approach for demonstration
        Vector3 leanOffset = right * player.LeanAmount * 0.5f;
        transform.Position += leanOffset;
    }

    /// <summary>
    /// Handle interaction input
    /// </summary>
    private void HandleInteraction(ref PlayerComponent player, TransformComponent transform)
    {
        // Check for interaction input
        if (IsKeyPressed(Keys.F) || IsMouseButtonPressed(ButtonState.Pressed))
        {
            // Perform a raycast from player position in forward direction
            Entity interactedEntity = Raycast(transform.Position, transform.Forward, player.InteractionRange);

            // If we hit an interactable entity
            if (interactedEntity.Id != -1 && _world.HasComponent<InteractableComponent>(interactedEntity))
            {
                var interactable = _world.GetComponent<InteractableComponent>(interactedEntity);

                // Only interact if enabled
                if (interactable.IsEnabled)
                {
                    // Process the interaction based on type
                    ProcessInteraction(interactedEntity, interactable);
                }
            }
        }
    }

    /// <summary>
    /// Process an interaction with an object
    /// </summary>
    private void ProcessInteraction(Entity entity, InteractableComponent interactable)
    {
        // In a real implementation, this would be handled by an event system
        // or a dedicated interaction system

        switch (interactable.Type)
        {
            case InteractionType.Pickup:
                // Add to inventory
                // For now, just print debug info
                Console.WriteLine($"Picked up {interactable.DisplayName}");
                break;

            case InteractionType.Use:
                // Use the object
                Console.WriteLine($"Used {interactable.DisplayName}");
                break;

            case InteractionType.Toggle:
                // Toggle object state
                Console.WriteLine($"Toggled {interactable.DisplayName}");
                break;

            case InteractionType.Open:
                // Open container, door, etc.
                Console.WriteLine($"Opened {interactable.DisplayName}");
                break;

            // Handle other interaction types

            default:
                Console.WriteLine($"Interacted with {interactable.DisplayName}");
                break;
        }
    }

    /// <summary>
    /// Perform a raycast to find an entity
    /// </summary>
    /// <param name="origin">Ray origin</param>
    /// <param name="direction">Ray direction</param>
    /// <param name="maxDistance">Maximum ray distance</param>
    /// <returns>Hit entity or invalid entity if no hit</returns>
    private Entity Raycast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        // In a real implementation, this would use the physics system
        // to perform a proper raycast against colliders

        // For now, return an invalid entity as a placeholder
        return new Entity(-1);
    }

    /// <summary>
    /// Check if a key was just pressed this frame
    /// </summary>
    private bool IsKeyPressed(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }

    /// <summary>
    /// Check if a mouse button was just pressed this frame
    /// </summary>
    private bool IsMouseButtonPressed(ButtonState state)
    {
        return _currentMouseState.LeftButton == state && _previousMouseState.LeftButton != state;
    }
}