using thrifty.common.data;
using thrifty.debug;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace thrifty.common.utils;

public static class BitGen {

  internal static void issueBits(int count, BlockEntityAnvil anvil) {
    if (count < 1) {
      Logs.trace("not issuing bits, given count was 0");
      return;
    }

    var world = anvil.Api.World;
    var code = getBitCode(anvil);
    var item  = world.GetItem(code);

    if (item != null)
      world.SpawnItemEntity(new(item, count), anvil.Pos.ToVec3d());
    else
      Logs.debug("could not issue bits, no such item found for {0}", code);
  }

  internal static void issueBits(int count, IPlayer player, BlockEntityAnvil anvil) {
    if (count < 1) {
      Logs.trace("not issuing bits, given count was 0");
      return;
    }

    var world = player.Entity.World;
    var code = getBitCode(anvil);
    var item  = world.GetItem(code);

    if (item == null) {
      Logs.debug("could not issue bits, no such item found for {0}", code);
      return;
    }

    var stack = new ItemStack(item, count);

    if (!player.InventoryManager.TryGiveItemstack(stack, true)) {
      Logs.debug("failed to give player item stack for bits, throwing them on the floor instead");
      world.SpawnItemEntity(stack, player.Entity.Pos.XYZ);
    }
  }

  private static AssetLocation getBitCode(BlockEntityAnvil anvil) {
    var input = anvil.SelectedRecipe.Ingredient.Code!;
    return new(input.Domain, Paths.makePathOf(Const.Code.Default.MetalBitPathPrefix, Paths.lastPathEntry(input)));
  }
}
