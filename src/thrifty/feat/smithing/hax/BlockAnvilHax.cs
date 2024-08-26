using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using thrifty.common.data;
using thrifty.common.utils;
using thrifty.common.x;
using thrifty.debug;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace thrifty.feat.smithing.hax;

[HarmonyPatchCategory(Const.Harmony.Category.Server)]
[HarmonyPatch(typeof(BlockAnvil), nameof(BlockAnvil.OnBlockInteractStart))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class BlockAnvilHax {

  [HarmonyPrefix]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  private static void prefix(
    IWorldAccessor world,
    IPlayer byPlayer,
    BlockSelection blockSel,
    out StackInfo? __state
  ) {
    __state = null;

    if (byPlayer.Entity?.Controls?.ShiftKey != true) {
      Logs.trace("ignoring anvil interaction: player wasn't holding shift");
      return;
    }

    // If the entity is not an anvil entity??? then bail.
    if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityAnvil) {
      Logs.trace("ignoring anvil interaction: block wasn't an anvil entity?");
      return;
    }

    var stack = byPlayer.InventoryManager!.ActiveHotbarSlot?.Itemstack;

    // if the player doesn't have an item in hand (or there is no hotbar), then
    // bail
    if (stack == null) {
      Logs.trace("ignoring anvil interaction: player wasn't holding anything");
      return;
    }

    var type = itemTypeOf(stack.Item!);

    // if the held item is not one we care about, then bail.
    if (type == ItemType.Irrelevant) {
      Logs.trace("ignoring anvil interaction: we don't care about {0}", stack.Item.Code);
      return;
    }

    // If we got here, then we know the player shift-clicked an anvil entity
    // with an item in hand that we may care about.
    //
    // Store the relevant info to the temp state shared with the postfix
    // function to determine if the player placed one of the items on the anvil.
    __state = new StackInfo(
      type,
      stack.Item!.Code,
      byPlayer.InventoryManager!.ActiveHotbarSlotNumber,
      stack.StackSize
    );
    Logs.debug("setting anvil interaction state: {0}", __state);
  }

  [HarmonyPostfix]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  private static void postfix(
    IWorldAccessor world,
    IPlayer byPlayer,
    BlockSelection blockSel,
    StackInfo? __state,
    bool __result
  ) {
    // 1. Only operate when the underlying method returned true (allowed the
    //    interaction)
    // 2. Only run if we have state (null if nothing relevant was happening)
    if (!__result || __state is null) {
      if (!__result)
        Logs.trace("ignoring anvil interaction: anvil rejected it");
      return;
    }

    // Grab the item stack at the hotbar position that was active at the start
    // of the patched method call.
    var stack = byPlayer.InventoryManager.GetHotbarItemstack(__state.pos);

    // If no stack was available at the given position and the original stack
    // size was 1, then hopefully that means the player put down their single
    // item onto the anvil (leaving an empty slot where the item was).
    //
    // TODO: sort out this if/else-if
    if (stack == null && __state.size == 1) {
      Logs.debug("anvil interaction: player used the last item in their held stack");
    }

    // If the item code for whatever they were holding in the prefix method does
    // not match what is currently in the slot, wonkiness is happening.
    //
    // TODO: Is it possible that a player can put down multiple items in a single shift-click?
    else if (!__state.code.Equals(stack?.Item?.Code)) {
      Logs.debug("ignoring anvil interaction: couldn't tell what was going on");
      return;
    }

    var entity = (BlockEntityAnvil) world.BlockAccessor.GetBlockEntity(blockSel.Position);
    var remainingItems = stack?.StackSize ?? 0;

    // If the anvil has no work item, then the player interaction was hopefully
    // not relevant to us because we can't do anything about it.
    if (entity.WorkItemStack == null) {
      Logs.debug("ignoring anvil interaction: anvil had no work item");
      return;
    }

    // If the item stack in the player's hand did not lose any items, then they
    // didn't put anything down, bail here.
    //
    // TODO: what if the player is on creative mode?
    if (remainingItems >= __state.size) {
      Logs.debug("ignoring anvil interaction: player item stack size did not change");
      return;
    }

    // Grab mod-specific info about the work item.
    var workData = entity.getWorkData() ?? new();

    // Increase the count of the relevant item type for whatever the player just
    // put on the anvil.
    switch (__state.type) {
      case ItemType.Ingot:
        workData.ingotCount += (byte) (__state.size - remainingItems);
        break;

      case ItemType.Plate:
        workData.plateCount += (byte) (__state.size - remainingItems);
        break;

      case ItemType.Irrelevant:
      default:
        Logs.warn("something fishy is afoot in the BlockAnvil.OnBlockInteractStart patch");
        return;
    }

    // Save the updated work info.
    entity.setWorkData(workData);
  }

  private static ItemType itemTypeOf(Item item) =>
    Paths.firstPathEntry(item.Code) switch {
      Const.Code.Default.IngotPathPrefix => ItemType.Ingot,
      Const.Code.Default.MetalPlatePathPrefix => ItemType.Plate,
      _ => ItemType.Irrelevant,
    };

  private enum ItemType : byte { Ingot, Plate, Irrelevant }

  private record StackInfo {
    internal readonly ItemType type;
    internal readonly AssetLocation code;
    internal readonly int pos;
    internal readonly int size;

    public StackInfo(ItemType type, AssetLocation code, int pos, int size) {
      this.type = type;
      this.code = code;
      this.pos = pos;
      this.size = size;
    }

    public override string ToString() => $"Stack(type={type}, code={code}, pos={pos}, size={size})";
  }
}
