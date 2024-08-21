using ThriftySmithing.Data;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ThriftySmithing.Utils;

public static class BitGen {

  internal static void issueBits(int count, BlockEntityAnvil anvil) {
    if (count < 1)
      return;

    var world = anvil.Api.World;
    var item  = world.GetItem(getBitCode(anvil));

    if (item != null)
      world.SpawnItemEntity(new(item, count), anvil.Pos.ToVec3d());
  }

  internal static void issueBits(int count, IPlayer player, BlockEntityAnvil anvil) {
    if (count < 1)
      return;

    var world = player.Entity.World;
    var item = world.GetItem(getBitCode(anvil));

    if (item == null)
      return;

    var stack = new ItemStack(item, count);

    if (!player.InventoryManager.TryGiveItemstack(stack, true))
      world.SpawnItemEntity(stack, player.Entity.Pos.XYZ);
  }

  private static AssetLocation getBitCode(BlockEntityAnvil anvil) {
    var input = anvil.SelectedRecipe.Ingredient.Code!;
    return new(input.Domain, Paths.makePathOf(Const.DefaultMetalBitPathPrefix, Paths.lastPathEntry(input)));
  }
}
