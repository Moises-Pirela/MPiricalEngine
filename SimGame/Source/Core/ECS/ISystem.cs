namespace MPirical.Core.ECS;

/// <summary>
/// Interface for all systems in the ECS architecture.
/// Systems contain the logic that operates on components.
/// </summary>
public interface ISystem
{
    /// <summary>
    /// Unique identifier for this system
    /// </summary>
    string Name { get; }
        
    /// <summary>
    /// Determines the execution order of systems (lower values run first)
    /// </summary>
    int Priority { get; }
        
    /// <summary>
    /// Initialize the system with necessary dependencies
    /// </summary>
    /// <param name="world">The world this system operates in</param>
    void Initialize(World world);
        
    /// <summary>
    /// Update the system with the given time delta
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    void Update(float deltaTime);
}