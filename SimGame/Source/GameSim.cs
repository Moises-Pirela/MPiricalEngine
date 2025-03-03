using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MPirical.Components;
using MPirical.Components.AI;
using MPirical.Content;
using MPirical.Core;
using MPirical.Core.ECS;
using MPirical.Rendering;
using MPirical.Systems;

namespace MPirical;

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

/// <summary>
/// Main game class that initializes MonoGame and the ECS architecture
/// </summary>
public class GameSim : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // ECS world
    private World _world;

    // Rendering system reference
    private RenderingSystem _renderingSystem;

    // Asset manager
    private AssetManager _assetManager;

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

    //----------- MAIN MENU

    private string[] _menuOptions = { "NEW GAME", "LOAD GAME", "OPTIONS", "EXIT" };
    private int _selectedOption = 0;
    private SpriteFont _menuFont;
    private SpriteFont _titleFont;
    private Texture2D _menuBackground;
    private Color _normalTextColor = new Color(180, 180, 180);
    private Color _selectedTextColor = new Color(255, 0, 0);
    private float _menuAnimTime = 0f;
    private float _titleAnimTime = 0f;
    private bool _showCursor = true;
    private float _cursorBlinkTime = 0f;

    private Vector2 _baseResolution = new Vector2(1280, 720); // Design resolution
    private Vector2 _scaleVector = Vector2.One;
    
    private SoundEffect _menuSelectSound;
    private SoundEffect _menuConfirmSound;
    private SoundEffect _menuMoveSound;


    //----------- END MAIN MENU

    /// <summary>
    /// Constructor sets up the MonoGame environment
    /// </summary>
    public GameSim()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false; // Hide cursor for first-person view

        // Set window size
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
        Window.AllowUserResizing = true;

        Window.ClientSizeChanged += OnClientSizeChanged;

        // Initialize game state
        _currentState = GameState.Loading;
    }

    protected override void Initialize()
    {
        GameServices.AddService<GraphicsDevice>(GraphicsDevice);
        GameServices.AddService<ContentManager>(Content);
        GameServices.AddService<Game>(this);

        InitializeECS();

        _assetManager = new AssetManager(Content, _world);

        base.Initialize();

        _currentState = GameState.MainMenu;
    }

    protected void OnClientSizeChanged(object sender, EventArgs e)
    {
        UpdateUIScale();
    }


    private void UpdateUIScale()
    {
        // Calculate scale based on current resolution vs base design resolution
        float scaleX = GraphicsDevice.Viewport.Width / _baseResolution.X;
        float scaleY = GraphicsDevice.Viewport.Height / _baseResolution.Y;

        // Use the smaller scale to ensure UI fits within the screen
        float scale = Math.Min(scaleX, scaleY);

        // Update scale vector
        _scaleVector = new Vector2(scale, scale);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        UpdateUIScale();

        try
        {
            _menuFont = Content.Load<SpriteFont>("Fonts/MainMenuFont");
            _titleFont = Content.Load<SpriteFont>("Fonts/MainMenuFont");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading fonts: {ex.Message}");
        }

        _menuMoveSound = Content.Load<SoundEffect>("Sounds/menu_move");
        _menuSelectSound = Content.Load<SoundEffect>("Sounds/menu_select");
        _menuConfirmSound = Content.Load<SoundEffect>("Sounds/menu_confirm");

        _menuBackground = new Texture2D(GraphicsDevice, 1, 1);
        _menuBackground.SetData(new[] { Color.Black });

        _assetManager.LoadAllAssets();

        LoadLevel("test_level");
    }

    /// <summary>
    /// Unload content when the game exits
    /// </summary>
    protected override void UnloadContent()
    {
        // Unload content here
    }

    /// <summary>
    /// Update the game logic
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    protected override void Update(GameTime gameTime)
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        HandleGlobalInput();

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

    #region Game State Management

    /// <summary>
    /// Handle global input that works in any game state
    /// </summary>
    private void HandleGlobalInput()
    {
        if (_currentKeyboardState.IsKeyDown(Keys.Escape))
            Exit();

        if (_currentKeyboardState.IsKeyDown(Keys.LeftAlt) &&
            IsKeyPressed(Keys.Enter))
        {
            ToggleFullscreen();
        }
    }

    /// <summary>
    /// Toggle between fullscreen and windowed mode
    /// </summary>
    private void ToggleFullscreen()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();
    }

    /// <summary>
    /// Load a level by name
    /// </summary>
    /// <param name="levelName">Name of the level to load</param>
    private void LoadLevel(string levelName)
    {
        List<Entity> levelEntities = _assetManager.LoadLevel(levelName);

        if (levelEntities.Count == 0)
        {
            CreateTestLevel();
        }

        if (_renderingSystem != null && _playerEntity.Id != -1)
        {
            _renderingSystem.SetCameraEntity(_playerEntity);
        }
    }

    #endregion

    #region ECS Initialization

    /// <summary>
    /// Initialize the Entity Component System architecture
    /// </summary>
    private void InitializeECS()
    {
        _world = new World();
        
        CommonArchetypes.RegisterCommonArchetypes(_world);

        RegisterSystems();

        CreatePlayer();
    }

    /// <summary>
    /// Register all systems with the ECS world
    /// </summary>
    private void RegisterSystems()
    {
        _world.RegisterSystem(new TransformHierarchySystem());
        _world.RegisterSystem(new PhysicsSystem());

        _world.RegisterSystem(new PlayerInputSystem());
        _world.RegisterSystem(new InteractionUISystem());
        
        _world.RegisterSystem(new VisionPerceptionSystem());
        _world.RegisterSystem(new HearingPerceptionSystem());

        _world.RegisterSystem(new StealthSystem());

        // Environment systems
        // _world.RegisterSystem(new DoorSystem());
        // _world.RegisterSystem(new SwitchSystem());
        // _world.RegisterSystem(new ComputerTerminalSystem());
        // _world.RegisterSystem(new SecuritySystem());

        _world.RegisterSystem(new InventorySystem());

        _renderingSystem = new RenderingSystem(GraphicsDevice, Content);
        _world.RegisterSystem(_renderingSystem);
    }

    /// <summary>
    /// Create the player entity
    /// </summary>
    private void CreatePlayer()
    {
        // Create player entity
        _playerEntity = _world.CreateEntity();

        var transformConfig = new TransformComponentConfig();
        var transform = transformConfig.Create(t =>
        {
            t.Position = new System.Numerics.Vector3(0, 1.8f, 0); // Eye height
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = System.Numerics.Vector3.One;
        });
        _world.AddComponent(_playerEntity, transform);

        var rigidBodyConfig = new RigidBodyComponentConfig();
        var rigidBody = rigidBodyConfig.Create(rb =>
        {
            rb.Mass = 80.0f; // 80 kg
            rb.UseGravity = true;
            rb.IsKinematic = false;
            rb.Drag = 0.1f;
        });
        _world.AddComponent(_playerEntity, rigidBody);

        var colliderConfig = new ColliderComponentConfig();
        var collider = colliderConfig.Create(c =>
        {
            c.ShapeType = CollisionShapeType.Capsule;
            c.Size = new System.Numerics.Vector3(0.5f, 1.8f, 0.5f); // Human-sized capsule
            c.IsTrigger = false;
        });
        _world.AddComponent(_playerEntity, collider);

        var playerConfig = new PlayerComponentConfig();
        var player = playerConfig.Create(p =>
        {
            p.MovementSpeed = 5.0f;
            p.MouseSensitivity = 0.002f;
            p.InteractionRange = 2.5f;
            p.Health = 100.0f;
            p.MaxHealth = 100.0f;
        });
        _world.AddComponent(_playerEntity, player);

        var stealthConfig = new StealthComponentConfig();
        var stealth = stealthConfig.Create(s =>
        {
            s.Visibility = 0.8f;
            s.NoiseLevel = 0.5f;
        });
        _world.AddComponent(_playerEntity, stealth);

        var inventoryConfig = new InventoryComponentConfig();
        var inventory = inventoryConfig.Create(i =>
        {
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
        Entity floor = _world.CreateEntity();

        var floorTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = new System.Numerics.Vector3(0, 0, 0);
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = new System.Numerics.Vector3(50, 0.1f, 50);
        });
        _world.AddComponent(floor, floorTransform);

        var floorCollider = new ColliderComponentConfig().Create(c =>
        {
            c.ShapeType = CollisionShapeType.Box;
            c.Size = new System.Numerics.Vector3(50, 0.1f, 50);
        });
        _world.AddComponent(floor, floorCollider);

        Entity door = _world.CreateEntity();

        var doorTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = new System.Numerics.Vector3(5, 1, 5);
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = new System.Numerics.Vector3(1, 2, 0.1f);
        });
        _world.AddComponent(door, doorTransform);

        // var doorComponent = new DoorComponentConfig().Create(d =>
        // {
        //     d.IsOpen = false;
        //     d.IsLocked = true;
        //     d.RequiredKeyId = 1;
        //     d.OpenSpeed = 2.0f;
        //     d.IsLockpickable = true;
        // });
        // _world.AddComponent(door, doorComponent);

        var doorInteractable = new InteractableComponentConfig().Create(i =>
        {
            i.DisplayName = "Door";
            i.Type = InteractionType.Open;
        });
        _world.AddComponent(door, doorInteractable);

        Entity key = _world.CreateEntity();

        var keyTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = new System.Numerics.Vector3(2, 0.5f, 2);
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = new System.Numerics.Vector3(0.1f, 0.1f, 0.3f);
        });
        _world.AddComponent(key, keyTransform);

        var keyItem = new ItemComponentConfig().Create(i =>
        {
            i.ItemId = "key_door1";
            i.DisplayName = "Door Key";
            i.Description = "A key that unlocks a door.";
            i.Category = ItemCategory.Key;
            i.Weight = 0.1f;
        });
        _world.AddComponent(key, keyItem);

        var keyInteractable = new InteractableComponentConfig().Create(i =>
        {
            i.DisplayName = "Door Key";
            i.Type = InteractionType.Pickup;
        });
        _world.AddComponent(key, keyInteractable);

        Entity light = _world.CreateEntity();

        var lightTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = new System.Numerics.Vector3(0, 5, 0);
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = new System.Numerics.Vector3(1, 1, 1);
        });
        _world.AddComponent(light, lightTransform);

        var lightSource = new LightSourceComponentConfig().Create(l =>
        {
            l.Intensity = 1.0f;
            l.Range = 20.0f;
            l.Color = new System.Numerics.Vector3(1.0f, 0.9f, 0.8f); // Warm white
            l.Type = LightType.Point;
            l.CastsShadows = true;
            l.IsOn = true;
        });
        _world.AddComponent(light, lightSource);

        Entity guard = _world.CreateEntity();

        var guardTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = new System.Numerics.Vector3(10, 1.8f, 10);
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = new System.Numerics.Vector3(1, 1, 1);
        });
        _world.AddComponent(guard, guardTransform);

        var guardRigidBody = new RigidBodyComponentConfig().Create(rb =>
        {
            rb.Mass = 80.0f;
            rb.UseGravity = true;
            rb.IsKinematic = false;
        });
        _world.AddComponent(guard, guardRigidBody);

        var guardVision = new VisionPerceptionComponentConfig().Create(v =>
        {
            v.ViewDistance = 15.0f;
            v.FieldOfViewDegrees = 110.0f;
            v.AwarenessSpeed = 0.5f;
            v.LightSensitivity = 1.0f;
        });
        _world.AddComponent(guard, guardVision);

        var guardHearing = new HearingPerceptionComponentConfig().Create(h =>
        {
            h.HearingRange = 20.0f;
            h.HearingSensitivity = 1.0f;
            h.MemoryDuration = 10.0f;
        });
        _world.AddComponent(guard, guardHearing);

        var guardMemory = new MemoryComponentConfig().Create(m =>
        {
            m.AlertLevel = 0.0f;
            m.AlertDecayRate = 0.1f;
            m.MemoryRetention = 60.0f;
        });
        _world.AddComponent(guard, guardMemory);

        CreateWalls();

        //CreateComputerTerminal();

        //CreateSecuritySystem();
    }

    /// <summary>
    /// Create walls to form a simple room
    /// </summary>
    private void CreateWalls()
    {
        CreateWall(new System.Numerics.Vector3(0, 1, 10), new System.Numerics.Vector3(20, 2, 0.5f)); // North wall
        CreateWall(new System.Numerics.Vector3(0, 1, -10), new System.Numerics.Vector3(20, 2, 0.5f)); // South wall
        CreateWall(new System.Numerics.Vector3(10, 1, 0), new System.Numerics.Vector3(0.5f, 2, 20)); // East wall
        CreateWall(new System.Numerics.Vector3(-10, 1, 0), new System.Numerics.Vector3(0.5f, 2, 20)); // West wall
    }

    /// <summary>
    /// Create a single wall
    /// </summary>
    /// <param name="position">Position of the wall</param>
    /// <param name="scale">Scale of the wall</param>
    private void CreateWall(System.Numerics.Vector3 position, System.Numerics.Vector3 scale)
    {
        Entity wall = _world.CreateEntity();

        var wallTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = position;
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = scale;
        });
        _world.AddComponent(wall, wallTransform);

        var wallCollider = new ColliderComponentConfig().Create(c =>
        {
            c.ShapeType = CollisionShapeType.Box;
            c.Size = scale;
        });
        _world.AddComponent(wall, wallCollider);
    }

/*
    /// <summary>
    /// Create a computer terminal
    /// </summary>
    private void CreateComputerTerminal()
    {
        Entity terminal = _world.CreateEntity();

        var terminalTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = new System.Numerics.Vector3(-8, 1, 8);
            t.Rotation = System.Numerics.Quaternion.CreateFromAxisAngle(
                System.Numerics.Vector3.UnitY, MathHelper.ToRadians(45));
            t.Scale = new System.Numerics.Vector3(0.5f, 0.5f, 0.3f);
        });
        _world.AddComponent(terminal, terminalTransform);

        var terminalComponent = new ComputerTerminalComponentConfig().Create(c =>
        {
            c.IsPoweredOn = true;
            c.IsLoggedIn = false;
            c.SecurityLevel = 1;
            c.SecurityLevel = 1;
            c.Username = "admin";
            c.Password = "123456";
            c.IsHacked = false;
            c.HackingDifficulty = 0.5f;
            c.Emails = new string[]
            {
                "From: security@facility.com\nSubject: Security Protocol Update\n\nAll staff are reminded to lock their terminals when not in use."
            };
            c.TextFiles = new string[]
            {
                "security_codes.txt",
                "maintenance_log.txt"
            };
            c.AvailableCommands = new string[]
            {
                "help",
                "ls",
                "cat",
                "unlock_door",
                "security_status",
                "logout"
            };
        });
        _world.AddComponent(terminal, terminalComponent);

        var terminalInteractable = new InteractableComponentConfig().Create(i =>
        {
            i.DisplayName = "Computer Terminal";
            i.Type = InteractionType.Use;
        });
        _world.AddComponent(terminal, terminalInteractable);
    }

    /// <summary>
    /// Create a security system
    /// </summary>
    private void CreateSecuritySystem()
    {
        Entity security = _world.CreateEntity();

        var securityTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = new System.Numerics.Vector3(-9, 1.5f, -9);
            t.Rotation = System.Numerics.Quaternion.Identity;
            t.Scale = new System.Numerics.Vector3(0.4f, 0.6f, 0.2f);
        });
        _world.AddComponent(security, securityTransform);

        var securityComponent = new SecuritySystemComponentConfig().Create(s =>
        {
            s.IsActive = true;
            s.AlertLevel = 0.0f;
            s.CameraIds = new int[0]; // We'd add camera entities here
            s.AlarmIds = new int[0]; // We'd add alarm entities here
            s.GuardIds = new int[0]; // We'd add guard entities here
            s.IsDisabled = false;
            s.IsAlarmTriggered = false;
            s.CanBeReset = true;
        });
        _world.AddComponent(security, securityComponent);

        var securityInteractable = new InteractableComponentConfig().Create(i =>
        {
            i.DisplayName = "Security Panel";
            i.Type = InteractionType.Hack;
        });
        _world.AddComponent(security, securityInteractable);

        // Create a security camera
        CreateSecurityCamera(new System.Numerics.Vector3(8, 3, 8), 225);
    }

    /// <summary>
    /// Create a security camera
    /// </summary>
    /// <param name="position">Position of the camera</param>
    /// <param name="rotationDegrees">Rotation in degrees</param>
    private void CreateSecurityCamera(System.Numerics.Vector3 position, float rotationDegrees)
    {
        Entity camera = _world.CreateEntity();

        var cameraTransform = new TransformComponentConfig().Create(t =>
        {
            t.Position = position;
            t.Rotation = System.Numerics.Quaternion.CreateFromAxisAngle(
                System.Numerics.Vector3.UnitY, MathHelper.ToRadians(rotationDegrees));
            t.Scale = new System.Numerics.Vector3(0.3f, 0.3f, 0.5f);
        });
        _world.AddComponent(camera, cameraTransform);

        // Add vision perception to the camera
        var cameraVision = new VisionPerceptionComponentConfig().Create(v =>
        {
            v.ViewDistance = 20.0f;
            v.FieldOfViewDegrees = 60.0f;
            v.AwarenessSpeed = 0.8f;
            v.LightSensitivity = 0.5f;
        });
        _world.AddComponent(camera, cameraVision);

        // In a full implementation, we'd connect this camera to the security system
    }
*/

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
        _currentState = GameState.MainMenu;
    }

    /// <summary>
    /// Update game during main menu state
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void UpdateMainMenu(GameTime gameTime)
    {
        // Update menu animation timers
        _menuAnimTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        _titleAnimTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
        _cursorBlinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 2;

        if (_cursorBlinkTime > 1)
        {
            _showCursor = !_showCursor;
            _cursorBlinkTime = 0;
        }

        // Handle menu navigation
        if (IsKeyPressed(Keys.Down))
        {
            _selectedOption = (_selectedOption + 1) % _menuOptions.Length;
            _menuMoveSound.Play(1f, 1f, 0.0f);
        }
        else if (IsKeyPressed(Keys.Up))
        {
            _selectedOption = (_selectedOption - 1 + _menuOptions.Length) % _menuOptions.Length;
            _menuMoveSound.Play(1f, 1f, 0.0f);
        }

        // Handle menu selection
        if (IsKeyPressed(Keys.Enter))
        {
            _menuConfirmSound.Play(1f, 1f, 0.0f);
            switch (_selectedOption)
            {
                case 0: // New Game
                    _currentState = GameState.Gameplay;
                    IsMouseVisible = false;
                    Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                    break;

                case 1: // Load Game
                    // Load game implementation
                    break;

                case 2: // Options
                    // Show options implementation
                    break;

                case 3: // Exit
                    Exit();
                    break;
            }
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
        if (IsKeyPressed(Keys.P))
        {
            _currentState = GameState.Paused;
            IsMouseVisible = true; // Show cursor while paused
        }
    }

    /// <summary>
    /// Update game during paused state
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void UpdatePaused(GameTime gameTime)
    {
        if (IsKeyPressed(Keys.P))
        {
            _currentState = GameState.Gameplay;
            IsMouseVisible = false; // Hide cursor during gameplay

            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        }
    }

    /// <summary>
    /// Update game during game over state
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void UpdateGameOver(GameTime gameTime)
    {
        // Return to main menu when Enter is pressed
        if (IsKeyPressed(Keys.Enter))
        {
            _currentState = GameState.MainMenu;
        }
    }

    /// <summary>
    /// Check if a key was just pressed this frame
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>True if the key was just pressed</returns>
    private bool IsKeyPressed(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
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
        // Add loading text and progress bar later
        _spriteBatch.End();
    }

    /// <summary>
    /// Draw game during main menu state
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void DrawMainMenu(GameTime gameTime)
    {
    GraphicsDevice.Clear(Color.Black);

    _spriteBatch.Begin();
    _spriteBatch.Draw(_menuBackground,
        new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
    
    float screenWidth = GraphicsDevice.Viewport.Width;
    float screenHeight = GraphicsDevice.Viewport.Height;
    float baseScale = _scaleVector.X; // Use consistent scaling

    string title = "ExMORTALIS";
    string subtitle = "";

    float titleBaseScale = 3.0f * baseScale;
    float titleScale = titleBaseScale + (float)Math.Sin(_titleAnimTime) * 0.05f * titleBaseScale;
    Vector2 titleSize = _titleFont?.MeasureString(title) ?? new Vector2(200, 40);
    float titleX = (screenWidth - titleSize.X * titleScale) / 2;
    float titleY = screenHeight * 0.2f;

    float shadowOffset = 4 * baseScale;
    _spriteBatch.DrawString(_titleFont, title,
        new Vector2(titleX + shadowOffset, titleY + shadowOffset),
        new Color(0, 0, 0, 150), 0, Vector2.Zero, titleScale, SpriteEffects.None, 0);

    _spriteBatch.DrawString(_titleFont, title,
        new Vector2(titleX, titleY),
        new Color(180, 0, 0), 0, Vector2.Zero, titleScale, SpriteEffects.None, 0);

    float subtitleScale = baseScale;
    Vector2 subtitleSize = _menuFont?.MeasureString(subtitle) ?? new Vector2(150, 20);
    _spriteBatch.DrawString(_menuFont, subtitle,
        new Vector2((screenWidth - subtitleSize.X * subtitleScale) / 2,
                titleY + titleSize.Y * titleScale + (10 * baseScale)),
        new Color(180, 180, 180), 0, Vector2.Zero, subtitleScale, SpriteEffects.None, 0);

    float menuStartY = screenHeight * 0.5f;
    float menuSpacing = 70 * baseScale; // Increased from 40 to 70 for more spacing between options
    float menuScale = baseScale;

    for (int i = 0; i < _menuOptions.Length; i++)
    {
        Vector2 textSize = _menuFont?.MeasureString(_menuOptions[i]) ?? new Vector2(100, 30);
        
        float itemScale = menuScale;
        if (i == _selectedOption)
        {
            itemScale = menuScale * (1.0f + (float)Math.Sin(_menuAnimTime * 4) * 0.05f);
        }

        Vector2 position = new Vector2(
            (screenWidth - textSize.X * itemScale) / 2,
            menuStartY + i * menuSpacing);

        if (i == _selectedOption && _showCursor)
        {
            float cursorOffset = 30 * baseScale;
            _spriteBatch.DrawString(_menuFont, ">",
                new Vector2(position.X - cursorOffset, position.Y),
                _selectedTextColor, 0, Vector2.Zero, itemScale, SpriteEffects.None, 0);
        }

        Color color = (i == _selectedOption) ? _selectedTextColor : _normalTextColor;
        _spriteBatch.DrawString(_menuFont, _menuOptions[i], position, color,
            0, Vector2.Zero, itemScale, SpriteEffects.None, 0);
    }

    string copyright = "2025 MPirical Studios";
    float copyrightScale = 0.8f * baseScale;
    Vector2 copyrightSize = _menuFont?.MeasureString(copyright) ?? new Vector2(150, 20);
    _spriteBatch.DrawString(_menuFont, copyright,
        new Vector2((screenWidth - copyrightSize.X * copyrightScale) / 2,
                screenHeight - (75 * baseScale)),
        new Color(100, 100, 100), 0, Vector2.Zero, copyrightScale, SpriteEffects.None, 0);

    _spriteBatch.End();
    }

    /// <summary>
    /// Draw game during active gameplay state
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void DrawGameplay(GameTime gameTime)
    {
        _spriteBatch.Begin();
        
        _renderingSystem.Update(gameTime.ElapsedGameTime.Seconds);

        _spriteBatch.End();
    }

    /// <summary>
    /// Draw game during paused state
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void DrawPaused(GameTime gameTime)
    {
        DrawGameplay(gameTime);

        _spriteBatch.Begin();
        _spriteBatch.End();
    }

    /// <summary>
    /// Draw game during game over state
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void DrawGameOver(GameTime gameTime)
    {
        _spriteBatch.Begin();
        _spriteBatch.End();
    }

    #endregion
}