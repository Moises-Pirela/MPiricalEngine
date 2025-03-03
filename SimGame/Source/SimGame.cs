using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MPirical.Components;
using MPirical.Components.AI;
using MPirical.Core.ECS;
using MPirical.Systems;

namespace MPirical;


/// <summary>
/// Main game class that initializes MonoGame and the ECS architecture
/// </summary>
public class SimGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // ECS world
    private World _world;
        
    // Time tracking
    private float _deltaTime;
        
    // Game state
    private GameState _currentState;
        
    // Input handling
    private KeyboardState _currentKeyboardState;
    private KeyboardState _previousKeyboardState;
    private MouseState _currentMouseState;
    private MouseState _previousMouseState;
        
    // Player entity
    private Entity _playerEntity;
        
    /// <summary>
    /// Constructor sets up the MonoGame environment
    /// </summary>
    public SimGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false; // Hide cursor for first-person view
            
        // Set window size
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
            
        // Initialize game state
        _currentState = GameState.Loading;
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        // Initialize the ECS world
        InitializeECS();
            
        // Set initial game state
        _currentState = GameState.MainMenu;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        LoadGameAssets();
    }

    /// <summary>
    /// Update the game logic
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    protected override void Update(GameTime gameTime)
    {
        // Update input states
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
            
        // Calculate delta time in seconds
        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
        // Exit with Escape key
        if (_currentKeyboardState.IsKeyDown(Keys.Escape))
            Exit();
            
        // Update based on current game state
        switch (_currentState)
        {
            case GameState.Loading:
                UpdateLoading(gameTime);
                break;
                    
            case GameState.MainMenu:
                UpdateMainMenu(gameTime);
                break;
                    
            case GameState.Gameplay:
                UpdateGameplay(gameTime);
                break;
                    
            case GameState.Paused:
                UpdatePaused(gameTime);
                break;
                    
            case GameState.GameOver:
                UpdateGameOver(gameTime);
                break;
        }
            
        base.Update(gameTime);
    }

    /// <summary>
        /// Render the game
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            // Draw based on current game state
            switch (_currentState)
            {
                case GameState.Loading:
                    DrawLoading(gameTime);
                    break;
                    
                case GameState.MainMenu:
                    DrawMainMenu(gameTime);
                    break;
                    
                case GameState.Gameplay:
                    DrawGameplay(gameTime);
                    break;
                    
                case GameState.Paused:
                    DrawPaused(gameTime);
                    break;
                    
                case GameState.GameOver:
                    DrawGameOver(gameTime);
                    break;
            }
            
            base.Draw(gameTime);
        }
        
        #region ECS Initialization
        
        /// <summary>
        /// Initialize the Entity Component System architecture
        /// </summary>
        private void InitializeECS()
        {
            // Create ECS world
            _world = new World();
            
            // Register core systems
            RegisterSystems();
            
            // Create player entity
            CreatePlayer();
            
            // Setup test level
            CreateTestLevel();
        }
        
        /// <summary>
        /// Register all systems with the ECS world
        /// </summary>
        private void RegisterSystems()
        {
            // Core systems
            _world.RegisterSystem(new TransformHierarchySystem());
            _world.RegisterSystem(new PhysicsSystem());
            
            // Player systems
            _world.RegisterSystem(new PlayerInputSystem());
            _world.RegisterSystem(new InteractionUISystem());
            
            // AI systems
            _world.RegisterSystem(new VisionPerceptionSystem());
            _world.RegisterSystem(new HearingPerceptionSystem());
            
            // Stealth system
            _world.RegisterSystem(new StealthSystem());
            
            // Environment systems
            _world.RegisterSystem(new DoorSystem());
            _world.RegisterSystem(new SwitchSystem());
            _world.RegisterSystem(new ComputerTerminalSystem());
            _world.RegisterSystem(new SecuritySystem());
            
            // Inventory system
            _world.RegisterSystem(new InventorySystem());
            
            // Rendering system (custom for MonoGame)
            _world.RegisterSystem(new RenderingSystem(GraphicsDevice, Content));
        }
        
        /// <summary>
        /// Create the player entity
        /// </summary>
        private void CreatePlayer()
        {
            // Create player entity
            _playerEntity = _world.CreateEntity();
            
            // Add transform component
            var transformConfig = new TransformComponentConfig();
            var transform = transformConfig.Create(t => {
                t.Position = new System.Numerics.Vector3(0, 1.8f, 0); // Eye height
                t.Rotation = System.Numerics.Quaternion.Identity;
                t.Scale = System.Numerics.Vector3.One;
            });
            _world.AddComponent(_playerEntity, transform);
            
            // Add rigid body component
            var rigidBodyConfig = new RigidBodyComponentConfig();
            var rigidBody = rigidBodyConfig.Create(rb => {
                rb.Mass = 80.0f; // 80 kg
                rb.UseGravity = true;
                rb.IsKinematic = false;
                rb.Drag = 0.1f;
            });
            _world.AddComponent(_playerEntity, rigidBody);
            
            // Add collider component
            var colliderConfig = new ColliderComponentConfig();
            var collider = colliderConfig.Create(c => {
                c.ShapeType = CollisionShapeType.Capsule;
                c.Size = new System.Numerics.Vector3(0.5f, 1.8f, 0.5f); // Human-sized capsule
                c.IsTrigger = false;
            });
            _world.AddComponent(_playerEntity, collider);
            
            // Add player component
            var playerConfig = new PlayerComponentConfig();
            var player = playerConfig.Create(p => {
                p.MovementSpeed = 5.0f;
                p.MouseSensitivity = 0.002f;
                p.InteractionRange = 2.5f;
                p.Health = 100.0f;
                p.MaxHealth = 100.0f;
            });
            _world.AddComponent(_playerEntity, player);
            
            // Add stealth component
            var stealthConfig = new StealthComponentConfig();
            var stealth = stealthConfig.Create(s => {
                s.Visibility = 0.8f;
                s.NoiseLevel = 0.5f;
            });
            _world.AddComponent(_playerEntity, stealth);
            
            // Add inventory component
            var inventoryConfig = new InventoryComponentConfig();
            var inventory = inventoryConfig.Create(i => {
                i.MaxWeight = 50.0f;
                i.MaxSlots = 20;
            });
            _world.AddComponent(_playerEntity, inventory);
        }
        
        /// <summary>
        /// Create a test level with some interactive objects
        /// </summary>
        private void CreateTestLevel()
        {
            // Create floor
            Entity floor = _world.CreateEntity();
            
            var floorTransform = new TransformComponentConfig().Create(t => {
                t.Position = new System.Numerics.Vector3(0, 0, 0);
                t.Rotation = System.Numerics.Quaternion.Identity;
                t.Scale = new System.Numerics.Vector3(50, 0.1f, 50);
            });
            _world.AddComponent(floor, floorTransform);
            
            var floorCollider = new ColliderComponentConfig().Create(c => {
                c.ShapeType = CollisionShapeType.Box;
                c.Size = new System.Numerics.Vector3(50, 0.1f, 50);
            });
            _world.AddComponent(floor, floorCollider);
            
            // Create a door
            Entity door = _world.CreateEntity();
            
            var doorTransform = new TransformComponentConfig().Create(t => {
                t.Position = new System.Numerics.Vector3(5, 1, 5);
                t.Rotation = System.Numerics.Quaternion.Identity;
                t.Scale = new System.Numerics.Vector3(1, 2, 0.1f);
            });
            _world.AddComponent(door, doorTransform);
            
            var doorComponent = new DoorComponentConfig().Create(d => {
                d.IsOpen = false;
                d.IsLocked = true;
                d.RequiredKeyId = 1;
                d.OpenSpeed = 2.0f;
                d.IsLockpickable = true;
            });
            _world.AddComponent(door, doorComponent);
            
            var doorInteractable = new InteractableComponentConfig().Create(i => {
                i.DisplayName = "Door";
                i.Type = InteractionType.Open;
            });
            _world.AddComponent(door, doorInteractable);
            
            // Create a key
            Entity key = _world.CreateEntity();
            
            var keyTransform = new TransformComponentConfig().Create(t => {
                t.Position = new System.Numerics.Vector3(2, 0.5f, 2);
                t.Rotation = System.Numerics.Quaternion.Identity;
                t.Scale = new System.Numerics.Vector3(0.1f, 0.1f, 0.3f);
            });
            _world.AddComponent(key, keyTransform);
            
            var keyItem = new ItemComponentConfig().Create(i => {
                i.ItemId = "key_door1";
                i.DisplayName = "Door Key";
                i.Description = "A key that unlocks a door.";
                i.Category = ItemCategory.Key;
                i.Weight = 0.1f;
            });
            _world.AddComponent(key, keyItem);
            
            var keyInteractable = new InteractableComponentConfig().Create(i => {
                i.DisplayName = "Door Key";
                i.Type = InteractionType.Pickup;
            });
            _world.AddComponent(key, keyInteractable);
            
            // Create a light source
            Entity light = _world.CreateEntity();
            
            var lightTransform = new TransformComponentConfig().Create(t => {
                t.Position = new System.Numerics.Vector3(0, 5, 0);
                t.Rotation = System.Numerics.Quaternion.Identity;
                t.Scale = new System.Numerics.Vector3(1, 1, 1);
            });
            _world.AddComponent(light, lightTransform);
            
            var lightSource = new LightSourceComponentConfig().Create(l => {
                l.Intensity = 1.0f;
                l.Range = 20.0f;
                l.Color = new System.Numerics.Vector3(1.0f, 0.9f, 0.8f); // Warm white
                l.Type = LightType.Point;
                l.CastsShadows = true;
                l.IsOn = true;
            });
            _world.AddComponent(light, lightSource);
            
            // Create an AI entity
            Entity guard = _world.CreateEntity();
            
            var guardTransform = new TransformComponentConfig().Create(t => {
                t.Position = new System.Numerics.Vector3(10, 1.8f, 10);
                t.Rotation = System.Numerics.Quaternion.Identity;
                t.Scale = new System.Numerics.Vector3(1, 1, 1);
            });
            _world.AddComponent(guard, guardTransform);
            
            var guardRigidBody = new RigidBodyComponentConfig().Create(rb => {
                rb.Mass = 80.0f;
                rb.UseGravity = true;
                rb.IsKinematic = false;
            });
            _world.AddComponent(guard, guardRigidBody);
            
            var guardVision = new VisionPerceptionComponentConfig().Create(v => {
                v.ViewDistance = 15.0f;
                v.FieldOfViewDegrees = 110.0f;
                v.AwarenessSpeed = 0.5f;
                v.LightSensitivity = 1.0f;
            });
            _world.AddComponent(guard, guardVision);
            
            var guardHearing = new HearingPerceptionComponentConfig().Create(h => {
                h.HearingRange = 20.0f;
                h.HearingSensitivity = 1.0f;
                h.MemoryDuration = 10.0f;
            });
            _world.AddComponent(guard, guardHearing);
            
            var guardMemory = new MemoryComponentConfig().Create(m => {
                m.AlertLevel = 0.0f;
                m.AlertDecayRate = 0.1f;
                m.MemoryRetention = 60.0f;
            });
            _world.AddComponent(guard, guardMemory);
        }
        
        #endregion
        
        #region Asset Management
        
        /// <summary>
        /// Load game assets from content pipeline
        /// </summary>
        private void LoadGameAssets()
        {
            // In a full implementation, we would load textures, models, sounds, etc.
            // For this example, we'll keep it minimal
        }
        
        #endregion
        
        #region Game State Updates
        
        /// <summary>
        /// Update game during loading state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void UpdateLoading(GameTime gameTime)
        {
            // Loading logic
            // Transition to main menu when loading is complete
        }
        
        /// <summary>
        /// Update game during main menu state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void UpdateMainMenu(GameTime gameTime)
        {
            // Main menu logic
            
            // For testing, press Enter to start game
            if (_currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
            {
                _currentState = GameState.Gameplay;
            }
        }
        
        /// <summary>
        /// Update game during active gameplay state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void UpdateGameplay(GameTime gameTime)
        {
            // Update ECS world
            _world.Update(_deltaTime);
            
            // Check for pause
            if (_currentKeyboardState.IsKeyDown(Keys.P) && _previousKeyboardState.IsKeyUp(Keys.P))
            {
                _currentState = GameState.Paused;
            }
        }
        
        /// <summary>
        /// Update game during paused state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void UpdatePaused(GameTime gameTime)
        {
            // Pause menu logic
            
            // Resume game when P is pressed again
            if (_currentKeyboardState.IsKeyDown(Keys.P) && _previousKeyboardState.IsKeyUp(Keys.P))
            {
                _currentState = GameState.Gameplay;
            }
        }
        
        /// <summary>
        /// Update game during game over state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void UpdateGameOver(GameTime gameTime)
        {
            // Game over logic
            
            // Return to main menu when Enter is pressed
            if (_currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
            {
                _currentState = GameState.MainMenu;
            }
        }
        
        #endregion
        
        #region Drawing Methods
        
        /// <summary>
        /// Draw game during loading state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void DrawLoading(GameTime gameTime)
        {
            // Draw loading screen
            _spriteBatch.Begin();
            // Draw loading text and progress bar
            _spriteBatch.End();
        }
        
        /// <summary>
        /// Draw game during main menu state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void DrawMainMenu(GameTime gameTime)
        {
            // Draw main menu
            _spriteBatch.Begin();
            // Draw menu options
            _spriteBatch.End();
        }
        
        /// <summary>
        /// Draw game during active gameplay state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void DrawGameplay(GameTime gameTime)
        {
            // Rendering happens through the RenderingSystem in the ECS
            // We don't need to do anything here directly
        }
        
        /// <summary>
        /// Draw game during paused state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void DrawPaused(GameTime gameTime)
        {
            // First draw the game world (paused)
            DrawGameplay(gameTime);
            
            // Then draw pause menu overlay
            _spriteBatch.Begin();
            // Draw pause menu
            _spriteBatch.End();
        }
        
        /// <summary>
        /// Draw game during game over state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void DrawGameOver(GameTime gameTime)
        {
            // Draw game over screen
            _spriteBatch.Begin();
            // Draw game over message
            _spriteBatch.End();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Possible game states
    /// </summary>
    public enum GameState
    {
        Loading,
        MainMenu,
        Gameplay,
        Paused,
        GameOver
    }
}