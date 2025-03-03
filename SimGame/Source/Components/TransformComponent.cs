using System;
using System.Numerics;
using MPirical.Core.ECS;

namespace MPirical.Components
{
    /// <summary>
    /// Component that stores position, rotation, and scale data for an entity
    /// </summary>
    public struct TransformComponent : IComponent
    {
        /// <summary>
        /// Position in 3D space
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// Rotation as a quaternion
        /// </summary>
        public Quaternion Rotation;
        
        /// <summary>
        /// Scale in 3D space
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// Get the forward direction vector based on current rotation
        /// </summary>
        public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Rotation);
        
        /// <summary>
        /// Get the right direction vector based on current rotation
        /// </summary>
        public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);
        
        /// <summary>
        /// Get the up direction vector based on current rotation
        /// </summary>
        public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);
    }

    /// <summary>
    /// Configuration for the transform component
    /// </summary>
    public class TransformComponentConfig : IComponentConfig<TransformComponent>
    {
        /// <summary>
        /// Creates a transform component with default values
        /// </summary>
        /// <returns>A new transform component</returns>
        public TransformComponent CreateDefault()
        {
            return new TransformComponent
            {
                Position = Vector3.Zero,
                Rotation = Quaternion.Identity,
                Scale = Vector3.One
            };
        }

        /// <summary>
        /// Creates a transform component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured transform component</returns>
        public TransformComponent Create(Action<TransformComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }
    
    /// <summary>
    /// Component that establishes parent-child relationships between transforms
    /// </summary>
    public struct HierarchyComponent : IComponent
    {
        /// <summary>
        /// Parent entity ID, or -1 if this entity has no parent
        /// </summary>
        public int ParentId;
        
        /// <summary>
        /// Child entity IDs
        /// </summary>
        public int[] ChildIds;
        
        /// <summary>
        /// Local position relative to parent
        /// </summary>
        public Vector3 LocalPosition;
        
        /// <summary>
        /// Local rotation relative to parent
        /// </summary>
        public Quaternion LocalRotation;
        
        /// <summary>
        /// Local scale relative to parent
        /// </summary>
        public Vector3 LocalScale;
    }

    /// <summary>
    /// Configuration for the hierarchy component
    /// </summary>
    public class HierarchyComponentConfig : IComponentConfig<HierarchyComponent>
    {
        /// <summary>
        /// Creates a hierarchy component with default values
        /// </summary>
        /// <returns>A new hierarchy component</returns>
        public HierarchyComponent CreateDefault()
        {
            return new HierarchyComponent
            {
                ParentId = -1,
                ChildIds = Array.Empty<int>(),
                LocalPosition = Vector3.Zero,
                LocalRotation = Quaternion.Identity,
                LocalScale = Vector3.One
            };
        }

        /// <summary>
        /// Creates a hierarchy component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured hierarchy component</returns>
        public HierarchyComponent Create(Action<HierarchyComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }
}