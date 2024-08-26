using Vintagestory.API.Common;

namespace thrifty.common.x;

internal static class XPlayer {
  internal static void give(this IPlayer player, ItemStack stack) {
    if (!player.InventoryManager.TryGiveItemstack(stack, true))
      player.Entity.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ);
  }
}
