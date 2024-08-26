using HarmonyLib;
using thrifty.common.data;
using thrifty.debug;
using thrifty.feat.smithing.util;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace thrifty.feat.items.breaking.hax;

[HarmonyPatchCategory(Const.Harmony.Category.Server)]
[HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.DamageItem))]
internal class ToolBreakingPatch {

  [HarmonyPrefix]
  private static void prefix(
    IWorldAccessor world,
    Entity byEntity,
    ItemSlot itemslot,
    int amount,
    CollectibleObject __instance,
    out State __state
  ) {
    __state = new(false, itemslot.Itemstack, itemslot.Itemstack.StackSize);

    // If it's not being damaged by a player, skip
    if (byEntity is not EntityPlayer) {
      Logs.trace("ignoring: entity was not a player");
      return;
    }

    // If it's not an item type, skip
    if (__instance.ItemClass != EnumItemClass.Item) {
      Logs.trace("ignoring: object was not of type Item");
      return;
    }

    // If it's not from a player's inventory, skip.
    // Projectiles use a dummy item slot.
    if (itemslot.Inventory is null) {
      Logs.trace("ignoring: no inventory (probably a projectile)");
      return;
    }

    // If it doesn't have a known crafting voxel value, skip.
    if (!__instance.hasKnownVoxelValue()) {
      Logs.trace("ignoring: no known voxel value");
      return;
    }

    // If it's got enough remaining durability that this won't break it, skip.
    if (__instance.GetRemainingDurability(itemslot.Itemstack!) > amount) {
      Logs.trace("ignoring: not gonna break yet");
      return;
    }

    // Set the state to true to indicate that we should act in the postfix.
    __state = new(true, __state.instance, __state.count);
  }

  [HarmonyPostfix]
  private static void postfix(
    IWorldAccessor world,
    Entity byEntity,
    ItemSlot itemslot,
    int amount,
    CollectibleObject __instance,
    State __state
  ) {
    if (!__state.shouldAct) {
      Logs.trace("ignoring: __state.shouldAct was false");
      return;
    }

    if (
      itemslot.Itemstack is not null
      && ReferenceEquals(itemslot.Itemstack, __state.instance)
      && itemslot.Itemstack.StackSize == __state.count
    ) {
      Logs.trace("ignoring: nothing seems to have changed");
      return;
    }

    Smithy.calculateScrap()
  }

  private readonly struct State {
    internal readonly bool      shouldAct;
    internal readonly ItemStack instance;
    internal readonly int       count;

    public State(bool shouldAct, ItemStack instance, int count) {
      this.instance  = instance;
      this.shouldAct = shouldAct;
      this.count     = count;
    }
  }
}
