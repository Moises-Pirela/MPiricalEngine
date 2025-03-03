using System;
using System.Collections.Generic;
using System.Numerics;
using MPirical.Components;
using MPirical.Core.ECS;
namespace MPirical.Systems;

/// <summary>
/// System that manages inventories and items
/// </summary>
public class InventorySystem : ISystem
{
    private World _world;
    private List<Entity> _inventories = new List<Entity>();
    private List<Entity> _items = new List<Entity>();

    /// <summary>
    /// Name of this system
    /// </summary>
    public string Name => "InventorySystem";

    /// <summary>
    /// Priority of this system
    /// </summary>
    public int Priority => 700;

    /// <summary>
    /// Initialize the system with the world
    /// </summary>
    /// <param name="world">Reference to the world</param>
    public void Initialize(World world)
    {
        _world = world;
    }

    /// <summary>
    /// Update inventory behavior
    /// </summary>
    /// <param name="deltaTime">Time since last update</param>
    public void Update(float deltaTime)
    {
        // Update entity lists
        UpdateEntityLists();

        // Process each inventory
        foreach (var inventoryEntity in _inventories)
        {
            if (!_world.HasComponent<InventoryComponent>(inventoryEntity))
                continue;

            var inventory = _world.GetComponent<InventoryComponent>(inventoryEntity);

            // No continuous behavior to update for now

            // Update the inventory component in the world
            _world.AddComponent(inventoryEntity, inventory);
        }
    }

    /// <summary>
    /// Add an item to an inventory
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="itemEntity">Item entity to add</param>
    /// <param name="slotIndex">Specific slot to place item (-1 for auto-placement)</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddItem(Entity inventoryEntity, Entity itemEntity, int slotIndex = -1)
    {
        if (!_world.HasComponent<InventoryComponent>(inventoryEntity) ||
            !_world.HasComponent<ItemComponent>(itemEntity))
            return false;

        var inventory = _world.GetComponent<InventoryComponent>(inventoryEntity);
        var item = _world.GetComponent<ItemComponent>(itemEntity);

        // Check weight constraints
        if (inventory.CurrentWeight + item.Weight * item.StackCount > inventory.MaxWeight)
            return false; // Inventory too heavy

        // Handle stackable items
        if (item.IsStackable)
        {
            // Try to find an existing stack of the same item
            foreach (var existingItem in inventory.Items.Values)
            {
                if (existingItem.Item.ItemId == item.ItemId &&
                    existingItem.Item.StackCount < existingItem.Item.MaxStackSize)
                {
                    // Calculate how many items can fit in this stack
                    int spaceInStack = existingItem.Item.MaxStackSize - existingItem.Item.StackCount;
                    int itemsToAdd = Math.Min(spaceInStack, item.StackCount);

                    // Update the existing stack
                    var updatedInstance = existingItem;
                    updatedInstance.Item.StackCount += itemsToAdd;
                    inventory.Items[existingItem.SlotIndex] = updatedInstance;

                    // Update the source item if we didn't use all of it
                    if (itemsToAdd < item.StackCount)
                    {
                        item.StackCount -= itemsToAdd;
                        _world.AddComponent(itemEntity, item);

                        // Continue adding the rest of the stack
                        return AddItem(inventoryEntity, itemEntity, slotIndex);
                    }
                    else
                    {
                        // Used the entire item, remove it from the world
                        _world.DestroyEntity(itemEntity);

                        // Update inventory weight
                        inventory.CurrentWeight += item.Weight * itemsToAdd;
                        _world.AddComponent(inventoryEntity, inventory);

                        return true;
                    }
                }
            }
        }

        // If we're here, we need to add the item as a new instance

        // Find an available slot
        int targetSlot = slotIndex;
        if (targetSlot < 0 || inventory.Items.ContainsKey(targetSlot))
        {
            // Find first available slot
            for (int i = 0; i < inventory.MaxSlots; i++)
            {
                if (!inventory.Items.ContainsKey(i))
                {
                    targetSlot = i;
                    break;
                }
            }

            // No slots available
            if (targetSlot < 0)
                return false;
        }

        // Create item instance
        ItemInstance instance = new ItemInstance
        {
            SourceEntityId = itemEntity.Id,
            Item = item,
            SlotIndex = targetSlot
        };

        // Add to inventory
        inventory.Items[targetSlot] = instance;

        // Update inventory weight
        inventory.CurrentWeight += item.Weight * item.StackCount;

        // Hide the item in the world
        // In a real implementation, this would hide the item's visual representation

        // Update inventory
        _world.AddComponent(inventoryEntity, inventory);

        return true;
    }

    /// <summary>
    /// Remove an item from an inventory
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="slotIndex">Slot index to remove from</param>
    /// <param name="count">Number of items to remove (for stacks)</param>
    /// <param name="dropInWorld">Whether to drop the item in the world</param>
    /// <returns>The entity ID of the removed item, or -1 if failed</returns>
    public int RemoveItem(Entity inventoryEntity, int slotIndex, int count = 1, bool dropInWorld = false)
    {
        if (!_world.HasComponent<InventoryComponent>(inventoryEntity))
            return -1;

        var inventory = _world.GetComponent<InventoryComponent>(inventoryEntity);

        // Check if the slot has an item
        if (!inventory.Items.TryGetValue(slotIndex, out ItemInstance instance))
            return -1;

        // Handle stacks
        if (instance.Item.IsStackable && instance.Item.StackCount > count)
        {
            // Only remove part of the stack
            var updatedInstance = instance;
            updatedInstance.Item.StackCount -= count;
            inventory.Items[slotIndex] = updatedInstance;

            // Update inventory weight
            inventory.CurrentWeight -= instance.Item.Weight * count;

            // Create a new item entity for the removed items
            Entity newItemEntity = CreateItemInWorld(instance.Item, count, inventoryEntity, dropInWorld);

            // Update inventory
            _world.AddComponent(inventoryEntity, inventory);

            return newItemEntity.Id;
        }
        else
        {
            // Remove the entire item
            inventory.Items.Remove(slotIndex);

            // Update inventory weight
            inventory.CurrentWeight -= instance.Item.Weight * instance.Item.StackCount;

            // If the item was equipped, unequip it
            foreach (var kvp in inventory.EquippedItems)
            {
                if (kvp.Value == slotIndex)
                {
                    inventory.EquippedItems.Remove(kvp.Key);
                    break;
                }
            }

            // Create or restore the original entity
            Entity itemEntity;
            if (instance.SourceEntityId >= 0)
            {
                // Restore original entity
                itemEntity = new Entity(instance.SourceEntityId);
                _world.AddComponent(itemEntity, instance.Item);

                // Show the item in the world if dropping
                if (dropInWorld)
                {
                    // Position the item near the inventory owner
                    PositionItemInWorld(itemEntity, inventoryEntity);
                }
            }
            else
            {
                // Create a new item entity
                itemEntity = CreateItemInWorld(instance.Item, instance.Item.StackCount, inventoryEntity, dropInWorld);
            }

            // Update inventory
            _world.AddComponent(inventoryEntity, inventory);

            return itemEntity.Id;
        }
    }

    /// <summary>
    /// Equip an item from inventory
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="slotIndex">Inventory slot index to equip</param>
    /// <returns>True if item was equipped successfully</returns>
    public bool EquipItem(Entity inventoryEntity, int slotIndex)
    {
        if (!_world.HasComponent<InventoryComponent>(inventoryEntity))
            return false;

        var inventory = _world.GetComponent<InventoryComponent>(inventoryEntity);

        // Check if the slot has an item
        if (!inventory.Items.TryGetValue(slotIndex, out ItemInstance instance))
            return false;

        // Check if the item is equippable
        if (!instance.Item.IsEquippable)
            return false;

        // Check if something is already equipped in this slot
        if (inventory.EquippedItems.TryGetValue(instance.Item.EquipSlot, out int equippedSlot))
        {
            // Unequip the currently equipped item
            UnequipItem(inventoryEntity, instance.Item.EquipSlot);
        }

        // Equip the new item
        inventory.EquippedItems[instance.Item.EquipSlot] = slotIndex;

        // Update inventory
        _world.AddComponent(inventoryEntity, inventory);

        // Apply equipment effects
        ApplyEquipmentEffects(inventoryEntity, instance.Item, true);

        return true;
    }

    /// <summary>
    /// Unequip an item by equipment slot
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="slot">Equipment slot to unequip</param>
    /// <returns>True if an item was unequipped</returns>
    public bool UnequipItem(Entity inventoryEntity, EquipmentSlot slot)
    {
        if (!_world.HasComponent<InventoryComponent>(inventoryEntity))
            return false;

        var inventory = _world.GetComponent<InventoryComponent>(inventoryEntity);

        // Check if something is equipped in this slot
        if (!inventory.EquippedItems.TryGetValue(slot, out int slotIndex))
            return false;

        // Get the equipped item
        if (!inventory.Items.TryGetValue(slotIndex, out ItemInstance instance))
            return false;

        // Remove equipment effects
        ApplyEquipmentEffects(inventoryEntity, instance.Item, false);

        // Unequip the item
        inventory.EquippedItems.Remove(slot);

        // Update inventory
        _world.AddComponent(inventoryEntity, inventory);

        return true;
    }

    /// <summary>
    /// Use an item from inventory
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="slotIndex">Inventory slot index to use</param>
    /// <returns>True if item was used successfully</returns>
    public bool UseItem(Entity inventoryEntity, int slotIndex)
    {
        if (!_world.HasComponent<InventoryComponent>(inventoryEntity))
            return false;

        var inventory = _world.GetComponent<InventoryComponent>(inventoryEntity);

        // Check if the slot has an item
        if (!inventory.Items.TryGetValue(slotIndex, out ItemInstance instance))
            return false;

        // Process item use based on category
        bool wasUsed = false;

        switch (instance.Item.Category)
        {
            case ItemCategory.Consumable:
                wasUsed = UseConsumableItem(inventoryEntity, instance.Item);
                break;

            case ItemCategory.Key:
                wasUsed = UseKeyItem(inventoryEntity, instance.Item);
                break;

            case ItemCategory.Tool:
                wasUsed = UseToolItem(inventoryEntity, instance.Item);
                break;

            case ItemCategory.Readable:
                wasUsed = ReadItem(inventoryEntity, instance.Item);
                break;

            case ItemCategory.Equipment:
                // Equipment is used by equipping it
                wasUsed = EquipItem(inventoryEntity, slotIndex);
                break;

            // Add other categories as needed

            default:
                // Default item use behavior
                wasUsed = false;
                break;
        }

        // If the item was used and is consumable, reduce its count or remove it
        if (wasUsed && instance.Item.IsConsumable)
        {
            RemoveItem(inventoryEntity, slotIndex, 1, false);
        }

        return wasUsed;
    }

    /// <summary>
    /// Use a consumable item
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="item">Item to use</param>
    /// <returns>True if item was used successfully</returns>
    private bool UseConsumableItem(Entity inventoryEntity, ItemComponent item)
    {
        // In a real implementation, this would apply effects based on the item
        // For example, restore health, add temporary buffs, etc.

        // This is a placeholder implementation
        return true;
    }

    /// <summary>
    /// Use a key item
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="item">Item to use</param>
    /// <returns>True if item was used successfully</returns>
    private bool UseKeyItem(Entity inventoryEntity, ItemComponent item)
    {
        // In a real implementation, this would check for nearby doors or locks
        // that can be opened with this key

        // This is a placeholder implementation
        return false; // Keys typically aren't consumed on use
    }

    /// <summary>
    /// Use a tool item
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="item">Item to use</param>
    /// <returns>True if item was used successfully</returns>
    private bool UseToolItem(Entity inventoryEntity, ItemComponent item)
    {
        // In a real implementation, this would use the tool on a nearby
        // interactable object or perform some other action

        // This is a placeholder implementation
        return false; // Tools typically aren't consumed on use
    }

    /// <summary>
    /// Read a readable item
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="item">Item to read</param>
    /// <returns>True if item was read successfully</returns>
    private bool ReadItem(Entity inventoryEntity, ItemComponent item)
    {
        // In a real implementation, this would display the text content
        // of the readable item to the player

        // This is a placeholder implementation
        return true; // Reading always succeeds but doesn't consume the item
    }

    /// <summary>
    /// Apply or remove equipment effects
    /// </summary>
    /// <param name="inventoryEntity">Entity with inventory</param>
    /// <param name="item">Equipment item</param>
    /// <param name="isEquipping">True if equipping, false if unequipping</param>
    private void ApplyEquipmentEffects(Entity inventoryEntity, ItemComponent item, bool isEquipping)
    {
        // In a real implementation, this would modify the entity's stats or
        // appearance based on the equipped item

        // This is a placeholder implementation
    }

    /// <summary>
    /// Create a new item entity in the world
    /// </summary>
    /// <param name="item">Item component data</param>
    /// <param name="count">Stack count for the item</param>
    /// <param name="sourceEntity">Entity that's dropping the item</param>
    /// <param name="placeInWorld">Whether to position the item in the world</param>
    /// <returns>The created item entity</returns>
    private Entity CreateItemInWorld(ItemComponent item, int count, Entity sourceEntity, bool placeInWorld)
    {
        // Create a new entity for the item
        Entity itemEntity = _world.CreateEntity();

        // Create a copy of the item with the specified count
        var newItem = item;
        newItem.StackCount = count;

        // Add item component
        _world.AddComponent(itemEntity, newItem);

        // Add transform component
        var transform = new TransformComponent
        {
            Position = Vector3.Zero,
            Rotation = Quaternion.Identity,
            Scale = Vector3.One
        };
        _world.AddComponent(itemEntity, transform);

        // Add interactable component for picking up
        var interactable = new InteractableComponent
        {
            DisplayName = item.DisplayName,
            Type = InteractionType.Pickup,
            IsEnabled = true,
            HighlightColor = new Vector3(0.0f, 1.0f, 1.0f) // Cyan highlight for items
        };
        _world.AddComponent(itemEntity, interactable);

        // Position the item in the world if requested
        if (placeInWorld)
        {
            PositionItemInWorld(itemEntity, sourceEntity);
        }

        return itemEntity;
    }

    /// <summary>
    /// Position an item near an entity in the world
    /// </summary>
    /// <param name="itemEntity">Item to position</param>
    /// <param name="sourceEntity">Entity to position near</param>
    private void PositionItemInWorld(Entity itemEntity, Entity sourceEntity)
    {
        if (!_world.HasComponent<TransformComponent>(sourceEntity) ||
            !_world.HasComponent<TransformComponent>(itemEntity))
            return;

        var sourceTransform = _world.GetComponent<TransformComponent>(sourceEntity);
        var itemTransform = _world.GetComponent<TransformComponent>(itemEntity);

        // Position the item slightly in front and below the source entity
        Vector3 forward = sourceTransform.Forward;
        Vector3 dropOffset = forward * 0.5f - new Vector3(0, 0.5f, 0);

        itemTransform.Position = sourceTransform.Position + dropOffset;

        // Add a small random offset to prevent item stacking
        Random random = new Random();
        Vector3 randomOffset = new Vector3(
            (float)random.NextDouble() * 0.4f - 0.2f,
            0.0f,
            (float)random.NextDouble() * 0.4f - 0.2f
        );

        itemTransform.Position += randomOffset;

        // Update the transform component
        _world.AddComponent(itemEntity, itemTransform);
    }

    /// <summary>
    /// Update the lists of entities we're tracking
    /// </summary>
    private void UpdateEntityLists()
    {
        // In a real implementation, we would have a more efficient way of tracking
        // entities with InventoryComponents and ItemComponents
        _inventories.Clear();
        _items.Clear();

        // We would need to iterate all entities in the world
        // For now, this is a placeholder implementation
    }
}