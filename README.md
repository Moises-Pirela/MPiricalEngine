# SimGame

A C# immersive sim game using an Entity Component System architecture with MonoGame as the rendering backend.

## Project Overview

SimGame is a first-person immersive sim featuring stealth mechanics, interactive environments, and systems-based gameplay. The game uses an Entity Component System (ECS) architecture to create emergent gameplay through the interaction of various game systems.

## Architecture

The project follows a strict ECS architecture:

- **Entities**: Simple numeric identifiers represented by the `Entity` struct
- **Components**: Pure data containers with no behavior (implement `IComponent`)
- **Systems**: Logic modules that operate on components (implement `ISystem`)

## Core Systems

- **World**: Central manager for all entities, components, and systems
- **EntitySerializer**: Handles saving/loading entities to/from JSON
- **AssetManager**: Loads and manages game assets
- **RenderingSystem**: Handles 3D rendering via MonoGame

## Game Features

### Player Systems
- First-person movement and camera controls
- Interaction with objects and items
- Inventory system for collecting and using items

### Stealth Mechanics
- **StealthSystem**: Manages visibility and noise levels
- **LightSourceComponent**: Affects player visibility
- **SurfaceType**: Different surfaces affect noise when moving

### AI Systems
- **VisionPerceptionSystem**: AI vision and detection
- **HearingPerceptionSystem**: AI response to sound
- **MemoryComponent**: AI memory of events and entities

### Environment Interaction
- Interactable objects (doors, items, etc.)
- Physics simulation for realistic movement

## Components

Key components include:

- **TransformComponent**: Position, rotation, and scale
- **RigidBodyComponent**: Physics properties
- **StealthComponent**: Visibility and noise properties
- **ItemComponent**: Item properties for inventory
- **InventoryComponent**: Container for items
- **LightSourceComponent**: Light emission properties
- **PlayerComponent**: Player-specific properties
- **VisionPerceptionComponent**: AI vision properties
- **HearingPerceptionComponent**: AI hearing properties

## Getting Started

### Prerequisites
- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [MonoGame](https://www.monogame.net/)

### Building and Running

```bash
# Clone the repository
git clone https://github.com/yourusername/SimGame.git

# Navigate to the project directory
cd SimGame

# Build the project
dotnet build

# Run the game
dotnet run
```

## Controls

- **WASD**: Movement
- **Mouse**: Look around
- **F**: Interact with objects
- **C**: Crouch
- **Q/E**: Lean left/right
- **P**: Pause the game
- **ESC**: Exit the game

## Project Structure

```
SimGame/
├── Source/
│   ├── Core/
│   │   ├── ECS/
│   │   │   ├── Entity.cs
│   │   │   ├── IComponent.cs
│   │   │   ├── ISystem.cs
│   │   │   ├── World.cs
│   │   │   └── ...
│   │   ├── Math/
│   │   │   └── MathUtil.cs
│   │   └── Serialization/
│   │       └── EntitySerialization.cs
│   ├── Components/
│   │   ├── AIPerceptionComponent.cs
│   │   ├── CameraComponent.cs
│   │   ├── InteractableComponent.cs
│   │   ├── InventoryComponent.cs
│   │   ├── ItemComponent.cs
│   │   ├── LightSourceComponent.cs
│   │   ├── PhysicsComponent.cs
│   │   ├── PlayerComponent.cs
│   │   ├── StealthComponent.cs
│   │   └── TransformComponent.cs
│   ├── Systems/
│   │   ├── InventorySystem.cs
│   │   ├── InteractionUISystem.cs
│   │   ├── PhysicsSystem.cs
│   │   ├── PlayerInputSystem.cs
│   │   ├── StealthSystem.cs
│   │   └── TransformHierarchySystem.cs
│   ├── Rendering/
│   │   └── RenderingSystem.cs
│   ├── Content/
│   │   └── AssetManager.cs
│   ├── GameSim.cs
│   └── Program.cs
└── Content/
    ├── Fonts/
    ├── Textures/
    ├── Sounds/
    └── Levels/
```

## License


## Acknowledgments

- MonoGame framework