using System;
using System.Collections.Generic;
using MPirical.Core.ECS;

namespace MPirical.Components;


/// <summary>
/// Represents an instance of an item in an inventory
/// </summary>
public struct ItemInstance
{
    /// <summary>
    /// Entity ID of the source item (or -1 if generated)
    /// </summary>
    public int SourceEntityId;

    /// <summary>
    /// Item component data
    /// </summary>
    public ItemComponent Item;

    /// <summary>
    /// Slot index in the inventory
    /// </summary>
    public int SlotIndex;
}

/// <summary>
/// Component that represents an inventory that can hold items
/// </summary>
public struct InventoryComponent : IComponent
{
    /// <summary>
    /// Maximum weight the inventory can hold
    /// </summary>
    public float MaxWeight;

    /// <summary>
    /// Current weight of all items in the inventory
    /// </summary>
    public float CurrentWeight;

    /// <summary>
    /// Maximum number of item slots
    /// </summary>
    public int MaxSlots;

    /// <summary>
    /// Items in the inventory
    /// </summary>
    public Dictionary<int, ItemInstance> Items;

    /// <summary>
    /// Currently equipped items
    /// </summary>
    public Dictionary<EquipmentSlot, int> EquippedItems;
}

/// <summary>
/// Configuration for inventory component
/// </summary>
public class InventoryComponentConfig : IComponentConfig<InventoryComponent>
{
    /// <summary>
    /// Creates an inventory component with default values
    /// </summary>
    /// <returns>A new inventory component</returns>
    public InventoryComponent CreateDefault()
    {
        return new InventoryComponent
        {
            MaxWeight = 50.0f,
            CurrentWeight = 0.0f,
            MaxSlots = 20,
            Items = new Dictionary<int, ItemInstance>(),
            EquippedItems = new Dictionary<EquipmentSlot, int>()
        };
    }

    /// <summary>
    /// Creates an inventory component with custom configuration
    /// </summary>
    /// <param name="configureAction">Action to configure the component</param>
    /// <returns>A configured inventory component</returns>
    public InventoryComponent Create(Action<InventoryComponent> configureAction)
    {
        var component = CreateDefault();
        configureAction(component);
        return component;
    }
}