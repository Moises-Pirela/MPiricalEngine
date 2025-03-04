using System;
using System.Collections.Generic;
using System.Numerics;
using MPirical.Components;
using MPirical.Core;
using MPirical.Core.ECS;

namespace MPirical.Systems;

/// <summary>
/// System that simulates basic physics for rigid bodies
/// </summary>
public class PhysicsSystem : ISystem
{
    private World _world;
    private readonly Vector3 _gravity = new Vector3(0, -19.62f, 0);
        
    /// <summary>
    /// Name of this system
    /// </summary>
    public string Name => "PhysicsSystem";
        
    /// <summary>
    /// Priority of this system
    /// </summary>
    public int Priority => 200;

    /// <summary>
    /// Initialize the system with the world
    /// </summary>
    /// <param name="world">Reference to the world</param>
    public void Initialize(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Update physics simulation
    /// </summary>
    /// <param name="deltaTime">Time since last update</param>
    public void Update(float deltaTime)
    {
        var rigidbodyEntities = _world.GetArchetype(Archetypes.PHYSICS).GetEntities();
        foreach (var entity in rigidbodyEntities)
        {
            if (!_world.HasComponent<TransformComponent>(entity))
                continue;
                
            var rigidBody = _world.GetComponent<RigidBodyComponent>(entity);
            var transform = _world.GetComponent<TransformComponent>(entity);
                
            if (rigidBody.IsKinematic)
                continue;
                
            if (rigidBody.IsSleeping)
                continue;
                
            if (rigidBody.UseGravity)
            {
                //rigidBody.Velocity += _gravity * deltaTime;
            }
                
            // if (rigidBody.Mass > 0)
            // {
            //     Vector3 acceleration = rigidBody.AccumulatedForce / rigidBody.Mass;
            //     rigidBody.Velocity += acceleration * deltaTime;
            //     rigidBody.AccumulatedForce = Vector3.Zero;
            // }
                
            // rigidBody.Velocity *= (1.0f - rigidBody.Drag * deltaTime);
            // rigidBody.AngularVelocity *= (1.0f - rigidBody.AngularDrag * deltaTime);
                
            transform.Position += rigidBody.Velocity * deltaTime;
                
            if (rigidBody.AngularVelocity != Vector3.Zero)
            {
                float angle = rigidBody.AngularVelocity.Length() * deltaTime;
                Vector3 axis = Vector3.Normalize(rigidBody.AngularVelocity);
                Quaternion deltaRotation = Quaternion.CreateFromAxisAngle(axis, angle);
                transform.Rotation = Quaternion.Multiply(deltaRotation, transform.Rotation);
                transform.Rotation = Quaternion.Normalize(transform.Rotation);
            }
                
            // TODO: Collision detection and response would be here
            // For an immersive sim, this would be quite complex
                
            _world.AddComponent(entity, rigidBody);
            _world.AddComponent(entity, transform);
        }
    }
}