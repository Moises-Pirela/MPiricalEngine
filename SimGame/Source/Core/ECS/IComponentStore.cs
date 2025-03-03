namespace MPirical.Core.ECS;

/// <summary>
/// Interface for component storage
/// </summary>
internal interface IComponentStore
{
    void Remove(int entityId);
}