using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

using thrifty.common;
using thrifty.common.util;
using thrifty.feature.smithing_scrap.ext;

namespace thrifty.feature.smithing_scrap.hax;

internal class BlockAnvilHax {
  private static MethodInfo? onBlockInteractStart;

  internal static void patch(Harmony harmony) {
    var target = typeof(BlockAnvil);
    Logs.trace("patching {0}", target.Name);
    onBlockInteractStart = harmony.Patch(
      target.GetMethod(nameof(BlockAnvil.OnBlockInteractStart)),
      new(prefix),
      new(postfix)
    );
  }

  internal static void unpatch(Harmony harmony) {
    if (onBlockInteractStart is not null) {
      var target = typeof(BlockAnvil);
      Logs.trace("unpatching {0}", target.Name);
      harmony.Unpatch(target.GetMethod(nameof(BlockAnvil.OnBlockInteractStart)), onBlockInteractStart);
      onBlockInteractStart = null;
    }
  }

  private static void prefix(
    IWorldAccessor world,
    IPlayer byPlayer,
    BlockSelection blockSel,
    out StackInfo? __state
  ) {
    __state = null;

    if (world.Side.IsClient() || byPlayer.Entity?.Controls?.ShiftKey != true)
      return;

    // If the entity is not an anvil entity??? then bail.
    if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityAnvil) {
      Logs.debug("block wasn't an anvil entity?");
      return;
    }

    var stack = byPlayer.InventoryManager!.ActiveHotbarSlot?.Itemstack;

    // if the player doesn't have an item in hand (or there is no hotbar), then
    // bail
    if (stack == null)
      return;

    var type = itemTypeOf(stack.Item!);

    // if the held item is not one we care about, then bail.
    if (type == IngredientType.Irrelevant)
      return;

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
  }

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
    // 3. Only run if we are executing on the server side.
    if (!__result || __state is null || world.Side.IsClient())
      return;

    // Grab the item stack at the hotbar position that was active at the start
    // of the patched method call.
    var stack = byPlayer.InventoryManager.GetHotbarItemstack(__state.pos);

    // If no stack was available at the given position and the original stack
    // size was 1, then hopefully that means the player put down their single
    // item onto the anvil (leaving an empty slot where the item was).
    //
    // TODO: sort out this if/else-if
    if (stack == null && __state.size == 1) {
      // Do nothing.
    }

    // If the item code for whatever they were holding in the prefix method does
    // not match what is currently in the slot, wonkiness is happening.
    //
    // TODO: Is it possible that a player can put down multiple items in a single shift-click?
    else if (!__state.code.Equals(stack?.Item?.Code)) {
      Logs.debug("couldn't tell what was going on with that anvil interaction, ignoring it");
      return;
    }

    var entity = (BlockEntityAnvil) world.BlockAccessor.GetBlockEntity(blockSel.Position);
    var remainingItems = stack?.StackSize ?? 0;

    // If the anvil has no work item, then the player interaction was hopefully
    // not relevant to us because we can't do anything about it.
    if (entity.WorkItemStack == null)
      return;

    // If the item stack in the player's hand did not lose any items, then they
    // didn't put anything down, bail here.
    if (remainingItems >= __state.size)
      return;

    // Grab mod-specific info about the work item.
    var workData = entity.getWorkData() ?? new();

    // Increase the count of the relevant item type for whatever the player just
    // put on the anvil.
    switch (__state.type) {
      case IngredientType.Ingot:
        workData.ingotCount += (byte) (__state.size - remainingItems);
        break;

      case IngredientType.Plate:
        workData.plateCount += (byte) (__state.size - remainingItems);
        break;

      case IngredientType.Irrelevant:
      default:
        Logs.warn("something fishy is afoot in the BlockAnvil.OnBlockInteractStart patch");
        return;
    }

    // Save the updated work info.
    entity.setWorkData(workData);
  }

  private static IngredientType itemTypeOf(Item item) =>
    Paths.firstPathEntry(item.Code) switch {
      Const.DefaultIngotPathPrefix => IngredientType.Ingot,
      Const.DefaultMetalPlatePathPrefix => IngredientType.Plate,
      _ => IngredientType.Irrelevant,
    };

  private record StackInfo {
    internal readonly IngredientType type;
    internal readonly AssetLocation code;
    internal readonly int pos;
    internal readonly int size;

    public StackInfo(IngredientType type, AssetLocation code, int pos, int size) {
      this.type = type;
      this.code = code;
      this.pos = pos;
      this.size = size;
    }
  }
}
