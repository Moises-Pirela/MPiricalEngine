using System;
using System.Numerics;
using MPirical.Core.ECS;

namespace MPirical.Components
{
    /// <summary>
    /// Component that adds physical properties to an entity
    /// </summary>
    public struct RigidBodyComponent : IComponent
    {
        /// <summary>
        /// Mass of the rigid body in kilograms
        /// </summary>
        public float Mass;
        
        /// <summary>
        /// Linear velocity in meters per second
        /// </summary>
        public Vector3 Velocity;
        
        /// <summary>
        /// Angular velocity in radians per second
        /// </summary>
        public Vector3 AngularVelocity;
        
        /// <summary>
        /// Linear drag coefficient
        /// </summary>
        public float Drag;
        
        /// <summary>
        /// Angular drag coefficient
        /// </summary>
        public float AngularDrag;
        
        /// <summary>
        /// Whether this rigid body is affected by gravity
        /// </summary>
        public bool UseGravity;
        
        /// <summary>
        /// Whether this rigid body is kinematic (moved by code, not physics)
        /// </summary>
        public bool IsKinematic;
        
        /// <summary>
        /// Whether this rigid body is currently at rest
        /// </summary>
        public bool IsSleeping;
        
        /// <summary>
        /// Accumulated force to be applied on next physics update
        /// </summary>
        public Vector3 AccumulatedForce;
        
        /// <summary>
        /// Accumulated torque to be applied on next physics update
        /// </summary>
        public Vector3 AccumulatedTorque;
    }

    /// <summary>
    /// Configuration for the rigid body component
    /// </summary>
    public class RigidBodyComponentConfig : IComponentConfig<RigidBodyComponent>
    {
        /// <summary>
        /// Creates a rigid body component with default values
        /// </summary>
        /// <returns>A new rigid body component</returns>
        public RigidBodyComponent CreateDefault()
        {
            return new RigidBodyComponent
            {
                Mass = 1.0f,
                Velocity = Vector3.Zero,
                AngularVelocity = Vector3.Zero,
                Drag = 0.05f,
                AngularDrag = 0.05f,
                UseGravity = true,
                IsKinematic = false,
                IsSleeping = false,
                AccumulatedForce = Vector3.Zero,
                AccumulatedTorque = Vector3.Zero
            };
        }

        /// <summary>
        /// Creates a rigid body component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured rigid body component</returns>
        public RigidBodyComponent Create(Action<RigidBodyComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }

    /// <summary>
    /// Available collision shapes for physics
    /// </summary>
    public enum CollisionShapeType
    {
        Box,
        Sphere,
        Capsule,
        Cylinder,
        Mesh,
        Compound
    }

    /// <summary>
    /// Component that defines collision detection for an entity
    /// </summary>
    public struct ColliderComponent : IComponent
    {
        /// <summary>
        /// Type of collision shape
        /// </summary>
        public CollisionShapeType ShapeType;
        
        /// <summary>
        /// Size/dimensions of the collider based on shape type
        /// For Box: x, y, z are half-extents
        /// For Sphere: x is radius
        /// For Capsule: x is radius, y is height
        /// For Cylinder: x is radius, y is height
        /// </summary>
        public Vector3 Size;
        
        /// <summary>
        /// Offset from the transform position
        /// </summary>
        public Vector3 Offset;
        
        /// <summary>
        /// Material properties for physics interactions
        /// </summary>
        public PhysicsMaterial Material;
        
        /// <summary>
        /// Whether this collider is a trigger (no physical reaction)
        /// </summary>
        public bool IsTrigger;
        
        /// <summary>
        /// Layer mask for collision filtering
        /// </summary>
        public int LayerMask;
    }

    /// <summary>
    /// Material properties for physics interactions
    /// </summary>
    public struct PhysicsMaterial
    {
        /// <summary>
        /// Static friction coefficient
        /// </summary>
        public float StaticFriction;
        
        /// <summary>
        /// Dynamic friction coefficient
        /// </summary>
        public float DynamicFriction;
        
        /// <summary>
        /// Bounciness (restitution) coefficient
        /// </summary>
        public float Bounciness;
    }

    /// <summary>
    /// Configuration for the collider component
    /// </summary>
    public class ColliderComponentConfig : IComponentConfig<ColliderComponent>
    {
        /// <summary>
        /// Creates a collider component with default values
        /// </summary>
        /// <returns>A new collider component</returns>
        public ColliderComponent CreateDefault()
        {
            return new ColliderComponent
            {
                ShapeType = CollisionShapeType.Box,
                Size = Vector3.One,
                Offset = Vector3.Zero,
                Material = new PhysicsMaterial
                {
                    StaticFriction = 0.5f,
                    DynamicFriction = 0.5f,
                    Bounciness = 0.0f
                },
                IsTrigger = false,
                LayerMask = 1
            };
        }

        /// <summary>
        /// Creates a collider component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured collider component</returns>
        public ColliderComponent Create(Action<ColliderComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }
}