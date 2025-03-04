using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MPirical.Core.ECS;
using MPirical.Components;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

namespace MPirical.Rendering
{
    /// <summary>
    /// System that handles rendering entities using MonoGame
    /// </summary>
    public class RenderingSystem : ISystem
    {
        private World _world;
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;

        private BasicEffect _basicEffect;
        private List<Entity> _renderableEntities = new List<Entity>();

        // Camera parameters
        private Entity _cameraEntity;
        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;

        // Default primitive shapes for testing
        private VertexPositionColor[] _cubeVertices;
        private int[] _cubeIndices;

        /// <summary>
        /// Name of this system
        /// </summary>
        public string Name => "RenderingSystem";

        /// <summary>
        /// Priority of this system (runs after all updates but before rendering)
        /// </summary>
        public int Priority => 9000;

        /// <summary>
        /// Create a new rendering system
        /// </summary>
        /// <param name="graphicsDevice">MonoGame graphics device</param>
        /// <param name="contentManager">MonoGame content manager</param>
        public RenderingSystem(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;

            // Initialize rendering resources
            InitializeRenderingResources();
        }

        /// <summary>
        /// Initialize the system with the world
        /// </summary>
        /// <param name="world">Reference to the world</param>
        public void Initialize(World world)
        {
            _world = world;

            // Setup perspective projection
            float aspectRatio = _graphicsDevice.Viewport.AspectRatio;
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4 * 2, // 45 degrees field of view
                aspectRatio,
                0.1f, // Near clip plane
                100.0f // Far clip plane
            );

            // Initialize basic shader
            _basicEffect = new BasicEffect(_graphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.Identity,
                World = Matrix.Identity,
                Projection = _projectionMatrix
            };

            // Initialize test primitives
            InitializePrimitives();
        }

        /// <summary>
        /// Update rendering
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Update(float deltaTime)
        {
            UpdateCamera(deltaTime);

            RenderScene();
        }

        /// <summary>
        /// Initialize rendering resources
        /// </summary>
        private void InitializeRenderingResources()
        {
            //TODO: LOAD SHADERS, TEXTURES, ETC
        }

        /// <summary>
        /// Initialize primitive shapes for testing
        /// </summary>
        private void InitializePrimitives()
        {
            CreateCubePrimitive();
        }

        /// <summary>
        /// Create a colored cube primitive
        /// </summary>
        private void CreateCubePrimitive()
        {
            // Define the 8 corners of a cube
            Vector3[] cubeCorners = new Vector3[8]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), // Bottom-back-left
                new Vector3(0.5f, -0.5f, -0.5f), // Bottom-back-right
                new Vector3(0.5f, 0.5f, -0.5f), // Top-back-right
                new Vector3(-0.5f, 0.5f, -0.5f), // Top-back-left
                new Vector3(-0.5f, -0.5f, 0.5f), // Bottom-front-left
                new Vector3(0.5f, -0.5f, 0.5f), // Bottom-front-right
                new Vector3(0.5f, 0.5f, 0.5f), // Top-front-right
                new Vector3(-0.5f, 0.5f, 0.5f) // Top-front-left
            };

            _cubeVertices = new VertexPositionColor[8];

            _cubeVertices[0] = new VertexPositionColor(cubeCorners[0], Microsoft.Xna.Framework.Color.Red);
            _cubeVertices[1] = new VertexPositionColor(cubeCorners[1], Microsoft.Xna.Framework.Color.Green);
            _cubeVertices[2] = new VertexPositionColor(cubeCorners[2], Microsoft.Xna.Framework.Color.Blue);
            _cubeVertices[3] = new VertexPositionColor(cubeCorners[3], Microsoft.Xna.Framework.Color.Yellow);
            _cubeVertices[4] = new VertexPositionColor(cubeCorners[4], Microsoft.Xna.Framework.Color.Purple);
            _cubeVertices[5] = new VertexPositionColor(cubeCorners[5], Microsoft.Xna.Framework.Color.Cyan);
            _cubeVertices[6] = new VertexPositionColor(cubeCorners[6], Microsoft.Xna.Framework.Color.White);
            _cubeVertices[7] = new VertexPositionColor(cubeCorners[7], Microsoft.Xna.Framework.Color.Orange);

            _cubeIndices = new int[]
            {
                // Front face
                4, 5, 6, 4, 6, 7,
                // Back face
                1, 0, 3, 1, 3, 2,
                // Left face
                0, 4, 7, 0, 7, 3,
                // Right face
                5, 1, 2, 5, 2, 6,
                // Top face
                7, 6, 2, 7, 2, 3,
                // Bottom face
                0, 1, 5, 0, 5, 4
            };
        }

        /// <summary>
        /// Update the camera view matrix
        /// </summary>
        private void UpdateCamera(float _deltaTime)
        {
            if (_cameraEntity.Id == -1 || !_world.HasComponent<TransformComponent>(_cameraEntity))
            {
                _viewMatrix = Matrix.CreateLookAt(
                    new Vector3(0, 2, -5),
                    new Vector3(0, 0, 0),
                    Vector3.Up
                );

                _basicEffect.View = _viewMatrix;
                return;
            }

            var transform = _world.GetComponent<TransformComponent>(_cameraEntity);

            bool hasCameraComponent = _world.HasComponent<CameraComponent>(_cameraEntity);
            CameraComponent camera = default;

            if (hasCameraComponent)
            {
                camera = _world.GetComponent<CameraComponent>(_cameraEntity);
            }

            Vector3 position = new Vector3(
                transform.Position.X,
                transform.Position.Y,
                transform.Position.Z
            );

            if (hasCameraComponent)
            {
                if (camera.PositionOffset != System.Numerics.Vector3.Zero)
                {
                    var worldOffset = Vector3.Transform(
                        new Vector3(camera.PositionOffset.X, camera.PositionOffset.Y, camera.PositionOffset.Z),
                        Quaternion.CreateFromRotationMatrix(_viewMatrix)
                    );

                    position += worldOffset;
                }

                if (camera.HeadBobAmount > 0)
                {
                    if (_world.HasComponent<RigidBodyComponent>(_cameraEntity))
                    {
                        var rigidBody = _world.GetComponent<RigidBodyComponent>(_cameraEntity);

                        float speed = rigidBody.Velocity.Length();
                        if (speed > 0.1f)
                        {
                            camera.HeadBobTime += _deltaTime * camera.HeadBobFrequency * speed;

                            float bobOffset = (float)Math.Sin(camera.HeadBobTime) * camera.HeadBobAmount;
                            position.Y += bobOffset;

                            _world.AddComponent(_cameraEntity, camera);
                        }
                    }
                }

                if (camera.LeanOffset != 0)
                {
                    Vector3 _right = Vector3.Transform(Vector3.Right,
                        new Quaternion(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z,
                            transform.Rotation.W));
                    position += _right * camera.LeanOffset;
                }

                float aspectRatio = _graphicsDevice.Viewport.AspectRatio;
                camera.AspectRatio = aspectRatio; 

                if (camera.IsOrthographic)
                {
                    _projectionMatrix = Matrix.CreateOrthographic(
                        camera.OrthographicSize * aspectRatio * 2 * camera.Zoom,
                        camera.OrthographicSize * 2 * camera.Zoom,
                        camera.NearClipPlane,
                        camera.FarClipPlane
                    );
                }
                else
                {
                    _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(camera.FieldOfView) / camera.Zoom,
                        aspectRatio,
                        camera.NearClipPlane,
                        camera.FarClipPlane
                    );
                }

                _basicEffect.Projection = _projectionMatrix;
            }

            Quaternion yawRotation = new Quaternion(
                transform.Rotation.X,
                transform.Rotation.Y,
                transform.Rotation.Z,
                transform.Rotation.W
            );
            
            Vector3 right = Vector3.Transform(Vector3.Right, yawRotation);
            Quaternion pitchRotation = Quaternion.CreateFromAxisAngle(right, camera.PitchAngle);
            Quaternion finalRotation = Quaternion.Multiply(pitchRotation, yawRotation);

            Vector3 forward = Vector3.Transform(Vector3.Forward, finalRotation);
            Vector3 up = Vector3.Transform(Vector3.Up, finalRotation);

            _viewMatrix = Matrix.CreateLookAt(
                position,
                position + forward,
                up
            );

            _basicEffect.View = _viewMatrix;
        }

        /// <summary>
        /// Render the scene
        /// </summary>
        private void RenderScene()
        {
            _graphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var entity in _renderableEntities)
            {
                RenderEntity(entity);
            }

            RenderTestGeometry();
        }

        /// <summary>
        /// Render a single entity
        /// </summary>
        /// <param name="entity">Entity to render</param>
        private void RenderEntity(Entity entity)
        {
            if (!_world.HasComponent<TransformComponent>(entity))
                return;

            var transform = _world.GetComponent<TransformComponent>(entity);

            Vector3 position = new Vector3(
                transform.Position.X,
                transform.Position.Y,
                transform.Position.Z
            );

            Quaternion rotation = new Quaternion(
                transform.Rotation.X,
                transform.Rotation.Y,
                transform.Rotation.Z,
                transform.Rotation.W
            );

            Vector3 scale = new Vector3(
                transform.Scale.X,
                transform.Scale.Y,
                transform.Scale.Z
            );

            Matrix worldMatrix = Matrix.CreateScale(scale) *
                                 Matrix.CreateFromQuaternion(rotation) *
                                 Matrix.CreateTranslation(position);

            _basicEffect.World = worldMatrix;
            
            //TODO: RENDER MESH 

            RenderCube();
        }

        /// <summary>
        /// Render test geometry for placeholder visualization
        /// </summary>
        private void RenderTestGeometry()
        {
            Matrix groundMatrix = Matrix.CreateScale(50.0f, 0.1f, 50.0f) *
                                  Matrix.CreateTranslation(0, 0, 0);
            _basicEffect.World = groundMatrix;
            RenderCube();

            for (int x = -5; x <= 5; x += 2)
            {
                for (int z = -5; z <= 5; z += 2)
                {
                    if (Math.Abs(x) < 3 && Math.Abs(z) < 3)
                        continue;

                    Matrix cubeMatrix = Matrix.CreateScale(1, 1, 1) *
                                        Matrix.CreateTranslation(x, 1, z);
                    _basicEffect.World = cubeMatrix;
                    RenderCube();
                }
            }
        }

        /// <summary>
        /// Render a cube using the configured effect
        /// </summary>
        private void RenderCube()
        {
            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _cubeVertices,
                    0, // vertex buffer offset
                    _cubeVertices.Length, // number of vertices
                    _cubeIndices,
                    0, // index buffer offset
                    _cubeIndices.Length / 3 // number of primitives
                );
            }
        }

        /// <summary>
        /// Set the camera entity to use for rendering
        /// </summary>
        /// <param name="cameraEntity">Entity to use as camera</param>
        public void SetCameraEntity(Entity cameraEntity)
        {
            _cameraEntity = cameraEntity;
        }

        /// <summary>
        /// Convert from System.Numerics.Vector3 to Microsoft.Xna.Framework.Vector3
        /// </summary>
        /// <param name="vector">Input vector</param>
        /// <returns>Converted vector</returns>
        private Vector3 ConvertVector(System.Numerics.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Convert from System.Numerics.Quaternion to Microsoft.Xna.Framework.Quaternion
        /// </summary>
        /// <param name="quaternion">Input quaternion</param>
        /// <returns>Converted quaternion</returns>
        private Quaternion ConvertQuaternion(System.Numerics.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
    }
}