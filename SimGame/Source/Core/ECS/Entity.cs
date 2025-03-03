using System;

namespace MPirical.Core.ECS;

/// <summary>
/// Represents a unique entity in the game world
/// </summary>
public readonly struct Entity : IEquatable<Entity>
{
    /// <summary>
    /// Unique identifier for this entity
    /// </summary>
    public readonly int Id { get; }

    /// <summary>
    /// Creates a new entity with the given ID
    /// </summary>
    /// <param name="id">Unique identifier for this entity</param>
    public Entity(int id)
    {
        Id = id;
    }

    public bool Equals(Entity other) => Id == other.Id;
    public override bool Equals(object obj) => obj is Entity entity && Equals(entity);
    public override int GetHashCode() => Id;
    public static bool operator ==(Entity left, Entity right) => left.Equals(right);
    public static bool operator !=(Entity left, Entity right) => !left.Equals(right);
    public override string ToString() => $"Entity({Id})";
}