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
            var player = _world.GetComponent<PlayerComponent>(entity);
            var transform = _world.GetComponent<TransformComponent>(entity);
            var rigidBody = _world.GetComponent<RigidBodyComponent>(entity);
            var cameraComponent = _world.GetComponent<CameraComponent>(entity);

            HandleMouseLook(ref player, ref transform, ref cameraComponent, deltaTime);
            HandleMovement(ref player, ref transform, ref rigidBody, ref cameraComponent, deltaTime);
            HandleInteraction(ref player, transform);

            _world.AddComponent(entity, player);
            _world.AddComponent(entity, transform);
            _world.AddComponent(entity, rigidBody);
            _world.AddComponent(entity, cameraComponent);
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

        if (!_isMouseLocked)
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
    private void HandleMouseLook(ref PlayerComponent player, ref TransformComponent transform, ref CameraComponent camera, float deltaTime)
    {
        if (_mouseDelta == Vector2.Zero)
            return;

        float yawDelta = -_mouseDelta.X * player.MouseSensitivity;
        float pitchDelta = -_mouseDelta.Y * player.MouseSensitivity;
    
        player.YawAngle += yawDelta;
        camera.PitchAngle += pitchDelta;
    
        camera.PitchAngle = MathUtil.Clamp(camera.PitchAngle, -90, 90);
    
        Quaternion bodyRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, player.YawAngle);
        transform.Rotation = bodyRotation;
    }

    private void HandleMovement(ref PlayerComponent player, ref TransformComponent transform,
        ref RigidBodyComponent rigidBody, ref CameraComponent cameraComponent, float deltaTime)
    {
        Vector3 movementInput = Vector3.Zero;

        if (_currentKeyboardState.IsKeyDown(Keys.W))
            movementInput.Z -= 1;
        if (_currentKeyboardState.IsKeyDown(Keys.S))
            movementInput.Z += 1;

        if (_currentKeyboardState.IsKeyDown(Keys.A))
            movementInput.X -= 1;
        if (_currentKeyboardState.IsKeyDown(Keys.D))
            movementInput.X += 1;

        if (movementInput != Vector3.Zero)
            movementInput = Vector3.Normalize(movementInput);

        if (IsKeyPressed(Keys.LeftControl) || IsKeyPressed(Keys.C))
        {
            player.IsCrouching = !player.IsCrouching;
            //TODO: ACTUALLY CROUCH
        }

        if (_currentKeyboardState.IsKeyDown(Keys.Q))
            player.LeanAmount = MathUtil.Clamp(player.LeanAmount - deltaTime * 3.0f, -1.0f, 0.0f);
        else if (_currentKeyboardState.IsKeyDown(Keys.E))
            player.LeanAmount = MathUtil.Clamp(player.LeanAmount + deltaTime * 3.0f, 0.0f, 1.0f);
        else
            player.LeanAmount = MathUtil.Lerp(player.LeanAmount, 0.0f, deltaTime * 10.0f);

        float moveSpeed = player.MovementSpeed;
        if (player.IsCrouching)
            moveSpeed *= 0.5f;


        Vector3 movement = (transform.Forward * movementInput.Z) + (transform.Right * movementInput.X);

        if (movement != Vector3.Zero)
            movement = Vector3.Normalize(movement) * moveSpeed;

        rigidBody.Velocity = new Vector3(movement.X, rigidBody.Velocity.Y, movement.Z);

        cameraComponent.LeanOffset = player.LeanAmount * 0.5f;
    }

    /// <summary>
    /// Handle interaction input
    /// </summary>
    private void HandleInteraction(ref PlayerComponent player, TransformComponent transform)
    {
        if (IsKeyPressed(Keys.F) || IsMouseButtonPressed(ButtonState.Pressed))
        {
            Entity interactedEntity = Raycast(transform.Position, transform.Forward, player.InteractionRange);

            if (interactedEntity.Id != -1 && _world.HasComponent<InteractableComponent>(interactedEntity))
            {
                var interactable = _world.GetComponent<InteractableComponent>(interactedEntity);

                if (interactable.IsEnabled)
                {
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
        switch (interactable.Type)
        {
            case InteractionType.Pickup:
                Console.WriteLine($"Picked up {interactable.DisplayName}");
                break;

            case InteractionType.Use:
                Console.WriteLine($"Used {interactable.DisplayName}");
                break;

            case InteractionType.Toggle:
                Console.WriteLine($"Toggled {interactable.DisplayName}");
                break;

            case InteractionType.Open:
                Console.WriteLine($"Opened {interactable.DisplayName}");
                break;


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
        //TODO: IMPLEMENT THIS 
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