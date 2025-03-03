using System;
using System.Collections.Generic;
using MPirical.Core.ECS;

namespace MPirical.Components;

/// <summary>
/// Component that represents an item that can be picked up and used
/// </summary>
public struct ItemComponent : IComponent
{
    /// <summary>
    /// Unique identifier for this type of item
    /// </summary>
    public string ItemId;

    /// <summary>
    /// Display name of the item
    /// </summary>
    public string DisplayName;

    /// <summary>
    /// Description of the item
    /// </summary>
    public string Description;

    /// <summary>
    /// Category of item
    /// </summary>
    public ItemCategory Category;

    /// <summary>
    /// Weight of the item in kilograms
    /// </summary>
    public float Weight;

    /// <summary>
    /// Value of the item (for selling/trading)
    /// </summary>
    public int Value;

    /// <summary>
    /// Whether the item is stackable
    /// </summary>
    public bool IsStackable;

    /// <summary>
    /// Maximum stack size
    /// </summary>
    public int MaxStackSize;

    /// <summary>
    /// Current stack count
    /// </summary>
    public int StackCount;

    /// <summary>
    /// Whether the item is consumable
    /// </summary>
    public bool IsConsumable;

    /// <summary>
    /// Whether the item is equippable
    /// </summary>
    public bool IsEquippable;

    /// <summary>
    /// Equipment slot this item can be equipped in
    /// </summary>
    public EquipmentSlot EquipSlot;

    /// <summary>
    /// Custom data for this item (JSON string)
    /// </summary>
    public string CustomData;
}

/// <summary>
/// Categories of items
/// </summary>
public enum ItemCategory
{
    Weapon,
    Ammunition,
    Consumable,
    Key,
    Tool,
    Quest,
    Valuables,
    Crafting,
    Readable,
    Equipment,
    Misc
}

/// <summary>
/// Equipment slots
/// </summary>
public enum EquipmentSlot
{
    None,
    MainHand,
    OffHand,
    Head,
    Body,
    Legs,
    Feet,
    Accessory1,
    Accessory2
}

/// <summary>
/// Configuration for item component
/// </summary>
public class ItemComponentConfig : IComponentConfig<ItemComponent>
{
    /// <summary>
    /// Creates an item component with default values
    /// </summary>
    /// <returns>A new item component</returns>
    public ItemComponent CreateDefault()
    {
        return new ItemComponent
        {
            ItemId = "item_default",
            DisplayName = "Item",
            Description = "A generic item.",
            Category = ItemCategory.Misc,
            Weight = 1.0f,
            Value = 10,
            IsStackable = false,
            MaxStackSize = 1,
            StackCount = 1,
            IsConsumable = false,
            IsEquippable = false,
            EquipSlot = EquipmentSlot.None,
            CustomData = ""
        };
    }

    /// <summary>
    /// Creates an item component with custom configuration
    /// </summary>
    /// <param name="configureAction">Action to configure the component</param>
    /// <returns>A configured item component</returns>
    public ItemComponent Create(Action<ItemComponent> configureAction)
    {
        var component = CreateDefault();
        configureAction(component);
        return component;
    }
}