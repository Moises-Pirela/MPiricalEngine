# MPirical Engine Documentation

## Introduction

MPirical is a C#-based game engine designed for creating immersive simulation games in the vein of Thief and System Shock. The engine uses an Entity Component System (ECS) architecture with MonoGame for rendering.

## Core Architecture

### Entity Component System (ECS)

The engine is built around an ECS architecture:

- **Entities**: Simple identifiers represented by the `Entity` struct
- **Components**: Pure data containers that attach to entities (implement `IComponent`)
- **Systems**: Logic that processes entities with specific component combinations

### Key Classes

- `World`: Central manager for all entities, components, and systems
- `Entity`: Lightweight struct representing a unique game object
- `IComponent`: Interface for all data components
- `ISystem`: Interface for all systems that operate on components

## Getting Started

### Setting Up a New Project

1. Create a new MonoGame project
2. Add references to MPirical assemblies
3. Set up a main game class that inherits from `SimGame`

```csharp
using MPirical;

using var game = new SimGame();
game.Run();
```

## Understanding the Core Systems

### Rendering System

The `RenderingSystem` handles all visual output through MonoGame. It:

- Manages the camera view
- Renders all entities with appropriate components
- Handles lighting and shadows

### Physics System

The `PhysicsSystem` simulates physical interactions:

- Applies gravity and forces
- Manages collision detection
- Updates entity positions based on velocity

### Input System

The `PlayerInputSystem` handles player interaction:

- Processes keyboard and mouse input
- Updates player movement and camera rotation
- Manages player actions like jumping and crouching

### Stealth System

The `StealthSystem` implements core immersive sim mechanics:

- Calculates entity visibility based on lighting
- Manages sound propagation based on movement
- Tracks detection levels for AI awareness

## Creating Game Entities

### Player Setup

```csharp
// Create player entity
Entity player = world.CreateEntity();

// Add essential components
world.AddComponent(player, new TransformComponent { 
    Position = new Vector3(0, 1.8f, 0), 
    Rotation = Quaternion.Identity, 
    Scale = Vector3.One 
});

world.AddComponent(player, new PlayerComponent {
    MovementSpeed = 5.0f,
    MouseSensitivity = 0.002f,
    InteractionRange = 2.5f,
    Health = 100.0f
});

world.AddComponent(player, new RigidBodyComponent {
    Mass = 80.0f,
    UseGravity = true
});
```

### Interactive Objects

```csharp
// Create a door entity
Entity door = world.CreateEntity();

// Add components
world.AddComponent(door, new TransformComponent {
    Position = new Vector3(5, 1, 5)
});

world.AddComponent(door, new InteractableComponent {
    DisplayName = "Door",
    Type = InteractionType.Open
});
```

## AI and Perception

The engine includes sophisticated AI perception systems:

- `VisionPerceptionSystem`: Handles AI sight with field of view, distance, and lighting factors
- `HearingPerceptionSystem`: Processes sound propagation and AI response to noise
- `MemoryComponent`: Gives AI the ability to remember events and track suspicion levels

## Important Components

- `TransformComponent`: Position, rotation, and scale
- `RigidBodyComponent`: Physical properties for simulation
- `ColliderComponent`: Collision detection shapes
- `StealthComponent`: Visibility and noise emission
- `InteractableComponent`: Player interaction properties

## Content Management

The `AssetManager` class handles loading and managing game assets:

- Textures, models, and effects
- Level data via the serialization system
- Fallback creation for testing

## Extending the Engine

### Creating Custom Components

```csharp
public struct CustomComponent : IComponent
{
    public float SomeValue;
    public string SomeProperty;
}

public class CustomComponentConfig : IComponentConfig<CustomComponent>
{
    public CustomComponent CreateDefault()
    {
        return new CustomComponent
        {
            SomeValue = 1.0f,
            SomeProperty = "Default"
        };
    }

    public CustomComponent Create(Action<CustomComponent> configureAction)
    {
        var component = CreateDefault();
        configureAction(component);
        return component;
    }
}
```

### Creating Custom Systems

```csharp
public class CustomSystem : ISystem
{
    private World _world;
    
    public string Name => "CustomSystem";
    public int Priority => 500;
    
    public void Initialize(World world)
    {
        _world = world;
    }
    
    public void Update(float deltaTime)
    {
        // System logic here
    }
}
```

## Utility Classes

- `MathUtil`: Extended math functions beyond standard libraries
- `EntitySerializer`: Save/load entities to JSON
- `GameServices`: Access global services like GraphicsDevice

## Game Loop

The engine follows a standard pattern:

1. `Initialize()`: Set up systems and initial entities
2. `LoadContent()`: Load game assets
3. `Update()`: Process game logic
4. `Draw()`: Render the scene
## Performance Considerations

- The ECS architecture enables efficient batch processing
- Unsafe C# blocks are used for performance-critical sections
- The engine handles garbage collection carefully to prevent stutters
- Spatial partitioning optimizes collision detection