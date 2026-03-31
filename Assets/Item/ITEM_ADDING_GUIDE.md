# Item Adding Guide

Current item range is `I001` to `I014`.
`I007` is reserved by an old placeholder script, so the safest next ID is `I015`.

## 1. Script

Create a new script in `Assets/Item/Scripts/ItemList`, usually by copying a nearby item.

Minimum checklist:

- Class name matches the file name, for example `I015`.
- `Reset()` sets a unique `itemID`, `itemName`, grade, expiration type, and usage type.
- If the item changes runtime data, implement:
  - `ResetRuntimeState()`
  - `CaptureRuntimeState()`
  - `RestoreRuntimeState()`
- Use `OnAcquire`, `OnUse`, `OnRemove`, `OnReapply`, `OnPray`, `OnSpeech`, or `OnHpReachedZero` only when needed.

## 2. ScriptableObject Asset

Create the item asset in `Assets/Item/Assets`.

Checklist:

- Asset file name matches the script name.
- `itemID` exactly matches the code value.
- `itemImage` is assigned.
- Description and effect text are filled in for the inventory UI.

## 3. Field Prefab

Create or duplicate a prefab in `Assets/Item/ItemPrefab`.

Checklist:

- `FieldItem.itemData` points to the correct item asset.
- Collider is set as a trigger.
- Visuals and animation are connected if needed.

## 4. Spawn Registration

Register the prefab in `InGameManager`:

- `commonItemPrefabs` for `ItemGrade.Common`
- `rareItemPrefabs` for `ItemGrade.Rare`

This is what drives field spawning and save restoration for dropped field items.

## 5. Runtime Behavior Notes

The inventory now creates runtime copies of item assets when items are acquired or restored from save.
That means:

- Duplicate items can coexist safely.
- Runtime state is saved per acquired item, not shared across the base asset.
- Any item with mutable state should keep that state inside the item instance and persist it through the runtime-state methods.

## 6. Hook Points

Important gameplay hooks already wired in the project:

- `InventoryManager.AddItem()` for acquisition
- `InventoryManager.UseItem()` for active item use
- `Cardinal.Pray()` for prayer effects
- `Cardinal.Speech()` for speech effects
- `Cardinal.OnHpReachedZero()` item passives for last-stand effects
- `ElectionManager` for special-case election items like `I012`

## 7. Current Caveats

- `I007.cs` is only a placeholder and does not define a usable item.
- Special-case items that depend on manager logic may still need manual integration outside the base item hooks.
