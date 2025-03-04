using System;
using System.Numerics;
using MPirical.Core.ECS;

namespace MPirical.Components
{
    /// <summary>
    /// Component that represents a camera for rendering
    /// </summary>
    public struct CameraComponent : IComponent
    {
        /// <summary>
        /// Field of view in degrees
        /// </summary>
        public float FieldOfView;
        
        /// <summary>
        /// Near clip plane distance
        /// </summary>
        public float NearClipPlane;
        
        /// <summary>
        /// Far clip plane distance
        /// </summary>
        public float FarClipPlane;
        
        /// <summary>
        /// Aspect ratio (width/height)
        /// </summary>
        public float AspectRatio;
        
        /// <summary>
        /// Camera offset from entity position
        /// </summary>
        public Vector3 PositionOffset;
        
        /// <summary>
        /// Whether this is an orthographic camera
        /// </summary>
        public bool IsOrthographic;
        
        /// <summary>
        /// Orthographic size (half height)
        /// </summary>
        public float OrthographicSize;
        
        /// <summary>
        /// Current zoom level
        /// </summary>
        public float Zoom;
        
        /// <summary>
        /// Head bob amount for first-person cameras
        /// </summary>
        public float HeadBobAmount;
        
        /// <summary>
        /// Head bob frequency for first-person cameras
        /// </summary>
        public float HeadBobFrequency;
        
        /// <summary>
        /// Current head bob time (internal)
        /// </summary>
        public float HeadBobTime;
        
        /// <summary>
        /// Lean offset for leaning around corners
        /// </summary>
        public float LeanOffset;
        
        /// <summary>
        /// Pitch from input
        /// </summary>
        public float PitchAngle; 
        
        /// <summary>
        /// Whether to apply post-processing effects
        /// </summary>
        public bool UsePostProcessing;
        
        /// <summary>
        /// Whether this camera renders UI elements
        /// </summary>
        public bool RenderUI;
    }

    /// <summary>
    /// Configuration for camera component
    /// </summary>
    public class CameraComponentConfig : IComponentConfig<CameraComponent>
    {
        /// <summary>
        /// Creates a camera component with default values
        /// </summary>
        /// <returns>A new camera component</returns>
        public CameraComponent CreateDefault()
        {
            return new CameraComponent
            {
                FieldOfView = 60.0f,
                NearClipPlane = 0.1f,
                FarClipPlane = 1000.0f,
                AspectRatio = 16.0f / 9.0f,
                PositionOffset = Vector3.Zero,
                IsOrthographic = false,
                OrthographicSize = 5.0f,
                Zoom = 1.0f,
                HeadBobAmount = 0.05f,
                HeadBobFrequency = 5.0f,
                HeadBobTime = 0.0f,
                LeanOffset = 0.0f,
                UsePostProcessing = true,
                RenderUI = true
            };
        }

        /// <summary>
        /// Creates a first-person camera component
        /// </summary>
        /// <returns>A new first-person camera component</returns>
        public CameraComponent CreateFirstPerson()
        {
            var camera = CreateDefault();
            camera.FieldOfView = 120.0f;
            camera.HeadBobAmount = 0.05f;
            camera.HeadBobFrequency = 5.0f;
            return camera;
        }

        /// <summary>
        /// Creates a third-person camera component
        /// </summary>
        /// <returns>A new third-person camera component</returns>
        public CameraComponent CreateThirdPerson()
        {
            var camera = CreateDefault();
            camera.PositionOffset = new Vector3(0, 1.0f, -3.0f);
            camera.FieldOfView = 60.0f;
            return camera;
        }

        /// <summary>
        /// Creates a camera component with custom configuration
        /// </summary>
        /// <param name="configureAction">Action to configure the component</param>
        /// <returns>A configured camera component</returns>
        public CameraComponent Create(Action<CameraComponent> configureAction)
        {
            var component = CreateDefault();
            configureAction(component);
            return component;
        }
    }
}